using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem
{
    /*Tip：全权处理角色的运动即位置和旋转，本质上是处理Transform的position和rotation的变化。
    移动：通常的逻辑是确定速度，然后乘以经过时间得到应该的位移。（对于ARPG或者大部分3DRPG都是如此，而确定目标位置再移动，也是一个重要类型）
    旋转：确定目标朝向再按照旋转速度进行旋转。
    */

    public class ActorMovementComponent : MonoBehaviour
    {
        [Header("功能组件")]
        [SerializeField] private AnimatorAgent m_AnimPlayer; //TODO：这种表现层应该封装好、委托给其他组件或系统处理，比如封装为Timeline然后委托给TimelinePlayer处理。
        [SerializeField] private CharacterController m_CC;
        [Header("输入绑定")]
        //方向输入（默认奔跑Run）
        [SerializeField] private InputActionReference m_MoveAction;
        //跳跃指令
        [SerializeField] private InputActionReference m_JumpAction;
        //冲刺输入
        [SerializeField] private InputActionReference m_SprintAction; //翻滚（或闪避）与冲刺。
        // InputActionReference walkAction;
        [Header("所需数据")]
        [SerializeField] protected Transform m_CamTransform;
        [SerializeField] private MixerAnimation m_MoveAnims;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_TurnSpeed;
        //奔跑、冲刺、跳跃，都是明确的行为。

        //原始输入与由此计算得到的移动方向
        private Vector2 m_MoveInput;
        private Vector3 m_MoveDir;
        private Quaternion m_TargetRotation;
        private bool m_IsMoving; //是否正在移动，该变量用于信息交流，并没有存储有效数据。
        private bool m_IsSprinting; //默认不冲刺。
        private float m_TargetMoveSpeed;
        private float m_CurrentMoveSpeed;
        private const float m_ShiftTime = 0.1f; //变速时间

        private AnimationMixerState m_MoveState;

        private void OnEnable()
        {
            // m_MoveAction.action.performed += MoveAction;
        }

        private void OnDisable()
        {
            // m_MoveAction.action.performed -= MoveAction;
        }

        //TODO：战斗状态和蓄力状态时的移动有所不同。
        private void Update()
        {
            //感知，获取基本数据，与后续决策和执行无关。
            GetMoveDir(); //获取方向输入，计算移动方向，更新移动状态信息
            GetSprintAction(); //获取冲刺指令

            RotateAndMove();
            SetAnimation();
        }

        public void EnableInput()
        {
            
        }
        public void DisableInput()
        {
            
        }

        private void GetMoveDir()
        {
            m_MoveInput = m_MoveAction.action.ReadValue<Vector2>();
            // m_MoveInput = _input;
            // Debug.Log($"moveInput: {m_MoveInput}");
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + m_CamTransform.right * m_MoveInput.x;

            if (m_MoveInput.Equals(Vector2.zero))
            {
                m_IsMoving = false;
                m_IsSprinting = false; //默认为非冲刺状态。
            }
            else
            {
                m_IsMoving = true;
                // m_IsSprinting = false;
            }
        }

        private void GetSprintAction()
        {
            if (m_IsMoving == true)
            {
                if (m_SprintAction.action.WasPressedThisFrame()) m_IsSprinting = true;
                else if (m_SprintAction.action.WasReleasedThisFrame()) m_IsSprinting = false;
            }
        }

        /*Tip：移动和旋转，就是改变Transform的position和rotation的表现。改变的方式，就是根据moveSpeed和rotateSpeed来改变，*/

        private void RotateAndMove()
        {
            if (m_IsMoving == false)
            {
                m_TargetMoveSpeed = 0f;

            }
            else //在移动
            {
                if (m_IsSprinting == true)
                {
                    m_TargetMoveSpeed = m_SprintSpeed;
                } //因为只有Run和Sprint，所以直接if-else，否则的话可能就是else if了。
                else m_TargetMoveSpeed = m_RunSpeed;

                //Quaternion有重载。
                if (Quaternion.LookRotation(m_MoveDir, Vector3.up) != transform.rotation)
                {
                    m_TargetRotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
                }
            }

            RotateOneStep();
            MoveOneStep();

            // //Tip：即使不移动，也要旋转到移动时指定的目标朝向，这是卡通风格通常采取的处理，而写实风格一般移动停止就停止转向了。
            // if (transform.rotation != m_TargetRotation)
            // {
            //     // transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
            //     transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
            // }

            // //过渡改变移动速度。
            // m_CurrentMoveSpeed = MathUtilities.FloatLerp(m_CurrentMoveSpeed, m_TargetMoveSpeed, Time.deltaTime / m_ShiftTime);
            // //移动
            // m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime);

        }

        public void RotateToCamera()
        {
            
        }

        //移动一步，旋转一步，就是执行一帧。
        private void MoveOneStep()
        {
            //过渡改变移动速度。
            m_CurrentMoveSpeed = MathUtilities.FloatLerp(m_CurrentMoveSpeed, m_TargetMoveSpeed, Time.deltaTime / m_ShiftTime);
            //移动
            m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime);
        }
        private void RotateOneStep(bool _instant = false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
        }

        //设置移动和旋转的开关。
        public void SetMoveAndRotate()
        {
            
        }

        //表现层
        private void SetAnimation()
        {
            if (m_MoveState == null)
            {
                m_MoveState = m_AnimPlayer.Play(m_MoveAnims);
            }

            m_MoveState.SetParameter(m_CurrentMoveSpeed);
        }
    }
}