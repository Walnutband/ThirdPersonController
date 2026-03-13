using System;
using System.Collections.Generic;
using ARPGDemo.BattleSystem;
using ARPGDemo.CustomAttributes;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public abstract class AbilityTaskBase<THandle>
    {

        [DisplayName("持续时间")]
        [SerializeField] protected float m_Duration;
        public float duration => m_Duration;
        //Tip：完成时触发事件，这里使用SubTask实现，也可以采用其他各种各样的方式实现，但SubTask确实能够与其他内容一起被统一处理。
        // [ExpandInlineProperties("完成时触发事件")]
        // [SerializeField] protected AbilitySubTask_Event completedEvent;
        private Action m_CompletedEvent;

        //Tip：因为有多种派生的AbilityTask，可能有不同的内容，所以各自重写该属性。
        // public virtual IAbilitySubTask subTasks => null;
        public abstract IAbilitySubTask[] SubTasks {get;}

        //SubTask有Begin和End，作为集合体的Task也有。
        public virtual void Begin() { }
        public virtual void End() { }

        public void SetCompletedCallback(Action _action)
        {
            // Debug.Log("将完成时回调注册到Task中");
            // completedEvent.SetAction(_action);
            m_CompletedEvent = _action;
        }

        public void Completed() => m_CompletedEvent?.Invoke();

        //Tip：Handle就交由每个派生类自己实现，因为本质上是提供当前Task的一些运行信息，就不设定什么共同逻辑了。
        public abstract THandle GetHandle(AbilityTaskExecutor _executor);

    }

    //Tip：这里就是我对于单段攻击所提取出来的底层逻辑。
    [Serializable]
    public class AbilityTask_SingleAttack: AbilityTaskBase<AbilityTask_SingleAttack.TaskHandle>
    {
        // [DisplayName("持续时间")]
        // [SerializeField] protected float duration;
        [ExpandInlineProperties("动画信息")]
        [SerializeField] protected AbilitySubTask_AnimationInfo animation;
        [ExpandInlineProperties("音效信息")]
        [SerializeField] protected AbilitySubTask_AudioInfo audio;
        [ExpandInlineProperties("连段触发区间")]
        [SerializeField] private AbilitySubTask_Interval comboInterval;
        [ExpandInlineProperties("碰撞盒")]
        [SerializeField] protected AbilitySubTask_Hitbox hitbox;
        [ExpandInlineProperties("可退出时间点")]
        [SerializeField] protected AbilitySubTask_TimePoint canExit;

        public override IAbilitySubTask[] SubTasks => new IAbilitySubTask[] { animation, audio, hitbox, comboInterval, canExit };

        //明确知道自己有这个碰撞盒，所以提供注册击中回调的方法。
        public void SetHitCallback(HitCallback _action)
        {
            hitbox.SetHitCallback(_action);
        }

        public override TaskHandle GetHandle(AbilityTaskExecutor _executor)
        {
            return new TaskHandle(this, _executor);  
        }


        public struct TaskHandle
        {
            private AbilityTask_SingleAttack m_Task;
            public AbilityTask_SingleAttack task => m_Task;
            private AbilityTaskExecutor m_Executor; //Tip：获取Executor是为了（向Ability）提供对于Executor的调度功能，比如结束Task。
            public TaskHandle(AbilityTask_SingleAttack _task, AbilityTaskExecutor _executor)
            {
                m_Task = _task;
                m_Executor = _executor;
            }

            //能否触发连击（位于连击区间）、能否退出（主动退出）、是否已经结束（自动结束，返回到默认状态）。
            public bool CanCombo() => m_Task.comboInterval.internally;
            public bool CanExit() => m_Task.canExit.isOver;
            // public void Completed() => m_Executor.EndedCurrentTask();
            //如果已经触发了完成事件，那就肯定是完成了。但通常不会访问，因为在回调方法就已经退出该Ability执行其他Ability了。
            // public bool HasCompleted() => m_Task.completedEvent.hasTriggered;
            //指定完成Task。
            public void CompleteTask(bool _complete) => m_Executor.CompleteCurrentTask(_complete);
        }
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*Tip：AbilityTask是绑定到Ability的，主要负责表现层内容，但也会牵涉到逻辑层，比如连段攻击的可跳转区间以及共有的canExit和isEnd时间点，由于这些逻辑是固定的，所以大概不适合用事件，
而是直接读写Ability的相关属性。
*/
    [Serializable]
    public class AbilityTask
    {
        //Tip：TaskExecutor知道如何执行这些内容，这是底层规则。
        //层级与片段与过渡时间
        [ExpandInlineProperties("动画信息")]
        public FadeAnimation_ForAbilityTask animation;

        /*TODO：对于这类在执行时只是统一调用OnTick的对象，说白了就是时间轴那样的运行模式，如果想要在这里以IAbilityTaskTickable[]数组的形式存储，也就是为了数组的动态性，那么就必须
        要另外存储这些具体类型的对象：
        要么是使用者自己定义具体类型的字段，然后运行时添加到这里的数组中；
        要么就是像Timeline那样为每个具体类型写一个资产类，分别存储，这一点的好处在于可以在编辑器中动态增减这些具体对象，而不需要每次更改都必须直接改代码（改变字段）。
        
        第一种方式，也可以搞继承，就是继承该AbilityTask，在这里定义一个Tickable数组类型的虚属性，那么派生类只要重写该属性、将自己定义的那些具体类型的字段放进数组即可。
        */
        [ExpandInlineProperties("碰撞盒")]
        [SerializeField] private Hitbox hitbox; 
        [ExpandInlineProperties("连段触发区间")]
        [SerializeField] private AbilityTask_Interval comboInterval; //TODO：或许应该设置为Interval[]即区间数组，因为每个都只需要更新当前时间即可。
        [ExpandInlineProperties("可退出时间点")]
        [SerializeField] private AbilityTask_TimePoint canExit;
        [ExpandInlineProperties("已结束时间点")]
        [SerializeField] private AbilityTask_TimePoint isEnded;

        public virtual List<IAbilityTaskTickable> tickables => new List<IAbilityTaskTickable>() { hitbox, comboInterval, canExit, isEnded };

        public void SetHitCallback(HitCallback _action)
        {
            hitbox.detector.SetHitCallback(_action);    
        }

        //Tip：改了一下，将区间和时间点都存储在Handle中，每次
        //TODO：如果有不同的AbilityTask，那么应该也有对应的不同的Handle。
        public struct AbilityTaskHandle
        {
            private AbilityTask m_Task;
            public AbilityTask task => m_Task;
            private AbilityTaskExecutor_Obsolete m_Executor; 
            // private AbilityTask_Interval comboInterval; 
            // private AbilityTask_TimePoint canExit;
            // private AbilityTask_TimePoint isEnded;
            public AbilityTaskHandle(AbilityTask _task, AbilityTaskExecutor_Obsolete _executor)
            {
                m_Task = _task;
                m_Executor = _executor;
                // comboInterval = _task.comboInterval;
                // canExit = _task.canExit;
                // isEnded = _task.isEnded;
            }

            //能否触发连击（位于连击区间）、能否退出（主动退出）、是否已经结束（自动结束，返回到默认状态）。
            public bool CanCombo() => m_Task.comboInterval.internally;
            public bool CanExit() => m_Task.canExit.isOver;
            public bool IsEnded() => m_Task.isEnded.isOver;
            public void Completed() => m_Executor.EndedCurrentTask();
        }

    }

    [Serializable]
    public class AbilityTask_HasEvent
    {
        [SerializeField] private AbilityTask_Event m_Event;

        public void SetCallback(Action _action)
        {
            m_Event.SetAction(_action);
        }
    }

    public interface IAbilityTaskTickable
    {
        //TODO：这里关于“重置”，是为了重置一些状态变量，以便上次遗留结果影响此次判断，但其实更好的处理方式是每次开始执行时都常见新的实例，而要做到这一点最好是封装在资产类中，根据资产类直接创建实例。
        void Reset();
        void OnTick(float _curTime);
    }

    [Serializable]
    public class Hitbox : IAbilityTaskTickable
    {//检测器，启动时间，结束时间
        [DisplayName("检测器")]
        public CollisionDetector detector; //指定要使用的检测器
        [DisplayName("启动时刻")]
        public float startTime;
        [DisplayName("关闭时刻")]
        public float endTime;
        private bool isEnabled;

        public void OnTick(float _curTime)
        {
            if (_curTime >= startTime && _curTime <= endTime && !isEnabled)
            {
                isEnabled = true;
                detector.EnableDetector();
            }

            if (_curTime < startTime || _curTime > endTime && isEnabled)
            {
                isEnabled = false;
                detector.DisableDetector();
            }
        }

        public void Reset()
        {
            isEnabled = false;
        }
    }

    [Serializable]
    public class AbilityTask_Interval : IAbilityTaskTickable
    {
        [DisplayName("开始时刻")]
        public float startTime;
        [DisplayName("结束时刻")]
        public float endTime;
        // public float currentTime;
        //在这个时间段内就是true，否则就是false，可以用于连段攻击的“可跳转区间”，也可以是其他用途。
        // public bool internally => currentTime >= startTime && currentTime <= endTime;
        public bool m_Internally;
        public bool internally => m_Internally;

        public void OnTick(float _curTime)
        {
            //保证小的是startTime，大的是endTime。
            if (startTime > endTime)
            {//使用元组快捷交换变量值。
                (startTime, endTime) = (endTime, startTime);
            }
            m_Internally = _curTime >= startTime && _curTime <= endTime;
        }

        public void Reset()
        {
            m_Internally = false;
        }
    }

    [Serializable]
    //经过指定时间，就是true，否则就是false。这是将比如canExit和isEnded这样的标记的逻辑提取出来了。
    public class AbilityTask_TimePoint : IAbilityTaskTickable
    {
        public float timePoint;
        private bool m_IsOver;
        public bool isOver => m_IsOver;

        public void OnTick(float _curTime)
        {
            m_IsOver = _curTime >= timePoint;
        }

        public void Reset()
        {
            m_IsOver = false;
        }
    }   

    public class AbilityTask_Event : IAbilityTaskTickable
    {
        private Action m_Action;
        [DisplayName("触发时刻")]
        [SerializeField] private float timePoint;
        private float lastTime;

        public void Reset()
        {
            m_Action = null;
            lastTime = 0f;
        }

        public void OnTick(float _curTime)
        {
            //上一次还在之前，而此次就到了，那么就触发。
            if (lastTime < timePoint && _curTime >= timePoint)
            {
                m_Action?.Invoke();
            }
            lastTime = _curTime;
        }

        public void SetAction(Action _action) => m_Action = _action;
    }

}