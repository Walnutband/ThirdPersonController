using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace MyTools.BehaviourTreeTool
{
    public class BehaviourTreeView : GraphView {

        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { } 
        public Action<NodeView> OnNodeSelected; //选中节点时的回调，用于传递给NodeView，因为它才有被选中时调用的消息方法
        /*暴露给UI Builder，这样可以在UI Builder中将自定义控件添加到对应的容器中，如果是代码中添加的话，就必须先获取容器，
        然后再用容器调用Add方法将自定义控件放进去，其实完全没必要如此，当然完全的程序化也有它自己的好处。因为不会受到界面操作的影响，
        而且出错时能够获取报错信息，便于调试。*/
        BehaviourTree tree; //
        BehaviourTreeSettings settings;

        public struct ScriptTemplate {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        //脚本模板
        public ScriptTemplate[] scriptFileAssets = {
            
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateActionNode, defaultFileName="NewActionNode.cs", subFolder="Actions" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
        };

        public BehaviourTreeView() {
            //这样可以通过资产文件直接编辑要引用的内容，不用在代码中修改。
            settings = BehaviourTreeSettings.GetOrCreateSettings();
            
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection()); //双击可以同时选中其子节点
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.behaviourTreeStyle;  
            styleSheets.Add(styleSheet);

            // if (Undo.undoRedoPerformed != null)
            // {
            //     foreach (var f in Undo.undoRedoPerformed.GetInvocationList())
            //     {
            //         Undo.undoRedoPerformed -= OnUndoRedo;
            //     }
            // }

            /*Tip：偶然发现，只要在项目窗口中选中UXML文件就会调用其中UI控件的构造函数。所以应该在窗口类中即BehaviourTreeEditor类的CreateGUI方法
            中注册回调方法，因为其在打开窗口时调用，而且还可以再关闭窗口时调用的OnDestroy方法中进行注销。
            像Undo.undoRedoPerformed这样的委托，都是专门开放给开发者自行为固定事件、固定操作注册回调方法的，而其内置的那些回调方法并不包含在
            这个委托中，所以可以直接将其置空null，来实现一键注销的效果。*/

            // Debug.Log("dd");
            //Undo.undoRedoPerformed = null;
            //Undo.undoRedoPerformed -= OnUndoRedo;
            //在执行撤销或重做操作时触发该回调，为了保证对视图数据的改变能够立刻反映在视图上
            //Undo.undoRedoPerformed += OnUndoRedo;
        }

        public void OnUndoRedo() {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 通过传入数据节点的guid查找对应的视图节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public NodeView FindNodeView(Node node) { //视图节点拥有记录数据节点的字段，但反过来没有，这也很合理，因为数据就应该专注于数据，而不用牵涉显示。
            return GetNodeByGuid(node.guid) as NodeView;
        }

        /// <summary>
        /// 根据行为树生成GraphView视图内容（节点和连线）
        /// </summary>
        /// <param name="tree"></param>
        internal void PopulateView(BehaviourTree tree) {
            //BugFix：判空是因为可能在没有选中行为树的情况下打开窗口，并且按下撤销，就会传入空。其实正常人不会这样干，而且这也不会影响运行时，但开发者应该考虑周全。
            if (tree == null) return; 

            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged; //视图中发生某些改变时调用
            DeleteElements(graphElements.ToList()); //首先清空GraphView中的所有元素GraphElement
            graphViewChanged += OnGraphViewChanged;

            if (tree.rootNode == null) { //也就是说只要在编辑窗口中打开行为树，如果没有就会自动生成一个根节点
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view创建视图节点
            tree.nodes.ForEach(n => CreateNodeView(n));

            // Create edges创建连线
            tree.nodes.ForEach(n => {
                var children = BehaviourTree.GetChildren(n);
                children.ForEach(c => {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);
                    //在使用ConnectTo构造连线时就会根据端口的Direction值来对Edge的output和input对象赋值。
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }

        //获取可连接端口
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.elementsToRemove != null) { //可以按下Delete键删除（选中多个则同时删除多个）
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null) {
                        tree.DeleteNode(nodeView.node); //移除数据节点，因为本来就因为删除视图节点而调用该方法（注册的委托graphViewChanged）
                    }
                    //使用as进行类型转换，如果类型不匹配就返回空，不会报错，所以这里就是对删除节点和删除连线的两种情况进行处理，只要注意判空，就可以这样放在一起处理。
                    Edge edge = elem as Edge;
                    if (edge != null) {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null) {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) => {
                NodeView view = n as NodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        //GraphView的上下文菜单，此处进行自定义
        //GraphView的Node也有上下文菜单，同名。
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

            //base.BuildContextualMenu(evt);

            // New script functions
            //menu为DropdownMenu类，其实菜单几乎都是这样的没有折叠的下拉框形式，
            evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendSeparator(); //添加一个分隔线

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            /*TODO：创建节点应该作为单独的一个菜单项，点击后就会打开搜索窗口，这样更加直观方便，而且节点类型多了之后也可以用上查找功能。
            而且B版的搜索窗口的目录结构是通过为节点标记特性时指定的路径来构造的，显然更具有扩展性。*/
            //因为使用了同样命名的局部变量types，所以用大括号进行划分区域，将局部变量的作用域限定在大括号内
            {

                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }
        }

        void SelectFolder(string path) {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNewScript(ScriptTemplate template) {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        //同时创建数据节点和视图节点
        void CreateNode(System.Type type, Vector2 position) {
            Node node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }
        //根据运行时节点创建编辑时节点
        void CreateNodeView(Node node) {
            NodeView nodeView = new NodeView(node);
            //将注册到委托OnNodeSelected中的用于选中节点时的回调方法传递到NodeView中，然后通过GraphElement.OnSelected方法调用NodeView中的OnNodeSelected
            nodeView.OnNodeSelected = OnNodeSelected; 
            AddElement(nodeView);
        }

        /// <summary>
        /// 根据节点当前状态更新所用样式
        /// </summary>
        public void UpdateNodeStates() {
            nodes.ForEach(n => { //nodes是GraphView的属性，存储了视图中所有的视图节点，注意与行为树的nodes区别
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }

        //并非消息方法，只是被窗口类BehaviourTreeEditor的OnDestroy方法调用来处理关闭窗口时的一些清理工作
        // public void Destroy()
        // {
        //     //Debug.Log("行为树视图摧毁");
        //     Undo.undoRedoPerformed -= OnUndoRedo; //别忘了在关闭行为树窗口时注销，否则会因为tree为空而导致其他的撤销重做操作报空引用错误
        // }
    }
}