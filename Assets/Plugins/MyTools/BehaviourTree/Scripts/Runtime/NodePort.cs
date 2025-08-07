using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MyTools.BehaviourTreeTool
{

    public class NodePort : Port { //Port < GraphElement < VisualElement

        // GITHUB:UnityCsReference-master\UnityCsReference-master\Modules\GraphViewEditor\Elements\Port.cs
        //可以发现这里的这个类和Port源码中的定义完全相同。不过这样拷贝之后，可以直接在构造方法中添加操纵器。
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener {
            /*IEdgeConnectorListener被EdgeConnector所使用，用来完成实际的连线创建，可以重写来以不同的方式连线*/
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener() {
                m_EdgesToCreate = new List<Edge>();
                m_EdgesToDelete = new List<GraphElement>();

                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }
            //实现IEdgeConnectorListener的两个接口方法
            //TODO：可以在连线落在port之外时打开节点选择菜单，直接点击创建，同时直接相连，这也是节点编辑器通常的做法。
            public void OnDropOutsidePort(Edge edge, Vector2 position) { }
            //调用时，会传入Port所在的GraphView，以及传入将要创造的Edge（需要它的成员提供的相关信息）
            public void OnDrop(GraphView graphView, Edge edge) {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);

                // We can't just add these edges to delete to the m_GraphViewChange
                // because we want the proper deletion code in GraphView to also
                // be called. Of course, that code (in DeleteElements) also
                // sends a GraphViewChange.
                m_EdgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.input.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);
                if (edge.output.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.output.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);
                if (m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements(m_EdgesToDelete); //批量删除GraphElement

                var edgesToCreate = m_EdgesToCreate;
                if (graphView.graphViewChanged != null) {
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
                }
                /*Tip：创建连线的基本思路，就是一个Edge实例，其output和input成员都引用了对应的端口Port，此时其实就可以直接AddElement添加
                该连线了，就会在两个引用的端口之间连线，但此时两个端口的connections成员还没有添加该连线，所以还要对两个端口分别调用
                Connect方法，将该连线Edge成员加入到它们各自的类型IEnumerable<Edge>的成员connections中。这样一来图形元素的数据记录才是一致的。
                图形元素尤其忌讳数据不一致，导致显示错误，而这种错误很容易误导图形化的编辑过程。*/
                foreach (Edge e in edgesToCreate) {
                    graphView.AddElement(e);
                    edge.input.Connect(e);
                    edge.output.Connect(e);
                }
            }
        }
        //阅读Port源码会发现，它有个Create方法，其中也会添加一个名为DefaultEdgeConnectorListener的操纵器，也是一个私有的嵌套类，不过构造函数中不会添加
        //由于第一个参数Orientation和第二个参数System.Type基本用的是固定值，所以在派生类的构造函数中就可以更方便地构造实例了。
        //Ques:暂不清楚第四个参数（对应的是Port的portType成员）有什么用
        public NodePort(Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity, typeof(bool)) {
            var connectorListener = new DefaultEdgeConnectorListener();
            //EdgeManipulator < MouseManipulator < Manipulator < IManipulator
            m_EdgeConnector = new EdgeConnector<Edge>(connectorListener); //Edge也可以是自定义的派生类
            //这是扩展方法，在VisualElementExtensions类中，this就是方法的第一个参数，会被传入的Manipulator的target字段所引用
            this.AddManipulator(m_EdgeConnector); 
            this.AddToClassList("node-port");
            //style.width = 100; //改为在选择器中设置，NodePort的大小决定了Port的有效范围。
        }
        /// <summary>
        /// 用来确定落点是否在Port上
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public override bool ContainsPoint(Vector2 localPoint) { //Ques：经测试，其实不加这个也可以，需要看一下Port中原本的实现才能理解。
            //这里的localPoint应该是以NodePort元素自身的局部坐标系为参考系计算的坐标，否则就不准确了
            //根据该方法的结果而决定调用OnDrop还是OnDropOutsidePort
            Rect rect = new Rect(0, 0, layout.width, layout.height);
            return rect.Contains(localPoint);
        }
    }
}