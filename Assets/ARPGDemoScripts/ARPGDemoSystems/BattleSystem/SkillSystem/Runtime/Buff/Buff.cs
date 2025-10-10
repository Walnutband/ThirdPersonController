
using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{

    public class BuffObj
    {
        private BuffData m_Data;

        public uint id => m_Data.id;
        public string name => m_Data.name;
        public BuffType type => m_Data.type;
        public string description => m_Data.description;
        public Sprite icon => m_Data.icon;
        public int maxStack => m_Data.maxStack;
        public BuffTags tags => m_Data.tags;
        public int priority => m_Data.priority;
        public float tickTime => m_Data.tickTime;

        //搞清楚哪些是动态属性，哪些是静态属性
        private float m_Duration; //持续时间
        public float duration => m_Duration;
        private int m_Stack; //层数
        public int stack => m_Stack;

        //回调点，只是为了强调封装性，不让外部访问Buff内部的BuffData
        public BuffOnAdded onAdded => m_Data.onAdded;
        public BuffOnRemoved onRemoved => m_Data.onRemoved;
        public BuffOnTick onTick => m_Data.onTick;
        public BuffOnHit onHit => m_Data.onHit;
        public BuffOnBeHurt onBeHurt => m_Data.onBeHurt;
        public BuffOnKill onKill => m_Data.onKill;
        public BuffOnBeKilled onBeKilled => m_Data.onBeKilled;
        // public BuffOnCast onCast => m_Data.onCast;

        public BuffObj(BuffData _data)
        {
            m_Data = _data;
            m_Duration = _data.duration;
            m_Stack = 1;
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
        public BuffOnHit onHit;
        public BuffOnBeHurt onBeHurt;
        public BuffOnKill onKill;
        public BuffOnBeKilled onBeKilled;
        // public BuffOnCast onCast;


        #endregion

    }

    /*TODO：Buff回调点的签名，参数类型还需进一步考虑*/
    public delegate void BuffOnAdded(BuffObj buff);
    public delegate void BuffOnRemoved(BuffObj buff);
    public delegate void BuffOnTick(BuffObj buff);
    public delegate void BuffOnHit(BuffObj buff, ref DamageInfo damageInfo, GameObject target);
    public delegate void BuffOnBeHurt(BuffObj buff, ref DamageInfo damageInfo, GameObject attacker);
    public delegate void BuffOnKill(BuffObj buff, DamageInfo damageInfo, GameObject target);
    public delegate void BuffOnBeKilled(BuffObj buff, DamageInfo damageInfo, GameObject attacker);
    /**/
    // public delegate TimelineObj BuffOnCast(Buff buff, SkillObj skill, TimelineObj timeline);
}