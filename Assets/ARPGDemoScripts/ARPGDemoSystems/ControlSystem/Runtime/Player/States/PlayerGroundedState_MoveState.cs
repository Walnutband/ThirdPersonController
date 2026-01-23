using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem.Player
{
    [AddComponentMenu("ARPGDemo/ControlSystem/Player/PlayerGroundedState_MoveState")]
    public class PlayerGroundedState_MoveState : PlayerGroundedState
    {

        [SerializeField] protected MixerAnimation m_MoveAnims;
        // [SerializeField] private InputActionReference m_MoveAction; 
        [SerializeField] private InputActionReference m_SprintAction;
        // [SerializeField] private Transform m_CamTransform;
        // [SerializeField] private CharacterController m_CC;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_TurnSpeed;
        // private Vector2 m_MoveInput;
        // private Vector3 m_MoveDir;
        private Quaternion m_TargetRotation;
        private bool m_IsMoving; //是否正在移动，该变量用于信息交流，并没有存储有效数据。
        private bool m_IsSprinting;
        private float m_TargetMoveSpeed;
        private float m_CurrentMoveSpeed;
        private float m_ShiftTime = 0.1f; //变速时间

        private AnimationMixerState m_MoveState;

        public override void OnEnterState()
        {
            base.OnEnterState();

            // GetMoveDir();
            // m_Animator.SetFloat("BattleState", 0);
            m_MoveState = m_AnimPlayer.Play(m_MoveAnims);
            m_SprintAction.action.performed += StartSprint;
            m_SprintAction.action.canceled += StopSprint;
        } 

        public override void OnUpdate()
        {
            base.OnUpdate();

            RotateAndMove();
            SetParameter();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            m_IsSprinting = false;

            m_SprintAction.action.performed -= StartSprint;
            m_SprintAction.action.canceled -= StopSprint;
        }

        // private void StartSprint(InputAction.CallbackContext ctx) => m_IsSprinting = true;
        private void StartSprint(InputAction.CallbackContext ctx)
        {
            m_IsSprinting = true;
            m_TargetMoveSpeed = m_SprintSpeed;
        }
        private void StopSprint(InputAction.CallbackContext ctx)
        {
            m_IsSprinting = false;
            m_TargetMoveSpeed = m_RunSpeed;
        }

        private void RotateAndMove()
        {
            if (m_MoveInput.Equals(Vector2.zero))
            {
                m_IsMoving = false;
                m_TargetMoveSpeed = 0f;
            }
            else
            {
                m_IsMoving = true;
                if (m_IsSprinting == true)
                {
                    m_TargetMoveSpeed = m_SprintSpeed;
                } //因为只有Run和Sprint，所以直接if-else，否则的话可能就是else if了。
                else m_TargetMoveSpeed = m_RunSpeed;
            }


            if (!m_MoveInput.Equals(Vector2.zero))
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

            //过渡改变移动速度。
            m_CurrentMoveSpeed = MathUtilities.FloatLerp(m_CurrentMoveSpeed, m_TargetMoveSpeed, Time.deltaTime / m_ShiftTime);
            //移动
            m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime);

        }

        public void SetParameter()
        {
            // m_Animator.SetFloat("MoveSpeed", m_CurrentMoveSpeed);
            m_MoveState.SetParameter(m_CurrentMoveSpeed);
        }
    }
}