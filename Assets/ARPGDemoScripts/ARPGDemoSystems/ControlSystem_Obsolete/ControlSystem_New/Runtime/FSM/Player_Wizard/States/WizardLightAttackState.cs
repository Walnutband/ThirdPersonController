using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    [AddComponentMenu("ARPGDemo/ControlSystem_New/Player_Wizard/WizardLightAttackState")]
    public class WizardLightAttackState : WizardStateBase
    {
        private bool m_ToSelf;
        public override bool canTransitionToSelf => m_ToSelf;
        public override bool canExitState => m_IsEnd;
        public FadeAnimation m_LightAttack;
        private AnimationClipState m_State;

        public override void OnEnterState()
        {
            // Debug.Log("001进入LightAttack");
            base.OnEnterState();

            m_State = m_AnimPlayer.Play(1, m_LightAttack);
            // m_State = m_AnimPlayer.Play(0, m_LightAttack);
            m_State.EndedEvent += () =>
            {
                // Debug.Log("攻击End");
                m_IsEnd = true;
            };
            m_State.CustomEndedEvent += () =>
            {
                m_ToSelf = true;
            };
            m_ToSelf = false;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            Debug.Log("退出LightAttack");
            m_State.Stop();
        }
    }
}