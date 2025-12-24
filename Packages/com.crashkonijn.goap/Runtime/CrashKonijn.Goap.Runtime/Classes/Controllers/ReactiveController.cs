using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public class ReactiveController : IGoapController
    {
        private IGoap goap;

        public void Initialize(IGoap goap)
        {
            //监听Goap
            this.goap = goap;
            this.goap.Events.OnAgentResolve += this.OnAgentResolve;
            this.goap.Events.OnNoActionFound += this.OnNoActionFound;
        }

        public void Disable()
        {
            if (this.goap.IsNull())
                return;

            if (this.goap?.Events == null)
                return;

            this.goap.Events.OnAgentResolve -= this.OnAgentResolve;
            this.goap.Events.OnNoActionFound -= this.OnNoActionFound;
        }

        //Tip：应该是，触发OnAgentResolve事件，调用了这里的OnAgentResolve方法，将对应的ActionProvider添加到队列中，然后这里OnUpdate中获取队列，然后逐个运行。

        public void OnUpdate()
        {
            foreach (var (type, runner) in this.goap.AgentTypeRunners)
            {
                var queue = type.Agents.GetQueue();

                runner.Run(queue);
            }

            foreach (var agent in this.goap.Agents)
            {
                if (agent.IsNull())
                    continue;

                if (agent.Receiver == null)
                    continue;

                if (agent.Receiver.IsPaused)
                    continue;

                // Update the action sensors for the agent
                agent.AgentType.SensorRunner.SenseLocal(agent, agent.Receiver.ActionState.Action as IGoapAction);
            }
        }

        public void OnLateUpdate()
        {
            foreach (var runner in this.goap.AgentTypeRunners.Values)
            {
                runner.Complete();
            }
        }

        private void OnNoActionFound(IMonoGoapActionProvider actionProvider, IGoalRequest request)
        {
            this.Enqueue(actionProvider);
        }

        private void OnAgentResolve(IMonoGoapActionProvider actionProvider)
        {
            this.Enqueue(actionProvider);
        }

        private void Enqueue(IMonoGoapActionProvider actionProvider)
        {
            actionProvider.AgentType?.Agents.Enqueue(actionProvider);
        }
    }
}
