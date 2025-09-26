using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///管理游戏中所有的timeline（通过巧妙的封装，使其执行逻辑一致，只是各自的执行内容不同）
    ///</summary>
    public class TimelineManager : MonoBehaviour
    {
        private List<TimelineObj> timelines = new List<TimelineObj>();

        private void FixedUpdate()
        {
            //没有操作对象，意味着没有被分配任务，就提前结束。
            if (this.timelines.Count <= 0) return;

            int idx = 0; //充当索引的作用
            while (idx < this.timelines.Count)
            {
                float wasTimeElapsed = timelines[idx].timeElapsed;//上一次的时刻
                timelines[idx].timeElapsed += Time.fixedDeltaTime * timelines[idx].timeScale; //可以一定范围内自由放缩时间流速

                //判断有没有返回点
                if ( //这里是判断是否在相邻两次之间，并且从逻辑上推断来看，必然会经过这个情况，不会出现意外跳过的情况，因为始终是相邻连续的，如果是离散的话就可能出现跳过的情况。
                    timelines[idx].model.chargeGoBack.atDuration < timelines[idx].timeElapsed &&
                    timelines[idx].model.chargeGoBack.atDuration >= wasTimeElapsed
                )
                {/*Tip：这里是把蓄力就通过循环动画的方式来实现，但其实不一定，还有常见的做法是蓄力时减慢动画播放速度，松开时或者满蓄时恢复到正常速度。*/
                    if (timelines[idx].caster)
                    {
                        ChaState cs = timelines[idx].caster.GetComponent<ChaState>();
                        if (cs.charging == true)
                        {
                            timelines[idx].timeElapsed = timelines[idx].model.chargeGoBack.gotoDuration;
                            continue;
                        }
                    }
                }
                //执行时间点内的事情
                for (int i = 0; i < timelines[idx].model.nodes.Length; i++)
                {
                    if (
                        timelines[idx].model.nodes[i].timeElapsed < timelines[idx].timeElapsed &&
                        timelines[idx].model.nodes[i].timeElapsed >= wasTimeElapsed
                    )
                    { //对于delegate，直接调用和使用Invoke调用本质完全相同，只是Invoke可以使用?.方便判空，不过从逻辑上Invoke更加凸显出这是一个委托，而不是一个普通方法，更加增强代码的逻辑性。
                        timelines[idx].model.nodes[i].doEvent(
                            timelines[idx],
                            timelines[idx].model.nodes[i].eveParams
                        );
                    }
                }

                //判断timeline是否终结
                if (timelines[idx].model.duration <= timelines[idx].timeElapsed)
                {
                    timelines.RemoveAt(idx); //注意细节，移除会带动后面的元素向前移动一位填补空缺，所以此时idx没有加1，因为RemoveAt之后指向的就是原本下一个元素
                }
                else
                {
                    idx++;
                }
            }
        }

        ///<summary>
        ///添加一个timeline
        ///<param name="timelineModel">要添加的timeline的model</param>
        ///<param name="caster">timeline的负责人</param>
        ///<param name="source">添加的源数据，比如技能就是skillObj</param>
        ///</summary>
        public void AddTimeline(TimelineModel timelineModel, GameObject caster, object source)
        {
            if (CasterHasTimeline(caster) == true) return;
            this.timelines.Add(new TimelineObj(timelineModel, caster, source));
        }

        ///<summary>
        ///添加一个timeline
        ///<param name="timelineModel">要添加的timeline</param>
        ///</summary>
        public void AddTimeline(TimelineObj timeline)
        {
            if (timeline.caster != null && CasterHasTimeline(timeline.caster) == true) return;
            this.timelines.Add(timeline);
        }

        public bool CasterHasTimeline(GameObject caster)
        {
            for (var i = 0; i < timelines.Count; i++)
            {
                if (timelines[i].caster == caster) return true;
            }
            return false;
        }
    }
}