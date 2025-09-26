using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{

    public class RootNode : NodeData, ISerializationCallbackReceiver
    { //专门作为根节点，作为遍历的入口，不执行任何特殊操作。
        [HideInInspector] public NodeData child;


        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            return child.Update();
        }

        public override NodeData Clone() {
            RootNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }

        public void OnBeforeSerialize()
        {
            // Debug.Log("tree OnBeforeSerialize");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("RootNode OnAfterDeserialize");
        }
    }
}