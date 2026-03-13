using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace MyPlugins.BehaviourTree.EditorSection
{

    //与自定义Node区别，这里的Node是为了在GraphView视图中以节点的形式显示，而自定义Node是作为运行时的节点存在
    //Tip：当时的理解很奇怪，其实就是视图节点和数据节点，一个用于编辑器UI，一个用于运行时逻辑。
    public class NodeView : UnityEditor.Experimental.GraphView.Node { 
        public Action<NodeView> OnNodeSelected;
        public NodeData node; //对应的数据节点
        public Port input;
        public Port output;

        //视图节点所需要的就是一个uxml文件代表其样式，然后一个数据节点，读取其中的一些用于显示的数据来显示当前节点的标识信息
        //如果不传入UXML文件的话，就会采用默认的视图节点外观。
        public NodeView(NodeData node) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeViewUxml)) { //大概应该在初始化时就获取配置，然后作为一个全局成员存在
            this.node = node;
            this.node.name = node.GetType().Name; //获取运行时类型的类名
            //在节点中显示名称时将"(Clone)"和"Node"删掉
            this.title = node.name.Replace("(Clone)", "").Replace("Node", "");
            //用于视图数据持久化（GraphView的GetNodeByGuid方法就可以通过查找viewDataKey与传入的guid值相同的视图节点然后返回）
            this.viewDataKey = node.guid; 
            //应用之前保存的节点位置，即还原
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts(); //创建输入端口。
            CreateOutputPorts(); //创建输出端口。
            SetupClasses(); //设置样式
            SetupDataBinding(); //
        }


        /*Tip：这些都是NodeView的UXMl中*/
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
            else if (node is ControlNode)
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

        
        //Tip：都是GraphView提供的类型。
        //创建输入端口，主要是为不同类型设置能否连接多个端口（Single或Multiple）
        private void CreateInputPorts() {
            if (node is ActionNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is ControlNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is DecoratorNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is RootNode) { //根节点只有输出端口，没有输入端口。

            }

            if (input != null) {
                input.portName = ""; //没有端口名，就只是一个小圆圈。端口名就是与圆圈同级的一个Label的Text属性值，实在没啥必要。
                input.style.flexDirection = FlexDirection.Column;
                /*Tip；这一步添加端口，会自动将端口类Port的node字段设置为该类的引用。
                还有这里的inputContainer和下面的outputContainer，默认引用的是名为input和output的元素，所以在NoewView的uxml文件中
                一定不能修改对应元素的名字，*/
                inputContainer.Add(input); 
            }
        }

        private void CreateOutputPorts() {
            if (node is ActionNode) { //动作节点必然是叶子结点，所以没有输出端口。

            } else if (node is ControlNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            } else if (node is DecoratorNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            } else if (node is RootNode) {
                //Tip：不知道为什么之前写成了Single，根节点明显就是要连接多个分支啊。。。
                //TODO：但是发现，牵涉到运行时部分，暂时不动。
                output = new NodePort(Direction.Output, Port.Capacity.Single);
                // output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }

            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse; // 这样的话，端口元素靠边，因为有个标签和端口圆圈是一起的，这就是用来调这两个位置的。
                outputContainer.Add(output);
            }
        }

        //应该是在视图中拖拽节点时调用，此处就是在移动节点位置后，还要记录下节点当前的位置，以便在重新打开窗口时能够还原之前所在的位置
        //经测试，在按住节点拖拽时每次移动都会调用，而移动视图并不会调用
        public override void SetPosition(Rect newPos) { //设置视图节点位置
            //基类方法会根据鼠标移动长度来相应地移动节点位置。
            base.SetPosition(newPos); //如果没有调用基类方法，但存储了位置，会发现再次打开窗口时会设置为之前鼠标松开的位置
            // Debug.Log("调用SetPosition");
            //Tip：按下撤销键会发现退回到开始移动时的位置，其实是因为注册了treeView的OnUndoRedo方法，会立刻刷新视图数据，否则的话就只是回退了NodeData记录的值、并没有同步到UI显示。
            Undo.RecordObject(node, "Behaviour Tree (Set Position)");
            //左上角为原点，代表坐标位置
            //Tip：NodeData记录位置数据，因为需要脱离编辑器，而编辑器中更换行为树之后会将之前的视图节点全部清空，所以需要放在数据节点中才能持久存储。
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }


        /*Tip：在NodeView和BehaviourTreeView都有一个完全相同的OnNodeSelected成员，在编辑窗口类BehaviourTreeEditor的CreateGUI方法中为BehaviourTreeView中的该成员赋值（就是注册
        回调方法），因为在BehaviourTreeEditor中无法获取到各个视图节点，并且也不需要，只要在BehaviourTreeView中处理视图节点即可，所以BehaviourTreeView中的OnNodeSelected完全是
        为了将定义在BehaviourTreeEditor中的私有方法OnNodeSelectionChanged传递到NodeView这里调用，如果不是私有的话，就可以直接在此处调用并传入自身。设置为私有，并且因此而在
        NodeView和BehaviourTreeView中设置相同的Action委托成员，就是为了解耦合，保证各自的独立性，只通过固定的接口交流，然后在修改时可以避免受到修改目标之外的干扰，只要遵守
        明确的规定即可，比如委托成员OnNodeSelected的参数就是NodeView，那么就不能变动这个参数列表，至于执行逻辑则随意修改，其实接口类型的作用就是如此，规定在比较自由的修改的
        状态下必须遵守的一些基本限制。

        OnNodeSelectionChanged定义在BehaviourTreeEditor中是因为其逻辑所依赖的对象，也就是要读取的对象inspectorView只能在BehaviourTreeEditor中获取，
        但确实触发该方法的位置又在NodeView这里。
        还有一点，设置为Action委托主要是为了扩展性（其实就是观察者模式），因为选中节点以后可能会增加其他要执行的方法，而不仅仅是更新检视视图。
        */

        //Tip：GraphElement被选中时调用（似乎是鼠标单击选中时触发，但框选不会触发）
        public override void OnSelected() { 
            base.OnSelected();
            if (OnNodeSelected != null) {
                OnNodeSelected.Invoke(this);
            }
        }

        /// <summary>
        /// 根据视图节点位置决定数据节点顺序即优先级
        /// </summary>
        public void SortChildren() {
            if (node is ControlNode composite) { //因为只有组合节点才会有多个子节点
                composite.children.Sort(SortByHorizontalPosition);
            }
        }
        //根据视图节点的水平位置对数据节点排序，也就是数据与显示同步。
        private int SortByHorizontalPosition(NodeData left, NodeData right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        /// <summary>
        /// 根据节点状态更新样式类，就是使用特定颜色来表示此时处于哪个状态
        /// </summary>
        public void UpdateState() {

            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying) {
                switch (node.state) {
                    case NodeData.State.Running:
                        //一般情况下处于Running必然是started为true，但存在中断机制，即Node的Abort方法，会使得处于Running状态，但是started为false
                        if (node.started) { 
                            AddToClassList("running");
                        }
                        break;
                    case NodeData.State.Failure:
                        AddToClassList("failure");
                        break;
                    case NodeData.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }

        //Tip：重写以给视图节点的上下文菜单添加更多内容（在默认的基础上），当然也可以不调用base实现，就不会有默认的菜单项了。
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            // evt.menu.AppendAction($"Delete", );
        }
    }
}