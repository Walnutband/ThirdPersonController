using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.AbilitySystem
{

    /*Ques：似乎不需要搞成资产复用，那是实例复用，类型和数据完全相同，似乎代表的是一个攻击模组。但是应该也不能直接复用，因为会有一些数据发生调整，而且最关键的是，
    关于复用攻击模组，有没有可能，真正提高效率和降低成本的根本就不是引用同一个这样的资产，而是已经做好适配的那些动画等资产和测试好的固有逻辑？？
    */
    //连段攻击，逻辑有所不同。
    // public class NormalAttackAbility : AbilityBase
    public class ComboAttackAbility : AbilityBase //Tip：似乎ComboAttack才是“行为逻辑”，NormalAttack偏向“行为表现”了，而行为表现应该是由变量名来表达。
    {
        /*Tip：要不要从ASC或者其他对象获取InputAction而不是直接在检视器中单独编辑？这是早期想法，现在完全不这么认为，因为这些输入绑定是纯数据配置的，就是策划的事，就是应该
        这样分开独立地编辑，这样策划就有最灵活的编辑空间。*/
        [SerializeField] private InputActionReference m_AttackAction;
        
        /*Tip：如何理解“一段伤害”所需的信息是关键——应该考虑到逻辑层和表现层，表现层主要就是基于时间轴的内容以及用于UI显示的信息，逻辑层主要就是初始伤害来源，*/
        //每一段伤害，来自于特定基础属性乘以一定倍率，在属性相同的情况下就直接比较倍率即可得知伤害高低
        // [SerializeField] private ActorAttributeSet.AttributeValueAcquirer[] acquirers;

        //记录当前是第几段攻击
        private int m_CurrentAttackIndex = 0;

        //是否可以跳转到下一段攻击了。
        private bool m_CanJump;

        //对于连段攻击来说，Activate就是第一段攻击开始，之后连段就是在Ability内部处理。
        
        //Tip：获取初始值，具体属性和倍率都编辑保存在了对应属性集的Acquirer中。
        private float GetInitialValue(ActorAttributeSet.AttributeValueAcquirer _acquirer)
        {
            if (m_ASC == null)
            {
                Debug.LogError("Ability未分配到ASC，无法获取初始值");
                return 0f;
            }
            return _acquirer.GetValueFromAttributes(m_ASC.actorAS);
        }

        public override void ActivateAbility()
        {
            base.ActivateAbility();

            // m_AttackAction.
        }
    }
    
    //Ques：应该是单纯封装数据，而具体执行逻辑还是交给Ability本身来处理。
    //TODO：单个攻击，就是代表一段攻击行为，“攻击行为”应该只是“行为”的一个子集，需要进一步探究所谓“行为”本质。
    [Serializable]
    public class SingleAttack
    {
        public string valueDescription; //值描述，即倍率。（可以通过固定规则直接从requirer中获取然后解析，但是从编辑角度来说，我感觉不如直接编辑清楚算了。）
        public ActorAttributeSet.AttributeValueAcquirer requirer;
        public AbilityTask task;

    }
}