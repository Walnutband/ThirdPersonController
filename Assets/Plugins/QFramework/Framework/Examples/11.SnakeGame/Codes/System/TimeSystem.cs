using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace SnakeGame
{
    public interface ITimeSystem : ISystem
    {
        float CurrentSeconds { get; }
        DelayTask AddDelayTask(float seconds, Action onDelayFinish, bool isContinue = false);
    }
    public class TimeSystem : AbstractSystem, ITimeSystem
    {
        private float mCurrentSeconds = 0; //当前经过时间，就是计时器

        private Queue<DelayTask> mTaskPool = new Queue<DelayTask>(); //任务池
        private LinkedList<DelayTask> mDelayTasks = new LinkedList<DelayTask>();//LinkedList是双向链表

        protected override void OnInit() => CommonMono.AddUpdateAction(OnUpdate);

        float ITimeSystem.CurrentSeconds => mCurrentSeconds;
        /// <summary>
        /// 添加延迟任务
        /// </summary>
        /// <param name="seconds">延迟时间</param>
        /// <param name="onDelayFinish">延迟结束回调方法</param>
        /// <param name="isContinue">是否循环</param>
        /// <returns></returns>
        DelayTask ITimeSystem.AddDelayTask(float seconds, Action onDelayFinish, bool isContinue)
        {
            //从 mTaskPool 任务池中尝试取出一个已存在的任务对象（池化机制），以避免频繁创建新对象来节省性能。
            DelayTask delayTask = mTaskPool.Count > 0 ? mTaskPool.Dequeue() : new DelayTask();
            delayTask.Init(seconds, onDelayFinish, isContinue);
            mDelayTasks.AddLast(delayTask);
            return delayTask;
        }

        private void OnUpdate()
        {
            mCurrentSeconds += Time.deltaTime; //计时
            if (mDelayTasks.Count == 0) return; //没有延迟任务
            var currentNode = mDelayTasks.First; //
            while (currentNode != null)
            {
                var nextNode = currentNode.Next;
                if (currentNode.Value.UpdateTasks(mCurrentSeconds))
                {//任务结束之后，入池并移出任务链表
                    mTaskPool.Enqueue(currentNode.Value);
                    mDelayTasks.Remove(currentNode);
                }
                currentNode = nextNode;
            }
        }
    }
    public class DelayTask
    {
        private float Seconds; //延迟时间
        private Action OnFinish; //结束时回调
        private float StartTime; //开始时间
        private float FinishTime; //结束时间
        private bool mIsStart; //是否已开始
        private bool mIsLoop; //是否循环

        public void Init(float seconds, Action onFinish, bool isLoop)
        {
            Seconds = seconds;
            OnFinish = onFinish;
            mIsStart = false;
            mIsLoop = isLoop;
        }
        public void StopTask() => mIsLoop = false;

        public bool UpdateTasks(float currentSeconds)
        {
            if (!mIsStart)
            {
                mIsStart = true;
                StartTime = currentSeconds; //从现在开始
                FinishTime = StartTime + Seconds;
            }
            else if (currentSeconds >= FinishTime)
            {
                //对Action来说，这和使用Invoke是完全等效的，只是在逻辑含义上有所不同，比如后者可以表明这是个委托，还可以配合?.来避免空引用
                OnFinish();
                if (mIsLoop) //循环就必然返回false
                {
                    mIsStart = false;
                    return false;
                }
                OnFinish = null;
                return true;
            }
            return false;
        }
    }
}