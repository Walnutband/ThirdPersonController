using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CrashKonijn.Goap.Resolver
{
    [BurstCompile]
    public struct NodeData
    {
        public int Index;

        // Cost of when using this node as a parent
        public float P;

        // Cost when performing this node
        public float G;

        // Heuristic
        public float H;
        public int ParentIndex;
        public float3 Position;

        //当前的总代价（已确定的实际代价+后续的估计代价）
        public float F => this.G + this.H;
    }

    [BurstCompile]
    public struct RunData
    {
        public NativeArray<int> StartIndex;

        public float3 AgentPosition;

        // Index = NodeIndex
        public NativeArray<bool> IsEnabled;

        public NativeArray<bool> IsExecutable;

        // Index = ConditionIndex
        public NativeArray<bool> ConditionsMet;
        public NativeArray<float3> Positions;
        public NativeArray<float> Costs;
        public float DistanceMultiplier;
    }

    [BurstCompile]
    public struct NodeSorter : IComparer<NodeData>
    {
        public int Compare(NodeData x, NodeData y)
        {
            return x.F.CompareTo(y.F);
        }
    }

    [BurstCompile]
    public struct GraphResolverJob : IJob //使用JobSystem技术。
    {
        // Graph specific
#if UNITY_COLLECTIONS_2_1
        // Dictionary<ActionIndex, ConditionIndex[]>
        [ReadOnly]
        public NativeParallelMultiHashMap<int, int> NodeConditions;

        // Dictionary<ConditionIndex, NodeIndex[]>
        [ReadOnly]
        public NativeParallelMultiHashMap<int, int> ConditionConnections;
#else
        // Dictionary<ActionIndex, ConditionIndex[]>
        [ReadOnly]
        public NativeMultiHashMap<int, int> NodeConditions;

        // Dictionary<ConditionIndex, NodeIndex[]>
        [ReadOnly]
        public NativeMultiHashMap<int, int> ConditionConnections;
#endif

        // Resolve specific
        [ReadOnly]
        public RunData RunData;

        // Results
        public NativeList<NodeData> Result;
        public NativeList<NodeData> PickedGoal;

        //赋予明确含义的常量值。
        public static readonly float3 InvalidPosition = new(float.MaxValue, float.MaxValue, float.MaxValue);

        [BurstCompile]
        public void Execute()
        {
            var nodeCount = this.NodeConditions.Count();
            var runData = this.RunData;

            //A*算法的两个清单。
            var openSet = new NativeHashMap<int, NodeData>(nodeCount, Allocator.Temp);
            var closedSet = new NativeHashMap<int, NodeData>(nodeCount, Allocator.Temp);

            // Add each start node's (goal) connections to the open set
            //StartIndex存储了所有Goal节点的索引。
            foreach (var i in runData.StartIndex) 
            {
                var nodeData = new NodeData //当前访问到的Goal节点的数据封装对象。
                {
                    Index = i,
                    G = this.RunData.Costs[i],
                    P = this.RunData.Costs[i],
                    H = int.MaxValue,
                    ParentIndex = -1,
                    Position = InvalidPosition,
                };

                // We're assuming the start node is always a goal, and as such not executable
                closedSet.TryAdd(nodeData.Index, nodeData);

                this.AddConnections(this.RunData, ref openSet, ref closedSet, nodeData);
            }

            /*Tip：上面只是准备数据，以下才是开始寻找路径即行动序列。
            应该具体来说，上面那个循环，实际上是因为该GOAP系统将Goal和Action节点统一处理，Goal节点本身是具有特殊性的，不需要也不应该作为探索路径上的节点，也就是不会加入到openSet中，
            而相应的，就是将直接与Goal节点相连的Action节点加入到openSet中作为开始节点，然后就开启了下面的循环，也就是真正开始寻找路径的过程。
            所以也可以认为上面循环就是在进行初始化、准备寻路的环境（Context，上下文）。
            */

            //开始搜索路径的起点。
            while (!openSet.IsEmpty) 
            {
                //将所有（处理过后的）节点数据NodeData取出来。
                var openList = openSet.GetValueArray(Allocator.Temp);
                //根据F值从小到大排序，也就是按照总代价从小到大排序。
                openList.Sort(new NodeSorter());

                var currentNode = openList[0];

                /*Tip：判断当前Action节点可执行，则说明可以立刻执行，也就是作为整个行动序列的起点，不用再找了。*/
                if (runData.IsExecutable[currentNode.Index])
                {
                    this.RetracePath(currentNode, closedSet, this.Result);
                    break;
                }

                closedSet.TryAdd(currentNode.Index, currentNode);
                openSet.Remove(currentNode.Index);

                // If this node has a condition that is false and has no connections, it is unresolvable
                if (this.HasUnresolvableCondition(currentNode.Index))
                {
                    continue;
                }
                //遍历下一层节点。
                this.AddConnections(this.RunData, ref openSet, ref closedSet, currentNode);

                openList.Dispose();
            }

            openSet.Dispose();
            closedSet.Dispose();
        }

        /*Tip：将传入节点currentNode的未满足条件所连接的节点、逐个计算代价值、创建NodeData实例记录相关数据并加入openSet。
        已满足的条件显然就不需要考虑了。
        */
        private void AddConnections(
            RunData runData, ref NativeHashMap<int, NodeData> openSet,
            ref NativeHashMap<int, NodeData> closedSet, NodeData currentNode
        )
        {
            //遍历当前Goal节点的所有Condition（首先获取其所有Condition的索引）
            foreach (var conditionIndex in this.NodeConditions.GetValuesForKey(currentNode.Index))
            {
                //获取该条件的满足信息，因为在此之前已经通过AgentTypeJobRunner的FillBuilders方法把当前的相关信息都填充好了。
                if (runData.ConditionsMet[conditionIndex])
                {
                    continue;
                }

                //当前条件未满足，遍历该Condition连接的所有节点。
                foreach (var neighborIndex in this.ConditionConnections.GetValuesForKey(conditionIndex))
                {
                    //是否已经探索过。
                    if (closedSet.ContainsKey(neighborIndex))
                    {
                        continue;
                    }
                    //节点是否可用
                    /*TODO：关于这个启用状态，实际上更加增强了AI角色在运行时的动态变化性，通过位于预设的前提条件之外的某些条件来控制特定Action的启用状态，直接丰富了GOAP系统可以插入的逻辑。
                    但具体可以有哪些应用，还有待进一步研究。*/
                    if (!runData.IsEnabled[neighborIndex])
                    {
                        continue;
                    }


                    var neighborPosition = this.GetPosition(currentNode, neighborIndex);

                    // The cost with distance from the current node to the neighbour node
                    var newParentG = this.GetNewCost(currentNode, neighborIndex, neighborPosition);
                    // The cost with distance from the agent to the neighbour node
                    //Ques：这个距离代价有必要吗？有什么用呢？
                    var newG = newParentG + this.GetDistanceCost(runData.AgentPosition, neighborPosition);
                    NodeData neighbor;

                    // Current neighbour is not in the open set
                    if (!openSet.TryGetValue(neighborIndex, out neighbor))
                    {
                        //封装节点数据。
                        neighbor = new NodeData
                        {
                            Index = neighborIndex,
                            P = newParentG,
                            G = newG, //走到这一个节点，从开始到现在的总代价。
                            H = this.GetHeuristic(neighborIndex), //该节点之后的估计代价（估计的最小代价）。
                            ParentIndex = currentNode.Index,
                            Position = neighborPosition,
                        };
                        //代表已发现且未探索的节点。
                        openSet.Add(neighborIndex, neighbor);
                        continue;
                    }

                    // This neighbour has a lower cost
                    /*Ques：意味着找到了新的最短路径。因为G代表的是从开始到这个节点的总代价，而该节点本身不变，所以它前面的过程所需要的代价必然减少了。
                    但是对于该情况的实际细节，还需要探究。*/
                    if (newG < neighbor.G)
                    {
                        neighbor.G = newG;
                        neighbor.P = newParentG;
                        neighbor.ParentIndex = currentNode.Index;
                        neighbor.Position = neighborPosition;

                        openSet.Remove(neighborIndex);
                        openSet.Add(neighborIndex, neighbor);
                    }
                }
            }
        }

        //这里的新代价是加上邻节点本身的代价，再加上额外的距离代价。
        private float GetNewCost(NodeData currentNode, int neighborIndex, float3 neighborPosition)
        {
            return currentNode.P + this.RunData.Costs[neighborIndex] +
                   this.GetDistanceCost(currentNode, neighborPosition);
        }

        //计算估计代价
        //TODO：从这里引出的逻辑就是该A*算法所采用的启发函数了，值得思考进一步的优化。
        private float GetHeuristic(int neighborIndex)
        {
            return this.UnmetConditionCost(neighborIndex);
        }

        private float GetDistanceCost(NodeData previousNode, float3 currentPosition)
        {
            return this.GetDistanceCost(previousNode.Position, currentPosition);
        }

        private float GetDistanceCost(float3 previousPosition, float3 currentPosition)
        {
            //有无效位置则为0即不存在距离代价。逻辑上就是此时位置根本不在代价的考虑范围内。
            if (previousPosition.Equals(InvalidPosition) || currentPosition.Equals(InvalidPosition))
            {
                return 0f;
            }
            //Tip：这里就看到DistanceMultiplier就类似于在自动寻路系统中的Cost，不同地形之间相同的距离但是代价不同。
            return math.distance(previousPosition, currentPosition) * this.RunData.DistanceMultiplier;
        }

        private float3 GetPosition(NodeData currentNode, int currentIndex)
        {
            var pos = this.RunData.Positions[currentIndex];
            //无效位置则返回currentNode的Position。
            if (pos.Equals(InvalidPosition))
                return currentNode.Position;

            return pos;
        }

        private void RetracePath(NodeData startNode, NativeHashMap<int, NodeData> closedSet, NativeList<NodeData> path)
        {
            var currentNode = startNode;
            //从Action节点回溯，就是回溯到根节点即Goal节点，因为Goal节点的ParentIndex就设置为-1
            while (currentNode.ParentIndex != -1)
            {//这里很巧，本来是从目标开始的，而这里是从子节点向目标回溯而逐个添加的，所以正好path是按照正常的顺序存储Action节点的。
                path.Add(currentNode);
                currentNode = closedSet[currentNode.ParentIndex];
            }
            //Ques：实际上PickedGoal最多只会有一个数据，为何要使用容器呢？
            this.PickedGoal.Add(currentNode);
        }

        private bool HasUnresolvableCondition(int currentIndex)
        {
            foreach (var conditionIndex in this.NodeConditions.GetValuesForKey(currentIndex))
            {
                if (this.RunData.ConditionsMet[conditionIndex])
                {
                    continue;
                }
                //条件不满足，且该条件没有连接任何节点，则判定为不可解析（unresolvable）
                if (!this.ConditionConnections.GetValuesForKey(conditionIndex).MoveNext())
                {
                    return true;
                }
            }

            return false;
        }

        private float UnmetConditionCost(int currentIndex)
        {
            var cost = 0f;
            //获取该节点所有未满足的条件来计算总代价。
            //具体含义在于，对于每个未满足的条件，
            foreach (var conditionIndex in this.NodeConditions.GetValuesForKey(currentIndex))
            {
                if (!this.RunData.ConditionsMet[conditionIndex])
                {
                    cost += this.GetCheapestCostForCondition(conditionIndex);
                }
            }

            return cost;
        }

        private float GetCheapestCostForCondition(int conditionIndex)
        {
            var cost = float.MaxValue;
            //每个条件就是一个端口，这里就是获取该条件连接的所有节点，并且求出其最小代价。
            //Ques：如果该条件没有连接上节点呢？就直接返回MaxValue是否会引起一些意外行为？或者说在编辑源数据的时候就保证必然有连接，因为否则的话无法保证运行时始终跑通。
            foreach (var nodeIndex in this.ConditionConnections.GetValuesForKey(conditionIndex))
            {
                if (this.RunData.Costs[nodeIndex] < cost)
                    cost = this.RunData.Costs[nodeIndex];
            }

            return cost;
        }
    }
}
