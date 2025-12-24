using System;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public abstract class GoalBase : IGoal
    {
        public int Index { get; set; }
        public IGoalConfig Config { get; private set; }

        //TODO：C#自带的GUID系统，有待进一步了解。
        public Guid Guid { get; } = Guid.NewGuid();
        //Ques：为何Goal会带有Effect呢？大概是因为IGoal继承了IConnectable，而Goal和Action都是以Node来处理的，所以成员是共同的，只是实际上Goal不会用到Effect而已。
        public IEffect[] Effects { get; } = { };
        //达成目标的条件，也就是要达到的目标状态。
        public ICondition[] Conditions { get; private set; }

        public void SetConfig(IGoalConfig config)
        {
            this.Config = config;
            this.Conditions = config.Conditions.ToArray();
        }

        public virtual float GetCost(IActionReceiver agent, IComponentReference references)
        {
            return this.Config.BaseCost;
        }
    }
}
