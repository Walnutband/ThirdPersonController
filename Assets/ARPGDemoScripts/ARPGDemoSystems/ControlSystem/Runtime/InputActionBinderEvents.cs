
using System;
using ARPGDemo.AbilitySystem;
using ARPGDemo.CustomAttributes;
using UnityEngine;

namespace ARPGDemo.ControlSystem.InputActionBindings
{

    //执行指定的Ability，不过是指定类型，具体Ability对象由ASC决定。
    [Serializable]
    public struct ExecuteSpecifiedAbility : IInputActionBinderEvent
    {
        [DisplayName("技能系统组件")]
        [SerializeField] private AbilitySystemComponent m_ASC;
        [DisplayName("目标技能")]
        [SerializeField] private AbilityType m_TargetAbility;
        [SerializeField] private AbilityType blockedAbility;
        public void Execute()
        {
            // Debug.Log($"执行{m_TargetAbility}");
            if (m_ASC.currentAbilityType == blockedAbility) return;
            m_ASC.ExecuteAbility(m_TargetAbility);
        }
    }

    [Serializable]
    public class InvokeSpecifiedCallback : IInputActionBinderEvent
    {
        private Action m_Action;

        public void Execute()
        {
            // Debug.Log("触发指定回调");
            m_Action?.Invoke(); //没有回调时Action为空，+=和-=也都会处理null的情况。并不需要给Action初始化。
        }

        //其实用“Register更严谨”，但是感觉真没必要那么讲究。
        public void AddCallback(Action _action)
        {
            m_Action += _action;
        }
        public void RemoveCallback(Action _action)
        {
            m_Action -= _action;
        }
    }
}