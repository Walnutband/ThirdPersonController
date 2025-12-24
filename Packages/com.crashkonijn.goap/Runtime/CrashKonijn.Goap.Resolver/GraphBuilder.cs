using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Resolver
{
    public class GraphBuilder
    {
        private readonly IKeyResolver keyResolver;

        public GraphBuilder(IKeyResolver keyResolver)
        {
            this.keyResolver = keyResolver;
        }

        public Graph Build(IEnumerable<IConnectable> actions)
        {
            var nodes = actions.ToNodes();

            var graph = new Graph
            {//首先确定根节点，其实就是确定所有目标。
                RootNodes = nodes.RootNodes.ToList(),
            };
            //根节点与子节点合并，就是Goal节点和Action节点合并。
            var allNodes = nodes.RootNodes.Union(nodes.ChildNodes).ToArray();
            //这里得到的Map记录的信息是：对于每一种存在的Effect，含有该Effect的所有节点与该Effect构成键值对存储在字典中。
            var effectMap = this.GetEffectMap(allNodes);
            var conditionMap = this.GetConditionMap(allNodes);

            foreach (var node in nodes.RootNodes)
            {
                this.ConnectNodes(node, effectMap, conditionMap, graph);
            }
            //存在但未连接的节点
            graph.UnconnectedNodes = allNodes.Where(x => !graph.ChildNodes.Contains(x) && !graph.RootNodes.Contains(x))
                .ToArray();

            return graph;
        }

        private void ConnectNodes(
            INode node, Dictionary<string, List<INode>> effectMap,
            Dictionary<string, List<INode>> conditionMap, IGraph graph
        )
        {
            //Ques：不是已经保证node是根节点了吗？？
            if (!graph.ChildNodes.Contains(node) && !node.IsRootNode)
                graph.ChildNodes.Add(node);
            //根节点的入口条件。
            foreach (var actionNodeCondition in node.Conditions)
            {
                //当前条件已经连接了元素，就跳过。
                if (actionNodeCondition.Connections.Any())
                    continue;

                var key = this.keyResolver.GetKey(actionNodeCondition.Condition);
                /*Tip：条件竟然不在Map中，正常情况下应该不可能出现。
                原来是我看错了。。这是将Condition放得EffectMap中检查，因为Condition是入口，Effect是出口，所以检查EffectMap中如果没有该Condition的话，就说明没有节点连接到该Condition上。*/
                if (!effectMap.ContainsKey(key))
                    continue;
                //
                var connections = effectMap[key].Where(x => !this.HasConflictingConditions(node, x)).ToArray();

                actionNodeCondition.Connections = connections;

                foreach (var connection in actionNodeCondition.Connections)
                {
                    connection.Effects.First(x => this.keyResolver.GetKey(x.Effect) == key)
                        .Connections = conditionMap[key].Where(x => !this.HasConflictingConditions(node, x)).ToArray();
                }
                //构建树就是这样的递归连接。
                foreach (var subNode in actionNodeCondition.Connections)
                {
                    this.ConnectNodes(subNode, effectMap, conditionMap, graph);
                }
            }
        }

        /*Tip：检查两个节点是否存在冲突，连接关系是otherNode的出口连接到node的入口，所以会判断otherNode的Effect与node的Condition
        是否存在冲突，因为连接就是按照Key和变化方向来匹配的，如果存在相同Key、不同变化方向（一个增一个减），那么就肯定不能连接在一起。
        而且注意，调用该方法时是在effectMap[key]或conditionMap[key]所关联的节点中，也就是说此时已经满足了连接的正条件（有相同Key），
        此处就是判断其他Condition和Effect是否存在冲突。*/
        private bool HasConflictingConditions(INode node, INode otherNode)
        {
            foreach (var condition in node.Conditions)
            {
                foreach (var otherEffects in otherNode.Effects)
                {
                    if (this.keyResolver.AreConflicting(otherEffects.Effect, condition.Condition))
                        return true;
                }
            }

            return false;
        }

        private Dictionary<string, List<INode>> GetEffectMap(INode[] actionNodes)
        {
            var map = new Dictionary<string, List<INode>>();

            foreach (var actionNode in actionNodes)
            {//因为每个节点可以有多个Effect（Effect是以个体存在的）
                /*Ques：这里是不是忽略了Goal节点应该是不会设置Effect的？不过这里如果是空的话确实会直接跳过，但这确实是隐性的，没有用显式逻辑表现出来。*/
                foreach (var actionNodeEffect in actionNode.Effects)
                {
                    //提取Effect的Key信息（名称加上变化方向）
                    var key = this.keyResolver.GetKey(actionNodeEffect.Effect);

                    if (!map.ContainsKey(key))
                        map[key] = new List<INode>();

                    map[key].Add(actionNode);
                }
            }

            return map;
        }

        private Dictionary<string, List<INode>> GetConditionMap(INode[] actionNodes)
        {
            var map = new Dictionary<string, List<INode>>();

            foreach (var actionNode in actionNodes)
            {
                foreach (var actionNodeConditions in actionNode.Conditions)
                {
                    var key = this.keyResolver.GetKey(actionNodeConditions.Condition);

                    if (!map.ContainsKey(key))
                        map[key] = new List<INode>();

                    map[key].Add(actionNode);
                }
            }

            return map;
        }
    }
}
