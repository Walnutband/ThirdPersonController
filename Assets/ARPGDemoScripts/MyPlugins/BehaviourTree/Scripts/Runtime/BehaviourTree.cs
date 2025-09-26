using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MyPlugins.BehaviourTree
{
    [CreateAssetMenu(fileName = "New BehaviourTree", menuName = "MyBehaviourTree/BehaviourTree")]
    public class BehaviourTree : ScriptableObject, ISerializationCallbackReceiver
    {
        [HideInInspector]public BehaviourTreeBlackboard blackboard; //绑定的黑板
        public NodeData rootNode; //根节点
        [ContextMenuItem("检查并同步子资产节点", nameof(CheckSubAssetsNodes))] //只能把菜单项改到这里来了
        [ContextMenuItem("输出装饰节点的子节点名", nameof(DebugNodesChild))]
        public NodeData.State treeState = NodeData.State.Running; //暂时没发现这个treeeState有什么用，大概与执行器切换行为树有关
        // [ContextMenuItem("sddwd", nameof(CheckSubAssetsNodes))] //为dataNodes设置该菜单项。由于enabled为false则无法添加菜单项
        public List<NodeData> dataNodes = new List<NodeData>(); //节点列表，就是该行为树中的所有节点，至于具体的连接关系就是由各节点本身存储。
        //监听blackboard字段值的变化
        public Action blackboardChanged; //Action不会被序列化
        public static string blackboardPath = "";

        /*TODO：每一帧都是从根节点开始向下遍历，作为一般情况确实如此，但面对实际游戏运行时的各种特殊情况，是否应该在此增加其他逻辑呢？*/
        public NodeData.State Update()
        {
            //执行行为树就是遍历行为树，所以从根节点入手
            if (rootNode.state == NodeData.State.Running)
            {
                treeState = rootNode.Update();
            }
            return treeState;
        }
        [ContextMenu("保存到外存")]
        public void SaveAssets()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        [ContextMenu("脏不脏")]
        public void DebugDirty()
        {
            Debug.Log($"行为树脏不脏: {EditorUtility.IsDirty(this)}");
        }
        [ContextMenu("内存中的dataNodes")]
        public void DebugDataNodes()
        {
            dataNodes.ForEach(n => {
                Debug.Log($"类型名：{n.GetType().Name}");
            });
        }

        //TODO：这种都属于编辑时方法，只是放在运行时代码方便直接调用运行时的内容，其实应该用UNITY_EDITOR包装起来。
        //用于加载作为子资产的节点，使得dataNodes与子资产同步。
        public void CheckSubAssetsNodes()
        {
            string treePath = AssetDatabase.GetAssetPath(this); //获取主资产路径
            //加载所有资产，包括主资产行为树和子资产各个节点
            List<UnityEngine.Object> subAssets = AssetDatabase.LoadAllAssetsAtPath(treePath).ToList(); //数组转换为列表，就是一个遍历的O(n)操作
            // UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(treePath);
            subAssets.Remove(this); //移除掉主资产，即行为树，才是真正的子资产列表
            // subAssets.ForEach(n => n = n as NodeData);
            //清空原列表再逐一添加。无法直接赋值列表，因为列表类型不同，可以改变的是列表元素的类型，但不可改变列表本身类型。
            dataNodes.Clear();
            Debug.Log("已清空");
            //使用Cast方法可以将元素进行强制类型转换，如果遇到不可转换的则报错，可以使用安全的OfType<T>() 方法，它会自动过滤出可以转换为指定类型的元素。
            foreach (NodeData nodeAsset in subAssets.Cast<NodeData>())
            {
                dataNodes.Add(nodeAsset);
            }
            //经测试，似乎添加到dataNodes的顺序与子资产的顺序是反的，所以在此进行反转，顺序就变得一样了。
            //其实是因为Project中固定按照字母顺序排列。其实本来dataNodes中顺序就无所谓，真正有所谓的是具体节点记录的子节点顺序。
            // dataNodes.Reverse();
            //LoadAllAssetsAtPath应该是按顺序读取资产文件中的YAML文本，而主资产的文本不一定在最开始。
        }
        public void DebugNodesChild()
        {
            dataNodes.ForEach(node => {
                if (node is DecoratorNode decoratorNode)
                {
                    Debug.Log($"{decoratorNode.GetType().Name}: child为 {(decoratorNode.child != null ? decoratorNode.child.name : "空") }");
                }
            });
        }

        public static List<NodeData> GetChildren(NodeData parent) {
            List<NodeData> children = new List<NodeData>();
            //条件和动作节点都是叶子节点，所以必然没有子节点
            if (parent is DecoratorNode decorator && decorator.child != null) {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child != null) {
                children.Add(rootNode.child);
            }
            //注意这里就没有判空
            if (parent is ControlNode composite) {
                composite.children.RemoveAll(n => n == null); //移除掉所有空引用
                return composite.children;
            }

            return children;
        }

        /// <summary>
        /// 遍历树（相当于二叉树的中序遍历，也就是深度优先搜索）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="visiter">代表要对每个节点执行某个统一操作</param>
        public static void Traverse(NodeData node, System.Action<NodeData> visiter) {
            if (node) {
                visiter.Invoke(node); //Action可以用Invoke调用，其实和直接括号调用没有任何区别，只是表明这是个Action
                var children = GetChildren(node);
                children.ForEach((n) => Traverse(n, visiter));
            }
        }

        /// <summary>
        /// 克隆树（实例化行为树，以及添加好各个节点）
        /// </summary>
        /// <returns></returns>
        public BehaviourTree Clone() { //行为树，根节点，各节点。由于是获取每一个具体的行为树资产文件，所以必然是实例方法
            BehaviourTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone(); //每个节点都记录了自己的父节点和子节点的信息，所以从根节点开始，递归遍历，最终将所有节点添加到nodes列表中
            tree.dataNodes = new List<NodeData>();
            Traverse(tree.rootNode, (n) => {
                tree.dataNodes.Add(n); //Tip：Add添加的是其本身，并非副本，所以如果是引用类型比如类，就会指向同一个对象
            });

            return tree;
        }

        //上下文指的是执行器所控制的对象身上的各个组件，比如要调用其方法、读写其属性等等，而黑板指的是那些记录全局状态的变量
        public void Bind(Context context, BehaviourTreeBlackboard blackboard) {
            //这里是通过让所有节点都拥有对于上下文和黑板的引用，来实现共享，也就是全局，也可以将其设置为静态来实现全局效果
            Traverse(rootNode, node => {
                node.context = context;
                node.blackboard = blackboard;
            });
        }


        #region Editor Compatibility编辑器兼容
#if UNITY_EDITOR
        
        /// <summary>
        /// 创建指定类型的数据节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public NodeData CreateNode(System.Type type) {
            //这个方法创建的实例仅存在于内存中，并不会自动保存为 .asset 文件。需要通过如下的AssetDatabase方法保存在外存中。
            //如果不保存到外存中的话，在方法结束后也仍然会存在于内存中，不过此处只有局部变量node指向它，在方法结束后node销毁，导致失去引用，则会被GC所回收。
            NodeData node = ScriptableObject.CreateInstance(type) as NodeData; 
            node.name = type.Name; //实例名
            node.guid = GUID.Generate().ToString(); //分配一个GUID
            //在执行这行代码后，对 this（当前对象）的任何更改都会被记录到撤销堆栈中。
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)"); //可以在Edit-Undo History中查看撤销堆栈
            dataNodes.Add(node);

            if (!Application.isPlaying) { //在运行时是不可做这种操作的。
                //将节点添加到行为树文件下（就是添加为同一个.asset文件，不过在项目窗口中会看到成为了原文件的子文件）
                AssetDatabase.AddObjectToAsset(node, this);
                //当然也可以创建为一个单独的文件，不过就不方便管理了
                //AssetDatabase.CreateAsset(node, "Assets");
            }
            //可撤销创建指定对象的操作。
            // 如果没有这行Undo，撤销会发现dataNodes中移除了，但是子资产还存在。
            // 大概因为dataNodes所存储的只是一个引用而已，恢复相当于只是删除了指针，而没有删除指针指向的数据即子资产。
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
            //Ques：AssetDatabase.SaveAssets和AssetDatabase.AddObjectToAsset以及nodes.Add分别起到了什么作用？
            //只能在编辑模式下调用，确保资源的修改被永久保存，而不仅仅是暂时存储在内存中
            AssetDatabase.SaveAssets(); 
            return node;
        }

        /// <summary>
        /// 删除节点（视图节点与数据节点）
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(NodeData node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            dataNodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node); //可撤销销毁操作
            //因为存储在nodes列表中的数据节点引用的就是在行为树下对应的节点子资产，在这里要保存一下就可以连同列表元素和对应的子资产一同删除
            AssetDatabase.SaveAssets();
        }
        //根节点（专门的一个类RootNode）、修饰节点、复合节点（组合节点，控制节点），才有子节点
        //TODO：这种写法应该可以用反射的方式来替代，就是获取运行时类型。
        public void AddChild(NodeData parent, NodeData child) {
            if (parent is DecoratorNode decorator) {
                // Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                Undo.RecordObjects(new UnityEngine.Object[]{ decorator, child}, "Behaviour Tree (AddChild)");
                decorator.child = child;
                child.parent = decorator;
                EditorUtility.SetDirty(decorator);
                EditorUtility.SetDirty(child);
            }

            if (parent is RootNode rootNode) {
                // Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                Undo.RecordObjects(new UnityEngine.Object[] { rootNode, child }, "Behaviour Tree (AddChild)");
                rootNode.child = child;
                child.parent = rootNode;
                EditorUtility.SetDirty(rootNode);
                EditorUtility.SetDirty(child);
            }

            if (parent is ControlNode composite) {
                // Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                Undo.RecordObjects(new UnityEngine.Object[] { composite, child }, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                child.parent = composite;
                EditorUtility.SetDirty(composite);
                EditorUtility.SetDirty(child);
            }
        }

        public void RemoveChild(NodeData parent, NodeData child) {
            if (parent is DecoratorNode decorator) {
                // Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                Undo.RecordObjects(new UnityEngine.Object[] { decorator, child }, "Behaviour Tree (AddChild)");
                decorator.child = null;
                child.parent = null;
                EditorUtility.SetDirty(decorator);
                EditorUtility.SetDirty(child);
            }

            if (parent is RootNode rootNode) {
                // Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                Undo.RecordObjects(new UnityEngine.Object[] { rootNode, child }, "Behaviour Tree (AddChild)");
                rootNode.child = null;
                child.parent = null;
                EditorUtility.SetDirty(rootNode);
                EditorUtility.SetDirty(child);
            }

            if (parent is ControlNode composite) {
                // Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                Undo.RecordObjects(new UnityEngine.Object[] { composite, child }, "Behaviour Tree (AddChild)");
                composite.children.Remove(child);
                child.parent = null;
                EditorUtility.SetDirty(composite);
                EditorUtility.SetDirty(child);
            }
        }

        public void OnBeforeSerialize()
        {
            // Debug.Log("tree OnBeforeSerialize");
        }
   
        public void OnAfterDeserialize()
        {
            Debug.Log("tree OnAfterDeserialize");
        }
#endif
        #endregion Editor Compatibility


    }


#if UNITY_EDITOR
    //自定义绘制行为树资产的检视面板，用以监听字段值变化（就是blackboard的变化）
    [CustomEditor(typeof(BehaviourTree))]
    public class BTInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // Debug.Log("调用CreateInspectorGUI");
            serializedObject.Update();
        
            VisualElement root = new VisualElement();
            // SerializedObject serializedObject = new SerializedObject(target);

            //专门处理blackboard字段
            SerializedProperty blackboardField = serializedObject.FindProperty("blackboard");
            //自动赋值binding Path，但是还需要手动Bind。而IMGUI是自动绑定的，不过UIElements确实更强大，而且可以用UI Toolkit工具来制作UI
            //尤其别忘了，显示是显示，数据是数据，往往通过显示来判断数据，但如果没有绑定好，很容易导致严重的误判
            PropertyField field = new PropertyField(blackboardField);
            field.Bind(serializedObject); //通过绑定后，还会自动支持撤销和重做操作
            //target是Object类型，可以通过类型转换获取其原对象以及序列化对象
            BehaviourTree targetBT = target as BehaviourTree;

            field.RegisterValueChangeCallback(evt =>
            {
                targetBT.blackboardChanged?.Invoke(); //触发黑板字段变化事件
            }); //为事件注册回调方法别忘了后面的分号，因为本质上这就是一行函数调用，只是传入的参数是一个方法
            field.style.marginBottom = 10; //增加一个下外间距
            root.Add(field);

            //获取迭代器，其实就是第一个序列化字段，只是SerializedProperty本来就具有可迭代性。所以其实使用FindProperty就可以自由控制要渲染的字段了。
            // SerializedProperty iterator = serializedObject.GetIterator();
            SerializedProperty firstField = serializedObject.FindProperty("m_Script"); //脚本引用字段，应该默认变灰，即不可交互，所以也无需绑定
            PropertyField tempField = new PropertyField(firstField);
            tempField.SetEnabled(false); //禁止与脚本引用字段交互,这样会将其变灰，也就是默认的Script字段UI效果。
            root.Add(tempField);

            //BugFix：值得注意的报错提示：Invalid iteration - (You need to call Next (true) on the first element to get to the first element)
            //把可见的渲染掉。由于继承，等等关系，有些字段本来是参与序列化，但是不显示在面板中的，使用Next会将其渲染出来，显然不合适
            while (firstField.NextVisible(false)) 
            {
                tempField = new PropertyField(firstField);
                if (firstField.name == "dataNodes" || firstField.name == "rootNode") tempField.SetEnabled(false);
                tempField.Bind(serializedObject);
                root.Add(tempField);
            }
            /*Ques：经测试，以下这样都不行，似乎必须要按照报错提示说的，
            必须使用Next(true)。不过貌似是GetIterator获取的才需要，如果是FindProperty直接找到的则更自由
            // while (iterator.NextVisible(false))
            // {
            //     PropertyField tempField = new PropertyField(iterator);
            //     tempField.Bind(serializedObject);
            //     root.Add(tempField);
            // }
            // do
            // {
            //     PropertyField tempField = new PropertyField(iterator);
            //     tempField.Bind(serializedObject);
            //     root.Add(tempField);
            // } while (iterator.NextVisible(false)); //把可见的渲染掉
            */

            // if (serializedObject.ApplyModifiedProperties()) Debug.Log("ApplyModifiedProperties为 true");
            return root;
        }

    }
#endif
}