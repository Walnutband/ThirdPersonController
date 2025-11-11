using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    [AddComponentMenu("ARPGDemo/ControlSystem_New/Player_Wizard/WizardGroundedState")]
    public class WizardGroundedState : WizardStateBase, ICommandHandler<MoveCommand>
    {
        [SerializeField] private MixerAnimation m_MoveAnims;
        // public MovablePlatform m_Platform;
        public bool onPlatform;
        [SerializeField] private Transform m_CamTransform;
        // [SerializeField] private Transform m_Transform;
        [SerializeField] private CharacterController m_CC;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        // [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_TurnSpeed;
        private float m_TargetMoveSpeed;
        private float m_CurrentMoveSpeed;
        public float currentSpeed => m_CurrentMoveSpeed;
        private Quaternion m_TargetRotation;
        //移动速度没有现成量所以定义一个成员currentMoveSpeed，但是旋转是可以直接使用transform.rotation的。
        // private Quaternion m_CurrentRotation; 
        private Vector2 m_MoveInput;
        private Vector3 m_MoveDir;
        private bool m_IsMoving;
        private float m_ShiftTime = 0.2f; //变速时间

        public bool isGrounded => m_CC.isGrounded;

        private AnimationMixerState m_MoveState;

        private void Awake()
        {
            m_CamTransform = Camera.main.transform;
        }

        public void HandleCommand(MoveCommand _command)
        {
            if (_command.moveType == MoveCommand.MoveType.Run) m_MoveInput = _command.moveInput;
            if (m_MoveInput.Equals(Vector2.zero))
            {
                m_IsMoving = false;
                m_TargetMoveSpeed = 0f;
            }
            else
            {
                m_IsMoving = true;
            }

            if (m_IsMoving == true)
            {
                if (_command.moveType == MoveCommand.MoveType.Walk)
                {
                    m_TargetMoveSpeed = m_WalkSpeed;
                }
                else if (_command.moveType == MoveCommand.MoveType.WalkCancel)
                {
                    m_TargetMoveSpeed = m_RunSpeed;
                }
                //如果在Walk时那么就不切换速度，否则会发现如果在WASD上改了下输入、触发了回调的话就会从Walk变成Run了，显然不合适。
                //利用目标速度的数值关系可以知道此时是否处于Walk状态，而不用单独使用一个变量来表示是否处于Walk状态。
                // else if (_command.moveType == MoveCommand.MoveType.Run)
                else if (!(Mathf.Abs(m_TargetMoveSpeed - m_WalkSpeed) < 0.001f) && _command.moveType == MoveCommand.MoveType.Run)
                {
                    m_TargetMoveSpeed = m_RunSpeed;
                }
                // else if (_command.moveType == MoveCommand.MoveType.Sprint)
                // {
                //     m_TargetMoveSpeed = m_SprintSpeed;
                // }
            }
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            m_MoveState = m_AnimPlayer.Play(m_MoveAnims);
            GetMoveDir();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            GetMoveDir();
            RotateAndMove();
            SetAnimParameter();
        }

        private void GetMoveDir()
        {
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + m_CamTransform.right * m_MoveInput.x;
        }

        private void RotateAndMove()
        {
            if (m_MoveInput != Vector2.zero)
            {
                //Quaternion有重载。
                if (Quaternion.LookRotation(m_MoveDir, Vector3.up) != transform.rotation)
                {
                    m_TargetRotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
                }
            }

            if (transform.rotation != m_TargetRotation)
            {
                // transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
            }

            m_CurrentMoveSpeed = MathUtilities.FloatLerp(m_CurrentMoveSpeed, m_TargetMoveSpeed, Time.deltaTime / m_ShiftTime);
            // if (m_Platform != null)
            if (onPlatform == true)
            {
                // Debug.Log($"随平台移动, deltaPos: {m_Platform.deltaPos}");
                m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime);
            }
            else
            {
                m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime + new Vector3(0f, -0.01f, 0f));
            }
        }

        private void SetAnimParameter()
        {
            m_MoveState.SetParameter(m_CurrentMoveSpeed);
        }
    }
}