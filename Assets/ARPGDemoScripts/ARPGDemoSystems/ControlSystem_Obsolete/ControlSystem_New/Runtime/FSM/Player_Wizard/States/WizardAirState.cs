
using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.ControlSystem_New
{
    [AddComponentMenu("ARPGDemo/ControlSystem_New/Player_Wizard/WizardAirState")]
    public class WizardAirState : WizardStateBase, ICommandHandler<MoveCommand>, ICommandHandler<JumpCommand>
    {
        [SerializeField] private CharacterController m_CC;
        [SerializeField] private MixerAnimation m_JumpAnims;
        // [SerializeField] private Vector3 m_UpGravity = new Vector3(0f, -9.81f, 0f);
        // [SerializeField] private Vector3 m_FallGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private float m_RiseGravity = -9.81f;
        [SerializeField] private float m_FallGravity = -9.81f;
        [SerializeField] private float m_InitialVerticalSpeed;
        //可以设置一个最大下落速度，其实从物理上来说由于空气阻力，最终都会变成一个恒定速度。
        // [SerializeField] private float m_MaxFallSpeed;
        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private float m_TurnSpeed;
        private Transform m_CamTransform;
        private float m_CurrentVerticalSpeed;
        private Vector3 m_CurrentVerocity;
        private Vector2 m_MoveInput;
        private Vector3 m_MoveDir;
        private Quaternion m_TargetRotation;
        private AnimationMixerState m_AirState;
        private float m_StartVSpeed;

        public override bool canExitState => m_IsEnd;
        // public override bool canTransitionToSelf => m_IsEnd;
        public override int tempPriority => 10;

        public bool isRising
        {
            // get => m_CurrentVerocity.y > 0f;
            get => m_CurrentVerticalSpeed > 0f;
        }
        public bool isFalling
        {
            // get => m_CurrentVerocity.y <= 0f;
            get => m_CurrentVerticalSpeed <= 0f;
            set
            {
                if (value == true)
                {
                    m_CurrentVerticalSpeed = 0f;
                }
            }
        }

        private void Awake()
        {
            m_CamTransform = Camera.main.transform;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            // m_CurrentVerticalSpeed = m_InitialVerticalSpeed;
            m_CurrentVerticalSpeed = m_StartVSpeed;
            m_CurrentVerocity = new Vector3(0f, m_CurrentVerticalSpeed, 0f);
            m_AirState = m_AnimPlayer.Play(m_JumpAnims);
            SetParameter();
            GetMoveDir();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (m_CurrentVerticalSpeed > 0f)
            {
                m_CurrentVerticalSpeed = Mathf.Max(0f, m_CurrentVerticalSpeed + m_RiseGravity * Time.deltaTime);
                // if (m_CurrentVerticalSpeed <= 0f) m_CurrentVerticalSpeed = -1 * m_InitialVerticalSpeed;
            }
            else
            {
                m_CurrentVerticalSpeed = m_CurrentVerticalSpeed + m_FallGravity * Time.deltaTime;
            }

            GetMoveDir();
            RotateAndMove();
            SetParameter();
            CheckGrounded();
        }

        private void GetMoveDir()
        {
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            m_MoveDir = (camFoward * m_MoveInput.y + m_CamTransform.right * m_MoveInput.x).normalized;
            //Tip：在空中的移动应该与地面上的移动逻辑不太一样。
            // var forward = transform.forward.normalized;
            // forward.y = 0f;
            // m_MoveDir = (forward * m_MoveInput.y + transform.right * m_MoveInput.x).normalized;
        }

        private void RotateAndMove()
        {
            if (m_MoveInput != Vector2.zero)
            {
                //Quaternion有重载。
                if (Quaternion.LookRotation(m_MoveDir, Vector3.up) != transform.rotation)
                {
                    m_TargetRotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
                }
            }

            // if (transform.rotation != m_TargetRotation)
            // {
            //     // transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
            //     transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
            // }

            // GetMoveDir();


            // m_CurrentVerocity = m_MoveDir * m_MoveSpeed + new Vector3(0f, m_CurrentVerticalSpeed, 0f);
            m_CurrentVerocity = transform.forward * m_MoveSpeed + new Vector3(0f, m_CurrentVerticalSpeed, 0f);
            m_CC.Move(m_CurrentVerocity * Time.deltaTime);
        }

        private void CheckGrounded()
        {
            //着地就结束Air状态。
            if (m_CC.isGrounded)
            {
                m_IsEnd = true; 
            }
        }

        private void SetParameter()
        {
            m_AirState.SetParameter(m_CurrentVerticalSpeed);
        }

        public void HandleCommand(MoveCommand _command)
        {
            m_MoveInput = _command.moveInput;
            // m_MoveSpeed = _command.moveSpeed;
        }

        public void HandleCommand(JumpCommand _command)
        {
            // m_CurrentVerticalSpeed = m_InitialVerticalSpeed;
            m_StartVSpeed = m_InitialVerticalSpeed;
            m_MoveSpeed = _command.moveSpeed;
        }

        public void HandleCommand(FallCommand _command)
        {
            // m_CurrentVerticalSpeed = 0f;
            m_StartVSpeed = 0f;
            m_MoveSpeed = _command.moveSpeed;
        }
    }
}