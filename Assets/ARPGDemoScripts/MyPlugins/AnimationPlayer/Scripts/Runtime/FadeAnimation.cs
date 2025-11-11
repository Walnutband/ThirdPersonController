

using System;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：FadeAnimation就是基础类型，而AnimationClip只是一个简便类型，通常来说就应该直接使用FadeAnimation、才能利用到该动画系统的价值功能*/
    [Serializable]
    public class FadeAnimation
    {
        [SerializeField] private AnimationClip m_Clip;
        public AnimationClip clip => m_Clip;
        [SerializeField] private float m_FadeDuration;
        public float fadeDuration => m_FadeDuration;
        [SerializeField] private float m_EndTime;
        public float endTime => m_EndTime;
        [SerializeField] private float m_CustomEndTime;
        public float customEndTime => m_CustomEndTime;
    }
}