using System;
using ARPGDemo.BattleSystem;
using ARPGDemo.CustomAttributes;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{

    /*Tip：在设计上，一个AbilityTask可以包含多个AbilitySubTask，但不一定每个都会参与逻辑，要看具体的数据配置，这是一种提高编辑自由度的机制，比如一个相机SubTask，定义一个CinemachineCamera字段，
    但是实际在编辑时该字段是空的，所以就无从执行逻辑，那么就应该在从Task获取SubTask时就检查是否有效，有效就添加到待执行的SubTask容器中，无效就忽略。
    */

    public interface IAbilitySubTask
    {
        void OnBegin();
        void OnTick(float _curTime); //因为多个SubTask同处于一个时间轴中，共享同样的时间进度，所以不需要自己存储时间进度，而是交给时间轴统一存储。
        // void OnTick(float _curTime, float _progress); //TODO：或许会同时传入当前进度？
        void OnEnd();
        bool IsValid();
    }

    [Serializable]
    public class AbilitySubTask_AnimationInfo : IAbilitySubTask
    {
        [DisplayName("动画播放器")]
        [SerializeField] private AnimatorAgent m_AnimPlayer;
        [DisplayName("动画片段")]
        // [AnimationClipPreview]
        [SerializeField] private AnimationClip m_Anim;
        //TODO：需要扩展AnimatorAgent，支持这个设置播放速度的功能。或者说，手动驱动PlayableGraph，那么就可以控制AbilityTask的速度来控制该SubTask的速度？Timeline就是这样做的。
        // [DisplayName("播放速度")] 
        // [SerializeField] private float m_PlaySpeed = 1f;
        [DisplayName("播放层级")]
        [SerializeField] private int m_LayerIndex;
        [DisplayName("播放过渡时间")]
        [SerializeField] private float m_FadeInDuration;
        [DisplayName("停止过渡时间")]
        [SerializeField] private float m_FadeOutDuration;
        private AnimationClipState m_State;

        public void OnBegin()
        {
            // Debug.Log("播放动画");
            if (m_AnimPlayer == null)
            {
                Debug.Log("AnimationInfo没有指定AnimatorAgent");
                return;
            }
            m_State = m_AnimPlayer.Play(m_LayerIndex, m_Anim, m_FadeInDuration);
        }

        public void OnTick(float _curTime)
        {
            
        }

        public void OnEnd()
        {
            // Debug.Log("停止动画");
            m_State.Stop(m_FadeOutDuration);
        }

        public bool IsValid()
        {
            if (m_AnimPlayer == null || m_Anim == null) return false;
            return true;
        }
    }

    [Serializable]
    public class AbilitySubTask_AudioInfo : IAbilitySubTask
    {
        public AudioSource audioPlayer;
        public AudioClip clip;
        public Vector3 position; //TODO：注意音频播放存在位置因素，不过暂时不考虑。

        public void OnBegin()
        {
            audioPlayer.PlayOneShot(clip, 1f);
        }

        public void OnTick(float _curTime)
        {
            
        }

        public void OnEnd()
        {
            
        }

        public bool IsValid()
        {
            if (audioPlayer == null || clip == null) return false;
            return true;
        }
    }

    [Serializable]
    //这里的Hitbox就是一个特殊的事件子任务。
    public class AbilitySubTask_Hitbox : IAbilitySubTask
    {
        [DisplayName("检测器")]
        [SerializeField] private CollisionDetector detector; //指定要使用的检测器
        [DisplayName("启动时刻")]
        [SerializeField] private float startTime;
        [DisplayName("关闭时刻")]
        [SerializeField] private float endTime;
        private bool isEnabled;

        public void OnBegin()
        {
            isEnabled = false;
        }

        public void OnTick(float _curTime)
        {
            if (!isEnabled && _curTime >= startTime && _curTime <= endTime)
            {
                isEnabled = true;
                detector.EnableDetector();
            }

            if (isEnabled && (_curTime < startTime || _curTime > endTime))
            {
                isEnabled = false;
                detector.DisableDetector();
            }
        }

        public void OnEnd()
        {
            isEnabled = false;
        }

        public void SetHitCallback(HitCallback _action)
        {
            detector.SetHitCallback(_action);
        }

        public bool IsValid()
        {
            if (detector == null) return false;
            return true;
        }
    }

    [Serializable]
    public class AbilitySubTask_Interval : IAbilitySubTask
    {
        [DisplayName("开始时刻")]
        [SerializeField] private float startTime;
        [DisplayName("结束时刻")]
        [SerializeField] private float endTime;
        // public float currentTime;
        //在这个时间段内就是true，否则就是false，可以用于连段攻击的“可跳转区间”，也可以是其他用途。
        // public bool internally => currentTime >= startTime && currentTime <= endTime;
        private bool m_Internally;
        public bool internally => m_Internally;



        public void OnBegin()
        {
            m_Internally = false;
        }

        public void OnTick(float _curTime)
        {
            //保证小的是startTime，大的是endTime。
            if (startTime > endTime)
            {//使用元组快捷交换变量值。
                (startTime, endTime) = (endTime, startTime);
            }
            m_Internally = _curTime >= startTime && _curTime <= endTime;
            // Debug.Log($"当前区间时间:{_curTime}");
        }

        public void OnEnd()
        {
            m_Internally = false;
        }

        public bool IsValid()
        {
            return true;
        }
    }

    [Serializable]
    public class AbilitySubTask_TimePoint : IAbilitySubTask
    {
        [DisplayName("时间点")]
        [SerializeField] private float timePoint;
        private bool m_IsOver;
        public bool isOver => m_IsOver;

        public void OnBegin()
        {
            m_IsOver = false;
        }

        public void OnTick(float _curTime)
        {
            m_IsOver = _curTime >= timePoint;
        }

        public void OnEnd()
        {
            m_IsOver = false;
        }

        public bool IsValid()
        {
            return true;
        }
    }

    [Serializable]
    public class AbilitySubTask_Event : IAbilitySubTask
    {
        private Action m_Action;
        [DisplayName("触发时刻")]
        [SerializeField] private float timePoint;
        // private float lastTime;
        /*Tip：如果将lastTime改成trigger标记的话，不仅可以记录自己是否已经被触发，而且还可以向外界提供“是否已经触发”的信息，而lastTime的额外信息则是上一次Tick的时刻，
        不过这个信息无用，所以替换成该Trigger标记，合情合理。*/
        private bool m_HasTriggered;
        public bool hasTriggered => m_HasTriggered;

        public void OnBegin()
        {
            m_Action = null;
            // lastTime = 0f;
            m_HasTriggered = false;
        }

        public void OnTick(float _curTime)
        {
            if (hasTriggered == false && _curTime >= timePoint)
            {
                Debug.Log("触发SubTask事件");
                m_Action?.Invoke();
                m_HasTriggered = true;
            }
            //上一次还在之前，而此次就到了，那么就触发。
            // if (lastTime < timePoint && _curTime >= timePoint)
            // {
            //     m_Action?.Invoke();
            // }
            // lastTime = _curTime;
        }

        public void OnEnd()
        {
            m_Action = null;
            // lastTime = 0f;
            m_HasTriggered = false;
        }
        //应当在执行前就设置，不应该在执行时才设置。
        public void SetAction(Action _action) => m_Action = _action;

        public bool IsValid()
        {
            if (m_Action == null) return false;
            return true;
        }
    }
}