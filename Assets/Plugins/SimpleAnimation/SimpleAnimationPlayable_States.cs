using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;

public partial class SimpleAnimationPlayable : PlayableBehaviour
{
    private int m_StatesVersion = 0;

    private void InvalidateStates() { m_StatesVersion++; }
    private class StateEnumerable: IEnumerable<IState>
    {
        private SimpleAnimationPlayable m_Owner;
        public StateEnumerable(SimpleAnimationPlayable owner)
        {
            m_Owner = owner;
        }

        public IEnumerator<IState> GetEnumerator()
        {
            return new StateEnumerator(m_Owner);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StateEnumerator(m_Owner);
        }

        class StateEnumerator : IEnumerator<IState>
        {
            private int m_Index = -1;
            private int m_Version;
            private SimpleAnimationPlayable m_Owner;
            public StateEnumerator(SimpleAnimationPlayable owner)
            {
                m_Owner = owner;
                m_Version = m_Owner.m_StatesVersion;
                Reset();
            }

            private bool IsValid() { return m_Owner != null && m_Version == m_Owner.m_StatesVersion; }

            IState GetCurrentHandle(int index)
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                if (index < 0 || index >= m_Owner.m_StateManager.count)
                    throw new InvalidOperationException("Enumerator is invalid");

                StateInfo state = m_Owner.m_StateManager[index]; //数组索引器
                if (state == null)
                    throw new InvalidOperationException("Enumerator is invalid");

                return new StateHandle(m_Owner, state.index, state.playable);
            }

            //对于IEnumerator和IEnumerator<T>的Current的显式实现。
            object IEnumerator.Current { get { return GetCurrentHandle(m_Index); } }

            IState IEnumerator<IState>.Current { get { return GetCurrentHandle(m_Index); } }

            public void Dispose() { }

            //这里的接口方法就是将m_Index移动到下一个不为空的元素位置，如果超出了边界当然就直接退出，而返回值就是表明是否超出了边界。
            //注意这是一个do-while语句，所以首先的一层含义是必然向后移动一位，如果发现是空元素的话，那么就继续移动。直到不为空或者超出边界。
            public bool MoveNext()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");

                do
                { m_Index++; } while (m_Index < m_Owner.m_StateManager.count && m_Owner.m_StateManager[m_Index] == null);

                return m_Index < m_Owner.m_StateManager.count;
            }

            public void Reset()
            {
                if (!IsValid())
                    throw new InvalidOperationException("The collection has been modified, this Enumerator is invalid");
                m_Index = -1;
            }
        }
    }
    
    //Tip：IState定义了一个动画片段的所有应有属性，需要做到的是一想到动画片段就能立刻联想到这些属性。
    public interface IState
    {
        bool IsValid(); //enabled指的是自己本身的有效状态，而IsValid涉及到处于某一个整体中的自身的有效状态

        bool enabled { get; set; }

        float time { get; set; }

        float normalizedTime { get; set; }

        float speed { get; set; }

        string name { get; set; }

        float weight { get; set; }

        float length { get; }

        AnimationClip clip { get; }

        WrapMode wrapMode { get; }
    }

    public class StateHandle : IState
    {
        public StateHandle(SimpleAnimationPlayable s, int index, Playable target)
        {
            m_Parent = s;
            m_Index = index;
            m_Target = target;
        }

        public bool IsValid()
        {
            return m_Parent.ValidateInput(m_Index, m_Target);
        }

        public bool enabled
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager[m_Index].enabled;
            }

            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value)
                    m_Parent.m_StateManager.EnableState(m_Index);
                else
                    m_Parent.m_StateManager.DisableState(m_Index);

            }
        }

        public float time
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateTime(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                m_Parent.m_StateManager.SetStateTime(m_Index, value);
            }
        }

        public float normalizedTime
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = m_Parent.m_StateManager.GetClipLength(m_Index);
                if (length == 0f)
                    length = 1f;

                return m_Parent.m_StateManager.GetStateTime(m_Index) / length;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");

                float length = m_Parent.m_StateManager.GetClipLength(m_Index);
                if (length == 0f)
                    length = 1f;

                m_Parent.m_StateManager.SetStateTime(m_Index, value *= length);
            }
        }

        public float speed
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateSpeed(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                m_Parent.m_StateManager.SetStateSpeed(m_Index, value);
            }
        }

        public string name
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateName(m_Index);
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value == null)
                    throw new System.ArgumentNullException("A null string is not a valid name");
                m_Parent.m_StateManager.SetStateName(m_Index, value);
            }
        }

        public float weight
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager[m_Index].weight;
            }
            set
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                if (value < 0)
                    throw new System.ArgumentException("Weights cannot be negative");

                m_Parent.m_StateManager.SetInputWeight(m_Index, value);
            }
        }

        public float length
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateLength(m_Index);
            }
        }

        public AnimationClip clip
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateClip(m_Index);
            }
        }

        public WrapMode wrapMode
        {
            get
            {
                if (!IsValid())
                    throw new System.InvalidOperationException("This StateHandle is not valid");
                return m_Parent.m_StateManager.GetStateWrapMode(m_Index);
            }
        }

        public int index { get { return m_Index; } }

        private SimpleAnimationPlayable m_Parent;
        private int m_Index;
        private Playable m_Target;
    }

    private class StateInfo
    {
        public void Initialize(string name, AnimationClip clip, WrapMode wrapMode)
        {//这些是基本信息，也就是静态信息，而比如time、speed、weight之类的就是动态信息，就会通过对应的方法来控制。
            m_StateName = name;
            m_Clip = clip;
            m_WrapMode = wrapMode;
        }

        public float GetTime()
        {
            if (m_TimeIsUpToDate)
                return m_Time;

            m_Time = (float)m_Playable.GetTime();
            m_TimeIsUpToDate = true;
            return m_Time;
        }

        /// <summary>
        /// 直接设置状态的time（以及对应的Playable的time）
        /// </summary>
        public void SetTime(float newTime)
        {
            m_Time = newTime;
            m_Playable.ResetTime(m_Time);
            m_Playable.SetDone(m_Time >= m_Playable.GetDuration());
        }

        public void Enable()
        {
            if (m_Enabled)
                return;

            m_EnabledDirty = true;
            m_Enabled = true; //注意这里的enabled只是一个标记而已，并没有直接参与到生命周期
        }

        public void Disable()
        {
            if (m_Enabled == false)
                return;

            m_EnabledDirty = true;
            m_Enabled = false;
        }

        public void Pause()
        {
            // m_Playable.SetPlayState(PlayState.Paused);
            m_Playable.Pause();
        }

        public void Play()
        {
            // m_Playable.SetPlayState(PlayState.Playing);
            m_Playable.Play();
        }

        /// <summary>
        /// 停止，注意区别于暂停Pause。这里会直接重置状态，就是重置各个属性值。
        /// </summary>
        public void Stop()
        {
            m_FadeSpeed = 0f;
            ForceWeight(0.0f);
            Disable();
            SetTime(0.0f);
            m_Playable.SetDone(false);
            if (isClone)
            {
                m_ReadyForCleanup = true;
            }
        }

        public void ForceWeight(float weight)
        {
           m_TargetWeight = weight;
           //因为Fade过渡就是逐渐改变权重。
           m_Fading = false;
           m_FadeSpeed = 0f;
           SetWeight(weight);
        }

        public void SetWeight(float weight)
        {
            m_Weight = weight;
            m_WeightDirty = true;
        }

        public void FadeTo(float weight, float speed)
        {
            m_Fading = Mathf.Abs(speed) > 0f;
            m_FadeSpeed = speed;
            m_TargetWeight = weight;
        }

        public void DestroyPlayable()
        {
            if (m_Playable.IsValid())
            {
                m_Playable.GetGraph().DestroySubgraph(m_Playable);
            }
        }

        public void SetAsCloneOf(StateHandle handle)
        {
            m_ParentState = handle;
            m_IsClone = true;
        }

        public bool enabled
        {
            get { return m_Enabled; }
        }

        private bool m_Enabled;

        public int index
        {
            get { return m_Index; }
            set
            {
                Debug.Assert(m_Index == 0, "Should never reassign Index");
                m_Index = value;
            }
        }

        private int m_Index;

        public string stateName
        {
            get { return m_StateName; }
            set { m_StateName = value; }
        }

        private string m_StateName;

        public bool fading
        {
            get { return m_Fading; }
        }

        private bool m_Fading;


        private float m_Time;

        public float targetWeight
        {
            get { return m_TargetWeight; }
        }

        private float m_TargetWeight;

        public float weight
        {
            get { return m_Weight; }
        }

        float m_Weight;

        public float fadeSpeed
        {
            get { return m_FadeSpeed; }
        }

        float m_FadeSpeed;

        public float speed
        {
            get { return (float)m_Playable.GetSpeed(); }
            set { m_Playable.SetSpeed(value); }
        }

        public float playableDuration
        {
            get { return (float)m_Playable.GetDuration(); }
        }

        public AnimationClip clip
        {
            get { return m_Clip; }
        }

        private AnimationClip m_Clip;


        public void SetPlayable(Playable playable)
        {
            m_Playable = playable;
        }

        public bool isDone { get { return m_Playable.IsDone(); } }

        /// <summary>
        /// 记录所关联的AnimationClipPlayable
        /// </summary>
        public Playable playable
        {
            get { return m_Playable; }
        }

        private Playable m_Playable;

        public WrapMode wrapMode
        {
            get { return m_WrapMode; }
        }

        private WrapMode m_WrapMode;

        //Clone information
        public bool isClone
        {
            get { return m_IsClone; }
        }

        private bool m_IsClone;

        public bool isReadyForCleanup
        {
            get { return m_ReadyForCleanup; }
        }

        private bool m_ReadyForCleanup;

        /// <summary>
        /// parent指的是克隆对象，并非是层级方面的关系。
        /// </summary>
        public StateHandle parentState
        {
            get { return m_ParentState; }
        }

        public StateHandle m_ParentState;

        //启用状态和权重，其实从含义上来看m_TimeIsUpToDate也可以设置为timeDirty。
        public bool enabledDirty { get { return m_EnabledDirty; } }
        public bool weightDirty { get { return m_WeightDirty; } }

        public void ResetDirtyFlags()
        { 
            m_EnabledDirty = false;
            m_WeightDirty = false;
        }

        private bool m_WeightDirty;
        private bool m_EnabledDirty;

        /*Tip：这里设置为方法而不是属性，是因为只需要只应该开放给外部标记该字段为false的权利，如果用属性的话，虽然可以自定义setter，但终究不够明确具体的开放内容。
        总之还是理解封装，封装一方面是把数据和行为合理地融洽地放在一个具有特定含义的类中，一方面是向外部开放非常有限的接口，限制外部可以对自己进行的操作，不过同时
        也是对外部的一些提示，因为既是不希望被执行某些操作，又是希望被执行某些操作。*/
        public void InvalidateTime() { m_TimeIsUpToDate = false; }
        private bool m_TimeIsUpToDate;
    }

    private StateHandle StateInfoToHandle(StateInfo info)
    {
        return new StateHandle(this, info.index, info.playable);
    }

    /// <summary>
    /// 存储与管理（所存在的）状态。
    /// </summary>
    private class StateManager
    {
        private List<StateInfo> m_States;

        public int count { get { return m_Count; } }

        /// <summary>
        /// 记录的是m_States列表中实际存在的状态，不包含为空引用的元素。
        /// </summary>
        private int m_Count; 

        public StateInfo this[int i]
        {
            get
            {
                return m_States[i];
            }
        }

        public StateManager()
        {
            m_States = new List<StateInfo>();
        }

        /// <summary>
        /// 插入状态到状态列表中。
        /// </summary>
        /// <returns></returns>
        public StateInfo InsertState()
        {
            StateInfo state = new StateInfo();

            //Tip:寻找空位，可能由于种种原因，导致已经分配过的位置上的引用为空，所以就正好利用空位直接插入。
            int firstAvailable = m_States.FindIndex(s => s == null);
            if (firstAvailable == -1)
            {
                //没有空位就添加到最后一个元素的下一个位置。
                firstAvailable = m_States.Count;
                m_States.Add(state);
            }
            else
            {
                m_States.Insert(firstAvailable, state);
            }

            state.index = firstAvailable; //状态自己记录索引。
            m_Count++;
            return state;
        }

        public bool AnyStatePlaying()
        {
            return m_States.FindIndex(s => s != null && s.enabled) != -1;
        }


        public void RemoveState(int index)
        {
            StateInfo removed = m_States[index];
            m_States[index] = null;
            removed.DestroyPlayable();
            m_Count = m_States.Count;
        }

        public bool RemoveClip(AnimationClip clip)
        {
            bool removed = false;
            for (int i = 0; i < m_States.Count; i++)
            {
                StateInfo state = m_States[i];
                if (state != null && state.clip == clip)
                {
                    RemoveState(i);
                    removed = true;
                }
            }
            return removed;
        }

        public StateInfo FindState(string name)
        {
            return m_States.Find(s => s != null && s.stateName == name);
            //Tip:有点没看懂这里官方先找到index，然后再返回元素，属实莫名其妙，不是可以直接用一行Find就可以解决了吗？
            // int index = m_States.FindIndex(s => s != null && s.stateName == name);
            // if (index == -1)
            //     return null;

            // return m_States[index];

        }

        public void EnableState(int index)
        {
            StateInfo state = m_States[index];
            state.Enable();
        }

        public void DisableState(int index)
        {
            StateInfo state = m_States[index];
            state.Disable();
        }

        public void SetInputWeight(int index, float weight)
        {
            StateInfo state = m_States[index];
            state.SetWeight(weight);

        }

        public void SetStateTime(int index, float time)
        {
            StateInfo state = m_States[index];
            state.SetTime(time);
        }

        public float GetStateTime(int index)
        {
            StateInfo state = m_States[index];
            return state.GetTime();
        }

        public bool IsCloneOf(int potentialCloneIndex, int originalIndex)
        {
            StateInfo potentialClone = m_States[potentialCloneIndex];
            return potentialClone.isClone && potentialClone.parentState.index == originalIndex;
        }

        public float GetStateSpeed(int index)
        {
            return m_States[index].speed;
        }
        public void SetStateSpeed(int index, float value)
        {
            m_States[index].speed = value;
        }

        public float GetInputWeight(int index)
        {
            return m_States[index].weight;
        }

        /// <summary>
        /// 获取状态长度，就是AnimationClip的考虑了speed的实际长度
        /// </summary>
        public float GetStateLength(int index)
        {
            AnimationClip clip = m_States[index].clip;
            if (clip == null)
                return 0f;
            float speed = m_States[index].speed;
            if (speed == 0f)
                return Mathf.Infinity;

            return clip.length / speed;
        }

        public float GetClipLength(int index)
        {
            AnimationClip clip = m_States[index].clip;
            if (clip == null)
                return 0f;

            return clip.length;
        }

        public float GetStatePlayableDuration(int index)
        {
            return m_States[index].playableDuration;
        }

        public AnimationClip GetStateClip(int index)
        {
            return m_States[index].clip;
        }

        public WrapMode GetStateWrapMode(int index)
        {
            return m_States[index].wrapMode;
        }

        public string GetStateName(int index)
        {
            return m_States[index].stateName;
        }

        public void SetStateName(int index, string name)
        {
            m_States[index].stateName = name;
        }

        
        public void StopState(int index, bool cleanup)
        {//
            if (cleanup)
            {
                RemoveState(index);
            }
            else
            {
                m_States[index].Stop();
            }
        }

    }

    //队列状态，就是连续过渡的若干状态，使用QueuedState封装起来而实现功能。
    private struct QueuedState
    {
        public QueuedState(StateHandle s, float t)
        {
            state = s;
            fadeTime = t;
        }

        public StateHandle state;
        public float fadeTime;
    }

}
