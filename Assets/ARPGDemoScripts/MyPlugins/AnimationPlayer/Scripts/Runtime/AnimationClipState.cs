using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public class AnimationClipState : AnimationStateBase
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

        internal AnimationClipState(AnimationGraph _graph, AnimationClip _clip) : base(_graph)
        {
            m_Playable = AnimationClipPlayable.Create(_graph.graph, _clip);
            // m_Key = _clip.GetInstanceID(); //使用唯一的实例ID作为Key
            // m_Key = AnimationStateManager.StateID(_clip); //Ques：将转换到ID的逻辑方法统一放到字典中是否合适呢？
            /*Tip：使用SetDuration之后自动循环就不会生效了，所以对于不循环的才SetDuration，而且SetDuration是专门为了触发结束事件的，而需要循环播放的动画确实不需要触发结束事件，
            而我这个简单的动画系统里面没有专门的事件系统，所以就这样简单地处理了。
            其实我发现AnimationClipPlayable是会自动将Clip的长度设置为duration的，似乎我这完全是多此一举。*/
            // if (_clip.isLooping != true)
            // {
            //     m_Playable.SetDuration(_clip.length);
            // }
            // m_Playable
            m_EndTime = _clip.length;
            m_Clip = _clip;
        }

        /*Tip：这两个事件的回调方法是开放给外界注册的，只要在对应时刻之前注册就可以正常触发。*/
        internal void CheckEvents()
        {
            if ((m_Playable.GetTime() >= m_Playable.GetDuration() || m_Playable.GetTime() >= m_EndTime) && EndedEvent != null)
            {
                EndedEvent.Invoke();
                EndedEvent = null; //清空。那么就算连续触发也不会重复执行了。
                                   // EndedEvent?.Invoke();

            }
            else if (m_Playable.GetTime() >= m_CustomEndTime && CustomEndedEvent != null)
            {
                CustomEndedEvent.Invoke();
                CustomEndedEvent = null;

                // CustomEndedEvent?.Invoke();
            }

            CheckLoop();
        }

        private void CheckLoop()
        {
            // if (m_Playable.GetTime() >= m_Playable.GetDuration() || m_Playable.GetTime() >= m_EndTime && )
            // {

            // }
        }
    }
}