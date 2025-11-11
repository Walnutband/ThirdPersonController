
using ARPGDemo.SkillSystemtest;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    // public abstract class WizardStateBase : StateBehaviour, ICommandHandler
    public abstract class WizardStateBase : StateBehaviour
    {
        [SerializeField] protected AnimatorAgent m_AnimPlayer;
        [SerializeField] protected TimelineExecutor m_TimelineExecutor;

        protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;

        public override int tempPriority => 0;

        //Tip：大概有些状态并不需要处理命令
        // public abstract void HandleCommand(CommandBase _command);
        // public virtual void HandleCommand(CommandBase _command) { }

        public override void OnEnterState()
        {
            base.OnEnterState();
            m_IsEnd = false;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            m_IsEnd = false;
        }
    }
}