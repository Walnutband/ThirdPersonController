using System;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    [Serializable]
    public class TimelineTrack_Animation : TimelineTrack
    {
        [SerializeField] private List<TimelineClip_Animation> m_Clips = new List<TimelineClip_Animation>();
        protected override IEnumerable<TimelineClip> clips => m_Clips;

        /*TODO：要看采用什么动画系统了，这里Animancer的话甚至是不需要Animator的，因为AnimancerComponent默认控制同对象的Animator。*/
        [SerializeField] private Animator m_Animator;
        [SerializeField] private AnimancerComponent m_AnimPlayer;
        // [SerializeField] private List<TimelineClip_Animation> m_Clips = new List<TimelineClip_Animation>();

        public override void Initialize()
        {
            /*TODO：这里很值得思考，因为作为一个特殊的具体的轨道，必然是知道自己用什么片段类型的，所以类型转换并没有破坏结构性，但是有没有更加一般化的处理方式呢？*/
            // clips.ForEach(clip => clip.Init(m_AnimPlayer));
            m_Clips.ForEach(clip =>
            {
                TimelineClip_Animation myclip = clip as TimelineClip_Animation;
                myclip.Init(m_AnimPlayer);
            });
        }
    }

    [Serializable]
    public class TimelineClip_Animation : TimelineClip
    {
        private AnimancerComponent m_AnimPlayer;
        [SerializeField] private AnimationClip m_Clip;
        private AnimancerState m_State;

        public void Init(AnimancerComponent _animPlayer)
        {
            m_AnimPlayer = _animPlayer;
        }

        protected override void OnStart(float _localTime)
        {
            m_State = m_AnimPlayer.Play(m_Clip);
            m_State.Time = _localTime;
        }

        /*Tip：对于动画播放来说，其实就是开头开始播放、末尾结束播放，中间过程完全由动画系统自行更新即可。
        为OnStart方法添加了localTime参数之后，似乎就不需要在Running方法中执行任何逻辑了。*/
        public override void Running(float _localTime)
        {
            // m_State.MoveTime(_localTime, false);
        }

        protected override void OnEnd()
        {
            // m_AnimPlayer.Stop(m_State);
            m_State.Stop();
        }


    }
    
}