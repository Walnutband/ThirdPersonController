using ARPGDemo.BattleSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace ARPGDemo.ControlSystem.Player
{
    [AddComponentMenu("ARPGDemo/ControlSystem/Player/PlayerController", -1)]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset m_InputActionAsset;
        // [SerializeField] private InputActionAsset m_InputActions;
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

        private InputActionReference m_NormalAttack;

        [SerializeField] private CharacterController m_CC; //控制移动
        //TODO：主控相机可能会发生变化，需要及时调整。
        [SerializeField] private Transform m_CamTransform; //为了获得相机

        //特定角色信息
        private ActorObject m_Actor; 

        //记录感知信息
        //地面或是空中。
        private bool isGrounded_Last = true; //似乎应该默认为true。
        private bool isGrounded = true; //突然感觉，这种纯数据变量（字段），就适合直接小驼峰。
        private bool justGrounded => !isGrounded_Last && isGrounded; //其实只需要为true的信息，不需要为false的信息。
        private bool justNotGrounded => isGrounded_Last && !isGrounded;

        private Vector2 moveInput_Raw; //原始移动输入
        private bool hasMoveInput => !moveInput_Raw.Equals(Vector2.zero);
        private Vector3 moveDir; //基于相机朝向的移动方向（归一化）。
        //方向乘以长度，就可以通过输入来连续控制移动速度，不过仅限于手柄摇杆之类。
        // private Vector3 moveDir_WithInput => moveDir * Mathf.Min(moveInput_Raw.magnitude, 1f);
        
        private void OnEnable()    
        {
            m_InputActionAsset.Enable();
            m_LightAttackAcion.action.started += DoLightAttack;
            m_RollAction.action.performed += DoRoll; //Tap抬起触发翻滚动作。

            // m_NormalAttack.action.
        }

        private void OnDisable()
        {
            m_InputActionAsset.Disable();
            m_LightAttackAcion.action.started -= DoLightAttack;
            m_RollAction.action.performed -= DoRoll;
        }

        /*TODO：如果要插入其他自动决策逻辑，比如保持奔跑（跑酷游戏那种），那么就可以或应该是在Update中插入，并且后续决策可以将其覆盖、然后再发送给状态机，如果没覆盖那么就是执行自动决策了。
        有没有可能，决策过程本来就该如此，层层递进、分层决策，按顺序根据当前情况（由各种感知得到的信息）逐步决策、最后得到落实决策。
        */

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

        private bool CanPlungingAttack()
        {
            //前提是在空中
            if (!isGrounded)
            {
                //
            }

            return false;
        }

        private void NormalAttackAction(InputAction.CallbackContext ctx)
        {
            ActionInstruction ins = ActionInstruction.None;

            if (ctx.interaction is TapInteraction)
            {
                switch (ctx.phase)
                {
                    case InputActionPhase.Started:
                        //Tip：这样写的逻辑表现就是，默认就是NormalAttack，然后检查是否有特殊情况，就会转换为其他指令，并且执行顺序体现了优先级。
                        ins = ActionInstruction.NormalAttack; 
                        if (CanPlungingAttack())
                        {
                            ins = ActionInstruction.PlungingAttack;
                        }
                        break;
                    case InputActionPhase.Performed:
                        ins = ActionInstruction.ContinueNormalAttack;
                        break;
                    case InputActionPhase.Canceled:
                        break;
                }
            }

            // if (ctx.interaction is HoldInteraction)
            if (ctx.interaction is SlowTapInteraction) //Tip：似乎SlowTapInteraction才更合适。
            {
                switch (ctx.phase)
                {
                    case InputActionPhase.Started:
                        ins = ActionInstruction.ChargedAttackStarted;
                        break;
                    case InputActionPhase.Performed:
                        ins = ActionInstruction.ChargedAttackPerformed;
                        break;
                    case InputActionPhase.Canceled:
                        break;
                }
            }
        }

        //TODO：命名为“Perceive感知”，就是按照AI系统结构来的，实际就是收集相关信息。
        private void Perceive()
        {
            //获取移动输入以及移动方向
            ReadMoveInputAndMoveDir();
            //确定着地状态相关信息
            CheckGrounded();
        }

        private void ReadMoveInputAndMoveDir()
        {
            moveInput_Raw = m_MoveAcion.action.ReadValue<Vector2>();
            Vector2 normalizedInput = moveInput_Raw.normalized;
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            moveDir = camFoward * normalizedInput.y + m_CamTransform.right * normalizedInput.x; //这里得到的就是归一化的方向向量。
            // moveDir *= 
        }

        //TODO：具体如何判断着地状态，方式还有待考虑。
        private void CheckGrounded()
        {
            isGrounded_Last = isGrounded;
            isGrounded = m_CC.isGrounded; 
            
            // if (isGrounded != m_CC.isGrounded)
            // {
            //     isGrounded = m_CC.isGrounded;
            //     justGrounded = isGrounded;
            //     justNotGrounded = !isGrounded;
            // }
            // else //相同即没变，就不存在刚变成Grounded或NotGrounded了。
            // {
            //     justGrounded = false;
            //     justNotGrounded = false;
            // }
            
        }

        // public static void 


    }

    //行动指令
    public enum ActionInstruction
    {
        None,
        NormalAttack,
        ContinueNormalAttack,
        ChargedAttackStarted,
        ChargedAttackPerformed,
        PlungingAttack,
    }

    /*TODO：*/
}