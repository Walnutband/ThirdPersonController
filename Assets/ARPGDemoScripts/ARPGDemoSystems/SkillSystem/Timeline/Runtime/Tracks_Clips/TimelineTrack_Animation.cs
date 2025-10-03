using System;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    [Serializable]
    // public class TimelineTrack_Animation : TimelineTrack
    public class TimelineTrack_Animation : TimelineTrack<TimelineClip_Animation>
    {
        // [SerializeField] private List<TimelineClip_Animation> m_Clips = new List<TimelineClip_Animation>();
        // protected override IEnumerable<TimelineClip> clips => m_Clips;

        /*TODO：要看采用什么动画系统了，这里Animancer的话甚至是不需要Animator的，因为AnimancerComponent默认控制同对象的Animator。*/
        // [SerializeField] private Animator m_Animator;
        private Animator m_Animator;

        private AnimancerComponent m_AnimPlayer;
        // [SerializeField] private List<TimelineClip_Animation> m_Clips = new List<TimelineClip_Animation>();

        public override void Initialize(TimelineContext _ctx)
        {
            m_AnimPlayer = _ctx.animPlayer;

            /*TODO：这里很值得思考，因为作为一个特殊的具体的轨道，必然是知道自己用什么片段类型的，所以类型转换并没有破坏结构性，但是有没有更加一般化的处理方式呢？*/
            // clips.ForEach(clip => clip.Init(m_AnimPlayer));
            // clips.ForEach(clip =>
            // {
            //     TimelineClip_Animation myclip = clip as TimelineClip_Animation;
            //     myclip.Init(m_AnimPlayer);
            // });
            // foreach (var clip in clips)
            foreach (var clip in m_Clips)
            {
                // TimelineClip_Animation myclip = clip as TimelineClip_Animation;
                // TimelineClip_Animation myclip = clip;
                // myclip.Init(m_AnimPlayer);
                clip.Init(m_AnimPlayer);
            }
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

        /*TODO：注意这里的Begin和End方法逻辑完全取决于所使用的动画系统，经测试使用Animancer的话，同一个轨道上的两个片段可能会产生相互干扰，
        比如因为顺序问题，前一个此时要开始，后一个此时要结束，那么前一个刚开始，但后一个就结束，由于播放的是同一个片段，所以后一个把前一个刚开始的给结束掉了，当然对于Animancer来说
        这是可以规避的问题，但这确实是一个值得思考的问题。*/

        protected override void OnBegin(double _localTime)
        {
            Debug.Log("OnBegin, localTime: " + _localTime);
            m_State = m_AnimPlayer.Play(m_Clip, 0f, FadeMode.FromStart);
            m_State.TimeD = _localTime;
        }

        /*Tip：对于动画播放来说，其实就是开头开始播放、末尾结束播放，中间过程完全由动画系统自行更新即可。
        为OnStart方法添加了localTime参数之后，似乎就不需要在Running方法中执行任何逻辑了。*/
        protected override void Running(double _localTime)
        {
            // m_State.MoveTime(_localTime, false);
        }

        protected override void OnEnd()
        {
            Debug.Log("OnEnd");
            // m_AnimPlayer.Stop(m_State);
            // m_State?.Stop(); //中途停止
            // m_State = null;
        }


    }
    
}