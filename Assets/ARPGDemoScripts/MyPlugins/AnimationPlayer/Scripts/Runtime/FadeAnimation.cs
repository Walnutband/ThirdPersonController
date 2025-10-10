

using System;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    [Serializable]
    public class FadeAnimation
    {
        [SerializeField] private AnimationClip m_Clip;
        public AnimationClip clip => m_Clip;
        [SerializeField] private float m_FadeDuration;
        public float fadeDuration => m_FadeDuration;
        
    }
}