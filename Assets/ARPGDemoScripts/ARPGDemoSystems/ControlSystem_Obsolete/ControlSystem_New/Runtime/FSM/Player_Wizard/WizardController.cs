using System.Collections;
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    [AddComponentMenu("ARPGDemo/ControlSystem_New/Player_Wizard/WizardController")]
    // public class WizardController : MonoBehaviour, ICommandHandler, ICommandHandler<MoveCommand>
    public class WizardController : PlayerCommandConsumer
    {

        public GroundedChecker m_GroundedChecker;
        // public MovablePlatform m_Platform;
        public CharacterController m_CC;
        // public bool justTouch;

        [SerializeField] private WizardStateMachine m_StateMachine;
        [SerializeField] private WizardGroundedState m_GroundedState;
        [SerializeField] private WizardLightAttackState m_LightAttackState;
        [SerializeField] private WizardAirState m_AirState;   

        // public Transform root { get => transform; } 
        // public bool onPlatform
        // {
        //     set
        //     {
        //         m_GroundedState.onPlatform = value;
        //     }
        // }

        private bool m_IsGrounded;

        private void Awake()
        {
            m_GroundedState ??= GetComponent<WizardGroundedState>();
            m_CC ??= GetComponent<CharacterController>();
        }

        private void Start()
        {
            m_GroundedChecker.root = transform;
            m_StateMachine.Initialize();
            // StartCoroutine(PreUpdate());
        }

        private void OnEnable()
        {
            m_GroundedChecker.onLanded += Landed;
            m_GroundedChecker.onAir += Fall;
            m_GroundedChecker.onPlatformEvent += OnPlatform;
        }
        private void OnDisable()
        {
            m_GroundedChecker.onLanded -= Landed;
            m_GroundedChecker.onAir -= Fall;
            m_GroundedChecker.onPlatformEvent -= OnPlatform;
        }

        private void Update()
        {
            m_StateMachine.OnUpdate();
            // SyncWithPlatform();

            // if (m_StateMachine.currentState == m_GroundedState && m_GroundedState.isGrounded == false)
            // {
            //     HandleCommand(new FallCommand());
            // }
            /*Tip：之前写成了true，导致一碰地就弹跳，其实也是个很常见的效果*/
            // if (m_CC.isGrounded == true)
            // if (!m_StateMachine.currentState == m_AirState && m_CC.isGrounded == false)
            // {
            //     HandleCommand(new FallCommand());
            // }
        }

        private void Landed()
        {
            if (m_StateMachine.currentState == m_AirState)
            {
                m_StateMachine.TrySetState(m_GroundedState);
            }
        }

        private void Fall()
        {
            if (m_StateMachine.currentState == m_GroundedState)
            {
                HandleCommand(new FallCommand());
                // m_StateMachine.TrySetState(m_AirState);
            }
        }
        
        private void OnPlatform(bool _value)
        {
            m_GroundedState.onPlatform = _value;
        }

        // private void SyncWithPlatform()
        // {
        //     // //平台上的移动，无关状态（无关就意味着与所有都有关），所以放在控制器中
        //     // if (justTouch)
        //     // {
        //     //     justTouch = false;
        //     //     return;
        //     // }
        //     // if (m_Platform != null) m_CC.Move(m_Platform.deltaPos);

        //     if (m_StateMachine.currentState == m_AirState && m_AirState.isRising) return;
        //     if (m_Platform != null)
        //     {
        //         Vector3 pos = transform.position;
        //         pos.y = m_Platform.upperSurface.position.y;
        //         transform.position = pos; //修正Y值，就是移动到平台上
        //     }
        // }

        // private IEnumerator PreUpdate()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForFixedUpdate();
        //         if (justTouch)
        //         {
        //             justTouch = false;
        //         }
        //         else if (m_Platform != null) m_CC.Move(m_Platform.deltaPos);
        //     }
        // }

        private void FixedUpdate()
        {
            m_StateMachine.OnFixedUpdate();
            // if (justTouch) justTouch = false;
        }

        // private void OnTriggerEnter(Collider other)
        // {
        //     Debug.Log("TriggerEnter");
        //     if (other.TryGetComponent<MovablePlatform>(out var platform))
        //     {
        //         m_Platform = platform;
        //         m_GroundedState.onPlatform = true;
        //         // Vector3 pos = transform.position;
        //         // pos.y = platform.upperSurface.position.y;
        //         // transform.position = pos; //修正Y值，就是移动到平台上
                
        //         // Debug.Log($"第{Time.frameCount}帧进入平台，上表面坐标：{platform.upperSurface.position}, 修正后坐标：{transform.position}");
        //         // justTouch = true;
                
        //     }
        // }

        // private void OnTriggerExit(Collider other)
        // {
        //     Debug.Log("TriggerExit");
        //     if (other.TryGetComponent<MovablePlatform>(out var platform))
        //     {
        //         m_Platform = null;
        //         m_GroundedState.onPlatform = false;
        //     }
        // }

        public override void HandleCommand(MoveCommand _command)
        {
            // if (m_StateMachine.TrySetState(m_GroundedState))
            // {
            //     m_GroundedState.HandleCommand(_command);
            // }
            // _command.moveSpeed = m_GroundedState.currentSpeed;
            m_StateMachine.TrySetState(m_GroundedState);
            m_GroundedState.HandleCommand(_command);
            m_AirState.HandleCommand(_command);
        }

        public override void HandleCommand(LightAttackCommand _command)
        {
            // Debug.Log("处理LightAttackCommand");
            m_StateMachine.TrySetState(m_LightAttackState);
        }

        public override void HandleCommand(JumpCommand _command)
        {
            if (m_StateMachine.currentState == m_GroundedState) _command.moveSpeed = m_GroundedState.currentSpeed;
            m_AirState.HandleCommand(_command);
            if (m_StateMachine.TrySetState(m_AirState))
            {
            }
        }

        public override void HandleCommand(FallCommand _command)
        {
            if (m_StateMachine.currentState == m_GroundedState) _command.moveSpeed = m_GroundedState.currentSpeed;
            m_AirState.HandleCommand(_command);
            if (m_StateMachine.TrySetState(m_AirState))
            {
            }
        }
    }
}