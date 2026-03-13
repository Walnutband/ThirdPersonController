using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    public class Selector : ControlNode, ISerializationCallbackReceiver
    {
        protected int current;

        protected override void OnStart() {
            current = 0;
        }

        protected override void OnStop() {
        }

        
        protected override State OnUpdate() {
            for (int i = current; i < children.Count; ++i) {
                current = i;
                var child = children[current];

                //只有失败才会继续，因为逻辑是“直到有一个成功的节点”
                switch (child.Update())
                {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        continue;
                }
            }

            return State.Failure;
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("Selector  OnAfterDeserialize");
        }

        public void OnBeforeSerialize()
        {

        }
    }
}