
using System;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    /*Tip：核心是确定如何获取指定的基础属性值。从属性集获取特定属性，首先要保证必须拥有指定属性，所以似乎更合适的是直接强绑定类型。另外还必须保证可以直接配置数据来指定属性，
    而不是必须在代码中逐个编写。*/
    public abstract class AbilityBase
    {
        /*Tip：Ability依附于使用者运行。本质上就是相同（复用）的逻辑，但是（部分）数据来源于使用者，还有比如移动逻辑就要通过ASC委托给AMC运行，这都是通过ASC实现的，所以ASC提供的就是Context。
        具体来说，伤害数据来源于个体的相关属性值（通常是攻击力，还可以是生命值、防御力等等），
        移动行为，
        */
        protected AbilitySystemComponent m_ASC;
        // protected ActorAttributeSet m_ActorAS; //TODO：暂时不搞多属性集，就先使用这个统一的属性集算了。

        //相关状态数据
        protected bool m_CanExit; //可以被主动退出，否则就是继续执行。
        public bool canExit => m_CanExit;
        protected bool m_IsEnd; //自己结束，一般来说应该回到默认行为。
        public bool isEnd => m_IsEnd;

        //相关
        [SerializeField] private float m_Cooldown; //冷却时间
        // [SerializeField] private ActorAttributeSet.AttributeValueAcquirer[] m_Cost; 
        //消耗，就是修改属性值。
        [SerializeField] private ActorAttributeSet.AttributeValueModifier[] m_Cost;


        //TODO：思考当附加到个体时（这里就是ASC代表）会有哪些成员被固定下来。实际上属性集也可以通过ASC访问，就像移动组件一样，只不过属性集会过于频繁地访问，所以可能自己维护更好，但我估计也没差区别。
        public virtual void AddToASC(AbilitySystemComponent _asc)
        {
            m_ASC = _asc;
            // m_ActorAS = _asc.actorAS;
        }
        public virtual void RemoveFromASC()
        {
            m_ASC = null;
            // m_ActorAS = null;
        }

        //执行Ability的起点，
        public virtual void ActivateAbility()
        {
            
        }
    }



    /*Tip：AbilityTask是绑定到Ability的，主要负责表现层内容，但也会牵涉到逻辑层，比如连段攻击的可跳转区间以及共有的canExit和isEnd时间点，由于这些逻辑是固定的，所以大概不适合用事件，
    而是直接读写Ability的相关属性。
    */
    
    public class AbilityTask
    {
        public TimelineObj timeline; 
    }
}