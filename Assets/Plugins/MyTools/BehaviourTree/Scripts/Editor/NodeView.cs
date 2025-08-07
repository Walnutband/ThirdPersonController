using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace MyTools.BehaviourTreeTool
{

    public class NodeView : UnityEditor.Experimental.GraphView.Node { //与自定义Node区别，这里的Node是为了在GraphView视图中以节点的形式显示，而自定义Node是作为运行时的节点存在
        public Action<NodeView> OnNodeSelected;
        public Node node; //对应的数据节点
        public Port input;
        public Port output;

        //视图节点所需要的就是一个uxml文件代表其样式，然后一个数据节点，读取其中的一些用于显示的数据来显示当前节点的标识信息
        //如果不传入UXML文件的话，就会采用默认的视图节点外观。
        public NodeView(Node node) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeXml)) { //大概应该在初始化时就获取配置，然后作为一个全局成员存在
            this.node = node;
            this.node.name = node.GetType().Name; //获取运行时类型的类名
            //在节点中显示名称时将"(Clone)"和"Node"删掉
            this.title = node.name.Replace("(Clone)", "").Replace("Node", "");
            //用于视图数据持久化（GraphView的GetNodeByGuid方法就可以通过查找viewDataKey与传入的guid值相同的视图节点然后返回）
            this.viewDataKey = node.guid; 
            //应用之前保存的节点位置，即还原
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        /// <summary>
        /// 设置样式类，以便自动应用对应节点类型的样式选择器
        /// </summary>
        private void SetupClasses()
        {
            //根据数据节点类型设置，这是添加到视图节点的根元素上，
            if (node is ActionNode)
            {
                AddToClassList("action");
            }
            else if (node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (node is DecoratorNode)
            {
                AddToClassList("decorator");
            }
            else if (node is RootNode)
            {
                AddToClassList("root");
            }
        }

        private void SetupDataBinding() { //数据绑定，这里只绑定了节点的描述
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node)); //自动寻找指定对象上的名为“description”的序列化变量
        }

        
        
        //创建输入端口，主要是为不同类型设置能否连接多个端口（Single或Multiple）
        private void CreateInputPorts() {
            if (node is ActionNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is CompositeNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is DecoratorNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is RootNode) {

            }

            if (input != null) {
                input.portName = ""; //没有端口名，就只是一个小圆圈。端口名就是与圆圈同级的一个Label的Text属性值。
                input.style.flexDirection = FlexDirection.Column;
                /*Tip；这一步添加端口，会自动将端口类Port的node字段设置为该类的引用。
                还有这里的inputContainer和下面的outputContainer，默认引用的是名为input和output的元素，所以在NoewView的uxml文件中
                一定不能修改对应元素的名字，*/
                inputContainer.Add(input); 
            }
        }

        private void CreateOutputPorts() {
            if (node is ActionNode) {

            } else if (node is CompositeNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            } else if (node is DecoratorNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            } else if (node is RootNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        //应该是在视图中拖拽节点时调用，此处就是在移动节点位置后，还要记录下节点当前的位置，以便在重新打开窗口时能够还原之前所在的位置
        public override void SetPosition(Rect newPos) { //设置视图节点位置
            //基类方法会根据鼠标移动长度来相应地移动节点位置。
            base.SetPosition(newPos); //如果没有调用基类方法，但存储了位置，会发现再次打开窗口时会设置为之前鼠标松开的位置
            Undo.RecordObject(node, "Behaviour Tree (Set Position");
            //左上角为原点，代表坐标位置
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        //GraphElement被选中时调用（似乎是鼠标单击选中时触发，但框选不会触发）
        /*Tip：在NodeView和BehaviourTreeView都有一个完全相同的OnNodeSelected成员，在编辑窗口类BehaviourTreeEditor的CreateGUI方法中为BehaviourTreeView中的该成员赋值（就是注册
        回调方法），因为在BehaviourTreeEditor中无法获取到各个视图节点，并且也不需要，只要在BehaviourTreeView中处理视图节点即可，所以BehaviourTreeView中的OnNodeSelected完全是
        为了将定义在BehaviourTreeEditor中的私有方法OnNodeSelectionChanged传递到NodeView这里调用，如果不是私有的话，就可以直接在此处调用并传入自身。设置为私有，并且因此而在
        NodeView和BehaviourTreeView中设置相同的Action委托成员，就是为了解耦合，保证各自的独立性，只通过固定的接口交流，然后在修改时可以避免受到修改目标之外的干扰，只要遵守
        明确的规定即可，比如委托成员OnNodeSelected的参数就是NodeView，那么就不能变动这个参数列表，至于执行逻辑则随意修改，其实接口类型的作用就是如此，规定在比较自由的修改的
        状态下必须遵守的一些基本限制。

        OnNodeSelectionChanged定义在BehaviourTreeEditor中是因为其逻辑所依赖的对象，也就是要读取的对象inspectorView只能在BehaviourTreeEditor中获取，
        但确实触发该方法的位置又在NodeView这里。
        还有一点，设置为Action委托主要是为了扩展性，因为选中节点以后可能会增加其他要执行的方法，而不仅仅是更新检视视图。
        */
        public override void OnSelected() { 
            base.OnSelected();
            if (OnNodeSelected != null) {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren() {
            if (node is CompositeNode composite) {
                composite.children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        /// <summary>
        /// 根据节点状态更新
        /// </summary>
        public void UpdateState() {

            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying) {
                switch (node.state) {
                    case Node.State.Running:
                        //一般情况下处于Running必然是started为true，但存在中断机制，即Node的Abort方法，会使得处于Running状态，但是started为false
                        if (node.started) { 
                            AddToClassList("running");
                        }
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}