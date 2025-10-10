using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*TODO：不继承自UnityEngine.Object的纯C#类是无法拥有对应的SerializedObject以及检视器对象，只能依附于SerializedObject被检视*/
    [Serializable]
    public class ActorAbility
    {
        /*Ques: 在Actor中感觉必须要以ActorProperty为字段，但是在升级的时候确实是先ActorAbility然后ActorProperty。这样来看，可能本来就应该把ActorAbility的内容放到ActorProperty中，
        否则就只有像这样相互拥有对方的引用，虽然确实也没什么耦合影响，因为与外界是完全隔离的，这种关系更类似于父节点拥有子节点的引用，同时子节点也拥有对于父节点的引用*/
        // private ActorProperty m_ActorProperty;

        //8项能力值
        [Range(0, 100)]
        [SerializeField] private int m_Vigor; //生命力
        public int vigor { get => m_Vigor; }
        [Range(0, 100)]
        [SerializeField] private int m_Mind; //集中力
        public int mind { get => m_Mind; }
        [Range(0, 100)]
        [SerializeField] private int m_Endurance; //耐力
        public int endurance { get => m_Endurance; }
        [Range(0, 100)]
        [SerializeField] private int m_Strength; //力气
        public int strength { get => m_Strength; }
        /*TODO：*/
        // [SerializeField] private int m_Dexterity; //灵巧
        [Range(0, 100)]
        [SerializeField] private int m_Intelligence; //智力
        public int intelligence { get => m_Intelligence; }
        // [SerializeField] private int m_Faith; //信仰
        [Range(0, 100)]
        [SerializeField] private int m_Arcane; //感应
        public int arcane { get => m_Arcane; }



        // [Header("属性成长系数")]
        // [SerializeField] private int m_HPGrowRate; //血量成长系数
        // public int hpGrowRate { get => m_HPGrowRate; set => m_HPGrowRate = value; }
        // [SerializeField] private int m_MPGrowRate; //蓝量成长系数
        // public int mpGrowRate { get => m_MPGrowRate; set => m_MPGrowRate = value; }
        // [SerializeField] private int m_SPGrowRate; //体力成长系数
        // public int spGrowRate { get => m_SPGrowRate; set => m_SPGrowRate = value; }

        // //能力成长影响
        // public void VigorChanged(int _value)
        // {//TODO:这里可能还需要用一个容器，存储关于一个能力值会影响哪些属性值，然后遍历容器，但是这样可能就不知道使用哪个系数了，所以可能需要考虑将角色属性进行一层封装，而不是直接使用int、float
        //     m_Vigor = _value;
        //     m_ActorProperty.hp = _value * m_HPGrowRate;
        // }

        // public void MindChanged(int _value)
        // {
        //     m_Mind = _value;
        //     m_ActorProperty.mp = _value * m_MPGrowRate;
        // }

        // public void EnduranceChanged(int _value)
        // {
        //     m_Endurance = _value;
        //     m_ActorProperty.sp = _value * m_SPGrowRate;
        // }
    }
}