using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public class AnimationMixerState : AnimationStateBase, IUpdatable
    {
        private List<float> m_Thresholds;
        private float m_Parameter; //TODO：默认为float，似乎也确实通用，有没有可能使用其他类型作为参数？
        private Func<float> m_ParameterGetter;

        public AnimationMixerState(AnimationGraph _graph, MixerAnimation _mixer) : base(_graph)
        {
            //有多少个Motion就分配多少个端口
            List<MixerAnimation.Motion> motions = _mixer.motions;
            m_Thresholds = new List<float>(motions.Count);
            m_Playable = AnimationMixerPlayable.Create(_graph.graph, motions.Count);
            for (int i = 0; i < motions.Count; i++)
            {
                /*在设计上，这些AnimationClipPlayable并不对应一个AnimationClipState，它只是AnimationMixerState的一部分。*/
                MixerAnimation.Motion motion = motions[i];
                AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph.graph, motion.clip);
                _graph.graph.Connect(playable, 0, m_Playable, i);
                m_Thresholds.Add(motion.threshold); //填充阈值表，保证按照从小到大的顺序。
            }
            // SetParameter(m_Parameter);

            m_Graph.pre.AddUpdatable(this);
        }

        //Tip：把监测逻辑一并交给Preprocessor，而不是由外部来监测更新这里的参数，并且使用委托实现，非常自然。只是要注意周期问题，也就是适时注册（Add）和注销（Remove）。
        public void SetParameter(Func<float> _getter)
        {
            // SetParameter(_getter());
            m_ParameterGetter = _getter;
        }


        internal override void OnStatePlay()
        {
            base.OnStatePlay();
            m_Graph.pre.AddUpdatable(this);
        }

        internal override void OnStateStop()
        {
            base.OnStateStop();
            m_Graph.pre.RemoveUpdatable(this);
        }

        
        public bool Update(float _deltaTime) 
        {
            //游离状态就算了。
            if (isPlaying == false) return false;

            if (m_ParameterGetter == null)
            {
                Debug.LogError("AnimationMixerState的ParameterGetter为空，请检查。");
                return false;
            }

            SetParameter(m_ParameterGetter.Invoke());
            return false;
        }

        // public void SetParameter(ref float _paramater)
        // {
        //     m_Parameter = _paramater;
        // }

        //Tip：由外部调用。注意这里影响的是该节点下的子节点的权重，所以该节点本身是否在播放，并不影响这部分逻辑。
        public void SetParameter(float _value)
        {
            // Debug.Log($"设置参数值：{_value}");

            m_Parameter = _value;

            if (m_Thresholds.Count <= 1) return; //没有或只有一个，这是开发者的事，谁TM在混合状态中只设置一个动画？

            /*Tip：常用技巧，当要将整个列表中的元素设置为某个共同的值、同时让其中某些值设置成不同的值时，就可以先全部设置为相同值，再单独设置不同值，就是覆盖。而不是设置不同值之后
            再找出其他元素设置为相同值。有效利用互补关系。*/
            for (int i = 0; i < m_Thresholds.Count; i++)
            {
                m_Playable.SetInputWeight(i, 0f);
            }
            //超出边界的特殊情况直接处理。修正阈值，同时设置权重。
            if (m_Parameter <= m_Thresholds[0])
            {
                m_Parameter = m_Thresholds[0];
                m_Playable.SetInputWeight(0, 1f);
                // for (int i = 1; i < m_Thresholds.Count; i++)
                // {
                //     m_Playable.SetInputWeight(i, 0f);
                // }
            }
            else if (m_Parameter >= m_Thresholds[m_Thresholds.Count - 1])
            {
                m_Parameter = m_Thresholds[m_Thresholds.Count - 1];
                m_Playable.SetInputWeight(m_Thresholds.Count - 1, 1f);
            }
            else
            {
                /*Tip：就是个二分查找算法，但是实际上最多也就三四个片段，这里只是一般性的演示*/
                // int index = -1;
                int left = 0, right = m_Thresholds.Count - 1;
                //这个条件意思就是左右两端加上中间存在的节点超过了2个，总之最后就是要找到距离m_Parameter最近的两个节点。
                while (left < right - 1) //当left = right - 1时就已经找到所在区间了
                {
                    int mid = (left + right) / 2;
                    if (m_Thresholds[mid] < m_Parameter) left = mid;
                    else right = mid;
                }
                float interval = m_Thresholds[left + 1] - m_Thresholds[left];
                //注意计算是反过来的，越接近于下一个则下一个权重就越大。
                m_Playable.SetInputWeight(left, Mathf.Clamp01((m_Thresholds[left + 1] - m_Parameter) / interval));
                m_Playable.SetInputWeight(left + 1, Mathf.Clamp01((m_Parameter - m_Thresholds[left]) / interval));
                // for ()

            }
        }

        public Action onComplete { get;set;} 
    }
}