
using System;
using ARPGDemo.AbilitySystem;
using ARPGDemo.CustomAttributes;
using UnityEngine;
using ARPGDemo.UISystem_Test;

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
    public struct OpenSpecifiedUIPanel : IInputActionBinderEvent
    {
        [DisplayName("UI面板")]
        [SerializeField] private GameObject m_Panel;
        public void Execute()
        {
            // m_Panel.SetActive(true);
            UIManager.Instance.OpenPanel(m_Panel);
        }
    }



    [Serializable]
    public class InvokeSpecifiedCallback : IInputActionBinderEvent
    {
        /*TODO：这种一般性，实际感觉没啥必要，要么是换成事件的标识符，设置统一的事件管理器，在执行事件时将标识符传入执行指定事件，这样就可以在检视器中编辑了，而另外一点在于，
        这本来就是用于输入触发的方法，其实非常固定，比如执行某个行为、打开某个UI面板等等，而且有的确实需要额外的数据，那么就可以直接写成特定类型，而不是写成这么一个一般性回调，
        不过从完整性来说，这种一般性类型确实也应该存在。
        */
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