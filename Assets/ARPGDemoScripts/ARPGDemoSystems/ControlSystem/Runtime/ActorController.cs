using System;
using System.Collections.Generic;
using ARPGDemo.AbilitySystem;
using ARPGDemo.ControlSystem.InputActionBindings;
using ARPGDemo.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace ARPGDemo.ControlSystem
{
    
    //TODO：不同角色大概需要定制？
    // [RequireComponent(typeof(InputController))]
    public class ActorController : MonoBehaviour
    {
        [SerializeField] private AbilitySystemComponent m_ASC;

        [SerializeField] private List<InputActionBinder<ExecuteSpecifiedAbility>> m_InputBinders; 
        public ComboAttackAbility normalAttackAbility;
        private NormalMoveAbility moveAbility;
        public NormalRollAbility rollAbility;


        private void Awake()
        {
            if (m_ASC == null) m_ASC = GetComponent<AbilitySystemComponent>();

            moveAbility = new NormalMoveAbility();
        }
        private void Start()
        {
            m_ASC.AddAbility(normalAttackAbility);
            // m_ASC.AddAbility(rollAbility);
            m_ASC.SetDefaultAbility(moveAbility); //默认行为，专用方法。
            m_ASC.ExecuteDefaultAbility(); //开头直接执行默认行为。

        }

        private void OnEnable()
        {
            EnableInput();
            m_ASC.AMC.AddOnMovingCallback(OnMovingCallback); //这种决策逻辑应该放在控制器而非ASC中。
        }

        private void OnDisable()
        {
            DisableInput();
            m_ASC.AMC.RemoveOnMovingCallback(OnMovingCallback);
        }


        public void EnableInput()
        {
            m_InputBinders.ForEach(binder => binder.Enable());
        }
        public void DisableInput()
        {
            m_InputBinders.ForEach(binder => binder.Disable());
        }

        /*TODO：其实这个挺别扭的，就相当于发出移动指令，这个必须监测，确实是移动行为的特殊性，对于游戏开发，必须尊重这样的特殊存在。*/
        private void OnMovingCallback()
        {
            m_ASC.ExecuteDefaultAbility();
        }
    }

}