

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    //处理事件。其实就是End事件。
    public class Postprocessor : PlayableBehaviour
    {
        /*Tip；突然想到，如果只需要调用其中某些方法的话，除了定义接口来划分以外，还可以直接使用函数指针即这里的委托来调用*/
        internal Func<List<AnimationStateBase>> GetStates;
        private List<AnimationClipState> m_States = new List<AnimationClipState>();

        /*Tip：其实实际情况最多就是每个层级有一个AnimationClipState可能触发事件而已，在设计上，处于过渡过程中的状态不会触发事件*/

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);

            List<AnimationStateBase> states = GetStates?.Invoke();
            if (states != null)
            {
                foreach (var state in states)
                {
                    if (state is AnimationClipState clipState)
                    {
                        m_States.Add(clipState);
                    }
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            // playable.GetGraph()
            // Debug.Log("ProcessFrame后处理器时间：" + playable.GetTime());
            /*Tip：单纯只是调用，具体逻辑和数据完全在AnimationClipState中*/
            foreach (var state in m_States)
            {
                state.CheckEvents();
            }
        }
    }
}