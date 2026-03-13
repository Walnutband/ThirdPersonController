
using System;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public enum EffectType
    {
        Instant,
        Infinite,
        HasDuration,
    }

    [Serializable]
    /*Tip：GE*/
    public class GameplayEffect
    {
        public EffectType effectType;
        // public ActorAttributeSet.AttributeValueChangedRule effect; 
        public ActorAttributeSet.AttributeValueModifier modifier;
        public float duration;
        public float period;

        //作用于属性集
        public void Apply(ActorAttributeSet _as)
        {
            modifier.Apply(_as);
        }

        //Tip：GE明确作用于特定属性集，所以就可以直接访问其中的事件注册回调方法了。
    }


    public class GEHandle
    {
        private float m_Timer;
        private float m_EffectTimer;
        private GameplayEffect m_GE;
        private ActorAttributeSet m_AS;
        private bool isPermanent;

        public GEHandle(GameplayEffect _ge, ActorAttributeSet _as)
        {
            m_GE = _ge;
            m_AS = _as;
            isPermanent = _ge.effectType == EffectType.Infinite;
            m_Timer = 0f;
            m_EffectTimer = 0f;
        }



        public bool OnTick(float _deltaTime)
        {
            m_Timer += _deltaTime;
            m_EffectTimer += _deltaTime;
            if (m_EffectTimer >= m_GE.period)
            {
                Debug.Log($"作用GE，timer:{m_Timer}, effectTimer:{m_EffectTimer}");
                m_GE.Apply(m_AS);
                m_EffectTimer = 0f;
            }
            return isPermanent ? false : m_Timer >= m_GE.duration;
        }
    }
    
}