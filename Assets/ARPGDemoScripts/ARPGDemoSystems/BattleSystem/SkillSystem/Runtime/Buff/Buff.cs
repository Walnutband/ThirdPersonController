using System;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{

    /*Tip：BuffInfo封装的是BuffData，它是专门用于存储所要添加或移除的Buff信息的对象，从成员区别来看BuffInfo与BuffObj的区别，BuffInfo添加的Buff可能是已经有的，或者是没有的，
    如果已有的话就可以修改一些属性比如增加层数，增加持续时间等，没有的话就直接添加即可，如何使用BuffInfo取决于AddBuff和RemoveBuff的逻辑，BuffInfo只要把该有的数据存储好即可。
    BuffInfo还应该做到的是，
    */
    public class BuffInfo
    {
        public BuffData buffData;
        public IBuffCarrier source; //该Buff来源于谁，可以为null
        public IBuffCarrier target; //该Buff添加到谁身上，不能为null
        public int stack; //要添加的层数
        public float duration; //要增加的持续时间
        /*TODO：标记一次性Buff，在逻辑上就是AddBuff时在真正添加到Buff容器之前触发onAdded回调，然后检查该变量如果是true的话那么就直接结束AddBuff逻辑。
        比如喝血瓶喝蓝屏这种都是一次性恢复一个数值的资源量，没有持续效果（当然也可以有），而这样将这个动作的效果通过AddBuff实现。
        */
        public bool instant; 
        
        public BuffInfo(BuffData _buffData, IBuffCarrier _source, IBuffCarrier _target, int _stack, float _duration, bool _instant = false)
        {
            buffData = _buffData;
            source = _source;
            target = _target;
            stack = _stack;
            duration = _duration;
            instant = _instant;
        }
    }

    public class BuffObj
    {
        private BuffData m_Data;

        //该Buff的携带者
        private IBuffCarrier m_Carrier;
        public IBuffCarrier carrier => m_Carrier;

        public uint id => m_Data.id;
        public string name => m_Data.name;
        public BuffType type => m_Data.type;
        public string description => m_Data.description; //显示在UI界面上的描述。
        public Sprite icon => m_Data.icon;
        public int maxStack => m_Data.maxStack;
        public BuffTags tags => m_Data.tags;
        public int priority => m_Data.priority;
        public float tickTime => m_Data.tickTime; //Tick间隔时间，就是间隔多久作用一次。

        //搞清楚哪些是动态属性，哪些是静态属性
        private float m_Duration; //持续时间
        public float duration => m_Duration;
        /*Tip：直接将Duration作为计时器，减到0就应该被移除了，而且很符合Buff的逻辑含义——持续一定时间的Buff，时间到了就消失了。
        当然也可以延长Buff的持续时间，直接修改Duration即可。
        WOC，似乎搞错了，其实这个计时器应该是用来记录Tick的间隔时间。
        */
        private float m_TimeElapsed;
        public float timeElapsed => m_TimeElapsed;
        /*TODO：使用一个专门的标记变量来表示是否是永久Buff，按理来说也可以使利用数值关系来实现永久效果（比如设置巨大值之类的），这样往往会增加额外的逻辑，可能还不如直接定义这样
        一个变量好。*/
        // private bool m_IsPermanent;
        // public bool isPermanent => m_IsPermanent;
        private int m_Stack; //当前层数
        public int stack => m_Stack;

        //回调点，只是为了强调封装性，不让外部访问Buff内部的BuffData
        public BuffOnAdded onAdded => m_Data.onAdded;
        public void OnAdded(BuffObj _buffObj) => onAdded?.Invoke(_buffObj);
        public BuffOnRemoved onRemoved => m_Data.onRemoved;
        public void OnRemoved(BuffObj _buffObj) => onRemoved?.Invoke(_buffObj);
        public BuffOnTick onTick => m_Data.onTick;
        public void OnTick(BuffObj _buffObj) => onTick?.Invoke(_buffObj);
        public BuffOnCast onCast => m_Data.onCast;
        public void OnCast(BuffObj _buffObj) => onCast?.Invoke(_buffObj);
        public BuffOnHit onHit => m_Data.onHit;
        public void OnHit(BuffObj _buffObj, ref DamageInfo _damageInfo, IDefender _target) => onHit?.Invoke(_buffObj, ref _damageInfo, _target);
        public BuffOnBeHurt onBeHurt => m_Data.onBeHurt;
        public void OnBeHurt(BuffObj _buffObj, ref DamageInfo _damageInfo, IDefender _target) => onBeHurt?.Invoke(_buffObj, ref _damageInfo, _target);
        public BuffOnKill onKill => m_Data.onKill;
        public void OnKill(BuffObj _buffObj, DamageInfo _damageInfo, IDefender _target) => onKill?.Invoke(_buffObj, _damageInfo, _target);
        public BuffOnBeKilled onBeKilled => m_Data.onBeKilled;
        public void OnBeKilled(BuffObj _buffObj, DamageInfo _damageInfo, IDefender _target) => onBeKilled?.Invoke(_buffObj, _damageInfo, _target);
        // public BuffOnCast onCast => m_Data.onCast;

        //根据BuffData创建BuffObj
        public BuffObj(BuffData _data)
        {
            m_Data = _data;
            m_Duration = _data.duration;
            m_Stack = 1;
        }

        public bool Tick(float _deltaTime)
        {
            //非永久Buff才需要消耗持续时间
            if (!float.IsNaN(m_Duration))
            {//持续时间优先于Tick，如果当前帧的Buff已经结束了但是到达了Tick时刻也不会执行Tick逻辑（触发Tick回调）。
                m_Duration -= _deltaTime;
                if (m_Duration <= 0f)
                {//持续时间到了，应当被移除，但是Tick是在Buff内部执行，而并没有在Buff所属的个体中执行，所以就返回该信息，让个体处理。
                    return true; 
                }
            }
            m_TimeElapsed += _deltaTime;
            //TODO：当然也可以让TickTime是可变的。
            if (m_TimeElapsed >= m_Data.tickTime)
            {
                m_TimeElapsed -= m_Data.tickTime;
                m_Data.onTick?.Invoke(this);
            }
            return false;
        }

        /*Tip：延长和缩短Buff的持续时间，设置为方法而不是直接开放setter，确实是限制只能加减而不能直接设置为一个绝对数值，但我其实最初考虑的时候就是感觉显式的方法比起隐式的setter
        更具有明确的逻辑含义，当然不考虑这一点，其实也应该这样分为多个方法来执行对应的操作。*/
        public void IncreaseDuration(float _duration)
        {
            if (_duration <= 0.0f) return;
            m_Duration += _duration;
        }
        public void DecreaseDuration(float _duration)
        {
            if (_duration <= 0.0f) return;
            m_Duration -= _duration;
        }
        public void SetPermanent()
        {//设置为永久
            m_Duration = float.NaN;
        }
    }

    [Serializable]
    public class BuffData
    {
        [SerializeField] private uint m_ID;
        public uint id => m_ID;
        [SerializeField] private string m_Name;
        public string name => m_Name;
        [SerializeField] private BuffType m_Type;
        public BuffType type => m_Type;
        [SerializeField] private string m_Description;
        public string description => m_Description;
        //Buff通常会有图标，但是也不一定。
        [SerializeField] private Sprite m_Icon;
        public Sprite icon => m_Icon;
        //最大层数
        [SerializeField] private int m_MaxStack;
        public int maxStack => m_MaxStack;
        //标签，用于特殊用途
        [SerializeField] private BuffTags m_Tags;
        public BuffTags tags => m_Tags;

        //Buff优先级非常重要。
        [SerializeField] private int m_Priority;
        public int priority => m_Priority;
        /*对于持续Buff，不断Tick，tickTime就是每次Tick产生效果的间隔时间。*/
        [SerializeField] private float m_TickTime;
        public float tickTime => m_TickTime;
        [SerializeField] private float m_Duration;
        public float duration => m_Duration;

        /*Tip：Buff带来的基础属性值的变化，除此之外比较常见的是资源值变化，比如加血加蓝之类的，这些也可以不用Buff实现，但是我认为统一使用Buff实现会有利于游戏设计。*/
        [SerializeField] private ActorProperty m_PropertyPlus;
        public ActorProperty propertyPlus => m_PropertyPlus;
        [SerializeField] private ActorProperty m_PropertyTimes;
        public ActorProperty propertyTimes => m_PropertyTimes;


        #region Buff回调点
        /*TODO：由于BuffData是在编辑时确定的，而方法只能在运行时注册，所以大概会利用专门的资产作为键来与运行时的方法建立映射关系，然后在进入运行模式后的初始化过程中，
        就会有专门的逻辑将BuffData的回调点根据映射关系找到对应的方法、然后注册————似乎也可以在构造方法中自己查找，如果把那些回调方法设置为静态的话，这样一来结构性也更强。*/

        public BuffOnAdded onAdded;
        public string onAddedCallbackKey; /*感觉改成枚举更好，如果是字符串的话也应该定制编辑器、在检视器中直接从已有名称中选择、避免任意字符串。*/

        public BuffOnRemoved onRemoved;
        public BuffOnTick onTick;
        public BuffOnCast onCast;
        public BuffOnHit onHit;
        public BuffOnBeHurt onBeHurt;
        public BuffOnKill onKill;
        public BuffOnBeKilled onBeKilled;
        // public BuffOnCast onCast;


        #endregion

    }

    /*TODO：Buff回调点的签名，参数类型还需进一步考虑
    是否要将个体参数设置为一个接口比如IBuffCarrier？但是需要定义哪些接口成员？暂时直接定为个体类ActorObject算了。
    */
    // public delegate void BuffOnAdded(BuffObj buff, ActorObject actor);
    public delegate void BuffOnAdded(BuffObj buff); //BuffObj自己就含有所属个体的信息，由此获取所添加到的对象即可。
    public delegate void BuffOnRemoved(BuffObj buff);
    public delegate void BuffOnTick(BuffObj buff);
    public delegate void BuffOnCast(BuffObj buff);
    public delegate void BuffOnHit(BuffObj buff, ref DamageInfo damageInfo, IDefender target);
    public delegate void BuffOnBeHurt(BuffObj buff, ref DamageInfo damageInfo, IDefender target);
    public delegate void BuffOnKill(BuffObj buff, DamageInfo damageInfo, IDefender target);
    public delegate void BuffOnBeKilled(BuffObj buff, DamageInfo damageInfo, IDefender target);
    /**/
    // public delegate TimelineObj BuffOnCast(Buff buff, SkillObj skill, TimelineObj timeline);
}


namespace ARPGDemo.AbilitySystem
{
    public class GameplayEffectHandler : MonoBehaviour
    {
        
    }
}