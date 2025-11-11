using Animancer;
using Unity.Cinemachine;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using ARPGDemo.BattleSystem;
using MyPlugins.AnimationPlayer;

namespace ARPGDemo.ControlSystem_Old
{
    public class PlayerController : CommandConsumer, IPlayerConsumer
    {
        [SerializeField] private ActorObject_OldVersion m_Actor;

        /*TODO：到底是使用更抽象的CommandConsumer还是这里同类型的PlayerController，其实具体的纠结点就在于相机，感觉最好还是用CommandConsumer，因为可以使用类型转换检测其实际类型，如果
        不是的话那就少执行一些逻辑，这样看来也是完全兼容的，不需要特殊考虑。当然也要看具体的游戏设计，总之不是什么难题，纯粹是与具体游戏设计紧密相关的问题*/
        [SerializeField] private CommandConsumer targetConsumer;
        // [SerializeField] private PlayerController targetConsumer;
        // [SerializeField] private CinemachineFreeLook freelook;
        [Header("管理相机")]
        [SerializeField] private CinemachineOrbitalFollow m_Camera;
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
        [SerializeField] private AnimatorAgent animPlayer;
        [SerializeField] private CollisionDetectionMonitor weaponMonitor; //监视和控制武器的碰撞检测

        // public bool isAttack;
        // public bool notMove ;


        //TODO:可以尝试用一个结构体来存储Rig高度和半径的初始值，这样可以避免用一系列字段。
        // private float zoomCounter = 1f;

        private FreeLookZoomer zoomer;
        private Transform camTransform;
        // private Coroutine m_PhysicsCoroutine;
        // private readonly WaitForFixedUpdate m_WaitForFixedUpdate = new();

        #region 状态部分

        [Header("状态机")] //TODO：应该要为状态机类写一个PropertyDrawer。
        [SerializeField] private PlayerStateMachine m_StateMachine;
        [Header("拥有状态")]
        [SerializeField] private PlayerGroundedState m_GroundedState;
        [SerializeField] private PlayerDodgeState m_DodgeState;
        [SerializeField] private PlayerLightAttackState m_LightAttackState;

        [Header("状态相关数据")]
        [SerializeField] private bool m_IsGrounded = false; //就应该一开始就是false，进入时碰到了地面那么就会变为true
        [SerializeField] private float m_JumpInitialSpeed = 5f;
        [SerializeField] private Vector2 m_MoveInput;
        [SerializeField] private Vector3 m_MoveDir; //就是考虑了相机朝向，并且这不是单位向量，而是带有输入量信息的方向向量。
        [SerializeField] private float m_GroundedWalkSpeed = 2f;
        [SerializeField] private float m_GroundedRunSpeed = 5f;
        [SerializeField] private float m_GroundedSprintSpeed = 8f;
        [SerializeField] private float m_GroundedTurnSpeed = 10f;
        [SerializeField] private float m_DodgeMoveSpeed = 15f;

        #endregion

        private void Awake()
        {
            m_Actor = GetComponent<ActorObject_OldVersion>(); //首先获取自己的控制对象

            //TODO：这种控制鼠标显隐的任务都不应该由控制器来做，应该要有专门的管理器。
            Cursor.lockState = CursorLockMode.Locked;
            // freelook ??= GameObject.FindObjectOfType<CinemachineFreeLook>(); //freelook为空才会查找赋值。
            // animPlayer ??= GetComponent<AnimancerComponent>();
            camTransform = Camera.main.transform;
            m_StateMachine ??= new PlayerStateMachine();
            // rb = GetComponent<Rigidbody>();
            physicsHandler = GetComponent<CharacterController>(); //BUG：使用??=似乎又会遇到销毁不同步的bug。
            animPlayer = transform.Find("Model")?.GetComponent<AnimatorAgent>();
            /*TODO：考虑到扩展性，可能不只一把武器，而且也会切换武器，等等————而且为了方便运行时查找等目的，必然会专门制定规范，比如游戏对象的命名规范。*/
            // weaponMonitor = transform.FindRecursively("Weapon").GetComponent<CollisionDetectionMonitor>();

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
            zoomer = new FreeLookZoomer(m_Camera, minZoom, maxZoom);
            weaponMonitor.triggerEnter += OnWeaponTriggerEnter;
        }

        private void OnDisable()
        {
            weaponMonitor.triggerEnter -= OnWeaponTriggerEnter;
        }


        #region 物理相关

        private void OnWeaponTriggerEnter(Collider collider)
        {
            Debug.Log("TriggerEnter" + collider.gameObject.name);
            /*可以在碰撞体组件上指定层级来初步筛选要检测的物体，然后在检测触发的回调方法中进一步筛选。*/
            // collider.GetComponent<SimpleController>().Hurt();
            SimpleController controller = collider.GetComponent<SimpleController>();
            if (controller != null) controller.Hurt();
        }

        #endregion


        private void GetOrCreateAllStates()
        {//Tip：各个状态类就应该
            if ((m_GroundedState = GetComponent<PlayerGroundedState>()) == null)
            {
                m_GroundedState = gameObject.AddComponent<PlayerGroundedState>();
            }
            if ((m_DodgeState = GetComponent<PlayerDodgeState>()) == null)
            {
                m_DodgeState = gameObject.AddComponent<PlayerDodgeState>();
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


        /*Ques：OnUpdate由ControlSystem调用是为了保证先接收命令再执行监测逻辑（输入处理会在FixedUpdate之后、Update之前，也就是与Update同频率，所以放在Update中处理很合理），
        而FixedUpdate主要是用于物理等逻辑计算，在某些状态中可能会需要这些逻辑，所以需要FixedUpdate方法，而其调用顺序似乎没有影响，因为会将比如刚体移动、旋转之类的逻辑全部
        计算完成之后，再统一进行碰撞检测等物理相关逻辑。*/
        private void FixedUpdate()
        {
            m_StateMachine.OnFixedUpdate();
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

        #region 处理状态数据
        //初始化各个状态的数据(主要是一些固定不变的数据，其实对于那些可能变化的数据，通常都会通过控制器中对应的Perform方法来进行处理)
        private void InitializeStateData()
        {
            InitializeGroundedState();
            InitializeDodgeState();
            InitializeLightAttackState();
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

        private void InitializeLightAttackState()
        {
            m_LightAttackState.animPlayer = animPlayer;
            m_LightAttackState.restart = true;
            m_LightAttackState.monitor = weaponMonitor;
        }

        private void UpdateMoveInput()
        {
            m_GroundedState.moveInput = m_MoveInput;
        }

        private void UpdateMoveDir()
        {
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + camTransform.right * m_MoveInput.x;
            m_GroundedState.moveDir = m_MoveDir;
            m_LightAttackState.moveDir = m_MoveDir;
        }

        private void UpdateIsGrounded()
        {
            m_IsGrounded = physicsHandler.isGrounded;
        }

        //Check回调检查状态数据
        // private bool CheckIsGrounded()
        // {

        // }

        #endregion


        /*TODO：Idle和Move在动画上应该是位于同一个混合树中，然后由MoveInput来调整各片段的权重，而不是像现在这样分开，导致代码复杂度提高，而且毫无价值*/

        // private void Idle()
        // {
        //     if (m_StateMachine.CanIdle()) animPlayer.Play(anims.idle);
        // }

        #region 接收命令
        /*Tip：在设想结构中，控制器仅仅作为接收命令的中间层，体现在实现了各个命令接口的接口方法，稍微进行一些处理，然后就调用控制对象即ActorObject类型的m_Actor的一些执行方法，
        比如PerformJump、PerformWalk之类的，这类方法就应该放在ActorObject中，当然状态机、状态管理、各个状态也都应该放在ActorObject中（或许专门定义一个ActorStateManager？），
        总之在该控制器中不应该直接执行任何有关Actor本身的逻辑，只是通知Actor要执行什么行为（通知就是调用Actor的方法）、这些行为是Actor本身就能做的（行为就是Actor自身定义的方法）。*/


        public bool Move(Vector2 _moveInput, MoveCommand.MoveType _moveType = MoveCommand.MoveType.Run)
        {
            switch (_moveType)
            {
                case MoveCommand.MoveType.Walk:
                    PerformGroundedWalk(true);
                    break;
                case MoveCommand.MoveType.WalkCancel:
                    PerformGroundedWalk(false);
                    break;
                case MoveCommand.MoveType.Run:
                    m_MoveInput = _moveInput; //遇到只是WASD的情况才更新MoveInput，因为Walk和Sprint都只是单个按钮，不提供输入值信息，只传入了默认值来占位。
                    PerformGroundedRun();
                    break;
                case MoveCommand.MoveType.Sprint:
                    PerformGroundedSprint(true);
                    break;
                case MoveCommand.MoveType.SprintCancel:
                    PerformGroundedSprint(false);
                    break;
            }

            return true;
        }

        public bool Dodge()
        {
            // Debug.Log("Dodge");


            return PerformDodge();
        }

        public bool skip = false;

        public bool LightAttack()
        {
            PerformLightAttack();

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

        public bool UseItem()
        {
            m_Actor.UseConsumable();
            return true;
        }

        public bool Interact()
        {
            m_Actor.Interact();
            return true;
        }

        #endregion

        /*Tip：通过私有的静态方法，使得相同Class的不同实例能够进行交流。其实还可以将freelook字段设置为静态，感觉其实更好。
        甚至更加特殊化一点，直接为每个要掌控相机的对象都准备一个相机（虚拟相机），到时候直接切换就完事了，还方便做相机过渡之类的效果*/
        private static void DeliverFreeLook(PlayerController current, PlayerController target, CinemachineOrbitalFollow freelook, FreeLookZoomer zoomer)
        {
            // PlayerController target = target as PlayerController;
            if (target != null) //确定要把相机交出去了。
            {
                target.m_Camera = freelook; //类内部的私有静态方法，可以访问类的私有成员？
                target.zoomer = zoomer;
                /*Tip：规定个体对象的根对象的直接子对象中有一个名为“Target”的对象专门作为相机的Follow和LookAt目标对象。
                为了兼容性（鲁棒性？），如果没找到的话就直接将其根对象作为目标对象*/
                Transform cameraTarget = target.transform.Find("Target");
                // freelook.LookAt = freelook.Follow = cameraTarget != null ? cameraTarget : target.transform; 
                freelook.GetComponent<CinemachineCamera>().LookAt = freelook.GetComponent<CinemachineCamera>().Follow = cameraTarget != null ? cameraTarget : target.transform;
                current.m_Camera = null;
            }
        }

        private static void DeliverData(PlayerController _current, CommandConsumer _target)
        {
            PlayerController target = _target as PlayerController;
            if (target != null) //为空的话说明是其他受控对象，那就不执行以下传递数据的逻辑，这是完全兼容的。
            {
                DeliverFreeLook(_current, target, _current.m_Camera, _current.zoomer);
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
            /*TODO：这里对于moveSpeed的赋值，会出现将Walk或Sprint的赋值覆盖的情况，而Walk和Sprint本身在松开时都会将MoveSpeed赋值为RunSpeed，就可以替代Run的赋值功能，
            所以可以在此注释掉。但我始终感觉这种处理方式存在隐患，不过对于这一个具体功能来说暂时也够了，看后续的开发和测试效果如何了，*/
            // m_GroundedState.moveSpeed = m_GroundedRunSpeed;
            m_GroundedState.turnSpeed = m_GroundedTurnSpeed;
            // m_AirState.moveInput = _moveInput;
            return m_StateMachine.TrySetState(m_GroundedState);
        }

        private bool PerformGroundedWalk(bool _enter)
        {
            if (_enter)
            {
                m_GroundedState.moveSpeed = m_GroundedWalkSpeed;
            }
            else
            {
                m_GroundedState.moveSpeed = m_GroundedRunSpeed;
            }

            return true;
        }

        private bool PerformGroundedSprint(bool _enter)
        {
            if (_enter)
            {
                m_GroundedState.moveSpeed = m_GroundedSprintSpeed;
            }
            else
            {
                m_GroundedState.moveSpeed = m_GroundedRunSpeed;
            }

            return true;
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

        private void PerformLightAttack()
        {
            if (m_StateMachine.currentState == m_LightAttackState)
            {
                // Debug.Log("No Restart");
                m_LightAttackState.restart = false;
                // m_StateMachine.TryResetState(m_LightAttackState);
                m_StateMachine.TrySetState(m_LightAttackState);
            }
            else
            {
                // Debug.Log("Restart");
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