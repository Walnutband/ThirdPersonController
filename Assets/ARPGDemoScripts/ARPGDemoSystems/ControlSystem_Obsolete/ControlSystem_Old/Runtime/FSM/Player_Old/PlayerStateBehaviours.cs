using Animancer;
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old.Player
{
    //这样的一个基类，算是区分了不同个体的状态（实际上从游戏设计层面应该是通用的），而且定义一些会被所有派生状态拥有的成员。
    public abstract class PlayerStateBehaviour : StateBehaviour
    {
        // protected PlayerStateMachine stateMachine;
        [SerializeField] protected AnimancerComponent m_AnimPlayer; //注意
        public AnimancerComponent animPlayer { get => m_AnimPlayer; set => m_AnimPlayer = value; }

        // [SerializeField] protected int m_TempPriority;
        public override int tempPriority { get => -10; } //默认为-10

        // [SerializeField] protected AnimationClip[] clips;

        /*Tip：组件不需要也不能手动构造实例*/
        // public PlayerStateBehaviour(PlayerStateMachine _stateMachine)
        // {
        //     stateMachine = _stateMachine;
        // }

        // protected delegate void 

    }

    // [AddComponentMenu("ARPGDemo/ContrySystem/States/PlayerGroundedState", 10)]
    // public class PlayerGroundedState : PlayerStateBehaviour
    // {
    //     public override bool isEnd => false;

    //     [SerializeField] protected ClipTransition idle;
    //     [SerializeField] protected ClipTransition move;

    //     [SerializeField] protected Vector2 m_MoveInput;
    //     public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }
    //     protected Vector3 m_MoveDir;
    //     public Vector3 moveDir { get => m_MoveDir; set => m_MoveDir = value; }
    //     /*TODO：因为移动速度是状态本身的，移动方向才是从输入、从控制器而来的。但是考虑到扩展性，就是比如减速Buff之类的，那就肯定要从比如控制器给自己的状态
    //     的对应变量赋值才能实现，而且还要与整个Buff系统对接上（ActorProperty等等），绝不可能像这样只是一个单独的由状态私有的变量，必然还会为这样的、这一类的
    //     操作专门编写另外的流程逻辑。*/
    //     [SerializeField] protected float moveSpeed = 3f;

    //     // [SerializeField] protected Transform m_CamTransform;
    //     // public Transform camTransform { get => m_CamTransform; set => m_CamTransform = value; }

    //     // [SerializeField] protected Transform m_TargetTransform;
    //     // public Transform targetTransform { get => m_TargetTransform; set => m_TargetTransform = value; }
    //     protected Rigidbody m_Rb;
    //     public Rigidbody rb { get => m_Rb; set => m_Rb = value; }

    //     public override void OnEnterState()
    //     {
    //         base.OnEnterState();

    //         if (m_MoveInput != Vector2.zero) animPlayer.Play(move);
    //         else animPlayer.Play(idle);
    //     }

    //     public override void OnUpdate()
    //     {
    //         base.OnUpdate();
    //         // MoveAndRotate();

    //         /*TODO：这里的动画播放肯定是要改进的，估计用混合树更好，但其实对于一个求职Demo来说，无所谓。*/
    //         if (m_MoveInput != Vector2.zero) animPlayer.Play(move);
    //         else animPlayer.Play(idle);
    //     }

    //     public override void OnFixedUpdate()
    //     {
    //         base.OnFixedUpdate();

    //         MoveAndRotate();
    //     }

    //     private void MoveAndRotate()
    //     {
    //         // Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
    //         // m_MoveDir = camFoward * m_MoveInput.y + camTransform.right * m_MoveInput.x;
    //         //有输入才转向，否则每次停止输入后都会回到原方向。
    //         /*TODO：应该要搞一个分类，逐渐转身还是立刻转身*/
    //         if (m_MoveInput != Vector2.zero) rb.MoveRotation(Quaternion.LookRotation(m_MoveDir, Vector3.up));
    //         // else if (isAttack == false && notMove == false)
    //         // {
    //         //     animPlayer.Play(anims.idle);
    //         // }
    //         // m_TargetTransform.position += m_MoveDir * moveSpeed * Time.deltaTime;
    //         rb.MovePosition(rb.transform.position + m_MoveDir * moveSpeed * Time.fixedDeltaTime); //别忘了transform.position是世界坐标
    //         // Idle();
    //     }

    // }

    public class PlayerIdleState : PlayerStateBehaviour
    {
        public override bool isEnd => false; //Idle作为默认状态，显然就是永不结束的逻辑（但是也不一定就是默认逻辑？）
        [SerializeField] protected ClipTransition idle;
        protected Vector2 m_MoveInput;
        public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }



        public override void OnEnterState()
        {
            base.OnEnterState();

            animPlayer.Play(idle);
        }
    }

    public class PlayerMoveState : PlayerStateBehaviour
    {
        public override bool isEnd => m_MoveInput == Vector2.zero;
        [SerializeField] protected ClipTransition move;
        protected Vector2 m_MoveInput;
        public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }

        /*TODO：为了动画的流畅性，Move不可能只有一个动画，应该会搞成混合树*/

        public override void OnEnterState()
        {
            base.OnEnterState();

            animPlayer.Play(move);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();


        }
    }

    // public class PlayerJumpState : PlayerStateBehaviour
    // {
    //     protected bool m_IsEnd;
    //     public override bool isEnd => m_IsEnd;

    //     [SerializeField] protected ClipTransition jump;
    //     [SerializeField] protected bool m_IsGrounded;
    //     public bool isGrounded { get => m_IsGrounded; set => m_IsGrounded = value; }
    //     [SerializeField] protected Vector2 m_MoveInput;
    //     public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }
    //     [SerializeField] protected Vector3 m_InitialSpeed;
    //     public Vector3 initialSpeed { get => m_InitialSpeed; set => m_InitialSpeed = value; }
    //     [SerializeField] protected Vector3 m_CurrentSpeed;
    //     public Vector3 currentSpeed { get => m_CurrentSpeed; set => m_CurrentSpeed = value; }
    //     [SerializeField] protected Vector3 m_Gravity;
    //     public Vector3 gravity { get => m_Gravity; set => m_Gravity = value; }
    //     [SerializeField] protected Rigidbody m_Rb;
    //     public Rigidbody rb { get => m_Rb; set => m_Rb = value; }

    //     public override void OnEnterState()
    //     {
    //         base.OnEnterState();

    //         animPlayer.Play(jump);
    //         currentSpeed = initialSpeed;
    //     }

    //     public override void OnFixedUpdate()
    //     {
    //         base.OnFixedUpdate();

    //         m_CurrentSpeed += m_Gravity * Time.fixedDeltaTime; //先受到重力影响，还是后受到，有待进一步考虑
    //         Vector3 actualSpeed = m_CurrentSpeed + new Vector3(); //在设计上，
    //         /**/
    //         rb.MovePosition(rb.transform.position + actualSpeed * Time.fixedDeltaTime);
    //     }
    // }








}