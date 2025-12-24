using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Resolver;
using Unity.Collections;
using Unity.Mathematics;

namespace CrashKonijn.Goap.Runtime
{
    public class AgentTypeJobRunner : IAgentTypeJobRunner
    {
        private readonly IAgentType agentType;
        private readonly IGraphResolver resolver;

        private List<JobRunHandle> resolveHandles = new();
        private readonly IExecutableBuilder executableBuilder;
        private readonly IEnabledBuilder enabledBuilder;
        private readonly IPositionBuilder positionBuilder;
        private readonly ICostBuilder costBuilder;
        private readonly IConditionBuilder conditionBuilder;

        private List<int> goalIndexes = new();

        public AgentTypeJobRunner(IAgentType agentType, IGraphResolver graphResolver)
        {
            //核心内容：Agent类型信息，以及GOAP图解析器。
            this.agentType = agentType;
            this.resolver = graphResolver;
            //这些构建器就是记录节点的一些额外信息而已。
            this.enabledBuilder = this.resolver.GetEnabledBuilder();
            this.executableBuilder = this.resolver.GetExecutableBuilder();
            this.positionBuilder = this.resolver.GetPositionBuilder();
            this.costBuilder = this.resolver.GetCostBuilder();
            this.conditionBuilder = this.resolver.GetConditionBuilder();

            agentType.SensorRunner.InitializeGraph(graphResolver.GetGraph());
        }

        public void Run(IMonoGoapActionProvider[] queue)
        {
            this.resolveHandles.Clear();
            //首先调用传感器，进行感知过程。然后再
            this.agentType.SensorRunner.Update();
            this.agentType.SensorRunner.SenseGlobal();

            foreach (var agent in queue)
            {
                this.Run(agent);
            }
        }

        //
        private void Run(IMonoGoapActionProvider actionProvider)
        {//传入GoapActionProvider组件。
            if (actionProvider.IsNull())
                return;
            //Ques：类型要一致，但是我在想，按理来说应该在结构上就保证类型是一致的。
            if (actionProvider.AgentType != this.agentType)
                return;

            //在这里从Provider获取目标，判断目标是否完成，然后再访问Provider的Events触发事件。
            if (this.IsGoalCompleted(actionProvider))
            {
                var goal = actionProvider.CurrentPlan;
                actionProvider.ClearGoal();
                actionProvider.Events.GoalCompleted(goal.Goal);
            }

            //是否能被解析。
            if (!this.MayResolve(actionProvider))
                return;

            //现在处理目标请求。
            var goalRequest = actionProvider.GoalRequest;

            if (goalRequest == null)
                return;

            this.agentType.SensorRunner.SenseLocal(actionProvider, goalRequest);
            //填充各个构建器，就是记录Goal和Action的额外特殊信息，通过这种明确分别的形式记录，之后的计算中（主要就是A*算法的规划过程）就会更容易也更高效。
            this.FillBuilders(actionProvider);

            this.LogRequest(actionProvider, goalRequest);

            this.goalIndexes.Clear();

            //因为一个请求可以含有多个目标，而有些目标可能已经完成了。
            //TODO：细想一下，这确实是个很重要的功能，同时冒出多个目标，现实即如此，不过问题是，处理多个目标的话，如何恰当地确定行动以及执行行动呢？？
            foreach (var goal in goalRequest.Goals)
            {
                if (this.IsGoalCompleted(actionProvider, goal))
                    continue;
                //显然，这些索引是通用的，全部源于GraphResolver中记录源数据的容器。
                this.goalIndexes.Add(this.resolver.GetIndex(goal));
            }

            //Tip：保存目标请求的处理句柄。
            this.resolveHandles.Add(new JobRunHandle(actionProvider, goalRequest)
            {
                Handle = this.resolver.StartResolve(new RunData
                {
                    //存储所有Goal的索引。
                    StartIndex = new NativeArray<int>(this.goalIndexes.ToArray(), Allocator.TempJob),
                    //记录Agent的位置。
                    AgentPosition = actionProvider.Position,
                    //记录各个节点的额外特殊信息。
                    IsEnabled = new NativeArray<bool>(this.enabledBuilder.Build(), Allocator.TempJob),
                    IsExecutable = new NativeArray<bool>(this.executableBuilder.Build(), Allocator.TempJob),
                    Positions = new NativeArray<float3>(this.positionBuilder.Build(), Allocator.TempJob),
                    Costs = new NativeArray<float>(this.costBuilder.Build(), Allocator.TempJob),
                    ConditionsMet = new NativeArray<bool>(this.conditionBuilder.Build(), Allocator.TempJob),
                    DistanceMultiplier = actionProvider.DistanceMultiplier,
                }),
            });
        }

        private void FillBuilders(IMonoGoapActionProvider actionProvider)
        {
            var conditionObserver = this.agentType.GoapConfig.ConditionObserver;
            conditionObserver.SetWorldData(actionProvider.WorldData);

            this.enabledBuilder.Clear();
            this.executableBuilder.Clear();
            this.positionBuilder.Clear();
            this.conditionBuilder.Clear();

            foreach (var goal in this.agentType.GetGoals())
            {
                this.costBuilder.SetCost(goal, goal.GetCost(actionProvider.Receiver, actionProvider.Receiver.Injector));
            }

            foreach (var node in this.agentType.GetActions())
            {
                var allMet = true;

                foreach (var condition in node.Conditions)
                {
                    if (!conditionObserver.IsMet(condition))
                    {//只要存在有条件不满足，显然allMet就是false，但是continue而非直接break，是因为每个条件都需要逐个SetConditionMet设置满足状态。
                        allMet = false;
                        continue;
                    }

                    this.conditionBuilder.SetConditionMet(condition, true);
                }

                var target = actionProvider.WorldData.GetTarget(node);

                this.executableBuilder.SetExecutable(node, node.IsExecutable(actionProvider.Receiver, allMet));
                this.enabledBuilder.SetEnabled(node, node.IsEnabled(actionProvider.Receiver));
                this.costBuilder.SetCost(node, node.GetCost(actionProvider.Receiver, actionProvider.Receiver.Injector, target));

                this.positionBuilder.SetPosition(node, target?.GetValidPosition());
            }
        }

        /*Tip：判断目标是否完成，没有当然无所谓，有的话就看此时的世界状态（WorldData）是否满足Goal的所有条件*/
        private bool IsGoalCompleted(IMonoGoapActionProvider actionProvider)
        {
            //如果CurrentPlan为空，则就是将CurrentPlan与null比较，而不为空，则会取其Goal与null比较。
            if (actionProvider.CurrentPlan?.Goal == null)
                return false;

            this.agentType.SensorRunner.SenseLocal(actionProvider, actionProvider.CurrentPlan.Goal);

            return this.IsGoalCompleted(actionProvider, actionProvider.CurrentPlan.Goal);
        }

        private bool IsGoalCompleted(IGoapActionProvider actionProvider, IGoal goal)
        {
            if (goal == null)
                return false;
            //观察条件变化，其实就是赋值WorldData，代表的是更新（认知中的）世界状态。
            //然后判断Goal的条件是否都满足，只要存在不满足的条件则是未完成、返回false。
            var conditionObserver = this.agentType.GoapConfig.ConditionObserver;
            conditionObserver.SetWorldData(actionProvider.WorldData);

            foreach (var condition in goal.Conditions)
            {
                if (!conditionObserver.IsMet(condition))
                    return false;
            }

            return true;
        }

        private bool MayResolve(IGoapActionProvider actionProvider)
        {
            if (actionProvider.Receiver.IsPaused)
                return false;

            if (actionProvider.Receiver.ActionState?.RunState == null)
                return true;

            if (actionProvider.Receiver is not IAgent agent)
                return true;

            return actionProvider.Receiver.ActionState.RunState.MayResolve(agent);
        }

        /*Tip：在Run之后调用，*/
        public void Complete()
        {
            foreach (var resolveHandle in this.resolveHandles)
            {
                var result = resolveHandle.Handle.Complete();

                if (resolveHandle.ActionProvider.GoalRequest != resolveHandle.GoalRequest)
                    continue;

                if (resolveHandle.ActionProvider.IsNull())
                    continue;

                var goal = result.Goal;
                if (goal == null)
                {
                    resolveHandle.ActionProvider.Events.NoActionFound(resolveHandle.GoalRequest);
                    continue;
                }

                var action = result.Actions.FirstOrDefault() as IGoapAction;

                if (action is null)
                {
                    resolveHandle.ActionProvider.Events.NoActionFound(resolveHandle.GoalRequest);
                    continue;
                }

                //要执行的Action发生了变化，
                if (action != resolveHandle.ActionProvider.Receiver.ActionState.Action)
                {
                    resolveHandle.ActionProvider.SetAction(new GoalResult
                    {
                        Goal = goal, //当前目标
                        Plan = result.Actions, //行动序列
                        Action = action, //当前应当执行的行动。
                    });
                }
            }

            this.resolveHandles.Clear();
        }

        private void LogRequest(IGoapActionProvider actionProvider, IGoalRequest request)
        {
#if UNITY_EDITOR
            if (actionProvider.Logger == null)
                return;

            if (!actionProvider.Logger.ShouldLog())
                return;

            var builder = new StringBuilder();
            builder.Append("Trying to resolve goals ");

            foreach (var goal in request.Goals)
            {
                builder.Append(goal.GetType().GetGenericTypeName());
                builder.Append(", ");
            }

            actionProvider.Logger.Log(builder.ToString());
#endif
        }

        public void Dispose()
        {
            foreach (var resolveHandle in this.resolveHandles)
            {
                resolveHandle.Handle.Complete();
            }

            this.resolver.Dispose();
        }

        private class JobRunHandle
        {
            public IMonoGoapActionProvider ActionProvider { get; }
            public IResolveHandle Handle { get; set; }
            public IGoalRequest GoalRequest { get; set; }

            public JobRunHandle(IMonoGoapActionProvider actionProvider, IGoalRequest goalRequest)
            {
                this.ActionProvider = actionProvider;
                this.GoalRequest = goalRequest;
            }
        }

        public IGraph GetGraph() => this.resolver.GetGraph();
    }
}
