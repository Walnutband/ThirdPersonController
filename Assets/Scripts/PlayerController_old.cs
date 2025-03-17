using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_old : MonoBehaviour
{
    #region 字段和属性的声明
    //获取所需相关组件
    public CharacterController controller;
    private Animator animator;
    private Transform playerTransform;
    private Transform camTransform;
    Camera mainCamera;
    PlayerSoundController playerSoundController; //角色音效播放脚本
    PlayerSensor playerSensor;

    #region 获取输入
    private Vector2 playerInputVec;
    //private Vector2 zoomInputVec;//由于只需要回调期间使用输入值改变FOV即可，不需要保存下来，所以不需要该变量
    private bool isRunPressed;
    private bool isCrouchPressed;
    private bool isAimPressed;
    [SerializeField]
    private bool isJumpPressed;
    #endregion

    #region 保存动画参数的哈希值（即为每个字符串参数生成一个唯一的ID，方便统一管理和处理）
    int postureHash;
    int moveSpeedHash;
    int turnSpeedHash;
    int verticalVelHash;
    int feetTweenHash;
    int climbHash;
    #endregion
    //玩家姿态
    public enum PlayerPosture
    {
        //Crouch,//下蹲姿态
        Stand,//站立姿态
        Falling,//掉落姿态
        Jumping,//滞空姿态
        Landing,//着陆姿态（玩家刚落地还不能起跳的状态）
        Climbing
    };
    //[HideInInspector]
    public PlayerPosture playerPosture = PlayerPosture.Stand;

    //注意在声明时初始化则会在类的实例被创建时获取到初始值，也就是说在构造函数运行之前
    float crouchThreshold = 0f;
    float standThreshold = 1f;
    float JumpingThreshold = 2.1f;//阈值为2，但为了兼容浮点值的浮动误差，需要略大值
    float landingThreshold = 1f;

    //运动状态
    public enum LocomotionState
    {
        Idle,
        Walk,
        Run
    }
    [HideInInspector]
    public LocomotionState locomotionState = LocomotionState.Idle;

    ////持有物品状态和瞄准状态
    //public enum ArmState
    //{
    //    Normal,
    //    Aim
    //};
    //[HideInInspector]
    //public ArmState armState = ArmState.Normal;

    public float rotateSpeed = 1000;
    public int zoomSpeed = 0;
    public Vector2 fovRange = new Vector2(20, 60);
    private Vector3 playerMovementWorldSpace = Vector3.zero;
    private Vector3 playerMovement = Vector3.zero;
    Quaternion targetRotation;

    //基本运动速度
    public float lerpT = 0.5f;//插值速度变化差值，在不同地面上或许有所不同
    private float currentSpeed;
    private float targetSpeed;
    private float crouchSpeed = 2.5f;
    private float walkSpeed = 3f;
    private float runSpeed = 6f;

    //手动添加重力
    public float gravity = -9.8f;
    float verticalVelocity = 0f;//垂直方向速度
    //float jumpVelocity = 5f;//跳跃初速度

    public float maxHeight = 1.5f;//最大跳跃高度，即可通过高度控制跳跃速度

    //滞空左右脚状态
    float feetTween;

    public bool isGrounded = true;
    float groundCheckOffset = 0.5f;//地面检测射线的偏移量

    //跳跃的CD设置
    float jumpCD = 0.4f;
    //是否处于跳跃CD状态
    public bool isLanding;

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

    float fallMultiplier = 1.5f;//下落时加速，增强手感

    //玩家是否可以跌落（非跳跃的下落：矮则正常受重力下落，高则Falling跌落）
    //主要用于解决下楼梯、下斜坡等鬼畜抖动问题
    bool couldFall;
    //跌落的最小高度，小于此高度不会跌落
    float fallHeight = 0.5f;

    //上一帧的动画normalized时间
    float lastFootCycle = 0f;
    #endregion

    #region 输入相关（操作方法）
    //InputAction.CallbackContext ctx这个结构体不会在回调函数执行完后继续保存，即只会在回调执行期间可以读取数据，然后用其他变量将其保存下来。
    public void GetPlayerMoveInput(InputAction.CallbackContext ctx)
    {
        playerInputVec = ctx.ReadValue<Vector2>(); //读取该动作的值（Move）
    }

    //不用放在Update中每帧检测，只要是绑定按键的都可以注册为相应按键事件的方法
    public void GetPlayerRunInput(InputAction.CallbackContext ctx)
    {
        //值超过button press threshold则返回true。如果是Button控件，则会考虑自定义的ButtonControl.pressPoint，否则就是按照全局设置中的默认值。
        isRunPressed = ctx.ReadValueAsButton(); //显然可以用连续值来实现按钮的效果，即用value来实现Button。不过此处用的是Button。
        /*
        ////isRunning = ctx.ReadValue<float>() > 0 ? true : false; 
        //switch (ctx.phase)
        //{
        //    case InputActionPhase.Started:
        //        isRunning = true;
        //        //Debug.Log(isRunning);
        //        break;
        //    case InputActionPhase.Canceled:
        //        isRunning = false;
        //        //Debug.Log(isRunning);
        //        break;
        //}
        */
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
        //isJumping = ctx.ReadValueAsButton();
        //需要防止按住跳跃键就自动连续跳跃,或者提前按跳跃键会在落地后再次跳起（不要混淆预输入机制）
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
                //Debug.Log("a");
                Jump();
                isJumpPressed = true;
                break;
            case InputActionPhase.Canceled:
                isJumpPressed = false;//松开时取假则重力加速度增大到下落时一样
                break;
        }
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
    #endregion

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        playerSensor = GetComponent<PlayerSensor>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main; //返回第一个带有MainCamera标签的启用的Camera相机组件
    }

    // Start is called before the first frame update
    void Start()
    {

        playerTransform = transform;//提前取值，避免重复访问
        targetRotation = transform.rotation; //初始当然保持和角色本身相同，即不变

        //记录动画参数的哈希值即id，使用变量表示，方便且安全。在调用设置参数的相关方法时就可以传入该id而不用字符串
        postureHash = Animator.StringToHash("玩家姿态");
        moveSpeedHash = Animator.StringToHash("移动速度");
        turnSpeedHash = Animator.StringToHash("转弯速度");
        verticalVelHash = Animator.StringToHash("垂直速度");
        feetTweenHash = Animator.StringToHash("左右脚");
        climbHash = Animator.StringToHash("攀爬方式");

        camTransform = mainCamera.transform;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //一转一动方法共同完成移动操作
        CheckGround();
        SwitchPlayerStates();
        CalculateGravity();
        //Jump();
        RotatePlayer();//获取视角方向，使用playerMovement储存
        //MovePlayer();
        //SwitchPlayerStates();
        SetupAnimation();
        //PlayFootStep();
    }

    //自定义地面检测方法
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
        if (controller.isGrounded) isGrounded = true; //在地面上
        else//球形射线没检测到地面，就再用直射线检测离地面有多高
        {
            //Debug.Log("a");
            isGrounded = false;
            couldFall = !Physics.Raycast(playerTransform.position, Vector3.down, fallHeight); //脚底往下的fallHeight高度都没到地面，则跌落
        }
    }

    //计算重力影响
    //直接反映在垂直速度上，由于直接使用的控制器，没有用到刚体那样的物理引擎，重力影响就仅此而已了
    void CalculateGravity()
    {//CharacterController.isGrounded这个属性描述的是上一次调用 CharacterController.Move 时，角色是否接触到地面,所以在地面需要对y速度取负而非0
        //if (isGrounded)
        if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)//状态机编程才是更标准更通用的方法
        {
            if (!isGrounded)//处理短下落过程中的竖直速度变化。脱离了水平面但又没有且不应该进入掉落状态
            {
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime; //这里得到的值就是这一帧的竖直高度变化了
            }
            //else if (!isJumping)
            else
            {
                //TODO:其实应该加上下面这一行的，但是由于射线检测的误差导致前一帧获得了竖直速度，后一帧就直接变负数了,
                /*所以在Jump方法中获得跳跃的初速度前直接将人物y位置上升0.1，这样就可以避免下一帧
                 射线检测到地面将isGrounded判定为true而导致速度直接变负，表现出来就是根本跳不起来*/
                //verticalVelocity = gravity * Time.deltaTime;
                verticalVelocity = -0.01f;
            }
            //if (playerPosture == PlayerPosture.Stand)
            //    animator.SetFloat(postureHash, standThreshold);
            //else if (playerPosture == PlayerPosture.Crouch)
            //    animator.SetFloat(postureHash, crouchThreshold);
            //verticalVelocity = 0f;


        }
        else
        {
            //if (verticalVelocity <= 0f)
            /*加上了根据按住跳跃键决定跳跃高度的功能（配合GetJumpInput方法）
             除了手动跳跃导致的滞空，还要考虑到从高处落下的滞空，而前者需要将速度归零，
            后者则无需，所以不能去掉竖直速度的条件判断*/
            if (verticalVelocity <= 0f)
            {//下落阶段加速，手感更好，尽管不符合现实物理
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            //处理跳跃高度与按键时长的对应
            else if (!isJumpPressed) //没有按住
            {
                verticalVelocity = 0f;
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }//机制就是按住则按照正常重力减速，没按住就直接变为0同时开始下降，其实应该稍微做一个短缓冲，否则松开跳跃键时就像头撞到空气一样。
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        //isGrounded = controller.isGrounded;
    }

    void Jump()
    {
        //if (controller.isGrounded && isJumping)
        //if (isGrounded)
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

        //if (isGrounded && playerPosture != PlayerPosture.Jumping)
        //if (isGrounded && Mathf.Abs(animator.GetFloat(postureHash) - standThreshold) <= 0.5f)
        //if (isGrounded)
        /*限制只有在站立，即下蹲、滞空、着地都不能跳，则通过限制姿态变量就从跳跃状态
         的根源即竖直速度的获得进行了限制，这才能让jumpCD真正发挥作用*/
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
                    verticalVelocity = Mathf.Sqrt(-2 * gravity * maxHeight);
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





    void RotatePlayer()//用于移动过程中的转向操作(因为可以自由选择移动的方向所以才会有这样专门处理转向的部分)
    {
        //每帧都要执行旋转，有方向输入的时候就更新targetRotation，这样按一下就会逐渐旋转到目标位置，不用按住
        playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        //以避免没有输入时出现复位的情况
        if (playerInputVec.Equals(Vector2.zero))
            return;
        //playerMovement.x = playerInputVec.x;
        //playerMovement.z = playerInputVec.y;

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
        //playerMovement = targetRotation.eulerAngles;
        //渐转
        //playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        //顿转
        //playerTransform.rotation = targetRotation;
    }

    void MovePlayer()
    {
        /*其实这种渐变性运动是写实风格的游戏使用的，更加沉重，也更加流畅
         * 而卡通风格游戏往往采用顿变性运动，更加轻便，也更加突兀。*/
        //通过插值算法实现渐变过程，此处即加速过程
        if (playerInputVec.magnitude <= 0.01f)
        {
            targetSpeed = 0f;
            //如果是0即无输入则直接停下，当然如果在比如冰面上那么理应向前滑动一段距离
            /*如果将0作为targetSpeed参与到插值中，其中Lerp方法也会由此从高到低插值
             ，但是限于计算机本身的特性，0是整数，而速度值是浮点数，而浮点数基本不可能
            完全等于一个整数，就像Vertical Speed为3，但是在Animator窗口中看到的插值
            结果也只是2.999999...，所以在减速时也只会减到无限接近于0，直到达到计算机
            表示数值的边界。*/

            //先设currentSpeed值保证在特殊情况的处理下也有相同的基本流程，否则容易出现一些意想不到的错误
            //currentSpeed = 0f;
            //animator.SetFloat("Vertical Speed", currentSpeed);
            //return;

            /*在接近于0时直接条件判断为0处理即可，这样既可以保留停下脚步时的渐变，
             又可以准确控制速度值。并且要放在无输入的情况下处理，否则可能会影响
            渐变加速的过程。*/
            if (currentSpeed <= 0.1f)
            {
                currentSpeed = 0f;
                animator.SetFloat("Vertical Speed", currentSpeed);
                return;
            }


            //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, -lerpT);
        }
        else
        {
            targetSpeed = isRunPressed ? runSpeed : walkSpeed;
            //键盘输入就是0和+1和-1，所以读取输入本质是读取的指令，具体的行动还需要定义其他量才行
            //targetSpeed = playerInputVec.magnitude;
        }
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, lerpT);
        animator.SetFloat("Vertical Speed", currentSpeed);
    }

    /// <summary>
    /// 用于切换玩家的各种状态
    /// </summary>
    void SwitchPlayerStates()
    {/*须知，在实际游戏操作中看似各种状态随意转换，其实都要遵循严格的转入转出规则，
      在实际开发中表现为，需要首先明确想要达到的视觉效果，但一定谨防被视觉效果迷惑，
        比如在此，攀爬后跳跃，实际上是从Climbing到Stand再到Jump，如果被视觉效果迷惑
        而编程，要么出现大量根本没法修的bug，要么压根编写不下去即运行都没法。*/
        //状态机写法，以状态作为入口，在各个状态中处理状态之间的变换,并且不应在其他地方进行状态转换
        switch (playerPosture)
        {
            case PlayerPosture.Stand:
                if (verticalVelocity > 0f)
                {
                    playerPosture = PlayerPosture.Jumping;
                }
                else if (!isGrounded && couldFall)
                {
                    playerPosture = PlayerPosture.Falling;
                }
                //else if (isCrouchPressed)
                //{
                //    playerPosture = PlayerPosture.Crouch;
                //}
                //只有在站立姿态下才可以攀爬，从状态切换的该函数体内容就能直观看到这一点
                //显然也只有对于人物的状态切换了如指掌才能写出这样完整完美的状态机
                else if (isClimbReady)
                {
                    playerPosture = PlayerPosture.Climbing;
                    //不在此处设为false，而在其他各处都要设置，因为只有在攀爬的一刻才会为真
                    //isClimbReady = false;
                }
                //isClimbReady = false;
                break;
            /*
            case PlayerPosture.Crouch:
                //下蹲时无法跳跃
                if (!isGrounded && couldFall)
                {
                    playerPosture = PlayerPosture.Falling;
                }
                else if (!isCrouchPressed)
                {
                    playerPosture = PlayerPosture.Stand;
                }
                //isClimbReady = false;
                break;
            */
            //掉落和跳跃落下的处理相同
            case PlayerPosture.Falling:
                if (isGrounded)
                {
                    StartCoroutine(CoolDownJump());
                }
                if (isLanding)
                {
                    playerPosture = PlayerPosture.Landing;
                }
                //isClimbReady = false;
                break;
            case PlayerPosture.Jumping:
                if (isGrounded)
                {
                    StartCoroutine(CoolDownJump());
                }
                if (isLanding)
                {
                    playerPosture = PlayerPosture.Landing;
                }
                //isClimbReady = false;
                break;
            case PlayerPosture.Landing:
                if (!isLanding)
                {
                    playerPosture = PlayerPosture.Stand;
                }
                //isClimbReady = false;
                break;
            case PlayerPosture.Climbing:
                //非此即彼，因为其为对立体的一方
                //区别Tag和Name
                //获取指定层级（在此例中为第0层）的当前动画状态信息,只要不是攀爬动画就切换为Stand状态
                //根据动画来切换状态
                if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("攀爬") && !animator.IsInTransition(0))
                {
                    playerPosture = PlayerPosture.Stand;
                }
                break;
        }

        //现在这里统一把isClimbReady设置为false，后续添加一个专门的变量控制方法
        isClimbReady = false;

        //非状态机写法
        /*
        //玩家姿态、运动状态、瞄准状态，分别用对应的状态机进行管理，此处即定义对应的枚举常量
        //根据isGrounded变量值决定姿态变量值再在SwitchAnimation方法中改变相应动画参数值
        if (!isGrounded)
        {
            if (verticalVelocity > 0f)
            {
                playerPosture = PlayerPosture.Jumping;
            }
            //脱离地面，竖直速度为负，且不是跳跃的下落过程
            else if (playerPosture != PlayerPosture.Jumping)
            {
                //距离地面较高，即可以下落
                if (couldFall)
                {
                    playerPosture = PlayerPosture.Falling;
                }
            }
            
        }
        //当前帧在地面，且上一帧处于Jumping
        else if (playerPosture == PlayerPosture.Jumping || playerPosture == PlayerPosture.Falling)
        {//TODO:其实应该只有跳跃落地有cd，高处跌落不需要CD
            //进入状态即开始计时
            StartCoroutine(CoolDownJump());
        }
        //isLanding由程序控制而非玩家控制，所以在此单独设置分支以延续其状态
        加入着地状态后需要过了jumpCD才能下蹲，但是可以直接走跑，因为着地状态中含有走跑的代码，
         可以直接改变动画参数值及其动画，不需要借助改变姿态为Stand而改变动画。可见姿态变量和相应
        动画不得不分开，而分开就必然产生距离，距离就伴随着变化。
        else if (isLanding)
        {
            playerPosture = PlayerPosture.Landing;
        }
        else if (isCrouchPressed)
        {
            playerPosture = PlayerPosture.Crouch;
        }
        else
        {
            playerPosture = PlayerPosture.Stand;
        }
        */
        //切换运动状态LocomotionState
        if (playerInputVec.magnitude == 0)
        {
            locomotionState = LocomotionState.Idle;
        }
        else if (!isRunPressed)
        {
            locomotionState = LocomotionState.Walk;
        }
        else
        {
            locomotionState = LocomotionState.Run;
        }

        //if (isAiming)
        //{
        //    armState = ArmState.Aim;
        //}
        //else
        //{
        //    armState = ArmState.Normal;
        //}
    }

    /*在代码中获取玩家输入，改变相应状态变量，再在代码中进行条件判断并修改相应的动画参数值，
     由此实现动画与输入的同步
    姿态树、姿态动画片段*/
    void SetupAnimation()
    {
        if (playerPosture == PlayerPosture.Stand)
        {//设置短暂的过渡时间更加真实，对于写实风格而言。(切换动画更加真实，对于切换姿态的作用在于更加丝滑流畅)
            animator.SetFloat(postureHash, standThreshold, 0.1f, Time.deltaTime);
            //animator.SetFloat(postureHash, standThreshold);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    if (animator.GetFloat(moveSpeedHash) <= 0.1f)//更准确应该是与目标差的绝对值小于0.1就直接取等
                    {
                        animator.SetFloat(moveSpeedHash, 0f);
                        break;
                    }
                    animator.SetFloat(moveSpeedHash, 0, lerpT, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, lerpT, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, lerpT, Time.deltaTime);
                    break;
            }
        }
        //else if (playerPosture == PlayerPosture.Crouch)
        //{
        //    animator.SetFloat(postureHash, crouchThreshold, 0.1f, Time.deltaTime);
        //    switch (locomotionState)
        //    {
        //        case LocomotionState.Idle:
        //            animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
        //            break;
        //        default:
        //            animator.SetFloat(moveSpeedHash, playerMovement.magnitude * crouchSpeed, 0.1f, Time.deltaTime);
        //            break;
        //    }
        //}
        //起跳动画、跳到最高点的动画、落地动画
        //瞬间切换，否则会出现抖动等问题
        else if (playerPosture == PlayerPosture.Jumping)
        {
            animator.SetFloat(postureHash, JumpingThreshold, 0.1f, Time.deltaTime);
            //animator.SetFloat(postureHash, JumpingThreshold);
            //animator.SetFloat(verticalVelHash, verticalVelocity, 0.1f, Time.deltaTime);
            animator.SetFloat(verticalVelHash, verticalVelocity);
            animator.SetFloat(feetTweenHash, feetTween);
        }
        /*Landing状态只是为了防止连续跳跃，而移动是通过各个状态管理的，
         * 为了避免在Landing状态无法移动，所以在此使用与Stand站立状态一样的操作逻辑*/
        else if (playerPosture == PlayerPosture.Landing)
        {
            //增加damping过渡时间使得落地时缓冲效果更加明显
            //Debug.Log(landingThreshold);
            //比jumpCD还短的时间，才能看到蹲下的缓冲效果
            animator.SetFloat(postureHash, landingThreshold, 0.03f, Time.deltaTime);
            //animator.SetFloat(postureHash, landingThreshold);
            //animator.SetFloat(postureHash, standThreshold);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    if (animator.GetFloat(moveSpeedHash) <= 0.1f)//更准确应该是与目标差的绝对值小于0.1就直接取等
                    {
                        animator.SetFloat(moveSpeedHash, 0f);
                        break;
                    }
                    animator.SetFloat(moveSpeedHash, 0, lerpT, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, lerpT, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, lerpT, Time.deltaTime);
                    break;
            }
        }
        else if (playerPosture == PlayerPosture.Falling)
        {
            animator.SetFloat(postureHash, JumpingThreshold);
            //animator.SetFloat(postureHash, JumpingThreshold);
            //animator.SetFloat(verticalVelHash, verticalVelocity, 0.1f, Time.deltaTime);
            animator.SetFloat(verticalVelHash, verticalVelocity);
        }
        else if (playerPosture == PlayerPosture.Climbing)
        {
            animator.SetInteger(climbHash, currentClimbParameter);
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            //使用Lerp方法将角色旋转正对墙壁
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, Quaternion.LookRotation(-playerSensor.climbHitNormal), 0.5f);
            /*只要在播放相应攀爬动画，就可以将参数归零了，因为攀爬动画的转出条件
             是在播放到90%后过渡（回）到非攀爬动画*/
            if (info.IsName("攀爬"))
            {
                currentClimbParameter = defaultClimbParameter;
                /*在启用rootMotion的情况下，MatchTarget方法会移动角色，并使角色身上的某个部位
                 在某个确定的时间对齐到某个确定的位置
                将左手骨骼完全对齐到leftHandPosition位置，在动画开始播放时进行，播放到10%即0.1时完成对齐*/
                //animator.MatchTarget(leftHandPosition, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.1f);
                //按照以上对齐发现还是会比实际高度低一点，所以做以下调整，在动画演出中一定要根据各方面数值进行一些微调以达到最好的视觉效果
                animator.MatchTarget(leftHandPosition, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.1f);
                animator.MatchTarget(leftHandPosition + Vector3.up * 0.18f, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.3f);
            }
            if (info.IsName("爬高"))
            {
                currentClimbParameter = defaultClimbParameter;
                animator.MatchTarget(rightFootPosition, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.13f);
                animator.MatchTarget(rightHandPosition, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.2f, 0.32f);
            }
            if (info.IsName("翻越"))
            {
                currentClimbParameter = defaultClimbParameter;
                animator.MatchTarget(rightHandPosition, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.1f, 0.2f);
                animator.MatchTarget(rightHandPosition + Vector3.up * 0.1f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.35f, 0.45f);
            }
        }

        //if (armState == ArmState.Normal)
        //{
        //    //通过反正切求出人物前向与视角前向之间夹角的弧度值，就可以将人物转向视角同向了。
        //    float rad = Mathf.Atan2(playerMovement.x, playerMovement.z);
        //    animator.SetFloat(turnSpeedHash, rad, 0.1f, Time.deltaTime);
        //}
    }

    /// <summary>
    /// 计算跳跃CD
    /// </summary>
    /// <returns></returns>
    IEnumerator CoolDownJump()
    {
        /*落地速度小于-10（也就是下落更快）时取-10，landingThreshold=0.5即半蹲，
         大于0时取0，则完全站立。就是在落地时具有更加真实的缓冲效果。*/
        landingThreshold = Mathf.Clamp(verticalVelocity, -10, 0);
        landingThreshold /= 20f;
        landingThreshold += 1f;
        isLanding = true;
        playerPosture = PlayerPosture.Landing;
        yield return new WaitForSeconds(jumpCD);
        isLanding = false;
    }

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
    private void OnAnimatorMove() //实现了该方法之后，Animator的applyRootMotion属性就不起作用了
    {
        /*在攀爬时禁用CharacterController，以避免受到重力和碰撞的影响，同时转用引擎本身
         的RootMotion通过攀爬动画驱动人物攀爬，并且随后在其他状态就恢复控制。*/
        if (playerPosture == PlayerPosture.Climbing)
        {
            controller.enabled = false;
            animator.ApplyBuiltinRootMotion(); //应用内置的根运动
        }
        else if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)
        { //就是在地面。标记变量用于转换状态，状态才能用来划分逻辑
            controller.enabled = true;

            //deltaPosition受帧率影响，显然帧率越高它越小，帧率越低它越大
            //水平位移和竖直位移
            Vector3 playerDeltaMovement = animator.deltaPosition;
            playerDeltaMovement.y = verticalVelocity * Time.deltaTime;//加上垂直速度（实现跳跃、下落等Y轴上的功能）
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

            /*沿用地面速度，比如使用最后一帧的速度（容易受到地形、操作等影响，破坏手感）
             所以此处选择沿用离地前几帧的平均速度*/
            averageVel.y = verticalVelocity;//单独处理竖直速度，因为不能使用动画本身的竖直位移
            Vector3 playerDeltaMovement = averageVel * Time.deltaTime;
            controller.Move(playerDeltaMovement);
            //isGrounded = controller.isGrounded;
        }
    }
}
