
using System;
using Animancer;
using ARPGDemo.BattleSystem;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    [Serializable]
    public class TimelineContext
    {

        [SerializeField] private Transform m_Transform;
        public Transform transform => m_Transform;
        [SerializeField] private Animator m_Animator;
        public Animator animator => m_Animator;
        [SerializeField] private AnimancerComponent m_AnimPlayer;
        public AnimancerComponent animPlayer => m_AnimPlayer;

        private AudioSource m_AudioSource;
        public AudioSource audioSource => m_AudioSource;

        [SerializeField] private CollisionDetector m_Hitbox;
        public CollisionDetector hitbox => m_Hitbox;
        

    }
}