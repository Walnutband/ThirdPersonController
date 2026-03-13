
using System;
using ARPGDemo.ControlSystem.Player;

namespace ARPGDemo.AbilitySystem
{
    [Serializable]
    public class AbilityTask_State : AbilityTaskBase<AbilityTask_State.TaskHandle>
    {
        // public PlayerStateBase State;
        public AbilitySubTask_State StateTask;
        public AbilitySubTask_TimePoint canExit;

        public struct TaskHandle
        {
            AbilityTask_State m_Task;
            AbilityTaskExecutor m_Executor;
            public TaskHandle(AbilityTask_State _state, AbilityTaskExecutor _executor)
            {
                m_Task = _state;
                m_Executor = _executor;
            }

            public bool CanExit() => m_Task.canExit.isOver;
            public void CompleteTask(bool _complete) => m_Executor.CompleteCurrentTask(_complete);
        }

        public override IAbilitySubTask[] SubTasks => new IAbilitySubTask[] { StateTask, canExit };

        public override TaskHandle GetHandle(AbilityTaskExecutor _executor)
        {
            return new TaskHandle(this, _executor);
        }
    }
}