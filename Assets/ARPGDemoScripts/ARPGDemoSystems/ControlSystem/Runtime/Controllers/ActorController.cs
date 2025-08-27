using Animancer;
using Unity.Cinemachine;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using NUnit.Framework.Internal.Commands;

namespace ARPGDemo.ControlSystem
{
    public class ActorController : CommandConsumer, IPlayerConsumer
    {
        /*TODO：到底是使用更抽象的CommandConsumer还是这里同类型的PlayerController，其实具体的纠结点就在于相机，感觉最好还是用CommandConsumer，因为可以使用类型转换检测其实际类型，如果
        不是的话那就少执行一些逻辑，这样看来也是完全兼容的，不需要特殊考虑。当然也要看具体的游戏设计，总之不是什么难题，纯粹是与具体游戏设计紧密相关的问题*/
        [SerializeField] private CommandConsumer targetConsumer;
        // [SerializeField] private PlayerController targetConsumer;
        // [SerializeField] private CinemachineFreeLook freelook;
        [Header("管理相机")]
        [SerializeField] private CinemachineOrbitalFollow freelook;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float minZoom = 0.5f;
        /*TODO：肯定要能够自由改变灵敏度，不过在实际游戏中，镜头的灵敏度应该是全局的，也就是独立于玩家所控制的角色（控制器）的，所以此处的deltaZoom
        可能应该改为一个只有getter的属性，并且将实际存储灵敏度数值的字段放在专门的静态类中，然后在角色控制器中的属性的getter中直接返回该字段值即可。
        这样的字段应该就属于游戏基础设置中的各项数值之一，可以通过设置界面的UI进行自由调整，不过UI对象肯定是非静态类，所以存放这些
        设置数值的类可能也会设置为非静态类，到时候只要在游戏开始时向UI元素的回调委托上注册方法、改变对应的字段值，即可实现通过UI
        实时调整相关的设置项了。*/
        //TODO：这个表示灵敏度的字段到底应该放在控制器中，还是放在FreeLookZoomer结构体中呢？
        [SerializeField] private float deltaZoom = 0.0005f;

        [Header("相关引用")]
        // [SerializeField] private Rigidbody rb;
        [SerializeField] private CharacterController physicsHandler;
        // [SerializeField] private CinemachineBrain brain;
        [SerializeField] private AnimancerComponent animPlayer;
        // [SerializeField] private AnimationList animationList; //用于animPlayer播放，其实就代表该控制器所控制的角色可以使用的所有动画。
        [SerializeField] private RootMotionController rmController;
        [SerializeField] private Animations anims;

        // public bool isAttack;
        // public bool notMove ;


        //TODO:可以尝试用一个结构体来存储Rig高度和半径的初始值，这样可以避免用一系列字段。
        // private float zoomCounter = 1f;

        private FreeLookZoomer zoomer;
        private Transform camTransform;
        // private Coroutine m_PhysicsCoroutine;
        // private readonly WaitForFixedUpdate m_WaitForFixedUpdate = new();

        [Header("其他数据")]
        //防止下台阶时意外进入Air状态，其实也可以同时作为土狼跳的计时器。
        [SerializeField] private float airTimer = 0f;
        [SerializeField] private float airTime = 0.2f;

        #region 状态部分

        [Header("状态机")] //TODO：应该要为状态机类写一个PropertyDrawer。
        [SerializeField] private PlayerStateMachine m_StateMachine;
        [Header("拥有状态")]
        // private PlayerIdleState m_IdleState;
        // private PlayerMoveState m_MoveState;
        [SerializeField] private PlayerGroundedState m_GroundedState;
        [SerializeField] private PlayerDodgeState m_DodgeState;
        // [SerializeField] private PlayerJumpState m_JumpState;
        [SerializeField] private PlayerAirState m_AirState;
        [SerializeField] private PlayerLightAttackState m_LightAttackState;
        [SerializeField] private PlayerHeavyAttackState m_HeavyAttackState;

        [Header("状态相关数据")]
        [SerializeField] private bool m_IsGrounded = false; //就应该一开始就是false，进入时碰到了地面那么就会变为true
        [SerializeField] private float m_JumpInitialSpeed = 5f;
        [SerializeField] private Vector3 m_UpGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private Vector3 m_FallGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private Vector2 m_MoveInput;
        [SerializeField] private Vector3 m_MoveDir; //就是考虑了相机朝向，并且这不是单位向量，而是带有输入量信息的方向向量。
        [SerializeField] private float m_GroundedRunSpeed = 5f;
        [SerializeField] private float m_GroundedSprintSpeed = 8f;
        [SerializeField] private float m_GroundedTurnSpeed = 10f;
        [SerializeField] private float m_DodgeMoveSpeed = 15f;
        [SerializeField] private float m_AirMoveSpeed = 5f;
        [SerializeField] private float m_AirTurnSpeed = 10f;

        #endregion

        private void Awake()
        {
            //TODO：这种控制鼠标显隐的任务都不应该由控制器来做，应该要有专门的管理器。
            Cursor.lockState = CursorLockMode.Locked;
            // freelook ??= GameObject.FindObjectOfType<CinemachineFreeLook>(); //freelook为空才会查找赋值。
            // animPlayer ??= GetComponent<AnimancerComponent>();
            camTransform = Camera.main.transform;
            m_StateMachine ??= new PlayerStateMachine();
            // rb = GetComponent<Rigidbody>();
            physicsHandler = GetComponent<CharacterController>();

            GetOrCreateAllStates();
        }

        private void Start()
        {
            /*BUG: 在我加入场景加载的逻辑之后，在加载MainScene也就是该组件所在场景之后，下面这一行代码获取不到，而后面一行直接用GameObject查找的方法才能获取到，可以猜想
            就是Cinemachine的生命周期管理上出现的问题，或者说是兼容性问题，但是这需要去阅读Cinemachine源码才能知道了。*/
            // freelook = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineFreeLook;
            // freelook ??= GameObject.FindObjectOfType<CinemachineFreeLook>(); //freelook为空才会查找赋值。
            // Idle();
            /*Ques：放在Start中可以获取到，而放在Awake中获取不到，大概是因为Start在OnEnable之后调用，而CinemachineBrain会在OnEnable之后才会注册到下面的数组中。*/
            // brain = CinemachineBrain.GetActiveBrain(0);

            InitializeStateData();
            m_StateMachine.Initialize(m_GroundedState);
        }

        private void OnEnable()
        {
            // Tip：在运行模式下可以简单地通过禁用再启用来切换（更新）FreeLook相机配置。
            zoomer = new FreeLookZoomer(freelook, minZoom, maxZoom);
        }


        private void GetOrCreateAllStates()
        {
            if ((m_GroundedState = GetComponent<PlayerGroundedState>()) == null)
            {
                m_GroundedState = gameObject.AddComponent<PlayerGroundedState>();
            }
            if ((m_DodgeState = GetComponent<PlayerDodgeState>()) == null)
            {
                m_DodgeState = gameObject.AddComponent<PlayerDodgeState>();
            }
            if ((m_AirState = GetComponent<PlayerAirState>()) == null)
                {
                    m_AirState = gameObject.AddComponent<PlayerAirState>();
                }
            if ((m_LightAttackState = GetComponent<PlayerLightAttackState>()) == null)
            {
                m_LightAttackState = gameObject.AddComponent<PlayerLightAttackState>();
            }
            // if ((m_HeavyAttackState = GetComponent<PlayerHeavyAttackState>()) == null)
            // {
            //     m_HeavyAttackState = gameObject.AddComponent<PlayerHeavyAttackState>();
            // }
        }

        // IEnumerator AfterPhysics()
        // {
        //     while (true)
        //     {
        //         // FixedUpdate can be called multiple times per frame
        //         yield return m_WaitForFixedUpdate;
        //         // DoFixedUpdate();
        //     }
        // }
        // /*TODO：这里是通过触发器来检测转换目标，其实还可以有很多方式，但大概归根到底是利用物理引擎尤其是碰撞体和触发器以及射线检测，这些方法来检测。*/
        // private void OnTriggerEnter(Collider other)
        // {
        //     Debug.Log($"TriggerEnter: {other.gameObject.name}");
        //     // targetConsumer = other.GetComponent<CommandConsumer>();
        //     if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //     {
        //         m_IsGrounded = true;
        //         // m_AirState.isGrounded = m_IsGrounded;
        //         // PerformFall();
        //     }
        // }

        // private void OnTriggerExit(Collider other)
        // {
        //     // targetConsumer = null;
        //     if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //     {
        //         m_IsGrounded = false;
        //         // m_AirState.isGrounded = m_IsGrounded;
        //         // m_StateMachine.TrySetState(m_AirState);
        //         // PerformFall();
        //     }
        // }

        // private void OnCollisionEnter(Collision collision)
        // {
        //     /*TODO：这里通过碰撞体来检测是否在地面，而不是像以前会用触发器来检测，大概是因为以前不理解预输入，直接用碰撞体的话会出现很多bug，但是用上预输入机制之后，
        //     就不会出现那些问题了。但是随后发现，这样会导致自身的多个碰撞体触发的同一个该方法根本分不清是谁触发的，反而导致逻辑混乱。
        //     不过暂时没想到如何恰当处理下台阶之类的干扰状态转换的问题*/
        //     // if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //     // {
        //     //     m_IsGrounded = true;
        //     //     m_JumpState.isGrounded = m_IsGrounded;
        //     // }
        // }


        // public void GetMoveInput(InputAction.CallbackContext ctx)
        // {
        //     moveInput = ctx.ReadValue<Vector2>();
        // }

        // public void GetCursorState(InputAction.CallbackContext ctx)
        // {//使用UIMainView控制鼠标状态的显隐之后，就必须限制只有在MainView时才可以手动控制显隐。
        // //并非，其实应该从输入的角度来看，即切换ActionMap，即Playing和UI分别使用一个ActionMap。
        //     // if (!UIManager.Instance.InMainView)
        //     //     return;

        //     switch (ctx.phase)
        //     {
        //         case InputActionPhase.Started:
        //             Cursor.lockState = CursorLockMode.None;
        //             //Cursor.visible = false;
        //             break;
        //         case InputActionPhase.Canceled:
        //             Cursor.lockState = CursorLockMode.Locked;
        //             break;
        //     }
        // }

        // public void GetZoomInput(InputAction.CallbackContext ctx)
        // {//Zoom动作所绑定的鼠标滚轮，每一次滚动都是在同一帧触发started和performed，在下一帧触发canceled，无论滚动的频率如何，都是这样一组一组地固定触发。
        //     Vector2 zoomInput = ctx.ReadValue<Vector2>();
        //     float delta = (-zoomInput.y) * deltaZoom; 
        //     zoomer.Zoom(delta); //传入处理之后的变化值，就是可以直接使用的值。
        // }

        // //相机复位
        // public void GetResetCMD(InputAction.CallbackContext ctx)
        // {
        //     if (ctx.phase == InputActionPhase.Started) //按下时复位。
        //     {
        //         zoomer.Reset();
        //         // zoomCounter = 1f; //别忘了复原计数器
        //     }
        // }

        // public void GetAttackCMD(InputAction.CallbackContext ctx)
        // {
        //     if (ctx.phase == InputActionPhase.Started)
        //     {

        //     }
        // }

        /*Ques：OnUpdate由ControlSystem调用是为了保证先接收命令再执行监测逻辑（输入处理会在FixedUpdate之后、Update之前，也就是与Update同频率，所以放在Update中处理很合理），
        而FixedUpdate主要是用于物理等逻辑计算，在某些状态中可能会需要这些逻辑，所以需要FixedUpdate方法，而其调用顺序似乎没有影响，因为会将比如刚体移动、旋转之类的逻辑全部
        计算完成之后，再统一进行碰撞检测等物理相关逻辑。*/
        private void FixedUpdate()
        {
            // brain.ManualUpdate();
            FixedUpdateStateData();
            m_StateMachine.OnFixedUpdate();
            FixedUpdateStateData();
        }

        public void OnUpdate()
        {
            /*TODO：输入与Update同频，而状态数据主要来自于输入，所以放在Update中。
            状态数据从理论上来讲应该是分为监测更新的数据以及触发更新的数据，触发也就是变化时才更新，而监测则是每帧都要更新无论是否发生了变化*/
            // brain.ManualUpdate(); //首先将这一帧对于虚拟相机的输入应用到Unity相机上。
            UpdateStateData(); //然后根据已经更新过的相机来更新状态数据（其实就是针对的MoveDir，因为会受到相机的影响，并且是人物抖动的元凶）
            m_StateMachine.OnUpdate();
            // if (m_StateMachine.currentState == m_LightAttackState) rmController.ApplyRootMotion(true);
            // else rmController.ApplyRootMotion(false);
            // UpdateStateData();
            // CheckStateChanged(); //检查状态是否发生了变化（是否应该变化）
            // MoveAndRotate();
            // brain.ManualUpdate();
        }

        // public void LateUdpate()
        public void LateUpdate()
        {
            // brain.ManualUpdate();
            /*Ques：放在Update还是LateUpdate中，还有待进一步测试*/
            // CheckStateChanged(); //检查状态是否发生了变化（是否应该变化）
        }


        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        /*Tip：状态应该分为触发式状态和监测式状态，这源于游戏逻辑，大多状态都是因为输入的变化而发生变化的，也就是主动变化的，当然是触发式的，还包括被怪物、环境等外部因素导致的变化也是触发式的，
        但是还有一些状态比如在走动过程中从悬崖边跌落，这种就必须要监测，只是把监测逻辑放在哪里的问题，当下处理移动是在状态内处理，而通知改变状态是在控制器中进行的，并且控制器无法直接获取到
        状态中运行得到的数据，所以需要在状态运行之后，再检测相关数据的当前值，以便及时作出状态改变。
        不过也可以尝试在状态中设置事件（因为某些数据来源于状态，但是要在控制器中处理，正好符合观察者模式），然后控制器向其中注册方法，也同样是解耦的，并且似乎能够更加优雅地实现目的？？*/
        private void CheckStateChanged()
        {
            if (m_IsGrounded == false && m_StateMachine.currentState != m_AirState)
            {
                airTimer += Time.deltaTime;
                if (airTimer >= airTime)
                {
                    // m_StateMachine.TrySetState(m_AirState);
                    PerformFall();
                }
            }
            else if (m_IsGrounded == true)
            {
                airTimer = 0f; //还原
            }
        }

        #region 处理状态数据
        //初始化各个状态的数据(主要是一些固定不变的数据，其实对于那些可能变化的数据，通常都会通过控制器中对应的Perform方法来进行处理)
        private void InitializeStateData()
        {
            InitializeGroundedState();
            InitializeDodgeState();
            InitializeAirState();
            InitializeLightAttackState();
            //这个算是经典bug了，就是人物在进入运行模式时初始位置在空中，就不会落下来，所以在商业游戏中会看到人物初始位置必然在地上，否则就需要为其增加一些额外逻辑，毫无必要。
            // if (m_IsGrounded == true) PerformFall(); 

            // m_GroundedState.animPlayer = animPlayer;
            // m_GroundedState.rb = rb;
            // m_GroundedState.moveInput = moveInput;
            //由于使用刚体驱动，其实不需要传入transform了。
            // m_GroundedState.camTransform = camTransform;
            // m_GroundedState.targetTransform = transform;
            // m_GroundedState.moveInput = moveInput;

            // m_AirState.moveInput = moveInput;
            // m_AirState.animPlayer = animPlayer;


            // m_LightAttackState.animPlayer = animPlayer;
            // m_LightAttackState.restart = true; //这种应该在状态内部去设置好。

            // m_HeavyAttackState.animPlayer = animPlayer;
        }

        //实时更新状态的数据，不管此时是否位于该状态。
        private void UpdateStateData()
        {
            // m_GroundedState.moveInput = moveInput;
            /*这个输入的特殊之处在于，它是控制相机的，而不是控制人物本身的，而且它的输入并不是在控制器中处理，而是在比如Cinemachine专门的组件中处理，所以需要监测更新，否则
            按理来说，采用触发式的InputSystem之后，所有状态的所有数据都应该是触发式更新的，而不需要监测。*/
            UpdateMoveInput();
            UpdateMoveDir(); //这类数据的特点可能是，确定、少量、共享，且需要一定逻辑来计算得出。\
            UpdateIsGrounded();
        }

        //以FixedUpdate的频率更新相关数据
        private void FixedUpdateStateData()
        {
            // FixedUpdateIsGrounded();
            // UpdateMoveDir();
        }

        //纯粹为了增强逻辑性而分开写在各个方法中。
        private void InitializeGroundedState()
        {
            m_GroundedState.animPlayer = animPlayer;
            // m_GroundedState.rb = rb;
            m_GroundedState.physicsHandler = physicsHandler;
            m_GroundedState.moveInput = m_MoveInput;
            m_GroundedState.moveDir = m_MoveDir;
            m_GroundedState.moveSpeed = m_GroundedRunSpeed;
        }

        private void InitializeDodgeState()
        {
            m_DodgeState.animPlayer = animPlayer;
            m_DodgeState.physicsHandler = physicsHandler;
        }

        /*TODO：对于状态的数据设置，应该还要考虑到同一个状态可能对应多种行为，比如这里的AirState就可以是跳跃引起的，也可以是移动过程中跌落引起的，甚至是被击飞引起的，这个时候
        AirState的数据肯定都各不相同，那么我在想是否应该为每个会导致进入某个状态的行为编写对应的方法，其中就包含设置状态的数据以及通知状态机改变状态的相关逻辑，也就是
        以行为、以事件为目标来写，而不是以状态为目标来写，那么一个状态就应该理解为对若干个其实逻辑相同的行为或事件的整合，这些本质的不同只是在于参与逻辑的数据对象（变量）的
        实际数据不同，从而显现出不同的行为。
        至于像AnimancerComponent、Rigidbody这种状态数据，才是真正需要初始化的。*/
        private void InitializeAirState()
        {
            m_AirState.animPlayer = animPlayer;
            // m_AirState.rb = rb;
            m_AirState.physicsHandler = physicsHandler;
            m_AirState.moveDir = m_MoveDir;
            m_AirState.horizontalSpeed = m_AirMoveSpeed;
            m_AirState.initialVerticalVelocity = new Vector3(0f, m_JumpInitialSpeed, 0f);
            m_AirState.isGrounded = true;
            // m_AirState.originalGravity = m_UpGravity;
            m_AirState.upGravity = m_UpGravity;
            m_AirState.fallGravity = m_FallGravity;
        }

        private void InitializeLightAttackState()
        {
            m_LightAttackState.animPlayer = animPlayer;
            m_LightAttackState.restart = true;
        }

        private void UpdateMoveInput()
        {
            m_GroundedState.moveInput = m_MoveInput;
            m_AirState.moveInput = m_MoveInput;
        }

        private void UpdateMoveDir()
        {
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + camTransform.right * m_MoveInput.x;
            m_GroundedState.moveDir = m_MoveDir;
            m_AirState.moveDir = m_MoveDir; //一定要区别速度和方向（速度甚至准确来说是速度大小，就是水平和竖直的float值，而方向是Vector3向量）
        }

        private void UpdateIsGrounded()
        {
            // Debug.Log($"{Time.frameCount}帧，isGrounded为{physicsHandler.isGrounded}");
            m_IsGrounded = physicsHandler.isGrounded;
            // m_GroundedState.isGrounded = m_IsGrounded;q
            m_AirState.isGrounded = m_IsGrounded;
        }

        //Check回调检查状态数据
        // private bool CheckIsGrounded()
        // {
             
        // }

        #endregion


        // private void UpdateIsGrounded()
        // {
        //     if ()
        // }

        /*TODO：Idle和Move在动画上应该是位于同一个混合树中，然后由MoveInput来调整各片段的权重，而不是像现在这样分开，导致代码复杂度提高，而且毫无价值*/

        // private void Idle()
        // {
        //     if (m_StateMachine.CanIdle()) animPlayer.Play(anims.idle);
        // }

        #region 接收命令
        public bool Move(Vector2 _moveInput)
        {
            // Debug.Log("Move");
            /*TODO：其实这种完全是状态所要使用的并且由输入产生的数据完全不需要在控制器中专门使用字段来存储，没啥意义。
            而对于非输入产生的比如跳跃的初速度，这种就需要专门定义字段来编辑了。
            但是计算移动方向确实需要用到moveInput，所以对于m_MoveInput来说确实还是需要的。*/
            m_MoveInput = _moveInput;
            // m_AirState.horizontalSpeed = m_JumpInitialSpeed;
            /*Tip：触发式的输入，按理来说可以不需要额外执行相关的监测逻辑*/
            // UpdateMoveDir();
            // m_GroundedState.moveInput = _moveInput;
            // m_GroundedState.moveDir = m_MoveDir;
            // m_GroundedState.moveSpeed = m_GroundedMoveSpeed;
            // // m_AirState.moveInput = _moveInput;
            // m_StateMachine.TrySetState(m_GroundedState);
            // ChangeToGroundedState();
            PerformGroundedRun();

            return true;
        }

        // public bool Sprint(Vector2 _moveInput)
        // {
        //     Debug.Log("Sprint");
        //     m_MoveInput = _moveInput;
        //     PerformGroundedSprint();
        //     return true;
        // }

        public bool Jump()
        {
            // Debug.Log("Jump");

            // m_AirState.moveInput = m_MoveInput;
            // m_AirState.initialVerticalVelocity = new Vector3(0f, m_JumpInitialSpeed, 0f);
            // m_AirState.gravity = m_Gravity;
            // m_StateMachine.TrySetState(m_AirState);
            // PerformJump();

            // return true;
            return PerformJump();
        }

        public bool Dodge()
        {
            // Debug.Log("Dodge");


            return PerformDodge();
        }

        public bool skip = false;

        public bool LightAttack()
        {
            // Debug.Log("LightAttack");

            // return true;

            PerformLightAttack();

            // if (m_StateMachine.CanAttack())
            // {
            //     /*TODO: 这里是依赖于AnimancerComponent记录了当前的动画状态，至于是否要依赖，还是应该在控制器内部自己记录这些内容，值得考虑。不过我还是倾向于
            //     依赖于这样的动画播放器，因为提供动画状态信息是其本职，而且足够准确。*/
            //     if (animPlayer.IsPlaying(anims.lightAttack_First))
            //     {
            //         // animPlayer.Play(anims.lightAttack_Second).Events(this).OnEnd = () => { isAttack = false; };
            //         animPlayer.Play(anims.lightAttack_Second).Events(this).OnEnd = Idle;
            //     }
            //     else if (animPlayer.IsPlaying(anims.lightAttack_Second))
            //     {
            //         animPlayer.Play(anims.lightAttack_Third).Events(this).OnEnd = Idle;
            //     }
            //     else if (!animPlayer.IsPlaying(anims.lightAttack_Third))
            //     {
            //         animPlayer.Play(anims.lightAttack_First).Events(this).OnEnd = Idle;
            //     }
            // }

            // isAttack = true;
            return true;
        }

        public bool HeavyAttack(bool _charging, bool? _full)
        {
            // Debug.Log("HeavyAttack");
            m_StateMachine.TrySetState(m_HeavyAttackState); //不可自己转入自己
            m_HeavyAttackState.isCharging = _charging;
            return true;
        }

        public bool Zoom(Vector2 _zoomInput)
        {
            Vector2 zoomInput = _zoomInput;
            float delta = (-zoomInput.y) * deltaZoom;
            zoomer.Zoom(delta); //传入处理之后的变化值，就是可以直接使用的值。
            return true;
        }

        public bool ResetCamera()
        {
            zoomer.Reset();
            return true;
        }

        /*TODO：这里应该会是一个通用方法而不是专用方法，总之就是在改变控制对象时要对原对象进行一些善后处理，然后再改变，因为只有控制器挂载在原对象上，才能够获取到原对象的许多信息，
        从而执行所需逻辑。并且这里最直接的理由就是，只有控制器才能获取到所要转换到的对象，只是*/
        public bool ChangePlayer(CommandProducer producer)
        {
            if (targetConsumer == null) return false; //没目标换不了。

            // freelook.LookAt = freelook.Follow = targetConsumer.GetComponent<Transform>();
            ControlSystem.Instance.ChangeConsumer(producer, targetConsumer);
            return true;
        }

        #endregion

        /*Tip：通过私有的静态方法，使得相同Class的不同实例能够进行交流。其实还可以将freelook字段设置为静态，感觉其实更好。
        甚至更加特殊化一点，直接为每个要掌控相机的对象都准备一个相机（虚拟相机），到时候直接切换就完事了，还方便做相机过渡之类的效果*/
        private static void DeliverFreeLook(ActorController current, ActorController target, CinemachineOrbitalFollow freelook, FreeLookZoomer zoomer)
        {
            // PlayerController target = target as PlayerController;
            if (target != null) //确定要把相机交出去了。
            {
                target.freelook = freelook; //类内部的私有静态方法，可以访问类的私有成员？
                target.zoomer = zoomer;
                /*Tip：规定个体对象的根对象的直接子对象中有一个名为“Target”的对象专门作为相机的Follow和LookAt目标对象。
                为了兼容性（鲁棒性？），如果没找到的话就直接将其根对象作为目标对象*/
                Transform cameraTarget = target.transform.Find("Target");
                // freelook.LookAt = freelook.Follow = cameraTarget != null ? cameraTarget : target.transform; 
                freelook.GetComponent<CinemachineCamera>().LookAt = freelook.GetComponent<CinemachineCamera>().Follow = cameraTarget != null ? cameraTarget : target.transform;
                current.freelook = null;
            }
        }

        private static void DeliverData(ActorController _current, CommandConsumer _target)
        {
            ActorController target = _target as ActorController;
            if (target != null) //为空的话说明是其他受控对象，那就不执行以下传递数据的逻辑，这是完全兼容的。
            {
                DeliverFreeLook(_current, target, _current.freelook, _current.zoomer);
                target.m_MoveInput = _current.m_MoveInput;
            }
        }


        /*Ques：在当前的设计上，OnStart和OnEnd代表开始生产和结束生产，注意与Mono组件的周期方法区别，周期方法处理的是控制器本身在相应时刻应该执行的逻辑，比如自身的初始化，当然
        监测方法Update和FixedUpdate可能会具有特殊性（需要被控制调用顺序）所以会考虑是否要实现为周期方法。*/
        public override void OnStart()
        {
            if (zoomer.Equals(default(FreeLookZoomer))) //值类型，default(T)就代表其未赋值状态，可以当做引用类型的null来使用。
            {
                // zoomer = new FreeLookZoomer(freelook, minZoom, maxZoom);
            }
        }

        public override void OnEnd()
        {
            // DeliverFreeLook(this, targetConsumer, freelook, zoomer);
            DeliverData(this, targetConsumer);
            //Tip：大概需要注意区别，传递是传递，而清理自己的数据是另一方面的逻辑，无论是否传递得出去，都要清理自己的数据。不过感觉也要看具体游戏设计。
            m_MoveInput = Vector2.zero;
        }

        /*Tip：考虑到状态切换主要就是由输入发送的命令控制，所以调度切换状态的相关逻辑就是与执行命令所调用的方法的目的是一致，而且不同命令即使是转换到相同的状态，但是对于状态所需要
        的数据的处理可能也不相同，所以也没什么复用性可言。
        以上是之前的判断，但是随着开发更加深入之后，发现其实状态是具有复用性的，或者说按照我现在这样的状态机写法，状态是具有高度可复用性的。
        */
        #region 转换状态（执行指定行为）

        private bool PerformGroundedRun()
        {
            // m_GroundedState.move = anims.run;
            m_GroundedState.moveInput = m_MoveInput;
            m_GroundedState.moveDir = m_MoveDir;
            m_GroundedState.moveSpeed = m_GroundedRunSpeed;
            m_GroundedState.turnSpeed = m_GroundedTurnSpeed;
            // m_AirState.moveInput = _moveInput;
            return m_StateMachine.TrySetState(m_GroundedState);
        }

        private bool PerformGroundedSprint()
        {
            m_GroundedState.moveInput = m_MoveInput;
            m_GroundedState.moveDir = m_MoveDir;
            m_GroundedState.moveSpeed = m_GroundedSprintSpeed;
            m_GroundedState.turnSpeed = m_GroundedTurnSpeed;
            return m_StateMachine.TrySetState(m_GroundedState);
        }

        private bool PerformDodge()
        {
            m_DodgeState.moveInput = m_MoveInput;
            m_DodgeState.moveDir = m_MoveDir;
            m_DodgeState.moveSpeed = m_DodgeMoveSpeed;
            return m_StateMachine.TrySetState(m_DodgeState);
        }

        private bool PerformLand()
        {
            m_GroundedState.moveInput = m_MoveInput;
            m_GroundedState.moveDir = m_MoveDir;
            // m_AirState.moveInput = m_MoveInput;
            return m_StateMachine.TrySetState(m_GroundedState);
        }

        // private bool ChangeToAirState
        private bool PerformJump()
        {
            /*TODO：大概率会隐含一个bug，如果像这样每次按下跳跃键都设置数据，但是随后才SetState，那么这时的数据可能会被覆盖，*/
            m_AirState.initialVerticalVelocity = new Vector3(0f, m_JumpInitialSpeed, 0f);
            // m_AirState.originalGravity = m_Gravity;
            /*TODO：不一定就是一个固定的、单独的速度值，通常是继承的移动时的速度，比如Run奔跑时跳跃和Sprint冲刺时跳跃，速度和距离肯定不一样，
            应该要在Jump方法中，通过状态机记录的当前状态的速度来决定此时应该赋值的horizontalSpeed，作为PerformJump的参数传入即可。
            总之，状态的一些可变成员值就可以设置为这样的Perform方法的参数，而不可变的成员大概就是在初始化时赋值*/
            m_AirState.horizontalSpeed = m_AirMoveSpeed;
            m_AirState.turnSpeed = m_AirTurnSpeed;
            m_AirState.isGrounded = m_IsGrounded;
            m_AirState.upGravity = m_UpGravity;
            m_AirState.fallGravity = m_FallGravity;
            return m_StateMachine.TrySetState(m_AirState);
        }

        private bool PerformFall()
        {
            m_AirState.initialVerticalVelocity = new Vector3(0f, 0f, 0f);
            // m_AirState.originalGravity = m_UpGravity;
            m_AirState.horizontalSpeed = m_AirMoveSpeed;
            m_AirState.turnSpeed = m_AirTurnSpeed;
            m_AirState.isGrounded = m_IsGrounded;
            m_AirState.fallGravity = m_FallGravity; //TODO：可能也要单独为Fall设置一个Gravity值，总之都要看游戏设计，在程序上简单调整即可，上下重力也主要是为了优化跳跃手感。
            return m_StateMachine.TrySetState(m_AirState);
        }

        

        private void PerformLightAttack()
        {
            if (m_StateMachine.currentState == m_LightAttackState)
            {
                Debug.Log("No Restart");
                m_LightAttackState.restart = false;
                // m_StateMachine.TryResetState(m_LightAttackState);
                m_StateMachine.TrySetState(m_LightAttackState);
            }
            else
            {
                Debug.Log("Restart");
                m_LightAttackState.restart = true;
                m_StateMachine.TrySetState(m_LightAttackState); //其实用Reset也是一样的，兼容的。
            }
        }


        #endregion

        [Serializable]
        private struct Animations
        {
            public AnimationClip idle;
            public AnimationClip run;
            public AnimationClip sprint;
            public AnimationClip jump;
            public AnimationClip roll;
            public AnimationClip stepBack;
            public AnimationClip lightAttack_First;
            public AnimationClip lightAttack_Second;
            public AnimationClip lightAttack_Third;
            public AnimationClip heavyAttack;
            public AnimationClip suddenAttack; //stepBack后撤接上轻攻击，出现向前突进攻击的动作
            public AnimationClip rollAttack; //翻滚后接轻攻击，具有快速反击的效果
            public AnimationClip jumpAttack; //跳劈，在空中按下重击。
        }
    }
}