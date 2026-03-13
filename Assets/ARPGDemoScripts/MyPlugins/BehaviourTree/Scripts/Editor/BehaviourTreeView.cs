using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace MyPlugins.BehaviourTree.EditorSection
{
    public class BehaviourTreeView : GraphView {

        //注册自定义控件。
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { } 
        public Action<NodeView> OnNodeSelected; //选中节点时的回调，用于传递给NodeView，因为它才有被选中时调用的消息方法
        /*暴露给UI Builder，这样可以在UI Builder中将自定义控件添加到对应的容器中，如果是代码中添加的话，就必须先获取容器，
        然后再用容器调用Add方法将自定义控件放进去，其实完全没必要如此，当然完全的程序化也有它自己的好处。因为不会受到界面操作的影响，
        而且出错时能够获取报错信息，便于调试。*/
        BehaviourTree tree; //当前视图内显示的行为树。
        BehaviourTreeSettings settings; //从该设置的资产文件中获取视图内要使用的UI Toolkit资产

        public struct ScriptTemplate {
            public TextAsset templateFile; //文本文件的运行时代表。脚本文件本质上就是文本文件。
            public string defaultFileName;
            public string subFolder;
        }

        //脚本模板
        public ScriptTemplate[] scriptFileAssets = {
            
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().ActionNodeTemplate, defaultFileName="NewActionNode.cs", subFolder="Actions" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().CompositeNodeTemplate, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().DecoratorNodeTemplate, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
        };

        public BehaviourTreeView() {
            //这样可以通过资产文件直接编辑要引用的内容，不用在代码中修改。
            settings = BehaviourTreeSettings.GetOrCreateSettings();
            
            Insert(0, new GridBackground()); //网格背景

            this.AddManipulator(new ContentZoomer()); //缩放网格。
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            //双击可以同时选中其子节点。这是个自定义操纵器
            this.AddManipulator(new DoubleClickSelection()); 

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

        //撤销或重做时都应该调用，
        public void OnUndoRedo() {
            AssetDatabase.SaveAssets(); //撤销修改的是内存对象，需要及时保存到外存上。
            PopulateTreeView(tree); //数据变化，所以重新填充显示内容。
        }

        /// <summary>
        /// 通过传入数据节点的guid查找对应的视图节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public NodeView FindNodeView(NodeData node) { //视图节点拥有记录数据节点的字段，但反过来没有，这也很合理，因为数据就应该专注于数据，而不用牵涉显示。
            return GetNodeByGuid(node.guid) as NodeView;
        }

        /// <summary>
        /// 根据行为树生成GraphView视图内容（节点和连线）
        /// </summary>
        /// <param name="tree"></param>
        internal void PopulateTreeView(BehaviourTree tree) {
            //BugFix：判空是因为可能在没有选中行为树的情况下打开窗口，并且按下撤销，就会传入空。其实正常人不会这样干，而且这也不会影响运行时，但开发者应该考虑周全。
            if (tree == null) return; 

            this.tree = tree; //节点视图的行为树。

            graphViewChanged -= OnGraphViewChanged; //视图中发生某些改变时调用
            //首先清空GraphView中的所有元素GraphElement，GraphView内置方法，直接调用即可。
            DeleteElements(graphElements.ToList()); 
            graphViewChanged += OnGraphViewChanged;

            //Tip：只要在编辑窗口中打开行为树，如果没有就会自动生成一个根节点
            if (tree.rootNode == null) { 
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            //Tip：为每个数据节点创建视图节点（这里注意所有节点的子节点都必须在dataNodes列表中，否则就会因为没有创建对应的NodeView而在下面的连线逻辑中出现空引用错误）
            tree.dataNodes.ForEach(n => {
                //Ques：忘了这是我写的还是从开源项目里面带过来的了。。。
                // if (n is Timeout timeout)
                // {
                //     if (timeout.child == null)
                //         Debug.Log("Timeout节点的子节点为空");
                // }
                CreateNodeView(n);
            });

            // Create edges创建连线，就是从根节点开始，将节点与其子节点相连。所以各个节点的子节点必须首先在dataNodes列表中，创建对应的NodeView，然后才能连线
            tree.dataNodes.ForEach(n => {
                var children = BehaviourTree.GetChildren(n);
                children.ForEach(c => { //如果为空，则不执行任何逻辑，不会报错。
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);
                    //在使用ConnectTo构造连线时就会根据端口的Direction值来对Edge的output和input对象赋值。
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    if (edge == null) Debug.Log($"连线为空\n父：{n.GetType().Name}, 子：{c.GetType().Name}");
                    if (edge != null) Debug.Log($"连线不为空\n父：{n.GetType().Name}, 子：{c.GetType().Name}");
                    AddElement(edge); 
                });
            });
            //默认选中根节点。由于还有除了打开编辑窗口的其他方式（比如撤销）会调用Populate方法，要是都选中根节点就不合适了，所以只需要在窗口类的初始化方法中调用即可
            // NodeView rootNodeView = FindNodeView(tree.rootNode);
            // AddToSelection(rootNodeView); //这是GraphView的选中列表，添加即选中。
        }

        /// <summary>
        /// 清除可视化节点视图
        /// </summary>
        public void ClearNodeGraph()
        {
            foreach (var node in nodes) //GraphView记录图中节点的集合。
            {
                //移除连线
                //ToList 方法是一个 LINQ 扩展方法，主要用于将实现了 IEnumerable<T> 接口的集合转化为一个 List<T> 对象。这对于需要以列表的形式操作集合时非常有用。
                edges.ToList()
                     .Where(x => x.input.node == node) //条件过滤（用output不行，因为input只能连接一个，也就是一个input对应一个连线，而output可以连接多个，而一次只能删除一个，所以会发生遗漏）
                     .ToList()
                     .ForEach(edge => RemoveElement(edge)); //统一处理

                //移除节点
                RemoveElement(node);
            }
        }

        //获取可连接端口
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        /// <summary>
        /// 注册到graphViewChanged事件中作为回调方法，在视图发生改变时调用，用于处理节点和连线
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.elementsToRemove != null) { //可以按下Delete键删除（选中多个则同时删除多个）
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView; //Tip：这种类型的方法都是基于基类定义的，都需要先将其转换为实际类型后才开始处理
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
            //因为在移动节点时就会触发graphViewChange事件，而节点的顺序是按照从左到右的，所以这样就可以保持数据节点顺序和视图节点位置同步。
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

            evt.menu.AppendAction($"删除选中节点", (a) => DeleteSelectedNodes(), (menuAction) => {
                //如果选中元素中，存在视图节点，则可以点击删除，否则不可点击。
                if (selection.OfType<NodeView>().Any()) return DropdownMenuAction.Status.Normal;
                else return DropdownMenuAction.Status.Disabled;
            });
            evt.menu.AppendSeparator();
            // New script functions
            //menu为DropdownMenu类，其实菜单几乎都是这样的没有折叠的下拉框形式。
            evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendSeparator(); //添加一个分隔线

            //将鼠标位置转换到当前节点视图的局部坐标
            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            /*TODO：创建节点应该作为单独的一个菜单项，点击后就会打开搜索窗口，这样更加直观方便，而且节点类型多了之后也可以用上查找功能。
            而且B版的搜索窗口的目录结构是通过为节点标记特性时指定的路径来构造的，显然更具有扩展性。*/
            //因为使用了同样命名的局部变量types，所以用大括号进行划分区域，将局部变量的生命域和作用域限定在大括号内
            {
                //结构体TypeCollection类型，内部自定义了迭代逻辑，可以直接获取Type类型
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[动作节点]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[条件节点]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<ControlNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[控制节点]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[修饰节点]/{type.Name}", (a) => CreateNode(type, nodePosition));
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
            NodeData node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }
        //根据运行时节点创建编辑时节点，注意传入选中视图节点时的回调方法，这里就是更新检视面板
        void CreateNodeView(NodeData node) {
            NodeView nodeView = new NodeView(node);
            //将注册到委托OnNodeSelected中的用于选中节点时的回调方法传递到NodeView中，然后通过GraphElement.OnSelected方法调用NodeView中的OnNodeSelected
            nodeView.OnNodeSelected = OnNodeSelected; 
            AddElement(nodeView); //GraphView内置方法，添加GraphElement。
        }

        /// <summary>
        /// 删除选中的节点，注意同时删除视图节点和数据节点
        /// </summary>
        void DeleteSelectedNodes() 
        {
            Undo.SetCurrentGroupName("删除节点"); //新建一个Undo组，用来把下面要进行的多个Undo记录整合到一个撤销操作中
            int groupIndex = Undo.GetCurrentGroup(); //获取创建的group的索引。

            //取出所有选中的视图节点。其实更严谨的应该是Node，但是项目中本来都是NodeView，所以在此直接转换算了。
            List<NodeView> selectedNodes = selection.OfType<NodeView>().ToList();
            if (selectedNodes.Remove(FindNodeView(tree.rootNode))) //不能移除根节点。选中的节点不存在的话会返回false，总之不会报错
                Debug.Log("不能删除根节点");

            // Undo.RegisterCompleteObjectUndo(tree, ""); 
            Undo.RecordObject(tree, "行为树"); //为了保存dataNodes的引用

            selectedNodes.ForEach(nodeView => {
                //BugFix：严重失误，这里本质上只是移除了引用即相当于指针，而没有移除引用对象，也就是对应的资产文件。显然还是应该以C++的思维来编写C#呐。
                // tree.nodes.Remove(nodeView.node); //移除数据节点
                // UnityEngine.Object.DestroyImmediate(nodeView.node, true); //这样才是删除引用对象,别忘了加true才能这样销毁资产。

                //引入撤销功能之后，虽然结果一样，但必须为此考虑执行顺序，撤销时的顺序是相反的，所以需要先恢复对象即节点资产，再恢复引用，即dataNodes列表元素。
                NodeData node = nodeView.node;
                if (node) 
                {
                    tree.dataNodes.Remove(node); //删除引用
                    if (node.parent != null)
                    {
                        // Undo.RecordObject(node.Parent, "");
                        tree.RemoveChild(node.parent, node); //处理父子关系
                    }
                }
                
                // //这是在内存中将从外存加载的子资产副本删除了。这个方法也是一个结束动作。（在下面的SaveAssets方法中才会真正删除掉外存上的资产）
                Undo.DestroyObjectImmediate(node); 
                // tree.DeleteNode(nodeView.node);
            });
            //BugFix：ToArray会创建并返回数组副本，不会改变原本的List。其实这里有坑，因为是副本，所以Undo实际上无法起到记录可撤销操作的作用。
            // Undo.RecordObjects(tree.dataNodes.ToArray(), ""); 
            // Undo.RecordObject(tree, "");
            // Undo.RegisterCompleteObjectUndo(tree, "");

            //感觉放在BehaviourTree中的GetChildren中处理更加方便，反正每次调用PopulateTreeView都会调用该方法来获取子节点，而且删除之后就会立刻调用Populate
            // tree.dataNodes.ForEach(node => {
            //     if (node is CompositeNode compositeNode) 
            //         compositeNode.children.RemoveAll(n => n == null);
            // });

            //别忘了删除节点之后排个序。
            //这里改变的也是tree的dataNodes即数据节点，其中组合节点的children的元素排序
            nodes.ForEach(node => { //注意这里的nodes是GraphView的字段，存储视图中所有的节点引用。
                NodeView nodeView = node as NodeView;
                nodeView.SortChildren();
            });
            //BUG：撤销时如果选中的是节点所在行为树，可以看到恢复的节点的图标是C#脚本图标，而不是资产图标，而切换一下选中对象就好了。
            EditorUtility.SetDirty(tree); //手动设置为dirty，属于安全措施，因为防止自动机制未生效，而丢失更改。
            // tree.dataNodes.ForEach(n => EditorUtility.SetDirty(n));
            AssetDatabase.SaveAssets(); //注意处理子资产
            //将以上记录的所有操作整合到创建的group中，这样就可以一键撤销。其实本来就应该一键撤销，只是因为操作的复杂度而不得不分开记录。
            // Undo.CollapseUndoOperations(groupIndex); //不过似乎上面只是结束标志，并不是分组标志，其实还是在同一个组中。
            //处理完数据，再重新填充一下节点视图。
            //Tip：这样的话只需要处理数据，而不需要处理视图，但是这毕竟是全部重新填充，当行为树更加复杂后会不会因为删除操作而带来卡顿？
            PopulateTreeView(tree);
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