
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    /*TODO: TimelineObj可能是作为运行时的Timeline时间轴的直接代表，不存在派生。而TimelineExecutor应该就是以TimelineObj为对象来执行Timeline的。*/
    public class TimelineObj
    {
        private TimelineModel m_Model;

        private TimelinePlayMode m_PlayMode;
        public TimelinePlayMode playMode => m_PlayMode;

        //应该算是三个基本概念：时长、时刻、时速
        // private float m_Duration;
        public float duration => m_Model.duration;
        private float m_TimeElapsed; //从开头即0开始的时间。
        private float m_TimeScale;

        private bool m_AtBeginning; //在时间轴开始位置
        private bool m_AtEnding; //到时间轴末尾了

        public float beginTime => 0f; //TODO：beginTime就应该是0，但也可以是其他。
        public float endTime => m_Model.duration;

        public TimelineObj(TimelineModel _model)
        {
            m_Model = _model;
            m_PlayMode = _model.playMode;
        }

        public bool Tick(float _deltaTime) //感觉时间轴就得说是Tick而不是Update。
        {
            /*TODO：如何实现时间静止？按照现在的逻辑，就算m_TimeScale为0都还是会照样执行，只是与上一帧的时刻相同而已，但这应该还是要看具体的Clip逻辑，如何符合时间静止时的表现。*/
            _deltaTime *= m_TimeScale; //受到时间流速影响，通常都是1。
            m_TimeElapsed += _deltaTime; //此时所处时刻。

            // if (m_TimeElapsed >= m_Model.duration) //时间流逝到了或超过时间轴长度。
            // if (m_TimeElapsed >= endTime) //时间流逝到了或超过时间轴长度。
            // {
            //     // m_AtEnding = true;
            //     ReachEnding();
            //     //TODO：应该要看PlayMode
            //     // m_TimeElapsed = m_Model.duration;
            //     // m_TimeElapsed = 0f;
            // }

            // m_Model.Run(m_TimeElapsed);
            GOTO(m_TimeElapsed);

            return m_AtEnding; //及时返回是否到达末尾的信息给执行器。也可以让执行器主动检查标记变量比如m_AtEnding
        }

        public void GOTO(TimelineInterval _interval, bool _inverse = false) //就是跳转到区间的开始时刻还是结束时刻，本质上是早时刻还是晚时刻
        {
            if (_inverse == false)
            {
                // m_Model.Run(_interval.beginTime);
                GOTO(_interval.beginTime);
            }
            else
            {
                // m_Model.Run(_interval.endTime);
                GOTO(_interval.endTime);
            }
        }

        /*Tip: 理论上可以跳转到任一时刻，但实际上跳转的目的往往就是跳转到某个区间或阶段，不会随便乱跳。*/
        public void GOTO(float _time)
        {
            //超出两端就直接取两端，也可以超出两端就直接不跳转、用返回值表示。
            // if (_time > m_Model.duration) _time = m_Model.duration;
            // else if (_time < 0f) _time = 0f;

            if (_time > endTime)
            {
                ReachEnding();
            }
            else if (_time < beginTime)
            {
                ReachBeginning();
            }

            // m_TimeElapsed = _time;
            // m_Model.Run(_time);
            m_Model.Run(m_TimeElapsed);
        }
        /*TODO：这里有一些问题值得思考，跳转是跳转，在跳转后是否要立刻执行呢？在这里，GOTO就是默认执行的，而它不管是否到达了开头还是结尾。*/
        public void GOTOBeginning()
        {
            m_AtBeginning = true;
            m_AtEnding = false;
            GOTO(beginTime);
        }
        public void GOTOEnding()
        {
            m_AtBeginning = false;
            m_AtEnding = true;
            GOTO(endTime);
        }

        //只调整时刻，但是不会执行此刻的逻辑
        private void ReachBeginning()//到达开头
        {
            // m_TimeElapsed = 0f;
            m_TimeElapsed = beginTime;
            m_AtBeginning = true;
            m_AtEnding = false;
        }
        private void ReachEnding() //到达末尾
        {
            // m_TimeElapsed = m_Model.duration;
            m_TimeElapsed = endTime;
            m_AtBeginning = false;
            m_AtEnding = true;
        }
    }



    //TODO：TimelineModel会牵涉到整个技能系统的核心。一个Model就代表一个行为、动作、技能
    [Serializable]
    public class TimelineModel
    {
        [SerializeField] private uint m_ID;
        [SerializeField] private string m_Name;

        [SerializeField] private TimelinePlayMode m_PlayMode;
        public TimelinePlayMode playMode => m_PlayMode;

        private List<TimelineInterval> m_Intervals;

        // [SerializeField] private List<TimelineTrack> m_Tracks; //TODO：这种静态数据可能确实用数组比列表更好。
        private List<TimelineTrack> m_Tracks; //TODO：这种静态数据可能确实用数组比列表更好。
        public TimelineTrack_Animation track;

        [SerializeField] private float m_Duration;
        public float duration => m_Duration;


        public void Run(float _time) //这里是绝对时间，就是从0开始、当前所处的时刻。
        {
            m_Tracks.ForEach(track =>
            {
                track.Run(_time);
            });
        }
    }

    public class TimelineInterval
    {
        [SerializeField] private float m_BeginTime;
        public float beginTime => m_BeginTime;
        [SerializeField] private float m_EndTime;
        public float endTime => m_EndTime;

        private Action m_Event;

        public void TriggerEvent() => m_Event?.Invoke();
    }

    /*TODO：暂时没有想到特别具体的用处，大概比如香菱的大招在一定时间段内一直转圈圈就会用到Loop模式？*/
    public enum TimelinePlayMode
    {
        Once,
        Loop
    }

    [Serializable]
    public abstract class TimelineTrack
    {

        /*Tip：这里是经过了深度的思考，因为将基类的Track和Clip都设置为了抽象类，导致其“无法实例化”的性质对利用序列化形成了阻碍，而且在TimelineClip的派生类中
        往往会有很多独特的成员，也就是说不可能完全靠在基类中设置好接口、利用好多态性来实现所有逻辑，所以必须使用派生类型指向派生实例、而非使用基类指向派生类。
        但是有关调用Start、Run、End的相关逻辑确实是所有Track和Clip共同的，所以还是需要在这里的基类TimelineTrack中调用Clips，由此就想到了多态性，而且是使用
        C#的属性的多态性，即本质上是方法、但逻辑上是字段，那么派生类自己定义存储所用的TimelineClip派生类型的容器字段即可，就可以任意调用派生类型的公开成员了。*/
        // [SerializeField] protected List<TimelineClip> m_Clips;
        // protected abstract List<TimelineClip> clips { get; }
        protected abstract IEnumerable<TimelineClip> clips { get; } //Ques：据说List是不协变的，而IEnumerable是协变的，但暂时不知根本原理。

        // protected virtual T Clips<T>()
        // {

        // }

        [SerializeField] protected bool m_IsMuted; //关闭轨道，禁用轨道。

        public abstract void Initialize(); //主要是用来初始化片段的

        /*TODO：本来想的是Track不存在开始结束，因为都是共用同一个时间轴，但是发现Track应当具有“Mute”功能，就是选择关闭、不播放，这样的话，确实会有自己的周期管理，
        而“Mute”的最小单位应该就是Track了，不会到Clip、因为没啥意义。*/
        public virtual void Start() { }

        public virtual void Run(float _time)
        {
            if (m_IsMuted) return;

            foreach (TimelineClip clip in clips)
            {
                clip.Run(_time);
            }
            
            // clips.ForEach(clip =>
            // {
            //     clip.Run(_time);
            // });
        }

        public virtual void End() { }

    }

    [Serializable]
    public abstract class TimelineClip
    {
        //划定Clip的时间段即区间。
        /*TODO: 像这种共同的基本数据，而且在运行时不会改变、只是在编辑模式下编辑的话，大概是不需要开放给派生类的，只需要在基类中序列化即可。
        还有如果要支持比如倒放这种特殊机制的话，定义为StartTime和EndTime其实没有那么准确，但是对于程序来说确实可以这样定义，但要知道本质上这是基于正常顺序的StartTime和EndTime,
        而且倒放似乎也不影响这里的逻辑，因为是使用的绝对时刻。*/
        [SerializeField] protected float m_BeginTime;
        [SerializeField] protected float m_EndTime;

        /*Ques：偶然发现，这里的处理都是自己内部处理的，而之前看到的那些timeline其实是Track将此时的时刻与自己内部的Clip的StartTime或EndTime判断，然后决定如何调用Clip的方法。
        到底谁好谁坏呢？因为现在暂时看来确实可以完全由Clip自己处理，*/

        [SerializeField, ShowInInspector]
        protected bool m_IsRunning;

        // public abstract void Initialize();

        public virtual void Start(float _localTime)
        {
            m_IsRunning = true;
            //单独提取为一个抽象方法是为了让派生类能够完全不管基类的Start的End的逻辑。其实实现为虚方法也可以，因为有些派生Clip本来就不会同时用到三个，但为了一致的结构性还是搞成了抽象方法。
            OnStart(_localTime);
        }
        // protected abstract void OnStart();
        /*Tip：添加了localTime参数是因为考虑到调用Start方法时并不一定是在片段的开始时刻，理论上来说可能是片段范围内的任意时刻。*/
        protected abstract void OnStart(float _localTime);


        public abstract void Running(float _localTime); //局部时间，这才是各个片段自己的实际逻辑要用到的时刻值。

        // public virtual void Run(float _time) //time就代表时刻，其实是整个时间轴的时刻，对于Clip来说就是绝对时刻或者说全局时刻。
        // public virtual void Run(float _globalTime)
        public void Run(float _globalTime)
        {
            //因为情况分类完全确定，所以直接用if-else就行了
            // if (_time < m_StartTime && m_IsRunning == true)
            // {
            //     End();
            // }
            // else if (_time > m_EndTime && m_IsRunning == true)
            // {

            // }
            // else
            // {

            // }

            float _localTime = _globalTime - m_BeginTime;

            //在范围之外
            if ((_globalTime < m_BeginTime || _globalTime > m_EndTime) && m_IsRunning == true)
            {
                End();
            }
            //在范围之内
            else if (_globalTime >= m_BeginTime && _globalTime <= m_EndTime && m_IsRunning == false)
            {
                Start(_localTime);
            }


            if (m_IsRunning == true)
            {//局部时间才是Clip本身逻辑所使用的时间。前面的逻辑保证在m_IsRunning为true时，此时的时间轴时刻必然位于该Clip范围内。
                // Running(_globalTime - m_BeginTime);
                Running(_localTime);
            }

        }

        public virtual void End()
        {
            m_IsRunning = false;
            OnEnd();
        }
        protected abstract void OnEnd(); //TODO：End是否也需要localTime参数呢？
    }
}