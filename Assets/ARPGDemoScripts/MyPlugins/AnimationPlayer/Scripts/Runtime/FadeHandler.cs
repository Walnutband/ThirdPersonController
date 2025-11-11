
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    public class FadeHandler : IUpdatable
    {
        private List<FadeInfo> m_FadeOuts;
        private FadeInfo m_FadeIn;
        private float m_FadeDuration;
        private float m_Progress;

        private bool OutsIsNull;
        private bool InIsNull;

        private Action m_OnEnd;
        public Action onComplete { get; set; }

        // public FadeHandler(IFadeTarget _out, IFadeTarget _in, float _fadeDuration, Action _onEnd)
        // {
        //     m_Progress = 0f; //首先确定进度从0开始。
        //     m_FadeDuration = _fadeDuration;
        //     m_OnEnd = _onEnd;

        //     m_FadeOuts = new List<FadeInfo> { new FadeInfo(_out, _out.weight, 0f) };
        //     m_FadeIn = new FadeInfo(_in, _in.weight, 1f);
        // }

        /*Tip：在注册时就保证列表中没有空元素*/
        //默认情况（大多数情况）下，就是转出节点从1到0，转入节点从0到1，不过更准确来说，目标权重必然是0和1，但开始权重并不一定。
        public FadeHandler(List<IFadeTarget> _outs, IFadeTarget _in, float _fadeDuration, Action _onEnd) : this(_outs, _in, _fadeDuration, 0f, 1f, _onEnd)
        {
        }
        /*Tip：权重应当由外部自己设置好，这里就默认以当前权重作为开始权重。当然这本来就是动画系统的内部逻辑，其实没什么讲究，怎么舒服怎么来。*/
        public FadeHandler(List<IFadeTarget> _outs, IFadeTarget _in, float _fadeDuration, float _outTargetWeight, float _inTargetWeight, Action _onEnd)
        {
            m_Progress = 0f; //首先确定进度从0开始。
            m_FadeDuration = _fadeDuration;
            m_OnEnd = _onEnd;

            if (_outs != null)
            {
                List<FadeInfo> outInfos = new List<FadeInfo>(_outs.Count);
                for (int i = 0; i < _outs.Count; i++)
                {
                    _outs[i].StartFadeOut();
                    outInfos.Add(new FadeInfo(_outs[i], _outs[i].weight, _outTargetWeight));
                }
                m_FadeOuts = outInfos;
            }
            else OutsIsNull = true;
            if (_in != null)
            {
                m_FadeIn = new FadeInfo(_in, _in.weight, _inTargetWeight);
                _in.StartFadeIn();
            }
            else InIsNull = true;
        }

        /*Tip：过渡算法是分开计算的，并不存在什么权重合为1的强制要求，只是各自从开始权重以指定的过渡时间过渡到目标权重而已。
        而且为了兼顾处理层级混合，甚至可以没有转出或转入对象。
        */

        public bool Update(float _deltaTime)
        {
            m_Progress = Mathf.Clamp01(m_Progress + _deltaTime / m_FadeDuration);

            if (m_Progress >= 1f - 0.001f)
            {//结束了，修正。
                Ended();
                return true; //告知处理器，已经结束。
            }

            if (OutsIsNull == false)
            {
                foreach (var info in m_FadeOuts)
                {
                    info.target.weight = Mathf.Lerp(info.startWeight, info.targetWeight, m_Progress);
                }
            }
            if (InIsNull == false)
            {
                m_FadeIn.target.weight = Mathf.Lerp(m_FadeIn.startWeight, m_FadeIn.targetWeight, m_Progress);
            }
            

            return false;
        }

        /*Tip这里是手动结束，而Ended是在Update过程中的自动结束，说白了就是自动结束的话预处理器可以判断结束了，而手动的话预处理器判断不了，
        所以加上了一个onComplete就是用来通知预处理器移除这个完成的FadeHandler*/
        public void Complete()
        {
            Ended();
            onComplete?.Invoke();
        }

        public void Cancel()
        {
            onComplete?.Invoke();    
        }

        private void Ended()
        {
            if (OutsIsNull == false)
            {
                m_FadeOuts.ForEach(info =>
                {
                    info.target.weight = info.targetWeight;
                });
            }
            if (InIsNull == false)
            {
                m_FadeIn.target.weight = m_FadeIn.targetWeight;
            }
            //触发结束事件，
            m_OnEnd?.Invoke();
        }

        private struct FadeInfo
        {
            public IFadeTarget target { get; private set; }
            public float startWeight { get; private set; }
            public float targetWeight { get; private set; }

            public FadeInfo(IFadeTarget _target, float _startWeight, float _targetWeight)
            {
                target = _target;
                startWeight = _startWeight;
                targetWeight = _targetWeight;
            }
        }
    }

}
