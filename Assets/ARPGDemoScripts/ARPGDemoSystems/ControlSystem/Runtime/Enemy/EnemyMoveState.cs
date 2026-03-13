
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem.Enemy
{
    public class EnemyMoveState : EnemyStateBase
    {
        [SerializeField] protected FadeAnimation m_IdleAnim;
        [SerializeField] protected FadeAnimation m_RunAnim;
        [SerializeField] private float m_RunSpeed;

        public override void OnEnterState()
        {
            base.OnEnterState();
            m_AnimPlayer.Play(m_IdleAnim);
        }

        
    }
}