using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using System.Reflection;
using System.Linq;

namespace MyPlugins.BehaviourTree.EditorSection
{

    public class BehaviourTreeEditor : EditorWindow {

        public BehaviourTree tree; //当前编辑的树
        BehaviourTreeBlackboard blackboard; //当前所使用的黑板
        BehaviourTreeView treeView; //GraphView视图(由UIElements内置提供，在开发节点式的编辑器时非常实用)
        //当前行为树绑定的黑板的文件路径，用于监听该黑板的删除。注意静态变量在热重载之后会恢复为初始值，但是热重载之后会再次调用CreateInspectorGUI方法，所以在ChangeBlackboard方法中又会对其赋值，所以并无问题
        public static string blackboardPath = ""; //TODO：可能更好的方式是直接显示黑板资产文件，这样可以点击直接在Project视图中高亮显示。
        
        /*TODO：以下定义了一系列字段来存储UI元素，实际上真正需要存储的是要在多处使用的元素，如果是那种只在CreateGUI方法中获取然后注册方法，就不再访问的话，就应该直接用局部变量存储就行了，
        但其实就算要多处使用的元素也可以不用设置成员，因为都可以临时查找的，而且毕竟这是编辑器，多点计算量是毫无影响的。所以严格来说只要保证把那些顶层容器存储好就行了。*/

        List<Button> tabButtons; //页面按钮列表（其实是完全确定的，并不需要动态性）
        VisualElement infoViewContainer; //界面显示容器
        InspectorView inspectorView; //检视视图
        VisualElement blackboardView; //黑板视图
        VisualElement noBlackboardMSG; //幕布，用于提示当前行为树没有绑定黑板
        TextField variableNameField; //变量名显示字段，可输入。
        DropdownField variableTypeField; //变量类型下拉菜单
        List<Type> variableTypes = new List<Type>(); //当前所有的黑板变量类型。注意初始化，否则对一个空引用的列表调用Clear是会报空引用错误的
        ListView variableList; //当前黑板变量的列表视图。
        VisualTreeAsset variableView; //黑板变量的UI结构

        Toolbar editorToolbar; //编辑窗口顶部工具栏
        Label GOName; //游戏对象名）
        Label BTName; //行为树名（资产文件名）
        TextField treeNameField; //Overlay的文件名字段
        //静态变量本身并非持久化，会随着比如热重载这样的过程而被重置即初始化。不过对于连续的创建行为树，可以作为临时缓存选中目录，还是挺方便的。
        static string locationPath = "Assets"; //新建行为树的目录路径，必须是相对路径（相对于项目路径，所以都是Assets起手）
        Button createNewTreeButton; //创建新树按钮
        Button createNewTreeButtonQuick; //创建新树快捷按钮
        VisualElement overlay;
        BehaviourTreeSettings settings;

        [MenuItem("MyPlugins/BehaviourTreeEditor")] 
        public static void OpenWindow() {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor"); //这里是IMGUI，因为UI Toolkit还不支持这样的一些小UI。
            wnd.minSize = new Vector2(600, 400);
        }

        [OnOpenAsset] //双击资源文件时触发自定义逻辑，就是一个反射回调。
        public static bool OnOpenAsset(int instanceId, int line) { //特性要求的参数
            if (Selection.activeObject is BehaviourTree) { //当前选中对象类型是行为树，就打开编辑窗口
                OpenWindow();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取指定类型的所有资产文件（这里就是用于加载BehaviourTree资产文件）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static List<T> LoadAssets<T>() where T : UnityEngine.Object {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}"); //通过类型名指定资产类型，获取各自的guid
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds) {
                string path = AssetDatabase.GUIDToAssetPath(assetId); //这是文件路径，而且是相对于项目文件夹的相对路径
                T asset = AssetDatabase.LoadAssetAtPath<T>(path); //转换为实际类型
                assets.Add(asset);
            }
            return assets;
        }

        public void CreateGUI() {
            //需要从设置文件中加载UXML和USS（因为在Unity编辑器中可以直接拖拽引用，非常方便）
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            // EditorWindow的根VisualElement
            VisualElement root = rootVisualElement;
            //导入行为树编辑器的UXML文件，从中读取数据并且实例化为VisualElement
            var visualTree = settings.behaviourTreeEditorUxml;
            visualTree.CloneTree(root); //从外存克隆到内存

            /*这里实际上就是个专门用于GridBackground元素的选择器，固定的那几个属性，另外主要的USS文件是在UI Builder中就添加好了的，那才是对于编辑器中各个元素所需要的选择器样式。*/
            var styleSheet = settings.behaviourTreeStyle; //加载uss文件到编辑窗口的根元素上，然后uss中的各个选择器匹配相应的UI元素
            root.styleSheets.Add(styleSheet);

            //获取Overlay也就是打开编辑窗口但是未选中任一行为树时的覆盖窗口，可以在这里直接新建行为树
            overlay = root.Q<VisualElement>("Overlay");
            treeNameField = overlay.Q<TextField>("TreeName");
            createNewTreeButton = overlay.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewTree(treeNameField.value); //value就是右边的TextInput里面的值
            createNewTreeButtonQuick = overlay.Q<Button>("QuickCreateButton");
            createNewTreeButtonQuick.clicked += () => QuickCreateNewTree(treeNameField.value); //value就是右边的TextInput里面的值

            //行为树节点视图
            treeView = root.Q<BehaviourTreeView>();
            //选中节点时的方法，用于更新检视面板，由于需要访问窗口中的检视视图，所以不得不定义在该类中
            //TODO：实际上选中节点很可能会影响多个方面，这里的方法只代表了一个方面（更新检视视图），当然也可以将各方面逻辑都塞到同一个方法中，但这显然很破坏结构性。
            treeView.OnNodeSelected = OnNodeSelectionChanged;
            //注册撤销方法，就是每次撤销都要重新绘制节点视图。撤销记录的是数据，撤销之后就要立刻刷新界面，也就是重新读取数据。
            /*TODO：关于在这种编辑器中注册方法，应该还有值得考虑的问题，因为现在只是没怎么开发编辑器，但是Unity编辑器本来就是由多个EditorWindow组成的，而它们所依赖的一些编辑器成员
            比如Selection的各种实用成员，可以认为每个成员都代表一个编辑器功能，所以只要编辑窗口过多的话，要是同时使用同一个成员，比如同时向同一个回调注册自己的方法，很可能造成混乱，
            所以应该要让这些窗口的方法注册和注销都自己统一集中管理起来，以免同时存在多个EditorWindow时发生相互干扰，甚至还应该在当前窗口未获取焦点时就取消响应某些操作，或者在
            获取焦点时就注册那些方法，失去焦点时就注销。*/
            Undo.undoRedoPerformed += treeView.OnUndoRedo; 

            //获取所有选项页，就是所有的界面按钮
            var tabContainer = root.Q<VisualElement>("TabContainer");
            tabButtons = tabContainer.Query<Button>().ToList(); //需要ToList转换为列表类型。
            tabButtons.ForEach(tab => { //对每个界面按钮注册点击方法
            //写成匿名函数，可以在这里直接调用Button控件（闭包），否则就需要注册回调RegisterCallback<ClickEvent>来调用Button本身
            //其实也可以作为参数传入方法，但是注意clicked是一个Action，注册方法不能有参数，所以还是需要匿名函数作为外壳。
                tab.clicked += () => {
                    SelectTabButton(tab);
                };
            });

            //界面显示的容器，存储各个页面
            infoViewContainer = root.Q<VisualElement>("InfoViewContainer"); //显示页面内容
            noBlackboardMSG = infoViewContainer.Q<VisualElement>("MSG-NoBlackboard"); //没有黑板的提示幕布。
            Button createBlackboard = noBlackboardMSG.Q<Button>("CreateBlackboard"); //创建黑板
            createBlackboard.clicked += () => CreateBlackboardForBT();
            //检视视图，注意是自定义控件InspectorView。
            inspectorView = infoViewContainer.Q<InspectorView>("InspectorView"); //虽然是唯一对应的，但是默认VisualElement类型，所以需要类型转换
            // Blackboard view黑板视图
            blackboardView = root.Q<VisualElement>("BlackboardView");
            variableNameField = blackboardView.Q<TextField>("VariableName"); //变量名
            variableTypeField = blackboardView.Q<DropdownField>("VariableType"); //变量类型（下拉框菜单）
            variableList = blackboardView.Q<ListView>("VariableList"); //变量列表
            variableList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; //设置为动态高度
            variableView = settings.variableUxml; //加载黑板变量的UXML
            //添加指定名和类型的黑板变量按钮
            Button addVariableButton = blackboardView.Q<Button>("AddVariable");
            addVariableButton.clicked += () => AddVariable();
            //黑板视图工具栏按钮注册回调
            ToolbarButton moveUp = blackboardView.Q<ToolbarButton>("MoveUp"); //BugFix:之前这里报错，因为写成了MoveUP而不是元素本来的MoveUp。任意字符串真的是高风险存在。
            moveUp.clicked += () => MoveListViewItem(-1); //上移一位
            ToolbarButton moveDown = blackboardView.Q<ToolbarButton>("MoveDown");
            moveDown.clicked += () => MoveListViewItem(1);
            ToolbarButton delete = blackboardView.Q<ToolbarButton>("Delete");
            delete.clicked += () => DeleteVariable();

            //设置编辑窗口顶部工具栏菜单
            editorToolbar = root.Q<Toolbar>("EditorToolbar");
            GOName = editorToolbar.Q<Label>("GameObjectName");
            BTName = editorToolbar.Q<Label>("BehaviourTreeName");
            ToolbarButton toolbarButton = editorToolbar.Q<ToolbarButton>("Frame");
            toolbarButton.clicked += () => treeView.FrameAll(); //提供一个可以回归原本位置的工具按钮
            //工具栏菜单
            ToolbarMenu toolbarMenu = editorToolbar.Q<ToolbarMenu>("Assets"); //查找菜单元素，然后设置菜单项 
            var behaviourTrees = LoadAssets<BehaviourTree>();
            behaviourTrees.ForEach(tree => {
                toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { //以文件名为菜单项名，选择即选中文件
                    Selection.activeObject = tree; //改变选中对象，会触发OnSelectionChange消息方法
                });
            });
            toolbarMenu.menu.AppendSeparator();//列出所有已存在的行为树文件后，加上一个分割线，然后给出一个新建行为树的选项
            toolbarMenu.menu.AppendAction("新建行为树...", (a) => CreateNewTree("New Behaviour Tree"));

            //根据OnSelectionChange的内容，其实tree为空的话就是要检查一下当前选中的对象
            if (tree == null) {
                OnSelectionChange(); //调用消息方法
            } else { //已经存在，就省去了判断有无的部分程序。
                SelectTree(tree);
            }
            //获取当前项目中存在的黑板变量类型，只要出现了增减，就需要调用该方法刷新列表，当然最直接的方式就是关闭窗口重新打开。
            UpdateVariableTypeSelector(); //与有无黑板无关

            //获取当前选择的行为树后再来执行，因为其中会同时尝试获取行为树绑定的黑板，如果没有的话，在选中黑板页面时就需要同时显示noBlackboardMSG
            // if (blackboard != null) GenerateVariableListView(); //这个工作就统一在SelectTree中做了。
            //默认选中第一个页面
            if (tabButtons != null) SelectTabButton(tabButtons[0]); //为了规避掉下面提到的BUG，还是默认选中1算了。
            //感觉还是默认选中黑板更合适，因为一开始没有选中节点，检视面板就是空的
            // if (tabButtons != null) SelectTabButton(tabButtons[1]); 

            //窗口打开时，默认选中根节点。
            /*BugFix：注意千万不要在初始化方法中调用tree，因为本来打开窗口初始化时，tree为空，会调用OnSelectionChange将要执行的逻辑注册到
            EditorApplication.delayCall中，也就是在所有检视面板更新完成之后才调用，即在该CreateInspectorGUI方法中时，tree始终为null，
            所以需要把调用tree的逻辑放到SelectTree中去，其实本来逻辑就是如此，在这里就是选中或切换行为树时，调用SelectTree方法，然后在方法的最后
            选中根节点。*/
            // NodeView rootNodeView = treeView.FindNodeView(tree.rootNode);
            // treeView.AddToSelection(rootNodeView); //这是GraphView的选中列表，添加即选中。

            /*BUG：打开面板后，按情况确实处于黑板视图，但是此时点击节点会发现渲染出了检视面板，也就是和黑板视图重叠在一起，
            而这只会在第一次出现，即如果切换一下检视面板，再切换回来，就不会出现这种情况了。而且甚至在出现这种情况时，在调试窗口中看到
            检视面板仍然的Display仍然是None，就很离谱，不过试了一下，此时无法修改意外显示出来的检视面板中的属性值。*/

        }


        private void OnEnable() {
            //Ques：这里的注册有什么用？难道是因为切换状态时会丢失掉选中的行为树？
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; //严谨，避免重复注册，浪费内存
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= treeView.OnUndoRedo; //由于treeView要在CreateGUI中获取到引用后才可访问，所以不能在Enable中注册
            //就是清理回调，避免残留逻辑，在对应时刻触发导致意外。
            if (tree) tree.blackboardChanged = null;
        }

        /// <summary>
        /// 在当前编辑的行为树所在路径新建一个Blackboard文件，并绑定到该行为树上
        /// </summary>
        private void CreateBlackboardForBT()
        {
            
            BehaviourTreeBlackboard blackboard = ScriptableObject.CreateInstance<BehaviourTreeBlackboard>();
            //获取行为树所在目录路径，中间方法返回的是文件路径（包括扩展名）
            string treeDirectoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(tree)); //从Assets开始，直到包含扩展名的文件名。
            string filePath = Path.Combine(treeDirectoryPath, "New Blackboard.asset"); 
            Debug.Log($"创建的Blackboard文件路径：{filePath}");
            //注意这里创建的资产文件和作为原本的实例并非相同，所以不能直接将原本实例绑定到行为树上，则可以通过创建的路径查找并绑定，
            //但是有个隐含问题是，会将同一路径下的New Blackboard覆盖。所以一般来说应该把行为树和黑板分开存储在不同文件夹下，总之就是要稍微遵循一些行为规范
            AssetDatabase.CreateAsset(blackboard, filePath); //创建资产文件。这样保证创建的资产路径必然为filePath
            AssetDatabase.SaveAssets();
            //保存为资产后再加载再赋值，而不能直接将上面创建的实例赋值给成员blackboard，因为上面的实例并不来自于所保存的资产、随后就该被销毁，而从资产加载才是正确的。
            tree.blackboard = AssetDatabase.LoadAssetAtPath<BehaviourTreeBlackboard>(filePath); //直接将新建的黑板绑定到行为树上
            //其实是需要的，因为回调方法绑定在序列化对象上，而这里只是在内存中修改，必须要选中行为树，显示其检视面板，才会进行序列化更新，然后触发回调
            noBlackboardMSG.style.display = DisplayStyle.None; //同时关闭幕布。似乎不需要，因为已经注册了回调方法
        }

        /// <summary>
        /// 选中界面按钮，保证只有选中按钮具有指定样式类
        /// </summary>
        /// <param name="selectedTab"></param>
        private void SelectTabButton(Button selectedTab)
        {
            foreach (var view in infoViewContainer.hierarchy.Children()) //这是所有的下一级子元素，即直接子元素的集合
            {
                //这是显示页面信息的地方，每个页面都有自己的容器、同时作为infoViewContainer的直接子对象，首先全部隐藏，然后再遍历查找当前选中的页面
                view.style.display = DisplayStyle.None; 
            }

            /*Tip：其实这些对于样式值的处理，都可以通过选择器来完成，也就是在UI Builder中编辑，然后只在代码这里Add和Remove样式类即可。而且这样逻辑更清晰，因为样式类的命名具有明确含义
            */

            foreach (Button tab in tabButtons)
            {
                if (tab == selectedTab) 
                {
                    tab.AddToClassList("tabButton-selected");
                    //Tip：这里命名必须与UXML文件中保持一致，而且每次添加新的页面，就必须在这里处理，说实话非常局限，但是又完全够用，因为必然只有那么几个固定页面，也就是说对于扩展性没有要求
                    switch (tab.name)
                    {
                        case "InspectorTab":
                            inspectorView.style.display = DisplayStyle.Flex;
                            break;
                        case "BlackboardTab":
                            //blackboardView是noBlackboardMSG和黑板内容容器的共同父对象，这样就可以统一设置显隐，而noBlackboardMSG又可以单独设置
                            blackboardView.style.display = DisplayStyle.Flex;
                            //根据此时有无黑板来决定是否显示“创建黑板”的幕布。但是放在处理blackboard改变时的逻辑中更加合理
                            // if (blackboard == null) noBlackboardMSG.style.display = DisplayStyle.Flex;
                            // else noBlackboardMSG.style.display = DisplayStyle.None;
                            break;
                    }
                    // return; //不是找到选中的按钮就结束了，必须要把其他按钮的样式类移除，保证唯一性，也就是互斥性。
                }
                else tab.RemoveFromClassList("tabButton-selected");
            }

        }

        /// <summary>
        /// 在编辑器状态改变时调用（就是编辑模式和运行模式）
        /// </summary>
        /// <param name="obj"></param>
        private void OnPlayModeStateChanged(PlayModeStateChange obj) {
            switch (obj) {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        //Tip：EditorWindow的消息方法，在 Project 窗口 或 Hierarchy 窗口 中更改选中的对象时调用
        private void OnSelectionChange() {
            /*Tip：EditorApplication.delayCall用于在所有 Inspector 面板更新完成后执行一次性操作。它非常适合在编辑器环境中实现延迟调用或处理需要在下一帧执行的任务。
            注意注册的函数只会执行一次，并且这个回调委托属于one-shot 委托（一次性回调），当所有 Inspector 面板更新完成后，Unity 会调用注册的函数，
            并在执行后自动将其从回调列表中移除。所以不用担心重复调用或占用内存。
            */
            EditorApplication.delayCall += () => { //所有检视器刷新后触发该回调
                GOName.text = "<无游戏对象>"; //默认为无，选中才显示
                BTName.text = "<无行为树>";
                // GOName.Unbind(); //默认每次切换选中对象就解绑
                // BTName.Unbind();
                //首先检查是否选中了行为树资产文件，然后检查是否选中了带有执行器的游戏对象，获取执行器上指定的行为树
                BehaviourTree tree = Selection.activeObject as BehaviourTree; //activeObject指的是所有对象。as转换，如果非BehaviourTree就返回null
                if (tree == null) { //并非行为树资产，再检查是否选中了带有执行器的游戏对象
                    if (Selection.activeGameObject) { //activeGameObject指的是场景中的对象以及Project视图中的预制体
                        BehaviourTreeExecutor runner = Selection.activeGameObject.GetComponent<BehaviourTreeExecutor>();
                        if (runner) {
                            GOName.text = runner.gameObject.name;
                            // GOName.Bind(new SerializedObject(runner.gameObject));
                            Debug.Log($"绑定对象:{runner.gameObject.name}");
                            tree = runner.tree;
                            //由于总会调用SelectTree，所以由于黑板绑定在行为树上，所以在其中获取黑板更具有逻辑意义。
                            //blackboard = tree.blackboard; 
                        }
                    }
                }
                // if (tree == null) Debug.Log("树为空");
                // else Debug.Log("树不空");
                if (tree != null) BTName.text = tree.name;
                if (this.tree == tree) return; //切换选中的是同一棵树，或者同为非树即null，则直接返回就可以了
                SelectTree(tree);
                // treeView.FrameAll();
            };
        }

        //Tip：在确定了要编辑的行为树之后，就要调用该方法，这属于编辑器UI的方法，主要用于更新UI数据。
        /// <summary>
        /// 在编辑窗口中显示选中树的视图(只有改变选中对象时会调用，所以属于初始化方法)
        /// </summary>
        /// <param name="newTree"></param>
        void SelectTree(BehaviourTree newTree) {

            if (treeView == null) {
                return;
            }
            //Tip:在编辑窗口打开时，只要选中了行为树，切换选中对象时如果不是其他行为树，则保持编辑当前的行为树
            if (!newTree) { 
                //如果当前编辑窗口未指定行为树才显示幕布，否则在编辑时点击其他只要是非行为树的对象就会显示幕布，就很不方便，正常来说是选中其他行为树就会进行切换，没选中就保持当前行为树
                if (tree == null) 
                {
                    // overlay.style.visibility = Visibility.Visible; //各情况都要进行设置，尽量避免依赖编辑时的默认设置。
                    overlay.style.display = DisplayStyle.Flex;
                    //在没有选中行为树时，会出现行为树的大幕布，而且因为默认选中的是黑板视图，那就还是不显示黑板的幕布了，因为还没选中行为树，就不应该出现“提醒绑定黑板”的内容
                    // blackboardView.style.visibility = Visibility.Hidden; 
                    noBlackboardMSG.style.display = DisplayStyle.None;
                    // treeView.PopulateTreeView(null);//清空节点视图
                    treeView.ClearNodeGraph();
                }
                return;
            }

            //不能放在这里，否则在选中行为树打开窗口时，然后保存c#脚本进行热重载，会发现直接变成未选中即出现幕布，因为热重载之后数据丢失了。
            // 所以要放在SelectionChange方法中执行，保证是在切换选择时发生，排除前面这种特殊情况
            // if (tree == newTree) return;

            ListenBlackboardChanged(tree, newTree); //保证newTree非空
            this.tree = newTree; //当前编辑的行为树
            // BTName.text = this.tree.name;
            // BTName.Bind(new SerializedObject(this.tree));
            // Debug.Log("绑定行为树");
            // 未选中行为树时的覆盖面板，可以直接选择创建行为树，当然也可以选择已有行为树。不过在打开窗口时才会出现，因为在窗口中只有选择其他行为树时才会切换视图显示内容，单纯取消选中并不会切换。
            // overlay.style.visibility = Visibility.Hidden;
            overlay.style.display = DisplayStyle.None;

            // if (Application.isPlaying) {
            //     treeView.PopulateView(tree);
            // } else {
            //     treeView.PopulateView(tree);
            // }
            treeView.PopulateTreeView(tree); //根据选中行为树更新节点视图
            // treeObject = new SerializedObject(tree); //行为树的序列化对象。似乎并不需要
            // blackboardProperty = treeObject.FindProperty("blackboard"); //获取行为树上的黑板对象BlackBoard
            //BUG:在窗口保持打开时，进行热重载，会发现FrameAll似乎没有效果
            EditorApplication.delayCall += () => {
                //用于调整视图的缩放和位置，以便将图中的所有元素都显示在视图中。它的主要作用是让用户能够快速聚焦到整个图形的内容，而无需手动缩放或拖动视图。
                treeView.FrameAll();
                // Debug.Log("执行了FreameAll");  
            };
            //默认选中根节点
            NodeView rootNodeView = treeView.FindNodeView(tree.rootNode);
            treeView.AddToSelection(rootNodeView); //这是GraphView的选中列表，添加即选中。

        }
        /// <summary>
        /// 监听当前编辑的行为树绑定的黑板变化，及时反映到编辑窗口。黑板幕布，以及视图内容，主要是变量列表，因为其他与具体黑板无关
        /// </summary>
        /// <param name="oldTree"></param>
        /// <param name="newTree"></param>
        private void ListenBlackboardChanged(BehaviourTree oldTree, BehaviourTree newTree)
        {
            //避免在之前选择过的行为树改变blackboard时还会影响到当前的编辑窗口
            if (oldTree != null) oldTree.blackboardChanged = null; //直接置空，简单方便。不再监听之前选择的行为树的黑板变化
            //切换黑板（注意不同行为树可能引用的是相同的黑板），以及监听当前行为树
            if (blackboard != newTree.blackboard) ChangeBlackboard(newTree);
            //考虑均为空的情况，别忘了打开幕布
            else if (blackboard == null) noBlackboardMSG.style.display = DisplayStyle.Flex;
            //监听当前选中的行为树绑定的黑板变化
            newTree.blackboardChanged += () => ChangeBlackboard(newTree);

        }
        /// <summary>
        /// 改变编辑窗口引用的黑板
        /// </summary>
        /// <param name="newTree"></param>
        private void ChangeBlackboard(BehaviourTree newTree)
        {
            blackboard = newTree.blackboard;
            //BUG:发现如果直接从Project视图中删除当前编辑的行为树的黑板，是不会调用该方法的，也就是说没有触发blackboardChanged。而选中树（删除操作必然会选中删除对象）之后才会触发、
            // Debug.Log("Change");
            if (blackboard == null)
            {
                blackboardPath = "";
                noBlackboardMSG.style.display = DisplayStyle.Flex;
                variableList.itemsSource = null; //只要置空数据源，就可以去掉所有显示项
                variableList.Rebuild();
                newTree.blackboard = null; //将Missing转换为None
            }
            else
            {
                /*记录路径，才可以对硬盘上存储的对应的资产文件进行操作。*/
                blackboardPath = AssetDatabase.GetAssetPath(blackboard);
                GenerateVariableListView();
                noBlackboardMSG.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// 改变所选中节点，更新检视面板显示此时选中节点的内容
        /// </summary>
        /// <param name="node"></param>
        void OnNodeSelectionChanged(NodeView node) {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate() {
            treeView?.UpdateNodeStates();
        }

#region 创建行为树
        /// <summary>
        /// 快速创建行为树，就是直接在Assets文件夹下创建
        /// </summary>
        /// <param name="assetName"></param>
        void QuickCreateNewTree(string assetName) {
            //目录路径+文件名
            string path = System.IO.Path.Combine("Assets", $"{assetName}.asset"); //该方法可以智能处理分隔符
            BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
            //tree.name = treeNameField.ToString(); //资产名
            //tree.name = assetName; //Object的name默认为文件名，所以不用特意赋值
            AssetDatabase.CreateAsset(tree, path); //从内存保存到外存上
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree; //创建后同时选中
            //EditorGUIUtility.PingObject(tree);
        }

        void CreateNewTree(string defaultName)
        {
            string assetsFolderPath = Application.dataPath; //项目的Assets文件夹路径。通过这个路径才能将绝对路径转换为相对路径
            string absFilePath = EditorUtility.SaveFilePanel("创建行为树", locationPath, defaultName, "asset"); //返回文件路径（包括扩展名）
            //StartsWith判断是否以指定的字符串开头，这里就是判断绝对路径是否在Assets路径中
            // Debug.Log($"assetsFolderPath: {assetsFolderPath}\nabsFilePath: {absFilePath}");

            if (string.IsNullOrEmpty(absFilePath)) //点击Cancel或关闭，都会返回空字符串，则不做任何处理
                return;
            
            if (!absFilePath.StartsWith(assetsFolderPath))  //在绝对路径下判断此时是否位于该项目的资产路径中
            {
                EditorUtility.DisplayDialog("创建失败", $"无法创建到{absFilePath}路径中，\n因为该路径超出了该项目所在位置，\n请在该项目的Assets文件夹中创建",
                "确定"); //也可以设置一个重来按钮，不过一般来说没必要，就是一个确定按钮即可。
                return;
            }
            //Ques：这里有可能会出现路径分隔符的问题，即“/”和“\”的区别，可以使用Path.Combine来自动处理，不过一般在跨平台时才会遇到，只是要知道存在这个隐患。
            string relFilePath = "Assets" + absFilePath.Substring(assetsFolderPath.Length); //取出绝对路径中在Assets之后的部分。
            // 获取目录路径
            locationPath = Path.GetDirectoryName(relFilePath);
            // Debug.Log($"文件路径：{filePath}\n目录路径：{locationPath}");
            // 获取文件名(包括扩展名)
            //string fileName = Path.GetFileName(relFilePath); 
            BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>(); //创建内存实例
            /*Tip：这里CreateAsset应该是创建在Unity编辑器所在的内存空间中即Project视图中的对象，而SaveAssets则是将其保存到硬盘上，而上面的CreateInstance应该算是在
            编辑器对象所要使用的实例所在的内存空间中创建对象。*/
            AssetDatabase.CreateAsset(tree, relFilePath); //从内存保存到外存（硬盘）上,需要指定相对文件路径
            AssetDatabase.SaveAssets();
            //Debug.Log($"tree name: {tree.name}");
            Selection.activeObject = tree; //创建后同时选中
        }
#endregion

#region 处理黑板视图
//对黑板变量处理，永远要留意刷新变量列表
        //生成黑板变量列表（实质上是绑定数据）
        private void GenerateVariableListView()
        {
            if (blackboard == null) return;

            variableList.makeItem = () => //决定每个元素的结构
            {
                TemplateContainer variableViewInstance = variableView.CloneTree();
                return variableViewInstance;
            };
            variableList.bindItem = (item, index) =>
            {
                //显示变量名称。
                item.Q<Label>("VariableName").text = blackboard.variables[index].key;
                //每个变量都是一个SO资产。
                SerializedObject serializedObject = new SerializedObject(blackboard.variables[index]);
                SerializedProperty property = serializedObject.FindProperty("val"); 
                //属性字段的检视器。
                var field = item.Q<PropertyField>("Field");
                field.label = "";
                field.BindProperty(property); //绑定，这样在编辑窗口中编辑黑板变量和在变量文件的检视面板中编辑是同步的。
            };
            variableList.itemsSource = blackboard.variables; //ListView会根据itemsSource.Count作为总行数，
            //设置为动态高度，以便不同类型的变量能够正常显示其检视样式。
            // variableList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; 
            variableList.Rebuild();//读取变化后的黑板数据，注意立刻刷新
        }

        /// <summary>
        /// 添加variable并刷新variableList
        /// </summary>
        private void AddVariable()
        {
            if (Application.isPlaying) return; //运行时不可增减变量
            if (string.IsNullOrEmpty(variableNameField.text)) return; //首先要有名字。
            int selectedIndex = -1;
            //读取UI对象所记录的当前数据，据此创建变量。
            blackboard.CreateVariable(variableNameField.text, variableTypes[variableTypeField.index], ref selectedIndex); //下拉框和变量类型列表是一一对应的
            // Debug.Log($"name: {variableNameField.text}, type: {variableTypes[variableTypeField.index]}");
            if (selectedIndex >= 0) variableList.selectedIndex = selectedIndex; //非负，说明已存在，则选中。
            variableList.Rebuild(); //刷新ListView
        }

        /// <summary>
        /// 删除选中的variable并刷新variableList
        /// </summary>
        private void DeleteVariable() //不需要传入参数，是因为
        {
            if (Application.isPlaying) return;
            if (variableList.selectedItem == null) return;
            //通过key寻找。
            var variable = blackboard.variables.Find(variable => variable.key == (variableList.selectedItem as BlackboardVariable).key);
            if (variable != null)
            {
                DestroyImmediate(variable, true); //由于这是子资产，默认是不可移除的，如果真要移除，就要加一个参数true。
                blackboard.variables.RemoveAll(v => v == null);
                variableList.Rebuild();
                //Tip：只有立刻保存，才能同时将对应的子资产也一并移除
                /*将所有未保存的资产更改写入到磁盘中，如果没有，则会发现只移除了列表元素，而子资产还存在，但实际上也不是如此，因为只是
                看到检视面板中移除了，打开黑板文件的YAML文本查看，并没有变化，而进行一下域重载，会发现子资产的检视面板消失了，但是仍然
                YAML文本没有变化。
                DestroyImmediate删除的应该是内存中的对象。*/
                AssetDatabase.SaveAssets(); 
            }
        }

        /// <summary> 
        /// 更新添加变量的type下拉菜单（更新变量variableTypes和variableTypeField.choices）
        /// </summary>
        private void UpdateVariableTypeSelector()
        {
            variableTypes.Clear(); //必须首先为列表分配实例
            variableTypeField.choices.Clear(); //这里字段的列表成员choices应该是自动分配了实例的

            //Tip：这里获取的就是BlackboardVariable类所在的程序集，更加准确，而不需要查找所有程序集，本来就应该将所有黑板变量类型定义在这个程序集中。
            Assembly assembly = typeof(BlackboardVariable).Assembly;
            IEnumerable<Type> enumerable = assembly //这里获取到的都是指定条件下的实际类型的元数据
                .GetTypes()
                .Where( //需要使用System.Linq查询
                    myType => //是类，非抽象类，是BlackboardVariable的派生类
                        myType.IsClass
                        && !myType.IsAbstract
                        && myType.IsSubclassOf(typeof(BlackboardVariable)) //需要转化为Type类型的元数据
    );
            //这样同时添加，就可以建立起一个映射关系，即在类型的下拉框中选中元素的索引可以直接作为在variableTypes中访问对应类型的索引
            //Tip：其实也可以用字典，typeName作为键，类型元数据作为值，因为确实具有唯一性，不过这里也是一种技巧，将两个数组的索引一一对应，实现字典的效果。
            foreach (Type type in enumerable)
            {
                string typeName = type.Name;
                string suffix = "Variable"; //去掉类名的后缀，直接显示单独的类型名
                                            //这里也是命名要求，就应该以这个后缀结尾来命名
                if (typeName.EndsWith(suffix)) typeName = typeName.Substring(0, typeName.Length - suffix.Length);
                variableTypes.Add(type);
                variableTypeField.choices.Add(typeName);
                // variableTypeField.choices.Add(type.Name);
            }


            //遍历整个程序集查找所有继承自BlackboardVariable类 即找到所有variable类型
            // foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            // {
            //     IEnumerable<Type> enumerable = assembly //这里获取到的都是指定条件下的实际类型的元数据
            //         .GetTypes()
            //         .Where( //需要使用System.Linq查询
            //             myType => //是类，非抽象类，是BlackboardVariable的派生类
            //                 myType.IsClass
            //                 && !myType.IsAbstract
            //                 && myType.IsSubclassOf(typeof(BlackboardVariable)) //需要转化为Type类型的元数据
            //         );
            //     //这样同时添加，就可以建立起一个映射关系，即在类型的下拉框中选中元素的索引可以直接作为在variableTypes中访问对应类型的索引
            //     //Tip：其实也可以用字典，typeName作为键，类型元数据作为值，但是
            //     foreach (Type type in enumerable)
            //     {
            //         string typeName = type.Name;
            //         string suffix = "Variable"; //去掉类名的后缀，直接显示单独的类型名
            //         //这里也是命名要求，就应该以这个后缀结尾来命名
            //         if (typeName.EndsWith(suffix))  typeName = typeName.Substring(0, typeName.Length - suffix.Length);
            //         variableTypes.Add(type);
            //         variableTypeField.choices.Add(typeName);
            //         // variableTypeField.choices.Add(type.Name);
            //     }
            // }
            // variableNameField.text = "newVariable"; //默认变量名。注意TextField的text成员只能读取，不能访问（受保护），所以只能在UXML中设置默认值
            variableTypeField.index = 0; //默认选中第一个，PopupField渲染时就根据该值设置UI数据。
        }

        /// <summary>
        /// 移动黑板变量列表的ListView元素位置
        /// </summary>
        /// <param name="direction"></param>
        private void MoveListViewItem(int direction)
        {
            int selectedIndex = variableList.selectedIndex;
            var items = variableList.itemsSource; //需要多次调用，所以用一个临时变量存储，避免反复访问
            //越界就不操作。
            if (selectedIndex < 0 || selectedIndex + direction < 0 || selectedIndex + direction >= items.Count)
                return;

            //只需要处理数据源，即blackboard.variables，即可直接反映到ListView上。
            var vars = blackboard.variables;
            (vars[selectedIndex + direction], vars[selectedIndex]) = (vars[selectedIndex], vars[selectedIndex + direction]);
            blackboard.variables = vars;
            variableList.selectedIndex += direction; //移动后、仍然选中它。总之数据源和选中对象没有直接关系，需要像这样进行手动同步。

            // 刷新 ListView
            variableList.Rebuild();
        }
        #endregion 处理黑板视图
    }

/*
    // //监听资产文件的删除事件，以便及时响应通过右键删除的资产，尤其是blackboard。
    // public class AssetDeletionListener : AssetPostprocessor
    // {
    //     static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    //     {
    //         // 遍历被删除的资产文件
    //         foreach (var deletedAsset in deletedAssets)
    //         {
    //             // 打印被删除文件的路径
    //             // Debug.Log($"Asset Deleted: {deletedAsset}");
    //             if (deletedAsset == BehaviourTreeEditor.blackboardPath)
    //             {
    //                 BehaviourTreeEditor.tree.blackboard = null;
    //             }
    //         }
    //     }
    // }
*/
}