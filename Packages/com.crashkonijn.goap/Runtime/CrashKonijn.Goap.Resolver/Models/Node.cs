using System;
using System.Collections.Generic;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Resolver
{
    public class Node : INode
    {
        public Guid Guid { get; } = Guid.NewGuid();

        //这里说明一个节点
        public IConnectable Action { get; set; }

        public List<INodeEffect> Effects { get; set; } = new();
        public List<INodeCondition> Conditions { get; set; } = new();

        //Tip：这里就表明，IGoal和IGoapAction就是图的节点类型，而IGoal则作为根节点。
        public bool IsRootNode => this.Action is IGoal;

        //获取当前节点下的所有Action节点。
        public void GetActions(List<IGoapAction> actions)
        {
            if (actions.Contains(this.Action as IGoapAction))
                return;

            if (this.Action is IGoapAction goapAction)
                actions.Add(goapAction);

            foreach (var condition in this.Conditions)
            {
                foreach (var connection in condition.Connections)
                {
                    connection.GetActions(actions);
                }
            }
        }
    }
}
