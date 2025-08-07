using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MyTools.BehaviourTreeTool
{
    [CreateAssetMenu()]
    public class BehaviourTree : ScriptableObject {
        public Node rootNode; //根节点
        public Node.State treeState = Node.State.Running; //暂时没发现这个treeeState有什么用，大概与执行器切换行为树有关
        public List<Node> nodes = new List<Node>(); //节点列表
        public Blackboard blackboard = new Blackboard(); //黑板对象

        public Node.State Update() {
            //执行行为树就是遍历行为树，所以从根节点入手
            if (rootNode.state == Node.State.Running) {
                treeState = rootNode.Update();
            }
            return treeState;
        }


        public static List<Node> GetChildren(Node parent) {
            List<Node> children = new List<Node>();
            //条件和动作节点都是叶子节点，所以必然没有子节点
            if (parent is DecoratorNode decorator && decorator.child != null) {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child != null) {
                children.Add(rootNode.child);
            }

            if (parent is CompositeNode composite) {
                return composite.children;
            }

            return children;
        }

        /// <summary>
        /// 遍历树（相当于二叉树的中序遍历，也就是深度优先搜索）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="visiter"></param>
        public static void Traverse(Node node, System.Action<Node> visiter) {
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
        public BehaviourTree Clone() { //行为树，根节点，各节点
            BehaviourTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone(); //每个节点都记录了自己的父节点和子节点的信息，所以从根节点开始，递归遍历，最终将所有节点添加到nodes列表中
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, (n) => {
                tree.nodes.Add(n);
            });

            return tree;
        }

        //上下文指的是执行器所控制的对象身上的各个组件，比如要调用其方法、读写其属性等等，而黑板指的是那些记录全局状态的变量
        public void Bind(Context context) {
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
        public Node CreateNode(System.Type type) {
            Node node = ScriptableObject.CreateInstance(type) as Node; //这个方法创建的实例仅存在于内存中，并不会自动保存为 .asset 文件。
            node.name = type.Name; //实例名
            node.guid = GUID.Generate().ToString(); //分配一个GUID
            //在执行这行代码后，对 this（当前对象）的任何更改都会被记录到撤销堆栈中。
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)"); //可以在Edit-Undo History中查看撤销堆栈
            nodes.Add(node);

            if (!Application.isPlaying) { //在运行时是不可做这种操作的。
                //将节点添加到行为树文件下（就是添加为同一个.asset文件，不过在项目窗口中会看到成为了原文件的子文件）
                AssetDatabase.AddObjectToAsset(node, this); 
            }
            //可撤销创建指定对象的操作
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
            //Ques：AssetDatabase.SaveAssets和AssetDatabase.AddObjectToAsset以及nodes.Add分别起到了什么作用？
            //只能在编辑模式下调用，确保资源的修改被永久保存，而不仅仅是暂时存储在内存中
            AssetDatabase.SaveAssets(); 
            return node;
        }

        public void DeleteNode(Node node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node); //可撤销销毁操作

            AssetDatabase.SaveAssets();
        }
        //根节点（专门的一个类RootNode）、修饰节点、复合节点（组合节点，控制节点），才有子节点
        //TODO：这种写法应该可以用反射的方式来替代，就是获取运行时类型。
        public void AddChild(Node parent, Node child) {
            if (parent is DecoratorNode decorator) {
                Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode) {
                Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite) {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child) {
            if (parent is DecoratorNode decorator) {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode) {
                Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite) {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
        #endregion Editor Compatibility
    }
}