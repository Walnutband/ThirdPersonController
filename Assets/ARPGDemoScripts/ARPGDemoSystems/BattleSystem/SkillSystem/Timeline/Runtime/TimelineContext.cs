using System;
using Animancer;
using ARPGDemo.BattleSystem;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    /*TODO：如果要改的话，大概是分为多个接口，就是每个轨道需要的Context都划分为一个接口，按需实现*/

    [Serializable]
    public class TimelineContext
    {

        [SerializeField] private Transform m_Transform;
        public Transform transform => m_Transform;
        [SerializeField] private Animator m_Animator;
        public Animator animator => m_Animator;
        [SerializeField] private AnimatorAgent m_AnimPlayer;
        public AnimatorAgent animPlayer => m_AnimPlayer;

        [SerializeField] private AudioSource m_AudioSource;
        public AudioSource audioSource => m_AudioSource;

        [SerializeField] private CollisionDetector m_Hitbox;
        public CollisionDetector hitbox => m_Hitbox;
        // public Action<Collider> hitCallback; //命中时的回调。（triggerEnter）
        [SerializeField] private ActorObject m_Actor;
        public IAttacker attacker => m_Actor;

    }
}