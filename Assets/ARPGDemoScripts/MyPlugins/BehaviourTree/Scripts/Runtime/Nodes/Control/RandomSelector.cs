using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MyPlugins.BehaviourTree
{
    public class RandomSelector : ControlNode {
        protected int current;

        protected override void OnStart() {
            current = Random.Range(0, children.Count);
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var child = children[current];
            return child.Update();
        }
    }
}