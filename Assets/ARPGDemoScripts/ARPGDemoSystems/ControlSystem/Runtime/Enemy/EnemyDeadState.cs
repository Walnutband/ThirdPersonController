
using MyPlugins.AnimationPlayer;

namespace ARPGDemo.ControlSystem.Enemy
{
    public class EnemyDeadState : EnemyStateBase
    {
        public override bool canExitState => false;

        public FadeAnimation m_DeadAnim;
        public override void OnEnterState()
        {
            base.OnEnterState();
            m_AnimPlayer.Play(m_DeadAnim);
        }
    }
}