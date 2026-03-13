
using MyPlugins.AnimationPlayer;
using Unity.Cinemachine;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public class ChargedThrowAbility : AbilityBase
    {
        // [SerializeField] private AnimatorAgent m_AnimPlayer;
        // [SerializeField] private FadeAnimation_ForAbilityTask m_ChargedAnimation;
        [SerializeField] private CinemachineCamera m_AimCamera; //瞄准相机
        [SerializeField] private int m_CameraPriority;

        // [Header("执行内容")]
        // private 

        public override void Activate()
        {
            m_AimCamera.Priority = m_CameraPriority;
        }

        public override bool TryDeactivate()
        {
            return true;
        }

        public override void Deactivate()
        {
            m_AimCamera.Priority = -1; 
        }
    }
}