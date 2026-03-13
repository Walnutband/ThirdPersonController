using System;
using ARPGDemo.CustomAttributes;
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
        // [SerializeField] private InputActionReference m_JumpAction;
        //冲刺输入
        [SerializeField] private InputActionReference m_SprintAction; //翻滚（或闪避）与冲刺。
        // InputActionReference walkAction;
        [Header("所需数据")]
        [SerializeField] protected Transform m_CamTransform;
        [DisplayName("普通移动动画")]
        [SerializeField] private MixerAnimation m_MoveAnims;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_TurnSpeed;
        //奔跑、冲刺、跳跃，都是明确的行为。

        [DisplayName("蓄力移动动画")]
        [SerializeField] private MixerAnimation m_HoldMoveAnims;
        [DisplayName("蓄力待机动画")]
        [SerializeField] private FadeAnimation idle;
        [DisplayName("蓄力移动速度")]
        [SerializeField] private float m_HoldMoveSpeed;

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
        public AnimationMixerState moveState => m_MoveState;
        private AnimationMixerState m_HoldMoveState; //蓄力时的移动

        [DisplayName("普通移动动画层级")]
        public int layerIndex = 0;
        [DisplayName("蓄力移动动画层级")]
        public int holdLayerIndex = 1;

        private bool canMove = true;
        private bool canRotate = true;

        private Action OnMoving;

        private NormalMoveState normalMoveState;
        private HoldMoveState holdMoveState;

        private Action OnUpdate;

        private void Start()
        {
            normalMoveState = new NormalMoveState(this);
            holdMoveState = new HoldMoveState(this);
        }

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

            // OnUpdate?.Invoke();

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
                OnMoving?.Invoke();
            }
        }

        public void AddOnMovingCallback(Action _action)
        {
            OnMoving += _action;
        }
        public void RemoveOnMovingCallback(Action _action)
        {
            OnMoving -= _action;
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

            if (canMove == true) RotateOneStep();
            if (canRotate == true) MoveOneStep();

        }

        //移动一步，旋转一步，就是执行一帧。
        private void MoveOneStep()
        {
            //过渡改变移动速度。
            m_CurrentMoveSpeed = MathUtilities.FloatLerp(m_CurrentMoveSpeed, m_TargetMoveSpeed, Time.deltaTime / m_ShiftTime);
            //移动
            m_CC.Move(m_MoveDir.normalized * m_CurrentMoveSpeed * Time.deltaTime);
            // Debug.Log($"当前移动速度：{m_CurrentMoveSpeed}");
            // Debug.Log($"普通移动， 当前移动速度：{m_CurrentMoveSpeed}");
        }
        private void RotateOneStep(bool _instant = false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotation, m_TurnSpeed * Time.deltaTime);
        }

        //设置移动和旋转的开关。
        public void SetMoveAndRotate(bool _canMove, bool _canRotate)
        {
            canMove = _canMove;
            canRotate = _canRotate;
        }

        //表现层
        private void SetAnimation()
        {
            if (m_MoveState == null)
            {
                // m_MoveState = m_AnimPlayer.Play(m_MoveAnims);
                m_MoveState = m_AnimPlayer.Play(layerIndex, m_MoveAnims);
                m_MoveState.SetParameter(GetMoveSpeed);
            }
        }

        public void EnterNormalMoveState()
        {
            normalMoveState.OnEnter();
        }

        //Tip：基于底层规则，实际上普通动画就是一直在运行的。
        public void ExitNormalMoveState()
        {
            normalMoveState.OnExit();
        }

        public void EnterHoldMoveState()
        {
            holdMoveState.OnEnter();
        }
        public void ExitHoldMoveState()
        {
            holdMoveState.OnExit();
        }

        private float GetMoveSpeed() => m_CurrentMoveSpeed;

        public class NormalMoveState
        {
            ActorMovementComponent m_AMC;

            public NormalMoveState(ActorMovementComponent _AMC)
            {
                m_AMC = _AMC;
            }

            public void OnEnter()
            {
                m_AMC.OnUpdate += OnUpdate;

                if (m_AMC.m_MoveState != null && m_AMC.m_MoveState.isPlaying == true) return;

                m_AMC.m_MoveState = m_AMC.m_AnimPlayer.Play(m_AMC.layerIndex, m_AMC.m_MoveAnims);
                m_AMC.m_MoveState.SetParameter(m_AMC.GetMoveSpeed);
            }

            public void OnUpdate()
            {
                m_AMC.GetSprintAction();
                m_AMC.RotateAndMove();
                m_AMC.SetAnimation();
            }

            public void OnExit()
            {
                m_AMC.OnUpdate -= OnUpdate;

                m_AMC.m_MoveState.Stop(0.2f);
                m_AMC.m_MoveState = null;
            }
        }

        public class HoldMoveState
        {
            ActorMovementComponent m_AMC;

            private AnimationMixerState m_MixerState;
            private AnimationClipState m_IdleState;

            public float angle = 0;
            public float Speed = 150;

            public HoldMoveState(ActorMovementComponent _AMC)
            {
                m_AMC = _AMC;
            }

            public void OnEnter()
            {
                m_AMC.OnUpdate += OnUpdate;

                if (m_AMC.m_HoldMoveState != null && m_AMC.m_HoldMoveState.isPlaying == true) return;

                m_AMC.m_HoldMoveState = m_AMC.m_AnimPlayer.Play(m_AMC.holdLayerIndex, m_AMC.m_HoldMoveAnims);
                m_AMC.m_HoldMoveState.SetParameter(m_AMC.GetMoveSpeed);
            }

            public void OnUpdate()
            {
                Move();
                SetAnimation();
            }

            public void OnExit()
            {
                m_AMC.OnUpdate -= OnUpdate;

                m_AMC.m_HoldMoveState.Stop(0.2f);
                m_AMC.m_HoldMoveState = null;
            }

            private void Move()
            {
                if (m_AMC.m_IsMoving == true) m_AMC.m_CC.Move(m_AMC.m_HoldMoveSpeed * m_AMC.m_MoveDir * Time.deltaTime);
            }

            private void SetAnimation()
            {
                if (m_AMC.m_MoveInput.Equals(Vector2.zero) && (m_IdleState == null || m_IdleState.isPlaying == false))
                {
                    m_IdleState = m_AMC.m_AnimPlayer.Play(m_AMC.idle);
                }
                else if ((m_AMC.m_MoveInput != Vector2.zero) && (m_MixerState == null || m_MixerState.isPlaying == false))
                {
                    m_MixerState = m_AMC.m_AnimPlayer.Play(m_AMC.m_HoldMoveAnims);
                    // m_MixerState.SetParameter(() => angle);
                    m_MixerState.SetParameter(GetAngle);
                    this.angle = Vector3.SignedAngle(m_AMC.m_CamTransform.right, m_AMC.m_MoveDir, Vector3.down);
                }

                Vector3 right = m_AMC.m_CamTransform.right; //相机右方向。
                float angle = Vector3.SignedAngle(right, m_AMC.m_MoveDir, Vector3.down);
                // this.angle = angle;
                // this.angle = Mathf.Lerp(this.angle, angle, Speed * Time.deltaTime);
                this.angle = MoveAngleTowards(this.angle, angle, Speed * Time.deltaTime);
            }

            private float GetAngle()
            {
                Debug.Log("获取角度");
                return angle;
            }

            public float MoveAngleTowards(float currentAngle, float targetAngle, float maxDelta)
            {
                // 1. 计算两个角度的差值（自动处理环绕，结果在 -180 到 180 之间）
                float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);

                // 2. 确定旋转方向（根据差值的符号）
                float step = Mathf.Sign(deltaAngle) * Mathf.Min(maxDelta, Mathf.Abs(deltaAngle));

                // 3. 应用旋转并保证结果仍在 -180 到 180 范围内
                float result = currentAngle + step;

                // 4. 将结果规范化到 -180 到 180 范围（防止边界溢出）
                return Mathf.Repeat(result + 180f, 360f) - 180f;
            }
        }

    }

}