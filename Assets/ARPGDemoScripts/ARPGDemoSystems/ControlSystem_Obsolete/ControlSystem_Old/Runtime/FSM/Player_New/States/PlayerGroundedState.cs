using System;
using Animancer;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old
{
    // [AddComponentMenu("ARPGDemo/ControlSystem/Player/States/PlayerGroundedState", 10)]
    public class PlayerGroundedState : PlayerStateBehaviour
    {
        public override bool isEnd => false;


        [SerializeField] protected MixerAnimation m_MoveAnims;
        /*TODO：想到通过切换动画的方式来实现Run和Sprint甚至是SlowMove，其实理论上确实可以，但感觉还是使用混合树更好。*/
        [SerializeField] protected Vector2 m_MoveInput;
        public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }
        [SerializeField] protected Vector3 m_MoveDir;
        public Vector3 moveDir { get => m_MoveDir; set => m_MoveDir = value; }
        /*TODO：因为移动速度是状态本身的，移动方向才是从输入、从控制器而来的。但是考虑到扩展性，就是比如减速Buff之类的，那就肯定要从比如控制器给自己的状态
        的对应变量赋值才能实现，而且还要与整个Buff系统对接上（ActorProperty等等），绝不可能像这样只是一个单独的由状态私有的变量，必然还会为这样的、这一类的
        操作专门编写另外的流程逻辑。*/
        [SerializeField] protected float m_MoveSpeed;
        public float moveSpeed { get => m_MoveSpeed; set => m_MoveSpeed = value; }

        [SerializeField] protected float m_TurnSpeed = 10f;
        public float turnSpeed { get => m_TurnSpeed; set => m_TurnSpeed = value; }

        // [SerializeField] protected Transform m_CamTransform;
        // public Transform camTransform { get => m_CamTransform; set => m_CamTransform = value; }

        // [SerializeField] protected Transform m_TargetTransform;
        // public Transform targetTransform { get => m_TargetTransform; set => m_TargetTransform = value; }
        // protected Rigidbody m_Rb;
        // public Rigidbody rb { get => m_Rb; set => m_Rb = value; }
        [SerializeField] protected CharacterController m_PhysicsHandler;
        public CharacterController physicsHandler { get => m_PhysicsHandler; set => m_PhysicsHandler = value; }

        protected Quaternion targetRotation;
        protected Quaternion originalRotation;
        protected float turnRatio = 0f;
        protected float m_ActualMoveSpeed;
        [Tooltip("实际是按照比例插值来变化移动速度的，并非按照绝对数值")]
        [SerializeField] protected float m_Acceleration = 5f;

        protected AnimationMixerState m_MoveState;


        public override void OnEnterState()
        {
            base.OnEnterState();

            /*TODO：这行代码相当于一个补丁，因为有时候在切换状态时、切换到该状态时会突然转向，就是因为在其他状态时没有执行该状态中的MoveAndRotate方法更新targetRotation，
            所以在转入该状态后如果（由于其他状态的相关逻辑导致）当前rotation确实不等于targetRotation的话，那么就会出现突然转向的现象，所以在此Enter方法中直接将目标设置为
            当前的rotation，就可以直接避免这种情况，但是是否有其他代价暂不清楚，不过在该方法中的、这一行代码的影响范围本身就有限，所以想必不会带来什么意外的问题。*/
            targetRotation = transform.rotation;

            m_MoveState = animPlayer.Play(m_MoveAnims);
            SetAnimParameter();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            // MoveAndRotate();

            /*TODO：这里的动画播放肯定是要改进的，估计用混合树更好，但其实对于一个求职Demo来说，无所谓。*/
            // if (m_MoveInput != Vector2.zero) animPlayer.Play(m_Move); //moveInput影响播放什么动画，主要就是对于移动动画来说会这样，要是其他比如攻击动画的话，就不需要这种逻辑。
            // else animPlayer.Play(idle);
            // m_ActualMoveSpeed = Mathf.Lerp(m_ActualMoveSpeed, m_MoveSpeed, m_Acceleration * Time.deltaTime);
            MoveAndRotate();
            SetAnimParameter();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            // MoveAndRotate();
        }

        /*TODO：现在依赖于CharacterController以及直接访问transform，前者只是为了方便，以后肯定会改用其他，后者是因为状态作为组件类，可以直接访问所挂载的游戏对象的transform，
        正常结构来看其实应该由控制器把要控制的transform传入状态。*/
        private void MoveAndRotate()
        {
            // Vector3 camFoward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            // m_MoveDir = camFoward * m_MoveInput.y + camTransform.right * m_MoveInput.x;
            //有输入才转向，否则每次停止输入后都会回到原方向。
            /*TODO：应该要搞一个分类，逐渐转身还是立刻转身*/
            if (m_MoveInput != Vector2.zero)
            {
                // m_ActualMoveSpeed = Mathf.Lerp(m_ActualMoveSpeed, m_MoveSpeed, m_Acceleration * Time.deltaTime);
                m_ActualMoveSpeed = MathUtilities.FloatLerp(m_ActualMoveSpeed, m_MoveSpeed, m_Acceleration * Time.deltaTime);

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
            else //moveInput为Vector2.zero就是无输入，无输入的话应该可以直接return了。
            {
                // m_ActualMoveSpeed = 0f; //停止输入的时候就是恢复到静止状态。
                // m_ActualMoveSpeed = Mathf.Lerp(m_ActualMoveSpeed, 0f, m_Acceleration * Time.deltaTime);
                m_ActualMoveSpeed = MathUtilities.FloatLerp(m_ActualMoveSpeed, 0f, m_Acceleration * Time.deltaTime);
                // return;
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
            /*BugFix：*/
            // if (transform.rotation != targetRotation) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);


            // if (m_MoveInput != Vector2.zero) rb.MoveRotation(Quaternion.LookRotation(m_MoveDir, Vector3.up));
            // if (m_MoveInput != Vector2.zero) rb.transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);

            // physicsHandler.Move(rb.transform.position + m_MoveDir * m_MoveSpeed * Time.fixedDeltaTime); //别忘了transform.position是世界坐标

            /*Tip：突然想到tm的我一直都使用了RootMotion，所以我在此之前对于移动方面的调整基本都没有起作用。
            还有我将moveSpeed设置为0之后，发现旋转时就不会出现抖动了，这样似乎可以确定旋转时的抖动并非由于采用Lerp或者RotateToTowards导致的，而是由于同时进行的移动导致的，
            */
            // transform.position = transform.position + m_MoveDir * m_MoveSpeed * Time.deltaTime;
            // m_PhysicsHandler.Move(m_MoveDir * m_MoveSpeed * Time.deltaTime);
            /*TODO：还值得考虑的是手柄适配，因为使用手柄和键鼠在代码上应该会有所差异，不过主要是在移动的控制上，其他操作的话应该是一样的。*/
            // m_PhysicsHandler.Move(m_MoveDir * m_ActualMoveSpeed * Time.deltaTime + new Vector3(0f, -0.1f, 0f));
            m_PhysicsHandler.Move(m_MoveDir.normalized * m_ActualMoveSpeed * Time.deltaTime + new Vector3(0f, -0.1f, 0f));
            // m_PhysicsHandler.Move(transform.TransformDirection(new Vector3(0f, 0f, 1f)) * m_MoveInput.magnitude* m_MoveSpeed * Time.deltaTime + new Vector3(0f, -0.1f, 0f));


            /*Tip：这里本来想设置成一个回调，但是突然想到如果要设置计时器，比如在下台阶的时候短暂滞空不会进入到Air状态，如果这样设置回调的话，就不太方便控制器处理相关逻辑了，
            所以还是让控制器监测吧*/
            // CollisionFlags flags = m_PhysicsHandler.Move(m_MoveDir * m_MoveSpeed * Time.deltaTime); //传入直接的移动数值
            // if ((flags & CollisionFlags.Below) == 0) onIsGroundedChanged?.Invoke(false);

            // transform.position = transform.position + transform.TransformDirection(new Vector3(0f, 0f, 1f)) * m_MoveInput.magnitude * m_MoveSpeed * Time.deltaTime;

            // rb.MovePosition(rb.transform.position + m_MoveDir * m_MoveSpeed * Time.fixedDeltaTime); //别忘了transform.position是世界坐标
            // rb.transform.position = rb.transform.position + m_MoveDir * m_MoveSpeed * Time.fixedDeltaTime; //别忘了transform.position是世界坐标
            // Idle();
        }

        private void SetAnimParameter()
        {
            /*这里只是需要在没有（WASD）方向输入的时候需要设置为Idle动画，应该直接设置参数，而不是设置m_MoveSpeed，因为moveSpeed牵涉到很多其他内容，而参数只是影响播放的动画，
            这也算是一个基本原则，从最直接、对其他影响最小的地方入手。*/
            // if (m_MoveInput == Vector2.zero) m_MoveSpeed = 0f;
            if (m_MoveInput == Vector2.zero)
            {
                // parameter.SetValue(0f);
                m_MoveState.SetParameter(m_ActualMoveSpeed);
            }
            else
            {
                m_MoveState.SetParameter(m_ActualMoveSpeed);
            }
            // m_MoveAnims.State.Parameter = parameter;
        }

    }
}