using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool
{

    public class RootNode : Node { //专门作为根节点，作为遍历的入口，不执行任何特殊操作。
        public Node child;

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            return child.Update();
        }

        public override Node Clone() {
            RootNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}