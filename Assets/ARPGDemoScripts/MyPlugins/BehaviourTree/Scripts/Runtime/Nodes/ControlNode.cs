using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    public abstract class ControlNode : NodeData {
        [HideInInspector] public List<NodeData> children = new List<NodeData>(); //显示在节点视图中，不需要也不应该显示在检视面板中，应该指定只能在窗口中编辑

        public override NodeData Clone() {
            //创建一个运行时的副本，以便在游戏运行时对数据进行修改，而不会影响原始的 ScriptableObject 资产文件。
            ControlNode node = Instantiate(this);
            /*List的ConverAll方法
            类型转换：将列表中的元素从一种类型转换为另一种类型。
            保持原列表不变：转换后的元素存储在一个新列表中（作为返回值），原列表不会受到影响。
            */
            /*Tip：克隆的本质就是创建占用不同内存地址的实例，而不是两个不同的指针引用同一个实例。*/
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}