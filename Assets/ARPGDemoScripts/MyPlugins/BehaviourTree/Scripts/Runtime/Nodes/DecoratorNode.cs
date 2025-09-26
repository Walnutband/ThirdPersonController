using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    public abstract class DecoratorNode : NodeData {

        [ContextMenuItem("打印child", nameof(DebugChild))]
        [HideInInspector] public NodeData child; //装饰节点只有一个子节点

        public override NodeData Clone() {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }

        public void DebugChild()
        {
            Debug.Log($"{(child == null ? "child为空" : child.GetType().Name)}");
        }
    }
}
