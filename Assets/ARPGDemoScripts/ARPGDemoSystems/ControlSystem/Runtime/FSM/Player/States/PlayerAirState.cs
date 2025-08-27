using Animancer;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    [AddComponentMenu("ARPGDemo/ControlSystem/States/PlayerAirState", 20)]
    public class PlayerAirState : PlayerStateBehaviour
    {
        protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;
        // public override bool isEnd => m_IsGrounded == true; //落地就结束
        public override bool canExitState => isEnd; //没有结束就不能(主动)退出，主要bug在于Air过程中会不断接收MoveInput，可能会与GroundedState冲突或混淆。

        // [SerializeField] protected ClipTransition jumpUp;
        // [SerializeField] protected ClipTransition jumpTop;
        // [SerializeField] protected ClipTransition fall;
        [SerializeField] protected LinearMixerTransition airAnims;
        [SerializeField] protected bool m_IsGrounded;
        public bool isGrounded { get => m_IsGrounded; set => m_IsGrounded = value; }
        [SerializeField] protected Vector2 m_MoveInput; //在空中的方向输入就是用于微调，至于调整程度大小，就要看具体设计了。
        public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }
        [SerializeField] protected Vector3 m_MoveDir; //包含了方向与输入值的信息，所以不需要moveInput
        public Vector3 moveDir { get => m_MoveDir; set => m_MoveDir = value; }
        /*TODO：如果是跳跃、下落之类的话，就是float horizontalSpeed，但是如果要更加扩展的话，包括Gravity也是，都可以具有更多变的值，不只是(0,x,0)。
        还可以猜测，对于那种翻转旋转效果，就是改变人物移动各方面数值以及镜头，而这样会牵涉到大量问题，如何从根本去解决这些问题，可能是设置一个专门的游戏对象
        作为参考系，然后在人物和镜头逻辑中使用局部坐标系而不是全局坐标系，这样将其抽象（提取）出来，可以在实现翻转旋转效果时改变这个专门的游戏对象，而人物和镜头的
        相关逻辑可以保持不变。*/
        [SerializeField] protected Vector3 m_InitialVerticalVelocity;
        public Vector3 initialVerticalVelocity { get => m_InitialVerticalVelocity; set => m_InitialVerticalVelocity = value; }
        //因为竖直速度的方向就是Z轴正负方向，其值就已经包含了方向信息，而velocity与speed的区别就在于前者是向量，后者是标量
        [SerializeField] protected Vector3 m_CurrentVerticalVelocity;
        // public Vector3 currentVerticalVelocity { get => m_CurrentVerticalVelocity; set => m_CurrentVerticalVelocity = value; }
        [SerializeField] protected float m_HorizontalSpeed;
        public float horizontalSpeed { get => m_HorizontalSpeed; set => m_HorizontalSpeed = value; }
        [SerializeField] protected float m_TurnSpeed;
        public float turnSpeed { get => m_TurnSpeed; set => m_TurnSpeed = value; }
        // [SerializeField] protected Vector3 m_OriginalGravity;
        // public Vector3 originalGravity { get => m_OriginalGravity; set => m_OriginalGravity = value; }
        // [SerializeField] protected Vector3 m_ActualGravity; //用于调整跳跃的手感
        [SerializeField] protected Vector3 m_UpGravity;
        public Vector3 upGravity { get => m_UpGravity; set => m_UpGravity = value; }
        [SerializeField] protected Vector3 m_FallGravity;
        public Vector3 fallGravity { get => m_FallGravity; set => m_FallGravity = value; }

        [SerializeField] protected Rigidbody m_Rb;
        public Rigidbody rb { get => m_Rb; set => m_Rb = value; }
        [SerializeField] protected CharacterController m_PhysicsHandler;
        public CharacterController physicsHandler { get => m_PhysicsHandler; set => m_PhysicsHandler = value; }

        protected Quaternion targetRotation;
        protected Quaternion originalRotation;
        protected float turnRatio = 0f;
        protected Parameter<float> parameter;
        protected Vector3 m_ActualGravity;

        public override void OnEnterState()
        {
            base.OnEnterState();

            m_IsEnd = false;
            m_CurrentVerticalVelocity = initialVerticalVelocity;
            parameter = animPlayer.Parameters.GetOrCreate<float>(airAnims.ParameterName);
            animPlayer.Play(airAnims);
            SetAnimParameter();
            // animPlayer.Play(jump);
            // m_IsEnd = false;
        }

        // public override void OnFixedUpdate()
        // {
        //     base.OnFixedUpdate();

        //     //并非落地就结束，还是考虑到底部碰撞检测的误差，可能这一帧跳起但是下一帧仍然检测在地面的情况，所以还要加入判断当前是否在下落。
        //     if (m_CurrentVerticalVelocity.y <= 0f && isGrounded == true) m_IsEnd = true;

        //     m_CurrentVerticalVelocity += m_Gravity * Time.fixedDeltaTime; //先受到重力影响，还是后受到，有待进一步考虑
        //     // Vector3 actualSpeed = m_CurrentSpeed + new Vector3(m_MoveInput.x, 0f, m_MoveInput.y); //在设计上，
        //     /*竖直方向的位移和水平方向的位移*/
        //     rb.MovePosition(rb.transform.position + (m_CurrentVerticalVelocity + m_HorizontalSpeed * m_MoveDir) * Time.fixedDeltaTime);
        // }

        public override void OnUpdate()
        {
            //并非落地就结束，还是考虑到底部碰撞检测的误差，可能这一帧跳起但是下一帧仍然检测在地面的情况，所以还要加入判断当前是否在下落。
            if (m_CurrentVerticalVelocity.y <= 0f && isGrounded == true) m_IsEnd = true;

            MoveAndRotate();
            SetAnimParameter();
            AdjustGravity();
        }

        private void MoveAndRotate()
        {
            if (m_MoveInput != Vector2.zero)
            {
                //有变化才执行相应逻辑
                if (Quaternion.LookRotation(m_MoveDir, Vector3.up) != transform.rotation)
                {
                    targetRotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
                    originalRotation = transform.rotation;
                    turnRatio = 0f;
                    // transform.rotation = targetRotation;
                }
                // physicsHandler.transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
                // transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                // transform.rotation = targetRotation;
            }
            //渐转但是不需要连续输入。Quaternion有自己的判断是否相等的逻辑。
            /*Tip：要知道，如果使用插值方法的话，turnSpeed就变成了比例速度而不是角度速度，也就是说虽然间隔时间相同，但实际旋转的角度不同，那就必然会出现旋转时卡顿、抖动的现象。
            还有一点，最容易出现抖动的情况是按住方向键的同时持续旋转镜头，那么targetRotation的值就会持续变化，*/
            // if (transform.rotation != targetRotation) transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            if (transform.rotation != targetRotation)
            {
                turnRatio += turnSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, turnRatio);
                // transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
            m_CurrentVerticalVelocity += m_ActualGravity * Time.deltaTime;
            m_PhysicsHandler.Move((m_CurrentVerticalVelocity + m_HorizontalSpeed * m_MoveDir) * Time.deltaTime);
        }

        private void SetAnimParameter()
        {
            // float verticalSpeed = m_CurrentVerticalVelocity.y;
            parameter.SetValue(m_CurrentVerticalVelocity.y);
            airAnims.State.Parameter = parameter;
            Debug.Log($"airparameter: {parameter.Value}");
            // if (verticalSpeed > 0f)
        }

        private void AdjustGravity()
        {
            m_ActualGravity = m_CurrentVerticalVelocity.y >= 0f ? m_UpGravity : m_FallGravity;
        }
    }
}