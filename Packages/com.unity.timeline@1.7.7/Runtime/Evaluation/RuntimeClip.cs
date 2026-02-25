using System;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    // The RuntimeClip wraps a single clip in an instantiated sequence.
    // It supports the IInterval interface so that it can be stored in the interval tree
    // It is this class that is returned by an interval tree query.
    class RuntimeClip : RuntimeClipBase
    {
        TimelineClip m_Clip;
        Playable m_Playable;
        Playable m_ParentMixer;

        public override double start
        {
            get { return m_Clip.extrapolatedStart; }
        }

        public override double duration
        {
            get { return m_Clip.extrapolatedDuration; }
        }

        public RuntimeClip(TimelineClip clip, Playable clipPlayable, Playable parentMixer)
        {
            Create(clip, clipPlayable, parentMixer);
        }

        void Create(TimelineClip clip, Playable clipPlayable, Playable parentMixer)
        {
            m_Clip = clip; //记录片段源信息。
            m_Playable = clipPlayable;
            m_ParentMixer = parentMixer;
            clipPlayable.Pause();
        }

        public TimelineClip clip
        {
            get { return m_Clip; }
        }

        public Playable mixer
        {
            get { return m_ParentMixer; }
        }

        public Playable playable
        {
            get { return m_Playable; }
        }

        public override bool enable
        {//TODO：应当在进入播放和退出播放的时刻设置回调。
            set
            {
                //从暂停到播放，所以调用Play，而且从ClipIn开始，这里就是支持的ClipIn功能。
                if (value && m_Playable.GetPlayState() != PlayState.Playing)
                {
                    m_Playable.Play();
                    SetTime(m_Clip.clipIn);
                }
                else if (!value && m_Playable.GetPlayState() != PlayState.Paused)
                {
                    m_Playable.Pause();
                    //这里是因为调用DisableAt才会设置enable=false，而调用DisableAt意味着不会调用EvaluateAt，也就是不会设置权重，所以在此处需要手动设置权重为0。所以也算是一个补丁代码
                    if (m_ParentMixer.IsValid()) 
                        m_ParentMixer.SetInputWeight(m_Playable, 0.0f);
                }
            }
        }

        public void SetTime(double time)
        {
            m_Playable.SetTime(time);
        }

        public void SetDuration(double duration)
        {
            m_Playable.SetDuration(duration);
        }

        //设置Weight、Time、Duration
        //Tip：须知，这里的localTime就是整个时间轴当前行进的时间，而各个片段所记录的时间也都是以这同一个时间轴作为基准的。
        public override void EvaluateAt(double localTime, FrameData frameData)
        {
            enable = true;
            if (frameData.timeLooped)
            {
                //这好像是本来就有的Bug。
                // case 1184106 - animation playables require setTime to be called twice to 'reset' event.
                SetTime(clip.clipIn); // 从中途开始播放。本质上是设置节点的开始时间，因为Graph正常运行就是按照特定频率对各个节点的Time按顺序步进，然后Evaluate的。
                SetTime(clip.clipIn);
            }

            float weight = 1.0f;
            if (clip.IsPreExtrapolatedTime(localTime))
                weight = clip.EvaluateMixIn((float)clip.start);
            else if (clip.IsPostExtrapolatedTime(localTime))
                weight = clip.EvaluateMixOut((float)clip.end);
            //注意此时仍然时间轴进度仍然可能位于片段之外，比如ExtrapolationMode都为None。而AnimationCurve默认WrapMode为ClampForever，即超出范围时取第一帧或最后一帧，所以能够得到正确结果
            else
                weight = clip.EvaluateMixIn(localTime) * clip.EvaluateMixOut(localTime);

            /*Tip：对于AnimationTrack来说，这里的mixer是AnimationLayerMixerPlayable，而对于连接在AnimationMixerPlayable上的各个片段，还是在进入Start时就设置权重为1，而退出End时设置权重为0*/
            if (mixer.IsValid())
                mixer.SetInputWeight(playable, weight);

            // localTime of the sequence to localtime of the clip
            double clipTime = clip.ToLocalTime(localTime);
            if (clipTime >= -DiscreteTime.tickValue / 2) //大于等于一个接近于0的数，大概要表达的就是大于等于0。
            {
                SetTime(clipTime); //设置进度，如果有子节点，则因为SetPropagateSetTime(true)而保证同步，这就是保证扩展性的机制。
            }

            //带上Extrapolation时间加上自身长度的总时间，作为Duration，似乎这才是更合理的做法，而初始时统一的都是设置为Clip本身的Duration。
            SetDuration(clip.extrapolatedDuration);
        }

        public override void DisableAt(double localTime, double rootDuration, FrameData frameData)
        {
            var time = Math.Min(localTime, (double)DiscreteTime.FromTicks(intervalEnd));
            if (frameData.timeLooped) //Ques：这一帧在循环，则当前进度会大于时间轴的长度。不过timeLooped这个值的决定因素并不明确，大概也基本是由内部使用，尽管是公开的。
                time = Math.Min(time, rootDuration);

            var clipTime = clip.ToLocalTime(time);
            if (clipTime > -DiscreteTime.tickValue / 2)
            {
                SetTime(clipTime); //进度正常设置，主要是控制权重和播放状态。
            }
            enable = false;
        }
    }
}
