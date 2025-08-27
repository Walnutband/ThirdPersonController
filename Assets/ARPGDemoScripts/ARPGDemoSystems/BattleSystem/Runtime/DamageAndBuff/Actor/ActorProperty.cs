using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*Tip：这里是运行时属性，属于动态数据，而编辑时属性即静态数据应该从外部读取比如从Excel读取，但是我看了一下Excel导入的相关项目还有luban这种商业级的开源项目，发现根本不是我现在
    能够开发的，完全不是那种用纯C#脚本就可以编写出来的插件，而是涉及到很多超出引擎开发层之外的知识，其实对于个人开发来说确实不是必需品。
    将编辑时数据设置为ScriptableObject类型，这样方便在检视器中编辑，运行时就是从引用的SO文件读取属性数据，不过还要注意存档读档的问题，因为这种属性数据其实属于存档数据，按理来说
    应该是从存档文件中读取，只有在创建新档时才会从静态数据文件中读取数据。当然还有大量不属于存档数据的静态数据，这就是必然从静态数据文件中读取了。*/
    // public class ActorProperty : MonoBehaviour
    [Serializable]
    public class ActorProperty
    {
        /*Tip：这时候就体现出嵌套结构的好处了，我之前的目的就只是纯粹将其分开，如果分为两个类的话又会导致很多副作用，这样嵌套之后就能够很好的解决这个问题*/
        [Serializable]
        protected struct ActorAbility 
        {
            private ActorProperty m_Property;
            private ActorAbility(ActorProperty _property) : this() => m_Property = _property;

            public int m_Vigor; //生命力
            public int m_Mind; //集中力
            public int m_Endurance; //耐力
            public int m_Strength; //力气
            public int m_Dexterity; //灵巧
            public int m_Intelligence; //智力
            public int m_Faith; //信仰
            public int m_Arcane; //感应

            public int m_HPGrowRate; //血量成长系数
            public int m_MPGrowRate; //蓝量成长系数
            public int m_SPGrowRate; //体力成长系数

            //能力成长影响
            public void VigorChanged(int _value)
            {//TODO:这里可能还需要用一个容器，存储关于一个能力值会影响哪些属性值，然后遍历容器，但是这样可能就不知道使用哪个系数了，所以可能需要考虑将角色属性进行一层封装，而不是直接使用int、float
                m_Vigor = _value;
                m_Property.hp = _value * m_HPGrowRate;
            }

            public void MindChanged(int _value)
            {
                m_Mind = _value;
                m_Property.mp = _value * m_MPGrowRate;
            }

            public void EnduranceChanged(int _value)
            {
                m_Endurance = _value;
                m_Property.sp = _value * m_SPGrowRate;
            }
        }

        #region 个体数据
        /*TODO: 这里应该还有问题，这种静态数据，只会在创建新档时读取，然后就是从存档文件中读取，只是单纯地读取一下数据，并不需要维持数据文件对象也就是这里的m_Data，所以
        在实际运行时应该是使用Addressable或者AssetBundle之类的运行时资源管理系统，来加载指定的文件，然后读取其数据其实就是将其字段值逐个赋值给该类中的对应字段。*/
        //该字段用于编辑时指定对应的SO文件，然后在运行时读取数据
        // [SerializeField] private SO_ActorProperty m_Data;

        /*TODO：我想的是，Actor表示一切个体，那么ActorProperty应该是个体共同的一些属性，而具体的个体主要是三类，角色、怪物、NPC*/

        [Header("角色等级")]
        [SerializeField] protected int m_Level;
        public int level { get => m_Level; set => m_Level = value; }

        [Header("上限属性值")] //因为这些属性值都会实时变化地参与逻辑，但同时受到上限的影响（其实还有下限，但其实都是0），所以需要单独存储
        [SerializeField] protected float m_MaxHP;
        public int maxHP { get => (int)m_MaxHP; set => m_MaxHP = value; }
        [SerializeField] protected float m_MaxMP;
        public int maxMP { get => (int)m_MaxMP; set => m_MaxMP = value; }
        [SerializeField] protected float m_MaxSP;
        public int maxSP { get => (int)m_MaxSP; set => m_MaxSP = value; }
        [SerializeField] protected int m_MaxEquipLoad;
        public int maxEquipLoad { get => m_MaxEquipLoad; set => m_MaxEquipLoad = value; }
        //韧性也没有上限，但是在游戏中实际作用时会经过一系列计算，因为有基础韧性和出手韧性这个机制，大概可以认为这里的就是基础韧性。
        // [SerializeField] private int m_MaxToughness;
        // public int maxToughness { get => m_MaxToughness; set => m_MaxToughness = value; }
        //观察力确实没有上限。
        // [SerializeField] private int m_MaxDiscovery;
        // public int maxDiscovery { get => m_MaxDiscovery; set => m_MaxDiscovery = value; }

        //TODO：使用float还是double可能需要考虑，但是float的表示范围似乎已经是巨大了。因为在计算时可能会出现小数，但实际使用又应该是整数，所d以将字段设置为float，而对外的属性设置为int。
        //属性值
        //（硬属性？）血条、蓝条、体力条，在战斗中实时变化，与个体的其他属性具有显著差异。
        [Header("属性值")]
        [SerializeField] protected float m_HP; //血量（Health Points）
        public int hp { get => (int)m_HP; set => m_HP = value; }
        [SerializeField] protected float m_MP; //蓝量（Mana Points，其实是资源量，蓝量只是其中一种表现形式）
        public int mp { get => (int)m_MP; set => m_MP = value; }
        [SerializeField] protected float m_SP; //体力（Stamina Points）
        public int sp { get => (int)m_SP; set => m_SP = value; }

        //（软属性？）
        [SerializeField] protected int m_EquipLoad; //装备重量，Weight确实是重量，但是Load表示载重，可能更加贴切。
        public int equipLoad { get => m_EquipLoad; set { m_EquipLoad = value; } }
        [SerializeField] protected int m_Toughness; //强韧度，韧性（法环这里的数值公式比较奇怪，需要进行恰当修改）
        public int toughness { get => m_Toughness; set { m_Toughness = value; } }
        [SerializeField] protected int m_Discovery; //观察力，幸运值（就是掉落物品的概率，不过还可以尝试加入其他作用）
        public int discovery { get => m_Discovery; set { m_Discovery = value; } }


        //能力值
        /*成长属性，其实在游戏中叫做能力值，作为成长系统的核心，这里就将其专门封装为一个类来集中处理，并非必须，只是增强结构性，不过具体的好坏程度，还需要后面再看。
        这里的数据首先是初始职业，这肯定要从数据文件中读取，然后就是注册方法到UI控件的事件中，以便进行升级等等操作。*/
        [Header("能力值")]
        [SerializeField] protected ActorAbility m_Abilities;


        #endregion


        #region 角色成长
        public void Chan()
        {
            // m_Vigor
        }
        #endregion

    }
}