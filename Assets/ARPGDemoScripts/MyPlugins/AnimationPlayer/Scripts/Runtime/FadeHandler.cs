
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    public class FadeHandler
    {
        private AnimationLayer m_Layer;
        private AnimationStateBase m_FadeIn;
        // private List<AnimationStateBase> m_FadeOuts = new List<AnimationStateBase>();
        private AnimationStateBase m_FadeOut;
        private float m_FadeDuration;
        private float m_Progress;
        //存储开始权重，因为每次插值都要以开始权重和目标权重为两端，而目标权重默认就是0和1所以不用存储
        private float m_OutStartWeight;
        private float m_InStartWeight;

        public FadeHandler(AnimationLayer _layer, AnimationStateBase _out, AnimationStateBase _in, float _fadeDuration)
        {
            Debug.Log("过渡时间  " + _fadeDuration + "\n转出开始权重: " + _out.weight + "\n转入开始权重: " + _in.weight);
            m_Layer = _layer;
            m_FadeOut = _out;
            m_FadeIn = _in;
            m_FadeDuration = _fadeDuration;
            m_Progress = 0f;
            m_OutStartWeight = _out.weight;
            m_InStartWeight = _in.weight;
        }



        public bool Update(float _deltaTime)
        {
            //TODO：是否要设置为1000 * m_FadeDuration除以1000 * _deltaTime
            m_Progress += _deltaTime / m_FadeDuration;
            m_Progress = Mathf.Clamp01(m_Progress); //限制在0-1之间

            //设置权重。
            // m_Layer.SetStateWeight(m_FadeOut, Mathf.Lerp(m_OutStartWeight, 0f, m_Progress));
            // m_Layer.SetStateWeight(m_FadeIn, Mathf.Lerp(m_InStartWeight, 1f, m_Progress));
            //BugFix：艹了，隔了好久才发现，这里没有同步AnimationStateBase记录的权重，导致AnimationLayer中读取该权重时是错误的。其实同步问题本来就是封装结构的一个核心问题。
            m_FadeOut.weight = Mathf.Lerp(m_OutStartWeight, 0f, m_Progress);
            m_FadeIn.weight = Mathf.Lerp(m_InStartWeight, 1f, m_Progress);
            m_Layer.SetStateWeight(m_FadeOut, m_FadeOut.weight);
            m_Layer.SetStateWeight(m_FadeIn, m_FadeIn.weight);

            if (Mathf.Abs(m_Progress - 1f) < 0.001f)
            {//过渡结束
                m_Layer.SetStateWeight(m_FadeIn, 1f); //修正权重为1
                m_Layer.RemoveState(m_FadeOut); //移除过渡完成的转出状态。
                return true;
            }
            else
            {
                return false;
            }
        }

        /*TODO：该方法仅为临时将就*/
        public void RefreshData()
        {
            m_OutStartWeight = m_FadeOut.weight;
            m_InStartWeight = m_FadeIn.weight;
        }

        //直接结束
        public void End()
        {
            m_Layer.SetStateWeight(m_FadeIn, 1f);
            m_Layer.RemoveState(m_FadeOut);
        }

    }
}