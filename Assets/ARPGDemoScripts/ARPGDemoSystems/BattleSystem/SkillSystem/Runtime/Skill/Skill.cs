using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*Tip：关于命名，对于Bag、Buff、Item都是用Data来命名的，而这里Skill我就尝试用Obj与Model来命名，因为作为技能，更加具有整体性、具有更加复杂的构成，更能称得上是一个Model，
    还有Timeline也是，感觉也更适合使用Obj和Model来命名*/

    public class SkillObj
    {

    }

    [Serializable]
    public class SkillModel
    {
        public uint id;
        public string name;
        public string description;

        /*TODO：条件和代价，不止于资源（其实可能有些资源值并不会作为条件或代价，取决于设计）。在SkillObj中扩展？因为只是运行时条件，而非编辑时的固有条件？
        就认为这是技能的硬性条件。
        */
        public ActorResource condition;
        /*Tip：在释放技能即执行特定动作时，首先应该经过Buff作用，所以比如有个“重击不消耗体力”的效果，就可以挂上一个Buff，然后在释放技能时，对传入的技能信息（SkillInfo？）这里的
        cost进行修改，将其sp属性设置为0即可，在Buff作用之后，才会将消耗应用到角色身上。*/
        public ActorResource cost;

        
    }

    public class SkillInfo
    {
        
    }

    public class SkillManager
    {
        
    }


}

/*
namespace ARPGDemo.AbilitySystem
{

    //元素附着器。
    public class ElementalApplicator : MonoBehaviour
    {
        private void FixedUpdate()
        {
            
        }
    }

    public class Element
    {
        public float quantity; //元素量
        public float duration;
    }

    public class AAA
    {
        void A()
        {
            
        }
    }

    [Serializable]
    // public class ElementalAttachmentCounter
    public class ElementalAttachmentCD
    {
        //TODO：还要看攻击对象。是否要以受击对象为主体来存储这些信息？
        [Serializable]
        public class CDHandler //计数信息，一组信息
        {
            //只要攻击的标签被该Tag容器包含，就会添加到这一组计数中。
            public GameplayTagContainer requiredTags;
            //原神里面默认是1、4、7，也就是中途跳过两段攻击，这两段就没有元素附着。
            //Ques：逻辑上似乎也应该是等间距的，否则的话就需要额外的信息，关于当前是第几段攻击的信息。
            //TODO：独立附着的话
            [SerializeField] private int skipCount; 
            private int counter = -1;
            [SerializeField] private float duration;
            private float timer;

            private bool canAttach;

            public void OnTick(float _deltaTime)
            {
                timer += _deltaTime;
                if (timer >= duration)
                {
                    timer = 0f;
                    // counter = 0;
                    counter = -1;
                    canAttach = true;
                }
            }

            //访问时，必然是在发起一个攻击
            public bool CanAttach()
            {
                //这是第一次访问。特殊处理，其实不够统一，但也能解决问题。
                if (counter < 0)
                {
                    counter = 0; //开始计数
                    canAttach = false;
                    return true;
                }
                counter++;
                if (counter > skipCount) //这一次攻击，已经超过了要跳过的轮次。
                {
                    counter = 0;
                    // counter = -1;
                    // timer = 0f; //计时器也重新开始计算。
                    canAttach = false;
                    return true; //这一次可以附着，
                }
                return canAttach;
            }
        }

        //实际上编辑之后就应该是角色固有不变的了。
        [SerializeField] private List<CDHandler> handlers;    

        public void OnTick(float _deltaTime)
        {
            handlers.ForEach(handler =>
            {
                    
            });
        }
        
    }

    // public class ElementalAttachmentTimer
    // {
    // }

    //TODO：应该使用结构体，或者是运行时创建专门的结构体类型参与逻辑。
    //作为对属性修改的唯一通道，
    public class GameplayEffect 
    {
        public struct Modifier
        {
            private enum ModifyType
            {
                AddValue,

            }
        }
        public enum DurationPolicy
        {
            Instant,
            Infinite,
            HasDuration
        }

        public DurationPolicy durationPolicy { get; private set; }

        public void OnTick()
        {
            
        }
    }

    public class GameplayTagContainer
    {
        
    }

    //Tip：因为有哪些属性，是游戏的底层规则，所以应该在代码中就写死，不需要与数据解耦而单独编辑配置。
    public enum ActorPropertyName
    {
        
    }

    public class AttributeSet
    {
        GameplayAttribute HPMax;
        GameplayAttribute HP;
        GameplayAttribute ATK;
        GameplayAttribute DEF;
        GameplayAttribute SPMax;
        GameplayAttribute SP;

        GameplayAttribute CriticalRate;
        GameplayAttribute CriticalDamage;

    }

    // public class GameplayAttribute
    // {
    //     public float baseValue;
    //     public float currentValue;
    // }


    public class ActorResource
    {
        
    }

    //个体技能
    public class ActorAbility
    {
        public GameplayAttribute cooldown;
        public ActorResource condition;
        public ActorResource cost;

        //激活技能，开始行为。
        public void ActivateAbility()
        {
            
        }

        //Tip：作用于目标。注册到Hitbox碰撞器的检测事件上，在检测到对象时，触发作用。
        private void ActOnTarget(GameObject _target)
        {
            
        }
    }
}
*/