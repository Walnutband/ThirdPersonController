using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool
{
    public abstract class CompositeNode : Node {
        [HideInInspector] public List<Node> children = new List<Node>();

        public override Node Clone() {
            //创建一个运行时的副本，以便在游戏运行时对数据进行修改，而不会影响原始的 ScriptableObject 资产文件。
            CompositeNode node = Instantiate(this); 
            /*List的ConverAll方法
            类型转换：将列表中的元素从一种类型转换为另一种类型。
            保持原列表不变：转换后的元素存储在一个新列表中（作为返回值），原列表不会受到影响。
            */
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}