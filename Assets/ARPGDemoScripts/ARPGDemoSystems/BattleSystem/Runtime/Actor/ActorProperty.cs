using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*Tip：这里是运行时属性，属于动态数据，而编辑时属性即静态数据应该从外部读取比如从Excel读取，但是我看了一下Excel导入的相关项目还有luban这种商业级的开源项目，发现根本不是我现在
    能够开发的，完全不是那种用纯C#脚本就可以编写出来的插件，而是涉及到很多超出引擎开发层之外的知识，其实对于个人开发来说确实不是必需品。
    将编辑时数据设置为ScriptableObject类型，这样方便在检视器中编辑，运行时就是从引用的SO文件读取属性数据，不过还要注意存档读档的问题，因为这种属性数据其实属于存档数据，按理来说
    应该是从存档文件中读取，只有在创建新档时才会从静态数据文件中读取数据。当然还有大量不属于存档数据的静态数据，这就是必然从静态数据文件中读取了。*/
    /*TODO：由于玩家角色的属性类型可能与怪物的属性类型有所不同，可能需要将共有的那些属性定义在一个接口中，然后让各自的属性类实现该接口的成员，并且定义自己独有的一些属性。*/
    // public class ActorProperty : MonoBehaviour
    [Serializable]
    public class ActorProperty
    {
        // /*Tip：这时候就体现出嵌套结构的好处了，我之前的目的就只是纯粹将其分开，如果分为两个类的话又会导致很多副作用，这样嵌套之后就能够很好的解决这个问题*/
        // [Serializable]
        // protected struct ActorAbility 
        // {
        //     private ActorProperty m_Property;
        //     private ActorAbility(ActorProperty _property) : this() => m_Property = _property;

        //     public int m_Vigor; //生命力
        //     public int m_Mind; //集中力
        //     public int m_Endurance; //耐力
        //     public int m_Strength; //力气
        //     public int m_Dexterity; //灵巧
        //     public int m_Intelligence; //智力
        //     public int m_Faith; //信仰
        //     public int m_Arcane; //感应

        //     public int m_HPGrowRate; //血量成长系数
        //     public int m_MPGrowRate; //蓝量成长系数
        //     public int m_SPGrowRate; //体力成长系数

        //     //能力成长影响
        //     public void VigorChanged(int _value)
        //     {//TODO:这里可能还需要用一个容器，存储关于一个能力值会影响哪些属性值，然后遍历容器，但是这样可能就不知道使用哪个系数了，所以可能需要考虑将角色属性进行一层封装，而不是直接使用int、float
        //         m_Vigor = _value;
        //         m_Property.hp = _value * m_HPGrowRate;
        //     }

        //     public void MindChanged(int _value)
        //     {
        //         m_Mind = _value;
        //         m_Property.mp = _value * m_MPGrowRate;
        //     }

        //     public void EnduranceChanged(int _value)
        //     {
        //         m_Endurance = _value;
        //         m_Property.sp = _value * m_SPGrowRate;
        //     }
        // }

        // #region 个体数据
        /*TODO: 这里应该还有问题，这种静态数据，只会在创建新档时读取，然后就是从存档文件中读取，只是单纯地读取一下数据，并不需要维持数据文件对象也就是这里的m_Data，所以
        在实际运行时应该是使用Addressable或者AssetBundle之类的运行时资源管理系统，来加载指定的文件，然后读取其数据其实就是将其字段值逐个赋值给该类中的对应字段。*/
        //该字段用于编辑时指定对应的SO文件，然后在运行时读取数据
        // [SerializeField] private SO_ActorProperty m_Data;

        /*TODO：我想的是，Actor表示一切个体，那么ActorProperty应该是个体共同的一些属性，而具体的个体主要是三类，角色、怪物、NPC*/

        // [Header("角色等级")]
        // //TODO：由于这些值都具有潜在的范围（至少通常是非负整数），所以是否应该专门用一个RangeInt类型来定义呢？
        // [SerializeField] private int m_Level;
        // public int level { get => m_Level; set => m_Level = value; }

        // [Header("上限属性值")] //因为这些属性值都会实时变化地参与逻辑，但同时受到上限的影响（其实还有下限，但其实都是0），所以需要单独存储
        // [SerializeField] private float m_MaxHP;
        // public int maxHP { get => (int)m_MaxHP; set => m_MaxHP = value; }
        // [SerializeField] private float m_MaxMP;
        // public int maxMP { get => (int)m_MaxMP; set => m_MaxMP = value; }
        // [SerializeField] private float m_MaxSP;
        // public int maxSP { get => (int)m_MaxSP; set => m_MaxSP = value; }
        // [SerializeField] private int m_MaxEquipLoad;
        // public int maxEquipLoad { get => m_MaxEquipLoad; set => m_MaxEquipLoad = value; }
        // //韧性也没有上限，但是在游戏中实际作用时会经过一系列计算，因为有基础韧性和出手韧性这个机制，大概可以认为这里的就是基础韧性。
        // // [SerializeField] private int m_MaxToughness;
        // // public int maxToughness { get => m_MaxToughness; set => m_MaxToughness = value; }
        // //观察力确实没有上限。
        // // [SerializeField] private int m_MaxDiscovery;
        // // public int maxDiscovery { get => m_MaxDiscovery; set => m_MaxDiscovery = value; }

        //TODO：使用float还是double可能需要考虑，但是float的表示范围似乎已经是巨大了。因为在计算时可能会出现小数，但实际使用又应该是整数，所d以将字段设置为float，而对外的属性设置为int。
        //属性值
        //（硬属性？）血条、蓝条、体力条，在战斗中实时变化，与个体的其他属性具有显著差异。

        [Header("基础数据")]
        //HP、MP和SP都属于具有资源性的属性，也就是在游戏中可以通过多种途径实现增减（值变化）
        // [SerializeField] private float m_HP; //血量（Health Points）
        // 感觉直接使用int计算也没啥问题，完全不影响游戏性。
        //这里的HP、MP、SP都是指的上限值。
        [SerializeField] private int m_HP; //血量（Health Points） 
        public int hp { get => m_HP; set => m_HP = value; }
        [SerializeField] private int m_MP; //蓝量（Mana Points，其实是资源量，蓝量只是其中一种表现形式）
        public int mp { get => m_MP; set => m_MP = value; }
        [SerializeField] private int m_SP; //体力（Stamina Points）
        public int sp { get => m_SP; set => m_SP = value; }

        [SerializeField] private int m_Poison;
        public int poison { get => m_Poison; set => m_Poison = value; }
        [SerializeField] private int m_Bleed;
        public int bleed { get => m_Bleed; set => m_Bleed = value; }

        //（软属性？）
        // [SerializeField] private int m_EquipLoad; //装备重量，Weight确实是重量，但是Load表示载重，可能更加贴切。
        // public int equipLoad { get => m_EquipLoad; set { m_EquipLoad = value; } }
        // [SerializeField] private int m_Toughness; //强韧度，韧性，主要是对于怪物而言，可以理解为“对冲击力的抵抗能力”（法环这里的数值公式比较奇怪，需要进行恰当修改）
        // public int toughness { get => m_Toughness; set { m_Toughness = value; } }

        //这是失衡条，在法环中就是怪物特有的属性，也是通常被认为的“韧性”，但玩家角色并没有失衡条，不过确实有会弹反的怪物可以在玩家攻击时触发弹反、然后处决玩家
        // [SerializeField] private int m_Poise;
        // public int poise { get => m_Poise; set { m_Poise = value; } }
        // [SerializeField] private int m_Discovery; //观察力，幸运值（就是掉落物品的概率，不过还可以尝试加入其他作用）
        // public int discovery { get => m_Discovery; set { m_Discovery = value; } }

        [Header("攻防属性")]
        /*TODO：暂定二分为物理和魔法，其实应该有更多类型的属性伤害。*/
        [SerializeField] private float m_PhysicsAttack; //物理攻击力
        public float physicsAttack { get => m_PhysicsAttack; set { m_PhysicsAttack = value; } }
        [SerializeField] private float m_PoisonEffect; //中毒效果
        public float poisonEffect { get => m_PoisonEffect; set { m_PoisonEffect = value; } }
        [SerializeField] private float m_BleedEffect; //流血效果
        public float bleedEffect { get => m_BleedEffect; set { m_BleedEffect = value; } }
        [SerializeField] private float m_PhysicsResist; //物抗
        public float physicsResist { get => m_PhysicsResist; set { m_PhysicsResist = value; } }
        [SerializeField] private float m_PoisonResist; //毒抗
        public float poisonResist { get => m_PoisonResist; set { m_PoisonResist = value; } }
        [SerializeField] private float m_BleedResist; //流血抗
        public float bleedResist { get => m_BleedResist; set { m_BleedResist = value; } }
        // [SerializeField] private int m_MagicAttack; //魔法攻击力
        // public int magicAttack { get => m_MagicAttack; set { m_MagicAttack = value; } }

        // //能力值
        // /*成长属性，其实在游戏中叫做能力值，作为成长系统的核心，这里就将其专门封装为一个类来集中处理，并非必须，只是增强结构性，不过具体的好坏程度，还需要后面再看。
        // 这里的数据首先是初始职业，这肯定要从数据文件中读取，然后就是注册方法到UI控件的事件中，以便进行升级等等操作。*/
        // [Header("能力值")]
        // [SerializeField] protected ActorAbility m_Abilities;

        // #endregion


        // #region 角色成长
        // public void Chan()
        // {
        //     // m_Vigor
        // }
        // #endregion

        public static ActorProperty operator +(ActorProperty self, ActorProperty incre)
        {
            ActorProperty result = new ActorProperty();
            result.hp = self.hp + incre.hp;
            result.mp = self.mp + incre.mp;
            result.sp = self.sp + incre.sp;
            result.poison = self.poison + incre.poison;
            result.bleed = self.bleed + incre.bleed;
            result.physicsAttack = self.physicsAttack + incre.physicsAttack;
            result.poisonEffect = self.poisonEffect + incre.poisonEffect;
            result.bleedEffect = self.bleedEffect + incre.bleedEffect;
            result.physicsResist = self.physicsResist + incre.physicsResist;
            result.poisonResist = self.poisonResist + incre.poisonResist;
            result.bleedResist = self.bleedResist + incre.bleedResist;
            return result;
        }

    }

    /*Tip：一定要分清楚资源Resource和属性Property，这两者所涉及到的逻辑都不一样。
    Resource本身可以当做是Property的子集，也就是Resource来自于Property。*/
    /*TODO：ActorResource不仅代表个体自身当前的资源量，还承担资源转移的职责。*/
    [Serializable]
    public class ActorResource
    {
        private int m_HP;
        public int hp { get => m_HP; private set => m_HP = value; }
        private int m_MaxHP;
        //可能也会有以MaxHP作为条件的技能、动作之类的
        public int maxHP { get => m_MaxHP; private set => m_MaxHP = value; }
        public event Action HPMinEvent;
        public event Action HPMaxEvent;

        //这些资源也可以照样设置事件，只是没有HP那么常见。
        private int m_MP;
        public int mp { get => m_MP; private set => m_MP = value; }
        private int m_MaxMP;

        private int m_SP;
        public int sp { get => m_SP; set => m_SP = value; }
        private int m_MaxSP;

        /*异常值计算，就是读条，从0到临界点，触发效果，持续效果如中毒就是从临界点再逐渐降到0，如果是即时效果如出血那就是直接变为0*/
        private int m_Poison;
        public int poison { get => m_Poison; private set => m_Poison = value; }
        private int m_PoisonThreshold;
        public event Action PoisonMinEvent;
        public event Action PoisonMaxEvent;
        // private ResInt m_Poison;
        // public ResInt poison { get => m_Poison; }

        //出血异常值。
        private int m_Bleed;
        public int bleed { get => m_Bleed; private set => m_Bleed = value; }
        private int m_BleedThreshold;
        public event Action BleedMinEvent;
        public event Action BleedMaxEvent;
        // private ResInt m_Bleed;
        // public ResInt bleed {get => m_Bleed; }

        // [SerializeField] private int m_Poise;
        // public int poise { get => m_Poise; set => m_Poise = value; }
        // [SerializeField] private int m_MaxPoise;




        //构造时当然就初始化为最大值了。
        public ActorResource(ActorProperty _property)
        {
            if (_property == null) return; //全部默认值，就是为0，正常情况应该保证不可能出现这种情况。
            m_HP = m_MaxHP = _property.hp;
            m_MP = m_MaxMP = _property.mp;
            m_SP = m_MaxSP = _property.sp;
            m_Poison = 0;
            m_PoisonThreshold = _property.poison;
            m_Bleed = 0;
            m_BleedThreshold = _property.bleed;
            // m_Poison = new ResInt(0, _property.poison);
            // m_Bleed = new ResInt(0, _property.bleed);
        }

        public ActorResource(int hp = 0, int mp = 0, int sp = 0)
        {
            m_HP = hp;
            m_MP = mp;
            m_SP = sp;
        }

        public static ActorResource EffectResource(int _poison, int _bleed)
        {
            ActorResource res = new ActorResource();
            res.m_Poison = _poison;
            res.m_Bleed = _bleed;
            return res;
        }

        public static ActorResource PhysicsResource(int _physics)
        {
            ActorResource res = new ActorResource();
            res.hp = -1 * _physics;
            return res;
        }

        public static ActorResource DamageResource(Damage _damage)
        {
            ActorResource res = new ActorResource();
            res.hp = (int)(-1 * _damage.physics);
            res.poison = (int)_damage.poison;
            res.bleed = (int)_damage.bleed;
            return res;
        }

        //TODO：有一说一，重载运算符的逻辑含义可能不如直接编写显式方法来得准确。
        public static ActorResource operator +(ActorResource self, ActorResource resource)
        {
            ActorResource result = new ActorResource(null);
            result.hp = self.hp + resource.hp;
            result.mp = self.mp + resource.mp;
            result.sp = self.sp + resource.sp;
            // result.poise = self.poise + resource.poise;
            result.m_Poison = self.m_Poison + resource.m_Poison;
            result.m_Bleed = self.m_Bleed + resource.m_Bleed;
            //这是静态方法，必须指明调用的哪个对象。
            result.CheckResourceThreshold();
            return result;
        }

        private void CheckResourceThreshold()
        {
            if (m_HP >= m_MaxHP)
            {
                m_HP = m_MaxHP;
                HPMaxEvent?.Invoke();
            }
            if (m_HP <= 0)
            {
                m_HP = 0;
                HPMinEvent?.Invoke();
            }

            if (m_Poison >= m_PoisonThreshold)
            {
                m_Poison = m_PoisonThreshold;
                PoisonMaxEvent?.Invoke();
            }
            if (m_Poison <= 0)
            {
                m_Poison = 0;
                PoisonMinEvent?.Invoke();
            }
            if (m_Bleed >= m_BleedThreshold)
            {
                m_Bleed = m_BleedThreshold;
                BleedMaxEvent?.Invoke();
            }
            if (m_Bleed <= 0)
            {
                m_Bleed = 0;
                BleedMinEvent?.Invoke();
            }
        }

        /*TODO：暂时不用，估计也没啥用*/

        public class Resource<TValue>
        {
            protected TValue m_Value;
            protected TValue m_MinValue;
            protected TValue m_MaxValue;
            /*Tip：event关键字限制只能在本类中触发，就算是派生类也不行。所以需要定义方法来让子类能够间接触发这里基类中的事件。*/
            public event Action MinEvent;
            public event Action MaxEvent;

            //只是封装基本数据类型，所以应当与原本的数据类型实现无缝的互相转换
            public static implicit operator TValue(Resource<TValue> self) => self.m_Value;
            // public static explicit operator TValue(Resource<TValue> self) => self.m_Value;
            // public static implicit operator Resource<TValue>(TValue value) => new Resource<TValue>() { m_Value = value };
            /*Tip: 似乎无法从比如int转为ResInt，因为没有最大值的信息，最小值可以默认为0，但是最大值不能默认。*/
            public static implicit operator Resource<TValue>(TValue value) => new Resource<TValue>() { m_Value = value };
            // public static TValue operator =(Resource<TValue> self, TValue value) => self.m_Value = value;

            protected void InvokeMinEvent() => MinEvent?.Invoke();
            protected void InvokeMaxEvent() => MaxEvent?.Invoke();

            // public static Resource<TValue> operator +(Resource<TValue> self, Resource<TValue> value)
            // {
            //     self.m_Value += value;
            //     if (self.m_Value > self.m_MaxValue)
            //     return self;
            // }

            // public Resource<>
        }

        public class ResInt : Resource<int>
        {
            public ResInt(int _value, int _minValue, int _maxValue) : base()
            {
                m_Value = _value;
                m_MinValue = _minValue;
                m_MaxValue = _maxValue;
            }
            //通常默认最小值就是0
            public ResInt(int _value, int _maxValue) : this(_value, 0, _maxValue)
            {
                
            }

            public static ResInt operator +(ResInt self, ResInt value)
            {
                self.m_Value += value;
                if (self.m_Value >= self.m_MaxValue)
                {
                    self.m_Value = self.m_MaxValue;
                    // self.MaxEvent?.Invoke(); 
                    self.InvokeMaxEvent();
                }
                if (self.m_Value <= self.m_MinValue)
                {
                    self.m_Value = self.m_MinValue;
                    // self.MinEvent?.Invoke();
                    self.InvokeMinEvent();
                }
                return self;
            }
        }



    }

}

namespace ARPGDemo.AbilitySystem
{


    public class ActorPropertySet
    {
        public float hp;
        public float mp;
        public float sp;    
    }
}