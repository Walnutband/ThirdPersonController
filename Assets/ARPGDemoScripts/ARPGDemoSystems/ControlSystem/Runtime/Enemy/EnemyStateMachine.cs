
using System;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem.Enemy
{
    [Serializable]
    public class EnemyStateMachine : StateMachine<EnemyStateBase>
    {
    }

    public class EnemyStateBase : StateBehaviour
    {
        // [SerializeField] protected Animator m_Animator;
        [SerializeField] protected AnimatorAgent m_AnimPlayer;
        protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;

        public override int tempPriority => 0;

        public override void OnEnterState()
        {
            base.OnEnterState();
            m_IsEnd = false;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            m_IsEnd = true;
        }
    }
}