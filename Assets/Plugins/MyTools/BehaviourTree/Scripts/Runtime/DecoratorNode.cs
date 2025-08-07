using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool
{
    public abstract class DecoratorNode : Node {
        [HideInInspector] public Node child; //装饰节点只有一个子节点

        public override Node Clone() {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}
