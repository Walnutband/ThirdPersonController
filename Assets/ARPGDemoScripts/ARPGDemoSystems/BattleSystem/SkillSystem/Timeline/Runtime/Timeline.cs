
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

        public uint id => m_Model.id;
        public string name => m_Model.name;
        public string description => m_Model.description;

        //应该算是三个基本概念：时长、时刻、时速
        // private double m_Duration;
        public double duration => m_Model.duration;
        private double m_TimeElapsed = 0; //已经经过的时间，即此时所处时刻，从开头即0开始的时间。
        private float m_TimeScale = 1f; //时间流速，但是我感觉timeScale时间缩放（时间拉伸？）比timeSpeed更好。

        private bool m_AtBeginning; //在时间轴开始位置
        private bool m_AtEnding; //到时间轴末尾了

        public double beginTime => 0f; //TODO：beginTime就应该是0，但也可以是其他。
        public double endTime => m_Model.duration;

        public TimelineObj(TimelineModel _model)
        {
            m_Model = _model;
            m_PlayMode = _model.playMode;
        }
        public TimelineObj(TimelineModel _model, TimelineContext _ctx)
        {
            m_Model = _model;
            m_PlayMode = _model.playMode;
            m_Model.Initialize(_ctx); //绑定环境Context
        }

        public bool Tick(double _deltaTime) //感觉时间轴就得说是Tick而不是Update。
        {
            /*TODO：如何实现时间静止？按照现在的逻辑，就算m_TimeScale为0都还是会照样执行，只是与上一帧的时刻相同而已，但这应该还是要看具体的Clip逻辑，如何符合时间静止时的表现。*/
            _deltaTime *= m_TimeScale; //受到时间流速影响，通常都是1。
            m_TimeElapsed += _deltaTime; //此时所处时刻。
            GOTO(m_TimeElapsed);

            //TODO：是否要交给TimelineExecutor处理？虽然TimelineObj可以自己调整时刻，但是控制自己是否参与运行、只能由TimelineExecutor来实现。
            if (m_AtEnding == true && m_PlayMode == TimelinePlayMode.Once) //到达末尾了。
            {
                ToBeginning();
            }

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

        /*Tip: 理论上可以跳转到任一时刻，但实际上跳转的目的往往就是跳转到某个区间或阶段，不会随便乱跳。
        统一使用该方法来推动Timeline运行*/
        public void GOTO(double _time)
        {
            //超出两端就直接取两端，也可以超出两端就直接不跳转、用返回值表示。
            // if (_time > m_Model.duration) _time = m_Model.duration;
            // else if (_time < 0f) _time = 0f;


            if (_time >= endTime)
            {
                ToEnding();
            }
            else if (_time <= beginTime)
            {
                ToBeginning();
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

        /*Tip：这里的主要目的是对于到达边界的情况进行一个修正，这是常见问题了*/
        //只调整时刻，但是不会执行此刻的逻辑
        private void ToBeginning()//到达开头
        {
            // m_TimeElapsed = 0f;
            m_TimeElapsed = beginTime;
            m_AtBeginning = true;
            m_AtEnding = false;
        }
        private void ToEnding() //到达末尾
        {
            // m_TimeElapsed = m_Model.duration;
            m_TimeElapsed = endTime;
            m_AtBeginning = false;
            m_AtEnding = true;
        }
    }



    //TODO：TimelineModel会牵涉到整个技能系统的核心。一个Model就代表一个行为、动作、技能
    [Serializable]
    public class TimelineModel  //C#提供了一个接口ICloneable，但也仅仅是接口。
    {
        [SerializeField] private uint m_ID;
        public uint id => m_ID;
        //在运行时，名称和描述并不直接参与逻辑，主要是用于在UI中显示，否则的话都不需要这两个成员。
        [SerializeField] private string m_Name;
        public string name => m_Name;
        [TextArea(3,6)]
        [SerializeField] private string m_Description;
        public string description => m_Description; 

        [SerializeField] private TimelinePlayMode m_PlayMode;
        public TimelinePlayMode playMode => m_PlayMode;

        
        [SerializeField] private List<TimelineInterval> m_Intervals;

        // [SerializeField] private List<TimelineTrack> m_Tracks; //TODO：这种静态数据可能确实用数组比列表更好。
        private List<TimelineTrack> m_Tracks; //TODO：这种静态数据可能确实用数组比列表更好。

        [SerializeField] private double m_Duration;
        public double duration => m_Duration;

        /*Tip：想要克隆，必然需要调用构造函数分配新的实例内存，然后就是将成员数据复制过去。
        这里的实现很巧妙，至少现在我感觉是这样的，我都不晓得怎么写出来的，总之很方便，而且能够统一接口，能够防止外部构造TimelineModel实例，而是让*/
        private TimelineModel() { }
        private TimelineModel Clone(TimelineModel _model)
        {
            /*Tip：同一个类型中，不同实例就可以像这样访问私有成员*/
            TimelineModel model = new TimelineModel();
            model.m_ID = _model.m_ID;
            model.m_Name = _model.m_Name;
            model.m_Description = _model.m_Description;
            model.m_PlayMode = _model.m_PlayMode;
            model.m_Intervals = _model.m_Intervals;
            model.m_Tracks = _model.m_Tracks;
            model.m_Duration = _model.m_Duration;
            return model;
        }
        public TimelineModel Clone() => Clone(this);
        /*TODO：这个比较特殊化，因为轨道单独存储为了资产，所以需要在运行时手动赋值给Model。但是我又感觉这不是最佳写法，如果要修改的话应该会牵涉到资产部分的逻辑。*/
        public TimelineModel Clone(IEnumerable<TimelineTrack> _tracks)
        {
            //注意不能修改克隆体原型，只能也只需要对克隆出来的对象进行修改。
            TimelineModel model = Clone(this);
            model.m_Tracks = new List<TimelineTrack>(_tracks);
            return model;
        }

        /*TODO：说是初始化，其实就是绑定Context，也就代表进入到了所在对象的运行环境中，真正为对象服务。*/
        public void Initialize(TimelineContext _ctx)
        {
            m_Tracks.ForEach(track => track.Initialize(_ctx));
        }

        public void Run(double _time) //这里是绝对时间，就是从0开始、当前所处的时刻。
        {
            m_Tracks.ForEach(track =>
            {
                track.Run(_time);
            });
        }
    }

    [SerializeField]
    public class TimelineInterval
    {
        [SerializeField] private double m_BeginTime;
        public double beginTime => m_BeginTime;
        [SerializeField] private double m_EndTime;
        public double endTime => m_EndTime;

        //TODO：eventCondition触发条件、eventBehaviour事件行为

        private event Action m_Event;

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
        C#的属性的多态性，即本质上是方法、但逻辑上是字段，那么派生类自己定义存储所用的TimelineClip派生类型的容器字段即可，就可以任意调用派生类型的公开成员了。
        而且另外，各个派生轨道都是继承自这里的TimelineTrack，并不会相互继承，也就是每一个派生Track就代表了一个独立的分支，并不会出现重复继承、冗余继承的情况*/
        // [SerializeField] protected List<TimelineClip> m_Clips;
        // protected abstract List<TimelineClip> clips { get; }
        protected abstract IEnumerable<TimelineClip> clips { get; } //Ques：据说List是不协变的，而IEnumerable是协变的，但暂时不知根本原理，不过肯定属于C#的语言特性。

        // protected virtual T Clips<T>()
        // {

        // }

        [SerializeField] private bool m_IsMuted; //关闭轨道，禁用轨道。

        /*每个轨道*/
        public abstract void Initialize(TimelineContext ctx); //主要是用来初始化片段的

        /*TODO：本来想的是Track不存在开始结束，因为都是共用同一个时间轴，但是发现Track应当具有“Mute”功能，就是选择关闭、不播放，这样的话，确实会有自己的周期管理，
        而“Mute”的最小单位应该就是Track了，不会到Clip、因为没啥意义。*/
        public virtual void Start() { }

        public virtual void Run(double _time)
        {
            if (m_IsMuted) return;

            foreach (TimelineClip clip in clips)
            {
                clip.Update(_time);
            }

            // clips.ForEach(clip =>
            // {
            //     clip.Run(_time);
            // });
        }

        public virtual void End() { }

        public void DebugRunning()
        {
            foreach (TimelineClip clip in clips)
            {
                clip.DebugRunning();
            }
        }

    }
    /*Tip：这里运行时是与对应的资产类结构相同，使用一个非泛型类作为基类，子类设置为泛型类、派生类只要定义自己的逻辑就行了不用管这些结构性的内容、传入所使用的片段类型即可，
    具体来说就是存储片段的m_Clips以及获取片段的clips，不过主要是m_Clips，而clips其实是给派生类用的。
    并非，上面理解错了，其实clips是给基类TimelineTrack用的，因为它无法访问字段，字段是从泛型类型开始才定义的，这样利用方法的多态性就可以实现基类访问派生类的字段了。*/
    [Serializable]
    public abstract class TimelineTrack<TClip> : TimelineTrack where TClip : TimelineClip
    {
        /*Tip：派生类直接传入自己的片段类型就能够让自己的m_Clips直接为自己的片段类型了，无需再做类型转换。
        派生类访问字段、执行各自的特殊逻辑，而基类就访问属性clips、执行共同的逻辑。
        */
        [SerializeField] protected List<TClip> m_Clips = new List<TClip>();
        protected override IEnumerable<TimelineClip> clips => m_Clips;
    }

    [Serializable]
    public abstract class TimelineClip
    {
        //划定Clip的时间段即区间。
        /*TODO: 像这种共同的基本数据，而且在运行时不会改变、只是在编辑模式下编辑的话，大概是不需要开放给派生类的，只需要在基类中序列化即可。
        还有如果要支持比如倒放这种特殊机制的话，定义为StartTime和EndTime其实没有那么准确，但是对于程序来说确实可以这样定义，但要知道本质上这是基于正常顺序的StartTime和EndTime,
        而且倒放似乎也不影响这里的逻辑，因为是使用的绝对时刻。*/
        //Tip：感觉begin和end更合适，表达“开始”和“结束”的含义，更加频繁更加容易的感觉，而start应该和stop一对，表达“启动”和“停止”的含义，就感觉前置条件更多，比如对于Timeline本身就更合适。
        [SerializeField] protected double m_BeginTime;
        [SerializeField] protected double m_EndTime;

        /*Ques：偶然发现，这里的处理都是自己内部处理的，而之前看到的那些timeline其实是Track将此时的时刻与自己内部的Clip的StartTime或EndTime判断，然后决定如何调用Clip的方法。
        到底谁好谁坏呢？因为现在暂时看来确实可以完全由Clip自己处理，*/

        // [SerializeField, ShowInInspector]
        // protected bool m_IsRunning;
        private bool m_IsRunning = false;

        public void DebugRunning()
        {
            Debug.Log("m_IsRunning: " + m_IsRunning);
        }

        // public abstract void Initialize();

        // public virtual void Start(double _localTime)
        // {
        //     m_IsRunning = true;
        //     //单独提取为一个抽象方法是为了让派生类能够完全不管基类的Start的End的逻辑。其实实现为虚方法也可以，因为有些派生Clip本来就不会同时用到三个，但为了一致的结构性还是搞成了抽象方法。
        //     OnStart(_localTime);
        // }
        // protected abstract void OnStart();
        /*Tip：添加了localTime参数是因为考虑到调用Start方法时并不一定是在片段的开始时刻，理论上来说可能是片段范围内的任意时刻。*/
        protected abstract void OnBegin(double _localTime);


        protected abstract void Running(double _localTime); //局部时间，这才是各个片段自己的实际逻辑要用到的时刻值。

        /*Tip：通过这一个Update方法，将开始、运行、结束的逻辑统一到一个地方*/
        // public virtual void Run(double _time) //time就代表时刻，其实是整个时间轴的时刻，对于Clip来说就是绝对时刻或者说全局时刻。
        // public virtual void Run(double _globalTime)
        public void Update(double _globalTime)
        {
            Debug.Log("Running: " + m_IsRunning);
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

            double _localTime = _globalTime - m_BeginTime;

            // Debug.Log("Update, globalTime: " + _globalTime + "beginTime" + m_BeginTime + "endTime" + m_EndTime + ", localTime: " + _localTime + "m_IsRunning: " + m_IsRunning);

            //在范围之外，且之前正在运行
            if ((_globalTime < m_BeginTime || _globalTime > m_EndTime) && m_IsRunning == true)
            {
                Debug.Log("结束运行");
                // End();
                m_IsRunning = false;
                OnEnd();
            }
            //在范围之内，且之前没有运行
            else if (_globalTime >= m_BeginTime && _globalTime <= m_EndTime && m_IsRunning == false)
            {
                Debug.Log("开始运行");
                // Start(_localTime);
                m_IsRunning = true;
                OnBegin(_localTime);
            }


            if (m_IsRunning == true)
            {//局部时间才是Clip本身逻辑所使用的时间。前面的逻辑保证在m_IsRunning为true时，此时的时间轴时刻必然位于该Clip范围内。
                Debug.Log("运行中");
                // Running(_globalTime - m_BeginTime);
                Running(_localTime);
            }

        }

        // public virtual void End()
        // {
        //     m_IsRunning = false;
        //     OnEnd();
        // }
        //TODO：End是否也需要localTime参数呢？但是从逻辑上来看，只要退出所在范围就是End，不管退出后到了哪个时刻，而进入范围时可能是范围内的任一时刻，所以Start就需要localTime参数。
        protected abstract void OnEnd(); 
    }
}