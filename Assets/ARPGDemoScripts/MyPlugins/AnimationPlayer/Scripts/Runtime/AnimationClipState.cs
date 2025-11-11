using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public class AnimationClipState : AnimationStateBase, IFadeTarget
    {
        private AnimationClip m_Clip;
        public AnimationClip clip => m_Clip;

        /*Tip：似乎混合动画并没有End事件，Animancer中也没有。*/
        //这个结束事件似乎没有什么信息。
        //默认结束，就是片段长度为触发时刻
        public event Action EndedEvent; //感觉事件用大驼峰命名更好。
        private double m_EndTime;
        //自定义结束，可以自行设置触发时刻，用于衔接动画。
        public event Action CustomEndedEvent;
        private double m_CustomEndTime;
        
        public float normalizedTime => (float)(m_Playable.GetTime() / m_Clip.length);

        internal AnimationClipState(AnimationGraph _graph, AnimationClip _clip) : base(_graph)
        {
            m_Playable = AnimationClipPlayable.Create(_graph.graph, _clip);
            m_EndTime = _clip.length;
            m_Clip = _clip;
        }

        internal AnimationClipState(AnimationGraph _graph, FadeAnimation _anim) : base(_graph)
        {
            m_Playable = AnimationClipPlayable.Create(_graph.graph, _anim.clip);
            m_Clip = _anim.clip;
            m_EndTime = (_anim.endTime - 0.001f) <= 0f ? _anim.clip.length : _anim.endTime; //不大于零的话，就等于没有设置，就直接设置为片段长度才是合理的。
            m_CustomEndTime = _anim.customEndTime;
        }

        internal AnimationClipState(AnimationGraph _graph, AnimationClip _clip, float _endTime, float _customEndTime) : base(_graph)
        {
            m_Playable = AnimationClipPlayable.Create(_graph.graph, _clip);
            m_Clip = _clip;
            m_EndTime = (_endTime - 0.001f) <= 0f ? _clip.length : _endTime; //不大于零的话，就等于没有设置，就直接设置为片段长度才是合理的。
            m_CustomEndTime = _customEndTime;
        }

        internal override void EnterPlaying() 
        {
            base.EnterPlaying();
            //因为需要复用，这里就是防止残留。
            ClearEvents();
        }

        /*Tip：这两个事件的回调方法是开放给外界注册的，只要在对应时刻之前注册就可以正常触发。*/
        internal void CheckEvents()
        {
            /*BugFix：我草，在遇到销毁产生的Bug之后才发现IsValid这个方法的用处，说白了因为销毁是延迟执行的，而在同一帧、某段逻辑之前调用了Destroy方法、但在某段逻辑中又访问了本该销毁的对象，
            此时并不为空，因为还没有真正地销毁，但此时会将其标记为无效，因为从逻辑上来看就应该已经被销毁了。所以在此检查是否有效，排除因为延迟销毁而出现的Bug。*/
            if (m_Playable.IsValid() == false) return;

            if ((m_Playable.GetTime() >= m_EndTime) && EndedEvent != null)
            {
                // Debug.Log($"片段{clip.name}触发EndEvent");
                EndedEvent.Invoke();
                EndedEvent = null; //清空。那么就算连续触发也不会重复执行了。
                                   // EndedEvent?.Invoke();

            }
            else if (m_Playable.GetTime() >= m_CustomEndTime && CustomEndedEvent != null)
            {
                // Debug.Log($"片段{clip.name}触发CustomEndEvent");
                CustomEndedEvent.Invoke();
                CustomEndedEvent = null;

                // CustomEndedEvent?.Invoke();
            }

        }

        public void ClearEvents()
        {
            EndedEvent = null;
            CustomEndedEvent = null;
        }

        void IFadeTarget.StartFadeOut()
        {//通常是，中途指定要播放其他动画，所以当前动画就变成了fadeOut状态参与过渡，这样的话就不要触发当前动画的后续事件了。
            Debug.Log("ClipState调用StartFadeOut");
            ClearEvents();
        }
    }
}