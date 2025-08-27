using Unity.Cinemachine;
using Ilumisoft.VisualStateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomController
{
    public class PlayerSimpleController : MonoBehaviour
    {
        #region 字段和属性的声明
        //获取所需相关组件
        public CharacterController controller;
        public Animator animator;
        Transform playerTransform;
        Transform camTransform;
        Camera mainCamera;
        PlayerSoundController playerSoundController; //角色音效播放脚本
        PlayerSensor playerSensor;
        StateMachine stateMachine;

        #region 角色状态
        public PlayerIdleState idleState;
        public PlayerMoveState moveState;
        public PlayerJumpState jumpState;
        public PlayerFallState fallState;
        public PlayerDodgeState dodgeState;
        #endregion

        #region 状态相关变量
        public bool canFall;  //玩家是否可以跌落（非跳跃的下落：矮则正常受重力下落，高则Falling跌落）。主要用于解决下楼梯、下斜坡等鬼畜抖动问题
        public bool justLand; //从Fall状态转出
        public bool dodgeing; //是否处于（短暂的）闪避过程
        public float fallHeight; //整个下落过程经过的高度
        float landBufferMul = 0.1f; //落地缓冲时间系数
        float noLandBufferHeight = 1.5f; //主要是保证普通的平地跳跃落地不会落地缓冲

        #endregion

        #region 获取输入的相关变量
        public Vector2 playerInputVec; //水平输入向量
        //private Vector2 zoomInputVec;//由于只需要回调期间使用输入值改变FOV即可，不需要保存下来，所以不需要该变量
        public bool isWalkPressed; //按住慢走
        public bool isJumpPressed; //按下跳跃
        public bool isDodgePressed; //按下闪避
        //public bool isAimPressed; //按住瞄准
        #endregion

        #region 保存动画参数的哈希值（即为每个字符串参数生成一个唯一的ID，方便统一管理和处理）
        int moveSpeedHash;
        int groundPosHash;
        int turnSpeedHash;
        int verticalVelHash;
        int feetTweenHash;
        int climbHash;
        int jumpHash;
        int fallHash;
        int landHash; //着地（trigger）
        int dodgeHash;
        #endregion

        public enum RotateType
        {
            Click, //按一下就会一直旋转到目标角度
            Hold, //需要持续按住才会旋转到目标角度
            Immediate //按一下会立刻旋转（变换）到目标角度
        }

        #region 动画状态切换阈值
        //注意在声明时初始化则会在类的实例被创建时获取到初始值，也就是说在构造函数运行之前
        //float crouchThreshold = 0f;
        //注意这里的阈值设置，是为了进行多个子混合树之间的转换，
        //如果只是在混合树中多个子片段之间的转换的话，比如慢走和奔跑，这里的值就应该设置为相应的速度值，并且等比例修改动画片段的播放速度，因为是使用RootMotion动画驱动
        float idleThreshold = 0f;
        float walkThreshold = 2f;
        float runThreshold = 6f;
        #endregion

        ////持有物品状态和瞄准状态
        //public enum ArmState
        //{
        //    Normal,
        //    Aim
        //};
        //[HideInInspector]
        //public ArmState armState = ArmState.Normal;

        [Header("运动基本量")]
        public float rotateSpeed = 1000; //转身速度
        public int zoomSpeed = 0; //镜头缩放速度。具体数值主要依赖于输入设备，尤其是鼠标滑轮和手柄右摇杆的输入值相差巨大。
        public Vector2 horizontalSpeed = Vector2.zero; //水平速度，初始为0即静止。但是这里的水平运动使用RootMotion进行动画驱动的，所以暂时用不上
        public Vector2 fovRange = new Vector2(20, 60); //相机FOV范围
        private Vector3 playerMovementWorldSpace = Vector3.zero;
        private Vector3 playerMovement = Vector3.zero; //实际位移向量（距离，包含方向和大小）
        Quaternion targetRotation; //目标角度，端点值，并非差值。

        //基本运动速度
        public float lerpT = 0.5f;//插值速度变化差值，在不同地面上或许有所不同
        private float currentSpeed;
        private float targetSpeed;
        [Header("基本运动速度")]
        public float walkSpeed = 1f; //慢走速度（其实不太慢，还需要调整）
        public float runSpeed = 6f; //奔跑速度（有点太快了，因为这是设置的常规速度）

        //手动添加重力
        public float gravity = -9.8f; //带方向
        public float verticalSpeed = 0f;//垂直方向速度（带方向）
        float fallMultiplier = 2.5f;//下落时加速，增强手感。应该还会添加一个上升的系数
        //float jumpVelocity = 5f;//跳跃初速度

        //滞空左右脚状态
        float feetTween;

        public bool isGrounded = true;
        float groundCheckOffset = 0.5f;//地面检测射线的偏移量

        #region 翻越相关
        public bool isClimbReady;//人物是否处于可以进行攀爬的状态 
        //使用变量值来控制攀爬动画，以代替使用更多的动画参数
        int defaultClimbParameter = 0;
        int vaultParameter = 1;
        int lowClimbParameter = 2;
        int highClimbParameter = 3;
        int currentClimbParameter;
        //调整攀爬动画位置
        Vector3 leftHandPosition;
        Vector3 rightHandPosition;
        Vector3 rightFootPosition;
        #endregion

        #region 速度缓存池定义
        //缓存最近相邻三帧的速度
        static readonly int CACHE_SIZE = 3;
        Vector3[] velCache = new Vector3[CACHE_SIZE];
        int currentCacheIndex = 0;
        Vector3 averageVel = Vector3.zero;
        #endregion


        //跌落的最小高度，小于此高度不会跌落
        float minFallHeight = 0.5f;

        //上一帧的动画normalized时间
        float lastFootCycle = 0f;
        #endregion

        #region 获取输入
        //InputAction.CallbackContext ctx这个结构体不会在回调函数执行完后继续保存，即只会在回调执行期间可以读取数据，然后用其他变量将其保存下来。
        public void GetPlayerMoveInput(InputAction.CallbackContext ctx)
        {
            playerInputVec = ctx.ReadValue<Vector2>(); //读取该动作的值（Move）
        }

        //不用放在Update中每帧检测，只要是绑定按键的都可以注册为相应按键事件的方法
        public void GetPlayerDodgeInput(InputAction.CallbackContext ctx)
        {
            bool keyDown = ctx.phase == InputActionPhase.Started;
            if (keyDown) isDodgePressed = true;
        }

        //public void GetCrouchInput(InputAction.CallbackContext ctx)
        //{
        //    isCrouchPressed = ctx.ReadValueAsButton();
        //}

        //public void GetAimInput(InputAction.CallbackContext ctx)
        //{
        //    isAiming = ctx.ReadValueAsButton();
        //}

        //TODO:应该在按下Alt显形鼠标的同时切换Default Map为UI，当然在很多游戏中也用不到鼠标直接点击UI的功能
        public void GetCursorState(InputAction.CallbackContext ctx)
        {
            switch (ctx.phase)
            {
                case InputActionPhase.Started:
                    Cursor.lockState = CursorLockMode.None;
                    //Cursor.visible = false;
                    break;
                case InputActionPhase.Canceled:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
        }

        public void GetJumpInput(InputAction.CallbackContext ctx)
        {
            //值超过button press threshold则返回true。如果是Button控件，则会考虑自定义的ButtonControl.pressPoint，否则就是按照全局设置中的默认值。
            //isJumpPressed = ctx.ReadValueAsButton();//显然可以用连续值来实现按钮的效果，即用value来实现Button。不过此处用的是Button。
            bool keyDown = ctx.phase == InputActionPhase.Started;
            if (keyDown) isJumpPressed = true;
            //isJumpPressed = ctx.started;
        }

        //获取鼠标滑轮输入，改变相机的FOV以实现缩放镜头的效果
        //TODO：还应该实现手柄的缩放效果
        public void GetZoomInput(InputAction.CallbackContext ctx)
        {
            Vector2 zoomInput = ctx.ReadValue<Vector2>();
            //CinemachineBrain activeBrain = CinemachineCore.Instance.GetActiveBrain(0); //其实可以这样获取到CinemachineBrain
            ICinemachineCamera activeVM = mainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera;
            //Debug.Log("scroll: " + zoomInput.y);
            //if (activeVM != null)
            if (activeVM != null)
            {//注意滑轮的值非常大，都是上百（经测试是固定的，120,240,360,480...），所以在此乘以0.0001，以便在zoomSpeed设置上可以设置为更加直观且准确的整数
                if (activeVM is CinemachineVirtualCamera virtualCamera)
                {//由于有Mathf.Clamp可以直接限定范围，就不用写if-else了
                    virtualCamera.m_Lens.FieldOfView = Mathf.Clamp(virtualCamera.m_Lens.FieldOfView + -zoomInput.y * 0.001f * zoomSpeed, fovRange.x, fovRange.y);
                }//由于fov越大，视野范围越广，事物就越小，所以要取个负号反转，实际上滑轮向上滑是正，向下滑是负
                else if (activeVM is CinemachineFreeLook freelook)
                {
                    freelook.m_Lens.FieldOfView = Mathf.Clamp(freelook.m_Lens.FieldOfView + -zoomInput.y * 0.001f * zoomSpeed, fovRange.x, fovRange.y);
                }
            }
            //如果你不在数字后面加 f 或 F，则默认情况下，带小数点的数字是 double 类型。比如0.0001
            //加上 f 或 F 后，可以显式地将数字指定为 float 类型。比如0.0001f
        }

        public void GetWalkInput(InputAction.CallbackContext ctx)
        {
            /*关于Button类控件的ReadValueAsButton值
            按下（Pressed）：当按钮被按下时，ctx.ReadValueAsButton()将返回true。
            按住（Held）：当按钮持续被按住时，ctx.ReadValueAsButton()将继续返回true。
            抬起（Released）：当按钮被松开时，ctx.ReadValueAsButton()将返回false。
            */
            isWalkPressed = ctx.ReadValueAsButton();
        }
        #endregion

        #region 设置状态动画
        public void SetIdleAnim()
        {
            //animator.SetTrigger(landHash);
            if (justLand) //如果是从Fall状态转换而来。
            {
                if (fallHeight > noLandBufferHeight)
                {
                    StartCoroutine(LandBuffer(fallHeight));
                }
                else animator.SetTrigger(landHash);
                justLand = false; //标记变量，其实这个是触发器的作用，但只能用布尔值来代替，用完即还原
            }

            //如果有落地缓冲，因为没有触发“着地”参数，所以并不会转换到Idle动画，正好这里就把阈值设置好，一旦触发“着地”就可以直接转换到Idle动画
            animator.SetFloat(groundPosHash, idleThreshold, 0.2f, Time.deltaTime); //转换到Idle需要更快，否则已经停下了还看到腿在动就很怪
        }

        public void SetWalkAnim()
        {
            animator.SetFloat(groundPosHash, walkThreshold, 0.1f, Time.deltaTime);
        }

        public void SetRunAnim()
        {
            animator.SetFloat(groundPosHash, runThreshold, 0.1f, Time.deltaTime);
        }

        public void SetMoveAnim()
        {
            //Fall状态只能转换为Idle状态，然后才是从Idle转换到此处的Move，所以只需要在Idle中处理
            //animator.SetTrigger(landHash);
            //if (justLand && fallHeight > noLandBufferHeight) StartCoroutine(LandBuffer(fallHeight));
            //else animator.SetTrigger(landHash);

            if (isWalkPressed)
                animator.SetFloat(groundPosHash, walkThreshold, 0.4f, Time.deltaTime);
            else
                animator.SetFloat(groundPosHash, runThreshold, 0.4f, Time.deltaTime);
            //animator.SetFloat(groundPosHash, runThreshold);
        }

        public void SetJumpAnim()
        {
            animator.SetTrigger(jumpHash); //触发跳跃（有待优化）
        }

        public void SetFallAnim()
        {
            animator.SetTrigger(fallHash);

        }

        public void SetDodgeAnim() //标记是否在闪避过程中。之后应该会在闪避过程中加入其他操作
        {
            if (dodgeing)
            {
                animator.SetBool(dodgeHash, true);
                //dodgeing = true;
            }
            else
            {
                animator.SetBool(dodgeHash, false);
                //dodgeing = false;
            }

        }

        public void DodgeFinish() => dodgeing = false;

        //落地缓冲，其实可以不用参数，直接读取fallHeight，主要是逻辑含义，关系更加直观明了
        //水平运动使用动画的RootMotion，所以此处在触发“着地”参数之前会保持落地动画，其实此时已经切换到了Idle状态，不过即使有输入也不会移动。
        //（其实更好的效果是看到弯腿然后缓慢站起来，而不是一个静止的动画片段）
        IEnumerator LandBuffer(float fallHeight)
        {
            float bufferTime = fallHeight * landBufferMul; //掉落高度乘上落地缓冲时间系数即可得到缓冲时间
            yield return new WaitForSeconds(bufferTime);
            animator.SetTrigger(landHash);
        }
        #endregion

        private void Awake()
        {
            //CreateStatesInstances();
            Cursor.lockState = CursorLockMode.Locked;
            animator = GetComponent<Animator>();
            playerSensor = GetComponent<PlayerSensor>();
            controller = GetComponent<CharacterController>();
            stateMachine = GetComponent<StateMachine>();
            /*自动获取状态实例
            idleState = GetComponentInChildren<PlayerIdleState>();
            moveState = GetComponentInChildren<PlayerMoveState>();
            jumpState = GetComponentInChildren<PlayerJumpState>();
            fallState = GetComponentInChildren<PlayerFallState>();
            dodgeState = GetComponentInChildren<PlayerDodgeState>();
            */
            mainCamera = Camera.main; //返回第一个带有MainCamera标签的启用的Camera相机组件
        }

        /// <summary>
        /// 创建各状态实例（暂时用不到，因为当前状态机的各状态是作为组件挂载的）
        /// </summary>
        private void CreateStatesInstances()
        {
            idleState = new PlayerIdleState();
            moveState = new PlayerMoveState();
            jumpState = new PlayerJumpState();
            fallState = new PlayerFallState();
            dodgeState = new PlayerDodgeState();
        }

        // Start is called before the first frame update
        void Start()
        {

            playerTransform = transform;//提前取值，避免重复访问
            targetRotation = transform.rotation; //初始当然保持和角色本身相同，即不变

            //记录动画参数的哈希值即id，使用变量表示，方便且安全。在调用设置参数的相关方法时就可以传入该id而不用字符串
            groundPosHash = Animator.StringToHash("地面姿态");
            moveSpeedHash = Animator.StringToHash("移动速度");
            turnSpeedHash = Animator.StringToHash("转弯速度");
            verticalVelHash = Animator.StringToHash("垂直速度");
            feetTweenHash = Animator.StringToHash("左右脚");
            climbHash = Animator.StringToHash("攀爬方式");
            jumpHash = Animator.StringToHash("跳跃");
            landHash = Animator.StringToHash("着地"); //Trigger
            dodgeHash = Animator.StringToHash("闪避"); //Bool
            fallHash = Animator.StringToHash("下落"); //Trigger

            camTransform = mainCamera.transform;
            //Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update()
        {
            //CheckGround();

            CorrectAnimatorParaFloat(); //如果目标是0就需要修正，因为浮点值很难判断相等，就会一直计算不断接近但始终不相等，如果不是0，就不需要修正
        }

        /// <summary>
        /// 修正浮点型动画参数
        /// </summary>
        void CorrectAnimatorParaFloat()
        {
            float groundPos = animator.GetFloat(groundPosHash);
            if (Mathf.Abs(groundPos - idleThreshold) <= 0.01f)
            {
                groundPos = idleThreshold;
                animator.SetFloat(groundPosHash, groundPos);
            }

            else if (Mathf.Abs(groundPos - walkThreshold) <= 0.01f)
            {
                groundPos = walkThreshold;
                animator.SetFloat(groundPosHash, groundPos);
            }
            else if (Mathf.Abs(groundPos - runThreshold) <= 0.01f)
            {
                groundPos = runThreshold;
                animator.SetFloat(groundPosHash, groundPos);
            }
        }

        /// <summary>
        /// 更新isGrouded是否在地面和canFall是否下落
        /// </summary>
        void CheckGround()
        {
            /*以人物中心（一般为脚底）向上偏移groundCheckOffset处为圆心，以控制器半径为半径，
             向下发出球形射线，如果投射的最大长度maxDistance为groundCheckOffset，则是
            在人物脚底做碰撞检测，显然球形射线底部距离地面太多，所以减去半径，同时由于
            脚底和地面存在skinWidth的距离，所以再加上两倍skinWidth即可保证地面检测效果
            发射射线须知：确定好发射起点和发射方向，然后是紧紧相关目标的最大发射距离maxDistance，
            再根据具体目标决定射线范围大小，此处即半径，当然这与maxDistance结合考虑。
        
             检测只是为了获取某些信息，以便在程序中使用，而落地与地面发生碰撞，直接停在地面上，
            这与如何检测并无关系。
            所以在加入Fall掉落动画后落地时会部分陷入再回到地面平齐，只是动画效果，而实际
            位置就是与地面平齐，但是如何在保留该动画的情况下去掉这个陷入情况呢？*/
            //if (Physics.SphereCast(playerTransform.position + (Vector3.up * groundCheckOffset), controller.radius, Vector3.down, out RaycastHit hit, groundCheckOffset - controller.radius + 2 * controller.skinWidth))
            //{
            //    isGrounded = true;
            //}
            //注意这个变量只是表示上一次调用控制器的Move方法时是否碰到了地面（下面发生碰撞），有时候是靠RootMotion运动而不是用Move方法。
            //确实有bug，如果单纯用控制器的isGrounded属性的话，还是得加上一个射线检测处理其他情况，不过isGrounded倒是可以在某些情况下做一个快捷判断
            //前者粗判，后者细判
            if (controller.isGrounded || Physics.SphereCast(playerTransform.position + (Vector3.up * groundCheckOffset), controller.radius, Vector3.down, out RaycastHit hit, groundCheckOffset - controller.radius + 2 * controller.skinWidth))
            {
                isGrounded = true; //在地面上
                verticalSpeed = 0f; //处理竖直速度
            }
            else//球形射线没检测到地面，就再用直射线检测离地面有多高（用直线是否准确，还有待测试）
            {
                //Debug.Log("a");
                isGrounded = false;
                canFall = !Physics.Raycast(playerTransform.position, Vector3.down, minFallHeight); //脚底往下的fallHeight高度都没到地面，则跌落
            }
        }

        public float CalcGravity(float vSpeed)
        {
            if (vSpeed > 0) //上升
            {
                vSpeed += gravity * Time.deltaTime;
            }
            else //下降
            {
                vSpeed += gravity * fallMultiplier * Time.deltaTime; //加速下降，除了跳跃下落以外，应该非跳跃下落也可以就这样加速
            }

            return vSpeed;
        }

        //需要用到在该类中定义的重力变量gravity，所以在此定义重力方法
        public float CalcUpGravity(float vSpeed) => vSpeed + gravity * Time.deltaTime;
        public float CalcDownGravity(float vSpeed) => vSpeed + gravity * fallMultiplier * Time.deltaTime;

        /*为何用姿态变量playerPosture判断仍会出现连续跳跃跳帧的情况？因为正常跳跃是从站立姿态（也
         可以是下蹲姿态）转换到滞空姿态，在此过程中postureHash值在0.1s内从1增加到2.1
         即从standThreshold增加到JumpingThreshold，那么由于引擎本身的动画混合功能就不会
        出现跳帧的感觉，也就实现了流畅地播放动画以表现从站立到跳跃的过程。
        跳跃后落地瞬间，isGrounded变成true，就会使playerPosture变成Stand，
        但此时postureHash仍然是2.1，如果此时瞬间起跳就不会有postureHash值的变化过程，
        即失去了保证动画流畅切换的动画混合过程，也就表现为抽搐、跳帧的感觉。而且要对立考虑，
        即不能在落地时直接将postureHash值设为1，因为落地也需要动画混合过程。也不能去掉
        postureHash值变化时的短暂过渡时间，因为在站和蹲以及其他过渡中同样需要。
        但是，如果改为postureHash参数值判断的话，由于从Jumping到Stand有一个0.1s的转换过程，
        所以就无法在落地的一瞬间再次起跳，这样就对手感有所破坏。
        姿态变量playerPosture和姿态参数值postureHash不得不分开处理，但须知必须统一管理*/
        /*之前写的Jump方法，涉及到攀爬动作的转换
        void Jump()
        {
            //if (controller.isGrounded && isJumping)
            //if (isGrounded)

            //if (isGrounded && playerPosture != PlayerPosture.Jumping)
            //if (isGrounded && Mathf.Abs(animator.GetFloat(postureHash) - standThreshold) <= 0.5f)
            //if (isGrounded)
            //限制只有在站立，即下蹲、滞空、着地都不能跳，则通过限制姿态变量就从跳跃状态的根源即竖直速度的获得进行了限制，这才能让jumpCD真正发挥作用
            if (playerPosture == PlayerPosture.Stand)
            {
                float velOffset;
                //switch-case语句代替多个if-else简洁处理可攀爬范围的偏移量
                switch (locomotionState)
                {
                    case LocomotionState.Run:
                        velOffset = 1f;
                        break;
                    case LocomotionState.Walk:
                        velOffset = 0.5f;
                        break;
                    case LocomotionState.Idle:
                        velOffset = 0f;
                        break;
                    default:
                        velOffset = 0f;
                        break;
                }
                //就是对于跳跃这一操作在不同情况下的特殊处理
                //问题是，在按下跳跃键之时检测，是否会导致卡顿问题？
                switch (playerSensor.ClimbDetect(playerTransform, playerMovement, velOffset))
                {
                    case PlayerSensor.NextPlayerMovement.jump:
                        Debug.Log("jump");
                        // 1/2 * a * t^2 = h(一端速度为0),v = at
                        //直接通过最大高度计算初速度
                        //在跳跃该帧给一个初始高度，以免下一帧仍然判定isGrounded为真而无法进入跳跃状态
                        transform.position = transform.position + new Vector3(0, 0.1f, 0);
                        verticalSpeed = Mathf.Sqrt(-2 * gravity * maxHeight);
                        //verticalVelocity = jumpVelocity;
                        //isJumping = false;

                        //添加相同的一套跳跃动画，取个随机数，连续跳跃时动画就可以产生左右脚的区别，更加灵活
                        //feetTween = Random.Range(-1f, 1f);
                        //获取相应控制器中第一个动画层级的动画播放进度（使用Repeat函数使其在0~1之间循环）
                        feetTween = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1);
                        //animator.SetFloat("左右脚", feetTween);
                        //看混合树中设置，1代表右脚，-1则左脚，而水平运动片段的中间以前右脚在后，以后左脚在后
                        feetTween = feetTween < 0.5f ? 1 : -1;

                        if (locomotionState == LocomotionState.Run)
                        {
                            feetTween *= 3;
                        }
                        else if (locomotionState == LocomotionState.Walk)
                        {
                            feetTween *= 2;
                        }
                        else
                        {
                            feetTween = Random.Range(0.5f, 1f) * feetTween;
                        }
                        break;
                    case PlayerSensor.NextPlayerMovement.climbLow:
                        Debug.Log("climbLow");
                        //以攀爬动画的左手作为锚点，控制攀爬动画的高度变化
                        //使用叉乘计算方向
                        //左手的位置向左移动0.3m
                        leftHandPosition = playerSensor.ledge + Vector3.Cross(-playerSensor.climbHitNormal, Vector3.up) * 0.3f;
                        isClimbReady = true;
                        currentClimbParameter = lowClimbParameter;
                        break;
                    case PlayerSensor.NextPlayerMovement.climbHigh:
                        Debug.Log("climbHigh");
                        //右手位置向右移动0.3m
                        rightHandPosition = playerSensor.ledge + Vector3.Cross(playerSensor.climbHitNormal, Vector3.up) * 0.3f;
                        //右脚在顶端以下1.2m
                        rightFootPosition = playerSensor.ledge + Vector3.down * 1.2f;
                        currentClimbParameter = highClimbParameter;
                        isClimbReady = true;
                        break;
                    case PlayerSensor.NextPlayerMovement.vault:
                        Debug.Log("vault");
                        rightHandPosition = playerSensor.ledge;
                        currentClimbParameter = vaultParameter;
                        isClimbReady = true;
                        break;
                }
            }
        
        }
        */

        /// <summary>
        /// 用于玩家转身（引入状态机后就由状态自己调用，不由控制器调用了）
        /// </summary>
        /// <returns></returns>
        public Quaternion RotatePlayer(float rotateSpeed, RotateType rotateType)//用于移动过程中的转向操作(因为可以自由选择移动的方向所以才会有这样专门处理转向的部分)
        {
            //每帧都要执行旋转，有方向输入的时候就更新targetRotation，这样按一下就会逐渐旋转到目标位置，不用按住
            //playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            //以避免没有输入时出现复位的情况
            if (playerInputVec.Equals(Vector2.zero))
            {//只要没有方向输入就直接返回player当前rotation，只是Click类型会保持进行一次旋转
                if (rotateType == RotateType.Click) //比如水平移动，只要按一下就会每帧旋转到目标角度
                {
                    playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                }

                return playerTransform.rotation;
            }

            //只要有方向输入，就要计算旋转的目标角度

            //该方法根据给定的两个参数forward和upward值生成一个表示角度值的四元数
            //Debug.Log(playerMovement.y);//默认为0
            /*此处表示的是方向，且以世界坐标系为参考来改变本地坐标系，只是不得不以数值表示而已，区别于数值表示的角度
            各个直接因素理应全面考虑，但由于存在代表性，即决定因素，所以会出现看似一些考虑一些不考虑的情况
            坐标轴、轴平面、位移、旋转
            LookRotation方法第二个参数upward默认为Vector3.up即（0,1,0），
            其原理可以粗略认为：游戏对象本地坐标系Z轴朝向forward方向，以此为前提条件使
            其Y轴朝向upward。更准确来说，X轴方向为forward与upward的叉积cross product，
            即通过右手定则从forward转向upward大拇指指向即为X轴方向，而Y轴则为此时已经
            确定的X轴和Z轴的叉积。*/
            //Quaternion targetRotation = Quaternion.LookRotation(playerMovement, new Vector3(1,-1,0));
            //Quaternion targetRotation = Quaternion.LookRotation(playerMovement, Vector3.up);

            //坐标是根本，其次可得长度和角度
            //取y为0则得水平投影，归一化(方向不变，长度为1)即为方向向量。由此取得镜头前向。
            Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            //从作用上来看，乘以输入值无非就是在该方向上前进还是后退或是不动。(键盘输入只有0和正负1，但手柄输入是-1~+1)
            //向量加法。还是从向量运算法则及其数形性质入手理解才行。
            playerMovement = camFoward * playerInputVec.y + camTransform.right * playerInputVec.x;
            //将向量从世界坐标系映射到指定对象的本地坐标系，方向不变，只是坐标变化
            //playerMovement = playerTransform.InverseTransformVector(playerMovement);
            //Quaternion targetRotation = Quaternion.LookRotation(playerMovement, Vector3.up);
            targetRotation = Quaternion.LookRotation(playerMovement, Vector3.up);

            if (rotateType == RotateType.Immediate) //立刻转向（主要应用在卡通风格的游戏中，轻便）
            {
                playerTransform.rotation = targetRotation;
            }
            else
            {
                playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }

            return targetRotation;
            //playerMovement = targetRotation.eulerAngles;
            //渐转
            //playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            //顿转
            //playerTransform.rotation = targetRotation;
        }

        /*
        /// <summary>
        /// 播放脚步声
        /// </summary>
        void PlayFootStep()
        {
            if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)
            {
                if (locomotionState == LocomotionState.Walk || locomotionState == LocomotionState.Run)
                {
                    float currentFootCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f); ;
                    if ((lastFootCycle < 0.1 && currentFootCycle >= 0.1) || (currentFootCycle >= 0.6 && lastFootCycle < 0.6))
                    {
                        playerSoundController.PlayFootStep();
                    }
                    lastFootCycle = currentFootCycle;
                }
            }
        }
        */

        /// <summary>
        /// 计算前三帧的速度平均值
        /// </summary>
        /// <param name="newVel">当前帧的速度平均值</param>
        /// <returns>平均速度</returns>
        Vector3 AverageVel(Vector3 newVel)
        {//处理在滞空状态下的水平运动。每帧在OnAnimatorMove方法中都会调用，所以可以成功缓存而得到平均值
            velCache[currentCacheIndex] = newVel;
            currentCacheIndex++;
            currentCacheIndex %= CACHE_SIZE;//取模防溢出
            Vector3 average = Vector3.zero;
            foreach (Vector3 vel in velCache)
            {
                average += vel;
            }
            return average / CACHE_SIZE;
        }

        /*把对人物移动的控制权从root motion动画中拿回来,须注意的是这仍然是基于动画驱动的，
         * 所以计算出速度后赋值给控制参数后运行相应动画就可以实现人物移动了
         只是对于驱动方式的控制在该方法中进行。除此之外可以直接使用代码驱动，也可以借助物理引擎
        驱动。
        同样，由于使用动画驱动，在跳跃时本应存续跳跃前的速度，但跳跃动画的位移并不等于或者说远小于
        地面运动动画的位移,所以要把运动分为滞空和非滞空情况来处理*/
        //比较别扭的是，由于要使用RootMotion功能，所以需要在该方法中实现角色的移动，但是本来应该由各个状态处理各自的移动。不过模块化写成各个函数也还勉强。
        private void OnAnimatorMove() //实现了该方法之后，Animator的applyRootMotion属性就不起作用了
        {


            //其实可以用switch-case来代替大量的if-else，但是case的条件必须是常量，而在基类StateBehaviour中声明的是不是常量，后面再看咋改一下。
            //跳跃和下落，其实就是滞空。
            if (stateMachine.CurrentState == jumpState.StateID || stateMachine.CurrentState == fallState.StateID)
            {
                controller.enabled = true;
                //averageVel.y = verticalSpeed; //averageVel用于继承水平速度
                //Vector3 playerDeltaMovement = averageVel * Time.deltaTime; //位移向量
                //为了将保存的水平速度始终朝向角色面朝的方向，只需要用角色前向方向向量乘以水平速度的大小即其向量长度即可得到，基本的数学原理
                Vector3 airSpeed = playerTransform.forward * horizontalSpeed.magnitude;
                airSpeed.y = verticalSpeed;
                //Vector3 airSpeed = new Vector3(horizontalSpeed.x, verticalSpeed, horizontalSpeed.y);
                controller.Move(airSpeed * Time.deltaTime);
                isGrounded = controller.isGrounded; //每次调用Move之后立刻检查，则可以准确反映每一帧是否在地面。如果离地了别忘了检测高度
                if (isGrounded) verticalSpeed = 0f;
                else canFall = !Physics.Raycast(playerTransform.position, Vector3.down, minFallHeight); //脚底往下的fallHeight高度都没到地面，则跌落
            }
            //水平移动状态
            else if (stateMachine.CurrentState == moveState.StateID)
            {
                Vector3 playerDeltaMovement = animator.deltaPosition; //取得运动动画在当前帧的位置变化
                //Vector2 hSpeedDir = new Vector2(transform.forward.x, transform.forward.z).normalized; //当前在移动，说明面向方向就是移动方向
                //Vector2 hSpeedMag = new Vector2(playerDeltaMovement.x, playerDeltaMovement.z);
                //别忘了除以相邻帧的间隔时间，才是速度
                //horizontalSpeed = AverageVel(hSpeedDir * hSpeedMag / Time.deltaTime); //虽然依赖于动画驱动，但是需要记录水平速度，以便改变状态时比如跳跃和下落需要继承速度
                Vector3 animVelocity = playerDeltaMovement / Time.deltaTime; //deltaPosition除以deltaTime
                horizontalSpeed = new Vector2(animVelocity.x, animVelocity.z);

                //verticalSpeed通常为0，但是遇到比如下楼梯以及凹凸不平的地面之类的低下落，就需 要计算一下下落速度
                playerDeltaMovement.y = verticalSpeed * Time.deltaTime;
                controller.Move(playerDeltaMovement);
                isGrounded = controller.isGrounded;
                if (isGrounded) verticalSpeed = 0;
                else canFall = !Physics.Raycast(playerTransform.position, Vector3.down, minFallHeight); //脚底往下的fallHeight高度都没到地面，则跌落
                //averageVel = AverageVel(animator.velocity);

            }
            else if (stateMachine.CurrentState == dodgeState.StateID)
            {
                //controller.enabled = false; //碰撞体被禁用意味着通过该碰撞体检测的攻击都无效了，也就如此实现（闪避时）无敌帧
                animator.ApplyBuiltinRootMotion(); //应用内置的根运动
            }

            /*
            //在攀爬时禁用CharacterController，以避免受到重力和碰撞的影响，同时转用引擎本身的RootMotion通过攀爬动画驱动人物攀爬，并且随后在其他状态就恢复控制。
            if (playerPosture == PlayerPosture.Climbing)
            {
                controller.enabled = false;
                animator.ApplyBuiltinRootMotion(); //应用内置的根运动
            }
            else if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)
            { //就是在地面。标记变量用于转换状态，状态才能用来划分逻辑
                controller.enabled = true;

                //deltaPosition受帧率影响，显然帧率越高它越小，帧率越低它越大
                //Animator.deltaPosition返回的是在当前动画帧中，由动画引起的角色位移。
                //只有在applyRootMotion启用的情况下，Unity才会计算并返回deltaPosition的值，因为这时动画的根运动才会被实际应用到角色上。
                //水平位移和竖直位移
                Vector3 playerDeltaMovement = animator.deltaPosition;
                playerDeltaMovement.y = verticalSpeed * Time.deltaTime;//加上垂直速度（实现跳跃、下落等Y轴上的功能）
                //形而下的模型和形而上的Transform
                //controller.Move(animator.deltaPosition);
                controller.Move(playerDeltaMovement);
                averageVel = AverageVel(animator.velocity);
                //isGrounded = controller.isGrounded;
            }
            //滞空状态下直接使用代码驱动，而非动画驱动
            else
            {
                controller.enabled = true;

                //沿用地面速度，比如使用最后一帧的速度（容易受到地形、操作等影响，破坏手感）所以此处选择沿用离地前几帧的平均速度
            averageVel.y = verticalSpeed;//单独处理竖直速度，因为不能使用动画本身的竖直位移
                Vector3 playerDeltaMovement = averageVel * Time.deltaTime;
                controller.Move(playerDeltaMovement);
                //isGrounded = controller.isGrounded;
            }
            */
        }
    }
}
