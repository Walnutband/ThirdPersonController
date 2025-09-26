using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///注意：和unity的timeline不是一个东西，这个概念出来的时候unity都还没出来。
    ///这是一段预约的事情的记录，也就是当timelineObj产生之后，就会开始计时，并且在每个“关键帧”（类似flash的概念）做事情。
    ///所有的道具使用效果、技能效果都可以抽象为一个timeline，由timeline来“指导”后续的事件发生。
    ///</summary>
    public class TimelineObj
    {
        ///<summary>
        ///Timeline的基础信息
        ///</summary>
        public TimelineModel model;


        ///<summary>
        ///Timeline的焦点对象也就是创建timeline的负责人，比如技能产生的timeline，就是技能的施法者
        ///</summary>
        public GameObject caster;

        ///<summary>
        ///倍速，1=100%，0.1=10%是最小值
        ///</summary>
        public float timeScale
        {
            get
            {
                return _timeScale;
            }
            set
            {
                _timeScale = Mathf.Max(0.100f, value);
            }
        }
        private float _timeScale = 1.00f;

        ///<summary>
        ///Timeline的创建参数，如果是一个技能，这就是一个skillObj
        ///</summary>
        public object param;

        ///<summary>
        ///Timeline已经运行了多少秒了（每个Timeline都有自己独立的时刻表）
        ///</summary>
        public float timeElapsed = 0;



        ///<summary>
        ///一些重要的逻辑参数，是根据游戏机制在程序层提供的，这里目前需要的是
        ///[faceDegree] 发生时如果有caster，则caster企图面向的角度（主动）。
        ///[moveDegree] 发生时如果有caster，则caster企图移动向的角度（主动）。
        ///</summary>
        public Dictionary<string, object> values;

        public TimelineObj(TimelineModel model, GameObject caster, object param)
        {
            this.model = model;
            this.caster = caster;
            this.values = new Dictionary<string, object>();
            this._timeScale = 1.00f;
            if (caster)
            {
                ChaState cs = caster.GetComponent<ChaState>();
                if (cs)
                {
                    this.values.Add("faceDegree", cs.faceDegree);
                    this.values.Add("moveDegree", cs.moveDegree);
                }
                this._timeScale = cs.actionSpeed;
            }
            this.param = param;
        }

        ///<summary>
        ///尝试从values获得某个值
        ///<param name="key">这个值的key{faceDegree, moveDegree}</param>
        ///<return>取出对应的值，如果不存在就是null</return>
        ///</summary>
        public object GetValue(string key)
        {
            if (values.ContainsKey(key) == false) return null;
            return values[key];
        }
    }

    ///<summary>
    ///策划预先填表制作的，就是这个东西，同样她也是被clone到obj当中去的
    ///</summary>
    public struct TimelineModel
    {
        public string id;

        ///<summary>
        ///Timeline运行多久之后发生，单位：秒
        ///</summary>
        public TimelineNode[] nodes;

        ///<summary>
        ///Timeline一共多长时间（到时间了就丢掉了），单位秒
        ///</summary>
        public float duration;

        ///<summary>
        ///如果有caster，并且caster处于蓄力状态，则可能会经历跳转点
        ///</summary>
        public TimelineGoTo chargeGoBack;

        public TimelineModel(string id, TimelineNode[] nodes, float duration, TimelineGoTo chargeGoBack)
        {
            this.id = id;
            this.nodes = nodes;
            this.duration = duration;
            this.chargeGoBack = chargeGoBack;
        }
    }

    ///<summary>
    ///Timeline每一个节点上要发生的事情
    ///</summary>
    public struct TimelineNode
    {
        //TODO：这里应当命名为startTime开始时刻，甚至对于一些数据流比如AnimationClip、AudioClip之类的还可以使用endTime，反正都不适合命名为timeElapsed
        ///<summary>
        ///Timeline运行多久之后发生，单位：秒
        ///</summary>
        public float timeElapsed; 

        /*TODO：这里的Timeline比较简单，只是触发一个委托而已，而且没有任何约束，除了委托对于参数的基本约束以外————而实际上
        对于一个成熟的Timeline系统，应该会有更多已经写好的Timeline节点（片段），每个节点除了对Timeline整体开放的一些共同接口以外，
        会定义自己的数据和逻辑以实现某个固定的功能，非常明确，不会像这里还有object[]这样完全毫无约束的参数类型。*/

        ///<summary>
        ///要执行的脚本函数
        ///</summary>
        public TimelineEvent doEvent;

        ///<summary>
        ///要执行的函数的参数
        ///</summary>
        public object[] eveParams { get; }

        public TimelineNode(float time, string doEve, params object[] eveArgs)
        {
            this.timeElapsed = time;
            this.doEvent = DesignerScripts.Timeline.functions[doEve];
            this.eveParams = eveArgs;
        }
    }

    ///<summary>
    ///Timeline的一个跳转点信息
    ///</summary>
    public struct TimelineGoTo
    {
        ///<summary>
        ///自身处于时间点
        ///</summary>
        public float atDuration; //在Timeline到达该时间点时就要执行跳转

        ///<summary>
        ///跳转到时间点
        ///</summary>
        public float gotoDuration;

        public TimelineGoTo(float atDuration, float gotoDuration)
        {
            this.atDuration = atDuration;
            this.gotoDuration = gotoDuration;
        }

        public static TimelineGoTo Null = new TimelineGoTo(float.MaxValue, float.MaxValue);
    }

    //因为是委托，所以执行的逻辑是由注册的回调方法决定的，传入的参数就是让回调方法知道是由哪个TimelineObj触发的，以及该TimeObj能够提供什么参数来使用。
    /*TODO：这里的args就是为了可以传入任意数量、任意类型的参数，这就要求为TimelineEvent委托注册回调方法的时候按照具体的回调方法的逻辑来传入它需要的参数，
    而且必须手动对齐，不然大概率效果不及预期或者更可能的是直接出错，我其实一直很怀疑这种参数传递方式是不是有问题，但现在没有足够经验也不知道有没有其他方式可以达到同样的目的*/
    public delegate void TimelineEvent(TimelineObj timeline, params object[] args);
}