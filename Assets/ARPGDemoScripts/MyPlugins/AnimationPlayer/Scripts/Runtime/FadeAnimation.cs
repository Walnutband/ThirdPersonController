

using System;
using ARPGDemo.CustomAttributes;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：FadeAnimation就是基础类型，而AnimationClip只是一个简便类型，通常来说就应该直接使用FadeAnimation、才能利用到该动画系统的价值功能*/
    [Serializable]
    public class FadeAnimation : IAnimationInfo
    {
        [SerializeField] private AnimationClip m_Clip;
        public AnimationClip clip => m_Clip;
        [SerializeField] private float m_FadeDuration;
        public float fadeDuration => m_FadeDuration;
        [SerializeField] private float m_EndTime;
        public float endTime => m_EndTime;
        [SerializeField] private float m_CustomEndTime;
        public float customEndTime => m_CustomEndTime;

        public int key => m_Clip.GetInstanceID();

        public FadeAnimation(AnimationClip _clip, float _fadeDuration)
        {
            m_Clip = _clip;
            m_FadeDuration = _fadeDuration;
        }
    }

    [Serializable]
    public class FadeAnimation_ForAbilityTask
    {
        [DisplayName("动画层级")]
        [SerializeField] private int m_LayerIndex;
        public int layerIndex => m_LayerIndex;
        [DisplayName("动画片段")]
        [SerializeField] private AnimationClip m_Clip;
        public AnimationClip clip => m_Clip;
        [DisplayName("过渡时间")]
        [SerializeField] private float m_FadeDuration;
        public float fadeDuration => m_FadeDuration;

        public FadeAnimation ToFadeAnimation()
        {
            return new FadeAnimation(m_Clip, m_FadeDuration);
        }

    }
}