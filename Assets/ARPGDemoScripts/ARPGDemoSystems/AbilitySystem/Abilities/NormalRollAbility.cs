
using System;

namespace ARPGDemo.AbilitySystem
{
    [Serializable]
    public class NormalRollAbility : AbilityBase
    {
        public AbilityTask_State m_RollTask;
        private AbilityTask_State.TaskHandle m_Handle;

        public override void Activate()
        {
            m_Handle = m_ASC.taskExecutor.ExecuteTask(m_RollTask);
        }

        public override bool TryDeactivate()
        {
            if (m_Handle.CanExit() == true)
            {
                Deactivate();
                return true;
            }
            return false;
        }

        public override void Deactivate()
        {
            m_Handle.CompleteTask(false);
        }
    }
}