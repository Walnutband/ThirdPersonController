using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    //该监视器应该是给每个个体专用的。
    public class CollisionDetectionMonitor : MonoBehaviour
    {
        /*TODO：这样的计时器还有所不足，很明显至少还需要区分是哪段攻击的命中，这样的逻辑应该要放在角色控制器中才能处理，而不只是在该监视器中。*/
        /*BugFix：我艹了，之前把Timer设置成struct，结果在字典中获取时其实获取的是副本，调用其FixedTick方法当然也是增长的其副本的timerCounter值，所以原本的Timer的timeCounter始终
        为0，也就是始终处于字典中、处于命中CD。*/
        [Serializable]
        public class Timer
        {
            //TODO：至于是否采用Collider还是GameObject还有待考虑，不过暂时认为区别不大。而且Collider是class而不是struct，所以作为参数传递时都是同一个实例。
            private Collider target;
            private float timeInterval;
            private float timeCounter;

            public bool FixedTick()
            {
                // Debug.Log($"timeCounter = {timeCounter}, Time.fixedDeltaTime = {Time.fixedDeltaTime}");
                timeCounter += Time.fixedDeltaTime;
                // Debug.Log($"timeCounter = {timeCounter}, timerInterval = {timeInterval}");
                if (timeCounter >= timeInterval) return true;
                else return false;
            }

            /*Tip：在设计上，当达到或超过时间间隔之后，就会从容器中移除，而在触发时首先会查找容器是否存在对应的Timer，如果没有就说明可以攻击，如果有的话就处于命中CD阶段。*/
            // public bool CanTrigger()
            // {
            //     if (timeCounter >= timeInterval) return true;
            //     else return false;
            // }

            public Timer(Collider _target, float _timeInterval)
            {
                target = _target;
                timeInterval = _timeInterval;
                timeCounter = 0f;
            }

        }

        public event Action<Collider> triggerEnter; //event关键字避免直接用“=”覆盖。
        private Collider m_UsedCollider; //用来检测的Collider，注意与被检测的各个Collider相区别。
        public Collider usedCollider => m_UsedCollider;

        /*TODO：每一个目标都要准备一个计时器，为了不同目标的独立计时、互不影响（当然也可以特意设计为相互影响）
        ————而且不难想到一种设计，不同目标的timeInterval还不一定相同，那么就要扩展Timer成员，而且会专门编辑静态数据，到时候直接通过检测到的目标的ID来匹配、找到对应的Timer，然后
        添加到容器中，这样的话可能采用字典会更加合适，当然即使不这样可能也更适合采用字典，因为查找和删除的时间复杂度都可以从O(n)变到O(1)，但是字典不能被直接序列化。*/
        // [SerializeField] private List<Timer> timer;
        private Dictionary<int, Timer> Timers = new Dictionary<int, Timer>(); //int是实例ID，因为Unity底层提供了这样一个ID，可以直接使用，也可以考虑另外的自定义ID，反正作用都差不多。

        private void OnTriggerEnter(Collider other)
        {
            // Debug.Log($"collider实例ID: {other.GetInstanceID()}");
            // if (Timers.ContainsKey(other.GetInstanceID())) return;
            if (Timers.ContainsKey(other.GetInstanceID())) return;
            triggerEnter?.Invoke(other);
            // Timers.Add(other.gameObject.GetInstanceID(), new Timer(other, 1f));
            Timers.Add(other.GetInstanceID(), new Timer(other, 1f));
        }

        private void Awake()
        {
            m_UsedCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            m_UsedCollider.isTrigger = true; //保证是触发器，这就是通常的设计。
        }

        private void FixedUpdate()
        {
            // int[] toRemove = new int[Timers.Keys.Count]; //注意tm的数组会将所有元素初始化为0，而且又不会自动扩容，容易出现各种bug，还是用List方便快捷。
            List<int> toRemove = new List<int>();
            foreach (int id in Timers.Keys)
            {
                // Debug.Log($"实例ID：{id}");
                // if (Timers[id].FixedTick()) Timers.Remove(id); //到点了就移除，说明已经过了命中CD、可以攻击了。
                if (Timers[id].FixedTick())
                {
                    // Debug.Log("TickTrue");
                    // toRemove.Append(id); //Append会返回一个新序列（不改变原序列），并且是惰性执行，这带来了一系列问题。
                    toRemove.Add(id); //不要在循环中修改所遍历的集合！！！
                }
            }
            foreach (int id in toRemove)
            {
                // Debug.Log("移除" + id);
                Timers.Remove(id);
            }
        }

        /*TODO：至于启用和禁用碰撞体到底属不属于监视器的职责，我认为是不属于的，但是这暂时只是个求职Demo而已。*/
        public void EnableCollider()
        {
            m_UsedCollider.enabled = true;
        }

        public void DisableCollider()
        {
            m_UsedCollider.enabled = false;
        }

        public void ClearTimers()
        {
            Timers.Clear();
        }
    }


    
}