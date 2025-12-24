using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public class AgentCollection : IAgentCollection
    {
        private readonly IAgentType agentType;
        private HashSet<IMonoGoapActionProvider> agents = new();
        private HashSet<IMonoGoapActionProvider> queue = new();

        public AgentCollection(IAgentType agentType)
        {
            this.agentType = agentType;
        }

        public HashSet<IMonoGoapActionProvider> All() => this.agents;

        public void Add(IMonoGoapActionProvider actionProvider)
        {
            if (!actionProvider.isActiveAndEnabled)
                return;

            if (this.agents.Contains(actionProvider))
                return;

            this.agents.Add(actionProvider);
            this.agentType.Events.AgentRegistered(actionProvider);
        }

        public void Remove(IMonoGoapActionProvider actionProvider)
        {
            if (!this.agents.Contains(actionProvider))
                return;

            this.agents.Remove(actionProvider);
            this.agentType.Events.AgentUnregistered(actionProvider);
        }

        public void Enqueue(IMonoGoapActionProvider actionProvider)
        {
            //agents就是收集当前可以使用的ActionProvider，而queue是收集当前需要执行的ActionProvider，所以很显然前提执行的前提是存在于agents中。
            if (!this.agents.Contains(actionProvider))
                return;

            this.queue.Add(actionProvider);
        }

        public int GetQueueCount() => this.queue.Count;

        public IMonoGoapActionProvider[] GetQueue()
        {
            var data = this.queue.ToArray();

            this.queue.Clear();

            return data;
        }
    }
}
