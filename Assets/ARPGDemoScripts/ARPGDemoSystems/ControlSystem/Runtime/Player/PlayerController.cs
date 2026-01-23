
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem.Player
{
    [AddComponentMenu("ARPGDemo/ControlSystem/Player/PlayerController", -1)]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset m_InputActionAsset;
        [DisplayName("<b>状态机</b>")] //因为支持富文本，所以就用粗体更好。
        [SerializeField] private PlayerStateMachine m_StateMachine;
        [SerializeField] private Animator m_Animator;
        [Header("输入")]
        [SerializeField] private InputActionReference m_MoveAcion;
        [SerializeField] private InputActionReference m_LightAttackAcion;
        [SerializeField] private InputActionReference m_RollAction;
        [Header("状态")]
        [SerializeField] private PlayerGroundedState_MoveState m_MoveState;
        [SerializeField] private PlayerLightAttackState m_LightAttackState;
        [SerializeField] private PlayerGroundedState_RollState m_RollState;

        private void OnEnable() 
        {
            m_InputActionAsset.Enable();
            m_LightAttackAcion.action.started += DoLightAttack;
            m_RollAction.action.performed += DoRoll; //Tap抬起触发翻滚动作。
        }

        private void OnDisable()
        {
            m_InputActionAsset.Disable();
            m_LightAttackAcion.action.started -= DoLightAttack;
            m_RollAction.action.performed -= DoRoll;
        }

        private void Update()
        {
            HandleInput();
            //为状态机提供运行动力。
            m_StateMachine.OnUpdate();
        }

        private void HandleInput()
        {
            if (m_MoveAcion.action.ReadValue<Vector2>() != Vector2.zero) DoMove();
        }

        private void DoMove() => m_StateMachine.TrySetState(m_MoveState);
        private void DoLightAttack(InputAction.CallbackContext ctx) => m_StateMachine.TrySetState(m_LightAttackState);
        private void DoRoll(InputAction.CallbackContext ctx) => m_StateMachine.TrySetState(m_RollState);
    }
}