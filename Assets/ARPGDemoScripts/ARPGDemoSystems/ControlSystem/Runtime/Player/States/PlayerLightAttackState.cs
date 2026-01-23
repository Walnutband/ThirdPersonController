
using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem.Player
{
    [AddComponentMenu("ARPGDemo/ControlSystem/Player/PlayerLightAttackState")]
    public class PlayerLightAttackState : PlayerStateBase
    {
        [SerializeField] private InputActionReference m_Action;
        [SerializeField] private FadeAnimation[] m_AttackAnims;

        private AnimationClipState m_State;
        private bool m_CanExitState;
        public override bool canExitState => m_CanExitState;

        private int combo = 0;
        private bool canContinue;
        private bool tempInput;

        public override void OnEnterState()
        {
            base.OnEnterState();
            PlayAttack();
            m_Action.action.started += PlayAttack;
            tempInput = false;
            m_CanExitState = false;
        }



        public override void OnUpdate()
        {
            //有预输入
            if (tempInput == true && (canContinue == true && combo < m_AttackAnims.Length))
            {
                PlayAttack();
            }    
        }

        public override void OnExitState()
        {
            base.OnExitState();
            m_Action.action.started -= PlayAttack;
            combo = 0; //注意归零还原。
            tempInput = false;
        }

        private void PlayAttack()
        {
            tempInput = false;
            m_CanExitState = false; //开始了一段攻击，那就不能退出（当然可以强制，那就是状态机的机制了）。
            m_State = m_AnimPlayer.Play(m_AttackAnims[combo++]);
            m_State.CustomEndedEvent += () => canContinue = true;
            m_State.EndedEvent += () =>
            {
                m_IsEnd = true;
                m_CanExitState = true;
            };
        }

        private void PlayAttack(InputAction.CallbackContext ctx)
        {
            if (canContinue == true && combo < m_AttackAnims.Length)
            {
                canContinue = false;
                PlayAttack();
            }
            else
            {
                tempInput = true;
            }
        }
    }
}