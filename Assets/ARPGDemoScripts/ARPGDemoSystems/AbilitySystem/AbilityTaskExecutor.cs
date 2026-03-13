using System;
using System.Collections.Generic;
using ARPGDemo.BattleSystem;
using ARPGDemo.ControlSystem;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{

    public class AbilityTaskExecutor
    {

        //TODO：正在执行的SubTask，都是从AbilityTask提取出来的，因为执行只需要该接口信息即可。不过现在假设的是只能执行一个AbilityTask，想必会有执行多个的需求，如何处理随之而来的管理问题？
        private List<IAbilitySubTask> m_SubTasks = new List<IAbilitySubTask>();
        private float m_Timer; //计时器。
        private bool m_IsRunning;
        private float duration;
        private Action completedEvent;
        //TODO：需要实现循环机制（模式），才能实现蓄力动作等行为。
        // private bool isLoop;

        //传入经过的时间，
        public void OnTick(float _deltaTime)
        {
            //未运行。
            if (m_IsRunning == false) return;

            m_Timer += _deltaTime;
            // m_Timer = Mathf.Min(m_Timer + _deltaTime, duration);
            // m_SubTasks.ForEach(task => task.OnTick(m_Timer));
            m_SubTasks.ForEach(task => task.OnTick(Mathf.Min(m_Timer, duration)));
            //完整运行完成。
            if (m_Timer >= duration) CompleteCurrentTask();
        }

        /*Tip：默认就是结束当前任务，调用SubTask的OnEnd，而对于同一个Ability中的Task，其实就是为了处理好动画，这些Task通常是在同一层级播放动画，就应该利用自动的过渡机制，
        而不是显式地调用Stop。*/
        public THandle ExecuteTask<THandle>(AbilityTaskBase<THandle> _task, bool _end = false) 
        {
            //先结束。
            if (_end == true) OnEnded(); //Tip：可以深思，这里没有决策，就是指定就执行，如果有决策的话，可能就是当前Task能否退出，之类的。不过这部分逻辑已经（且应该）放在Ability中处理。
            else CompleteCurrentTask(false); //这属于打断执行，所以不需要触发完成事件。

            //开始执行。对于执行器来说，它的行为就是调度SubTask，OnStarted就是行为准备、OnTick就是行为执行、OnEnded就是行为收尾。
            OnStarted();

            IAbilitySubTask[] subTasks = _task.SubTasks;
            for (int i = 0; i < subTasks.Length; i++)
            {//只取有效的SubTask。
                if (subTasks[i].IsValid() == true)
                {
                    m_SubTasks.Add(subTasks[i]);
                    subTasks[i].OnBegin(); //立刻执行OnBegin是为了利用此时信息，虽然可能对性能也毫无作用。
                }
            }

            duration = _task.duration;
            completedEvent = _task.Completed;

            // if (duration <= 0f) isLoop = true;

            return _task.GetHandle(this);    
        }

        //由外部主动调用，触发完成回调，
        public void CompleteCurrentTask(bool _complete = true)
        {
            // if (_complete == true) completedEvent?.Invoke();
            if (_complete == true)
            {
                completedEvent?.Invoke();
                completedEvent = null;
            }

            m_SubTasks.ForEach(task =>
            {
                task.OnEnd();    
            });


            OnEnded();
        }

        //准备执行。
        private void OnStarted()
        {
            m_IsRunning = true;
            m_Timer = 0f; //重置计时器。
            m_SubTasks.Clear();
        }

        //结束执行。
        private void OnEnded()
        {
            m_IsRunning = false;
            m_Timer = 0f; //重置计时器。
            m_SubTasks.Clear();
        }
    }

    /*Tip：专门用于调度执行SubTask的对象。就是把这部分数据和逻辑从AbilityTask中提取出来，让AbilityTask只需要存储重要数据即可、以及提供一些公开方法以供注册回调。
    不过如果Executor只执行一个AbilityTask的话，其实可以直接让Executor充当该对象。
    */


    /*Tip：作为执行器，明确知道AbilityTask有哪些内容，也知道如何执行这些内容。这时，AbilityTask就是纯粹用于存储数据了。
    显然其实本来可以把存储数据和执行逻辑放在一起，不过这样分开之后，就可以将AbilityTask放入到其他类型中定义字段，而该执行器就可以放在ASC中，而不需要和数据放在一起。
    */
    /*Tip：执行的情况会反映到Ability，或者说Ability要能够获取到当前的执行情况。*/
    //TODO：暂时认为，同时只能执行一个AbilityTask的内容。
    [Serializable]
    public class AbilityTaskExecutor_Obsolete
    {
        //播放动画内容
        [SerializeField] public AnimatorAgent animPlayer;
        private AnimationClipState m_AnimState;
        // private Hitbox hitbox; //用于检测是否命中目标
        private List<IAbilityTaskTickable> m_Tickables;
        private float m_Timer; //计时器。
        
        private bool m_IsRunning;

        //执行的是表现层内容，所以使用Update的频率。
        public void OnTick(float _deltaTime)
        {
            //未运行，直接返回。
            if (m_IsRunning == false) return; 

            m_Timer += _deltaTime;
            m_Tickables.ForEach(tickable => tickable.OnTick(m_Timer));
        }

        public AbilityTask.AbilityTaskHandle ExecuteTask(AbilityTask _task)
        {
            if (m_IsRunning) EndedCurrentTask(); //先结束当前的任务。
            m_IsRunning = true;
            m_AnimState = animPlayer.Play(_task.animation);
            //Tip：本来AbilityTask就能做的事，就交给它自己做，把结果返回即可。
            // m_Tickables.Add(_task.hitbox);
            // m_Tickables.Add(_task.interval); 
            m_Tickables = _task.tickables;
            //TODO：先重置一下，之后再正常运行。因为是复用的实例，而不是根据资产类创建的新实例，所以就需要这样的步骤，对象池也是这样的。
            m_Tickables.ForEach(tickable => tickable.Reset());
            Debug.Log("ExecuteTask执行指定Task");
            return new AbilityTask.AbilityTaskHandle(_task, this); 
        } 

        public void EndedCurrentTask()
        {
            m_IsRunning = false;
            m_Timer = 0f; //重置计时器。
            m_Tickables.Clear();
            m_AnimState.Stop();
        }

    }

    [Serializable]
    public class AbilityTaskExecutor_New
    {
        public AnimatorAgent m_AnimPlayer;
        private AbilityTask_TimePoint tickable;
        private AnimationClipState m_AnimState;
        public AnimationClipState animState => m_AnimState;
        private float m_Timer;
        public bool isRunning;
        public float stopDuration = 0f;
        public ActorMovementComponent AMC;

        private Action nextAction;

        public void OnTick(float _deltaTime)
        {
            if (isRunning == false) return;
            m_Timer += _deltaTime;
            tickable?.OnTick(m_Timer);
            // Debug.Log($"正在运行，当前时间为{m_Timer}");
            if (tickable.isOver)
            {
                // m_AnimState.Stop(stopDuration);
                m_Timer = 0f;
                tickable = null;
                isRunning = false;
                // AMC.moveState.Play();
                nextAction?.Invoke();
            }
        }

        public void Execute(AbilityTask_Test _task)
        {
            m_AnimState = m_AnimPlayer.Play(_task.animation);
            tickable = _task.endedPoint;
            m_Timer = 0f;
            isRunning = true;
            stopDuration = _task.stopDuration;
        }

        public void Stop()
        {
            m_AnimState?.Stop(stopDuration);
        }

        public void Execute(AbilityTask_Test _task, Action _action)
        {
            Execute(_task);
            nextAction = _action;
        }
    }
 
    [Serializable]
    public class AbilityTask_Test 
    {
        public FadeAnimation_ForAbilityTask animation;
        public AbilityTask_TimePoint endedPoint;
        public float stopDuration;
    }


}