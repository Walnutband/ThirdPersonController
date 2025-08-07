using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace MyTools.BehaviourTreeTool
{

    public class BehaviourTreeEditor : EditorWindow {

        BehaviourTreeView treeView; //GraphView视图
        BehaviourTree tree; //当前编辑的树
        InspectorView inspectorView;
        IMGUIContainer blackboardView; //
        ToolbarMenu toolbarMenu; //窗口顶部工具栏
        TextField treeNameField;
        //静态变量本身并非持久化，会随着比如热重载这样的过程而被重置即初始化。不过对于连续的创建行为树，可以作为临时缓存选中目录，还是挺有用的。
        static string locationPath = "Assets"; //新建行为树的目录路径
        Button createNewTreeButton;
        Button createNewTreeButtonQuick;
        VisualElement overlay;
        BehaviourTreeSettings settings;

        SerializedObject treeObject; //行为树的序列化对象
        SerializedProperty blackboardProperty; //由于黑板类被标记为可序列化，所以可以直接作为一个序列化属性来渲染，而不用单独处理每一个序列化属性

        [MenuItem("MyTools/BehaviourTreeEditor")]
        public static void OpenWindow() {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset] //双击资源文件时触发自定义逻辑
        public static bool OnOpenAsset(int instanceId, int line) { //特性要求的参数
            if (Selection.activeObject is BehaviourTree) { //当前选中对象类型是行为树，就打开编辑窗口
                OpenWindow();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取指定类型的所有资产文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> LoadAssets<T>() where T : UnityEngine.Object {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}"); //获取各自的guid
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds) {
                string path = AssetDatabase.GUIDToAssetPath(assetId); //这是文件路径，而且是相对于项目文件夹的相对路径
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        public void CreateGUI() {

            settings = BehaviourTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            //Debug.Log($"{root.name}");
            // Import UXML
            var visualTree = settings.behaviourTreeXml;
            visualTree.CloneTree(root); //从外存克隆到内存
            // Debug.Log($"{root.name}");

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.behaviourTreeStyle; //加载uss文件到编辑窗口的根元素上，然后uss中的各个选择器匹配相应的UI元素
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<BehaviourTreeView>();
            treeView.OnNodeSelected = OnNodeSelectionChanged; //选中节点时的方法
            Undo.undoRedoPerformed += treeView.OnUndoRedo; //注册撤销方法

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Blackboard view
            blackboardView = root.Q<IMGUIContainer>();
            blackboardView.onGUIHandler = () =>
            { //在SelectTree方法中获取tree的序列化对象赋值给treeObject，以及更新
                if (treeObject != null && treeObject.targetObject != null) {
                    treeObject.Update(); //更新序列化值
                    EditorGUILayout.PropertyField(blackboardProperty);
                    treeObject.ApplyModifiedProperties(); //将序列化值应用到原数据
                }
            };

            //设置工具栏菜单
            toolbarMenu = root.Q<ToolbarMenu>("Assets"); //查找菜单元素，然后设置菜单项 
            var behaviourTrees = LoadAssets<BehaviourTree>();
            behaviourTrees.ForEach(tree => {
                toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { //以文件名为菜单项名
                    Selection.activeObject = tree;
                });
            });
            toolbarMenu.menu.AppendSeparator();//列出所有已存在的行为树文件后，加上一个分割线，然后给出一个新建行为树的选项
            toolbarMenu.menu.AppendAction("新建行为树...", (a) => CreateNewTree("NewBehaviourTree"));

            // 
            overlay = root.Q<VisualElement>("Overlay");
            treeNameField = overlay.Q<TextField>("TreeName"); //
            createNewTreeButton = overlay.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewTree(treeNameField.value); //value就是右边的TextInput里面的值
            createNewTreeButtonQuick = overlay.Q<Button>("QuickCreateButton");
            createNewTreeButtonQuick.clicked += () => QuickCreateNewTree(treeNameField.value); //value就是右边的TextInput里面的值

            if (tree == null) {
                OnSelectionChange();
            } else { //已经存在，就省去了判断有无的部分程序。
                SelectTree(tree);
            }
        }


        private void OnEnable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; //严谨，避免重复注册，浪费内存
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= treeView.OnUndoRedo; //由于treeView要在CreateGUI中获取到引用后才可访问，所以不能在Enable中注册
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

        //EditorWindow的消息方法，在 Project 窗口 或 Hierarchy 窗口 中更改选中的对象时调用
        private void OnSelectionChange() {
            EditorApplication.delayCall += () => { //所有检视器刷新后触发该回调
                //首先检查是否选中了行为树资产文件，然后检查是否选中了带有执行器的游戏对象，获取执行器上指定的行为树
                BehaviourTree tree = Selection.activeObject as BehaviourTree;
                if (!tree) { 
                    if (Selection.activeGameObject) {
                        BehaviourTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                        if (runner) {
                            tree = runner.tree;
                        }
                    }
                }

                SelectTree(tree);
            };
        }

        /// <summary>
        /// 在编辑窗口中显示选中树的视图
        /// </summary>
        /// <param name="newTree"></param>
        void SelectTree(BehaviourTree newTree) {

            if (treeView == null) {
                return;
            }

            if (!newTree) {
                return;
            }

            this.tree = newTree; //当前编辑的行为树

            //未选中行为树时的覆盖面板，可以直接选择创建行为树，当然也可以选择已有行为树。不过在打开窗口时才会出现，因为在窗口中只有选择其他行为树时才会切换视图显示内容，单纯取消选中并不会切换。
            overlay.style.visibility = Visibility.Hidden;

            // if (Application.isPlaying) {
            //     treeView.PopulateView(tree);
            // } else {
            //     treeView.PopulateView(tree);
            // }
            treeView.PopulateView(tree);

            treeObject = new SerializedObject(tree); //行为树的序列化对象
            blackboardProperty = treeObject.FindProperty("blackboard"); //获取行为树上的黑板对象BlackBoard

            EditorApplication.delayCall += () => {
                //用于调整视图的缩放和位置，以便将图中的所有元素都显示在视图中。它的主要作用是让用户能够快速聚焦到整个图形的内容，而无需手动缩放或拖动视图。
                treeView.FrameAll();
            };
        }

        void OnNodeSelectionChanged(NodeView node) {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate() {
            treeView?.UpdateNodeStates();
        }
        
        /// <summary>
        /// 快速创建行为树，就是直接在Assets文件夹下创建
        /// </summary>
        /// <param name="assetName"></param>
        void QuickCreateNewTree(string assetName) {
            //目录路径+文件名
            string path = System.IO.Path.Combine("Assets", $"{assetName}.asset"); //该方法可以智能处理分隔符
            BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
            //tree.name = treeNameField.ToString(); //资产名
            tree.name = assetName;
            AssetDatabase.CreateAsset(tree, path); //从内存保存到外存上
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree; //创建后同时选中
            //EditorGUIUtility.PingObject(tree);
        }

        void CreateNewTree(string defaultName)
        {
            string filePath = EditorUtility.SaveFilePanel("创建行为树", locationPath, defaultName, "asset");
            // 获取目录路径
            locationPath = Path.GetDirectoryName(filePath);
            Debug.Log($"文件路径：{filePath}\n目录路径：{locationPath}");
            // 获取文件名(包括扩展名)
            //string fileName = Path.GetFileName(filePath); 
            // BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
            // AssetDatabase.CreateAsset(tree, filePath); //从内存保存到外存上
            // AssetDatabase.SaveAssets();
            // Debug.Log(tree.name);
            // Selection.activeObject = tree; //创建后同时选中
        }

        // private void OnDestroy()
        // {
        //     //Debug.Log("编辑窗口销毁");
        //     Undo.undoRedoPerformed -= treeView.OnUndoRedo;
        // }
    }
}