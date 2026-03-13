
using System;
using ARPGDemo.ControlSystem;
using ARPGDemo.SkillSystemtest;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{

    /*TODO：Ability各种各样，常见的是攻击技能，但其实还有很多效果也是Ability，只是因为不是由玩家主动触发的、所以体感上不感觉是技能，但是程序层面要明确其本质相同。
所以这里就能感觉到枚举类型的局限了，还是得专门的GameplayTags系统才能满足进一步的开发需求。
*/
    /*Tip：这就是游戏的底层逻辑，直接对应于游戏可以进行的操作，直接映射到“按键设置”，并且一个按键绑定明确对应一个操作，这个操作就是这里的一个枚举成员，而具体的操作内容就是在
    下面的该枚举成员作为下标对应的Ability，这样通过改变数组中对应位置的Ability就实现了同样的操作而执行不同的内容，但是对玩家而言，“普通攻击”就是普通攻击、只不过是不同角色或不同武器
    的普攻，当然元素战技也是元素战技，而长按短按其实是Ability内部的逻辑，对于角色控制器就只管触发，而元素爆发也是同样。
    但看原神，能够使用的Ability很明显并不止于此，但是仍然都是像这样完全明确的，通过动态改变对应的Ability从而实现操作不同的行为。
    */
    public enum AbilityType 
    {
        None,
        NormalAttack, //普通攻击
        ElementalSkill, //元素战技
        ElementalBurst, //元素爆发
        // End, //结束，所以该值就是有效枚举类型的个数。
        Roll,
        ChargedThrow, //蓄力投掷武器。
    }

    /*Tip：核心是确定如何获取指定的基础属性值。从属性集获取特定属性，首先要保证必须拥有指定属性，所以似乎更合适的是直接强绑定类型。另外还必须保证可以直接配置数据来指定属性，
    而不是必须在代码中逐个编写。*/
    public abstract class AbilityBase
    {
        //作为对于Ability的行为标识，直接对应的是操作输入。
        public AbilityType Tag;

        /*Tip：Ability依附于使用者运行。本质上就是相同（复用）的逻辑，但是（部分）数据来源于使用者，还有比如移动逻辑就要通过ASC委托给AMC运行，这都是通过ASC实现的，所以ASC提供的就是Context。
        具体来说，伤害数据来源于个体的相关属性值（通常是攻击力，还可以是生命值、防御力等等），
        移动行为，
        */
        protected AbilitySystemComponent m_ASC;


        /*Tip：CD算是一切Ability的底层机制了，因为Ability本身是没有监测逻辑的，它只相当于一个调度器，涉及监测逻辑的也只是其中用于表现层的AbilityTask。*/
        [SerializeField] private float m_Cooldown; //冷却时间

        //提供监测完成事件的通道。
        private Action onCompleted;



        //TODO：思考当附加到个体时（这里就是ASC代表）会有哪些成员被固定下来。实际上属性集也可以通过ASC访问，就像移动组件一样，只不过属性集会过于频繁地访问，所以可能自己维护更好，但我估计也没差区别。
        public virtual void AddToASC(AbilitySystemComponent _asc)
        {
            m_ASC = _asc;
        }
        public virtual void RemoveFromASC()
        {
            m_ASC = null;
        }

        //执行Ability的起点，
        public abstract void Activate();

        public abstract bool TryDeactivate();

        //退出Ability，做好收尾工作。
        public abstract void Deactivate();

        //Tip：关于结束，应该设置回调，而且明确是自然播放结束，如果
        // public abstract bool IsCompleted();

        //注册到Task，完成时触发回调。
        protected void Completed()
        {
            // Debug.Log("触发完成时回调");
            onCompleted?.Invoke();
            // onCompleted = null;
        }

        public void SetCompletedCallback(Action _onCompleted)
        {
            // Debug.Log("为Ability设置完成回调");
            onCompleted = _onCompleted;
            // onCompleted += _onCompleted; //可以设想，注册多个方法，在Invoke之后立刻清空。
        }
    }




}