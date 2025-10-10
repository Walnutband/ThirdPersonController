using System;
using System.Collections.Generic;
using ARPGDemo.SkillSystemtest;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.SkillSystemtest
{
    /*TODO：在技能编辑器中编辑的应该是一个ScriptableObject，而TimelineExecutor会引用一个该实例来获取自己要使用的Timeline、实际上应该不会直接引用，因为只会在初始化时使用，
    之后就应该直接释放内存，所以*/
    [AddComponentMenu("ARPGDemo/BattleSystem/TimelineExecutor")]
    public class TimelineExecutor : MonoBehaviour
    {
        /**/
        // [DisplayName("上下文")]
        [SerializeField] private TimelineContext m_Context;

        /*Tip：运行时没有集合的概念，也不需要，只需要具体的每一个Timeline*/
        // private uint m_TimelineSetID; //时间轴集合。

        /*Tip: 记录该执行器中所拥有的Timeline，同时记录正在运行的Timeline，而拥有的Timeline会在编辑器中编辑、运行时也可以动态增减，而运行的Timeline就不应该在编辑时编辑，应该
        只是在运行时动态变化。但在编辑器中测试时肯定需要在运行时实时查看当前在运行什么Timeline，这就是要在不参与序列化的情况下显示在编辑器中，有多种方式，可以参考笔记。*/
        // [SerializeField] private List<TimelineObj> m_AllTimelines = new List<TimelineObj>();
        //TODO：这里就是一个Timeline就只能有一个，不能有多个相同的Timeline，但是这并非游戏本身的限制，所以如果要支持能够同时运行多个相同的Timeline的话，应该怎么修改呢？
        private Dictionary<uint, TimelineObj> m_AllTimelines = new Dictionary<uint, TimelineObj>();

        // private List<TimelineObj> m_RunningTimelines = new List<TimelineObj>();
        // private LinkedList<TimelineObj> m_RunningTimelines = new LinkedList<TimelineObj>();
        // //键就是TimelineObj所引用的TimelineModel的ID，可能在使用跳转之类的功能时就需要立刻找到指定的TimelineObj。
        // private Dictionary<uint, TimelineObj> m_RunningTimelinesDic = new Dictionary<uint, TimelineObj>();

        private TimelineObj m_RunningTimeline;
        // public TimelineModel_SO m_Model;

        public InputAction input;
        public uint id;

        private void Start()
        {
            // m_RunningTimeline = 

            // m_RunningTimeline = new TimelineObj(m_Model);
        }

        private void OnEnable()
        {
            input.Enable();
            input.started += Attack;
        }

        private void OnDisable()
        {
            input.Disable();
            input.started -= Attack;
        }

        //直接参与生命周期，
        private void Update()
        {

            float deltaTime = Time.deltaTime; //两帧间隔时间。

            m_RunningTimeline?.Tick(deltaTime);

            // foreach (var node in m_RunningTimelines)
            // {
            //     if (node.Tick(deltaTime))
            //     {
            //         m_RunningTimelines.Remove(node);
            //     }
            // }

            // m_RunningTimelines.ForEach(timeline =>.
            // {
            //     // timeline.OnUpdate(deltaTime);
            //     if (timeline.Tick(deltaTime))
            //     {
            //         m_RunningTimelines.Remove(timeline);
            //     }
            // });


        }

        private void Attack(InputAction.CallbackContext context)
        {
            m_RunningTimeline = TimelineFactory.Instance.GetTimeline(id, m_Context);
            // SwitchTimeline(id);
        }

        public bool SwitchTimeline(uint _id)
        {
            if (m_AllTimelines.TryGetValue(_id, out TimelineObj timeline) == false)
                return false;
            else
            {
                //甚至还可以通过实例直接判断
                if (m_RunningTimeline.id == _id)
                    return false;

                // m_RunningTimeline.GOTO(m_RunningTimeline.beginTime); //回到开始时刻
                /*TODO：可能还可以搞出类似于负时刻、切换回来继续之前的进度，之类的功能*/
                m_RunningTimeline?.GOTOBeginning();
                m_RunningTimeline = timeline;
                m_RunningTimeline?.GOTOBeginning();
                return true;
            }
        }
    }

}