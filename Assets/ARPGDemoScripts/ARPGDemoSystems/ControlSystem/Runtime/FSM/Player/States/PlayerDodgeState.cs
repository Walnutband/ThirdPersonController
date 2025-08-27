using Animancer;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    /*Tip：注意各个状态使用的AnimancerComponent都是所在个体的那个，所以可以使用过TransitionLibrary来指定特定片段到特定片段的过渡时间，不用担心某些片段到某些片段的过渡过程会难以控制。*/
    [AddComponentMenu("ARPGDemo/ControlSystem/States/PlayerDodgeState", 30)]
    public class PlayerDodgeState : PlayerStateBehaviour
    {
        protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;
        protected bool m_CanExitState;
        public override bool canExitState => m_CanExitState;
        // public override bool canTransitionToSelf => true;
        public override int tempPriority => 10;

        [SerializeField] protected StringAsset m_CanExitEventName;
        [SerializeField] protected Vector2 m_MoveInput;
        public Vector2 moveInput { get => m_MoveInput; set => m_MoveInput = value; }
        [SerializeField] protected Vector3 m_MoveDir;
        public Vector3 moveDir { get => m_MoveDir; set => m_MoveDir = value; }
        [SerializeField] protected float m_MoveSpeed;
        public float moveSpeed { get => m_MoveSpeed; set => m_MoveSpeed = value; }


        [SerializeField] protected ClipTransition roll;
        [SerializeField] protected ClipTransition avoidBack;

        [SerializeField] protected CharacterController m_PhysicsHandler;
        public CharacterController physicsHandler { get => m_PhysicsHandler; set => m_PhysicsHandler = value; }

        protected Vector3 m_ActualMoveDir;
        protected float m_ActualMoveSpeed;
        protected AnimancerState m_CurrentState;
        /*TODO：如何控制在翻滚的过程中无敌呢？我猜测并不是在该状态内处理，而是在技能编辑器中处理，也就是附加到动画片段上，*/

        public override void OnEnterState()
        {
            base.OnEnterState();

            m_IsEnd = false;
            m_CanExitState = false;

            m_ActualMoveSpeed = 0f;
            if (moveInput == Vector2.zero)
            {
                m_ActualMoveDir = transform.TransformDirection(new Vector3(0f, 0f, -1f));
                /*BUG：注意有关动画系统的一个bug，就是在Play时如果是当前正在播放的动画，是重新播放还是继续播放（保持不变），会直接影响到后面的逻辑，
                因为后面全是依赖于动画播放状态来执行的逻辑*/
                m_CurrentState = animPlayer.Play(avoidBack);
                // animPlayer.Play(avoidBack).Events(this).OnEnd = () => m_IsEnd = true; //翻滚完就结束
                m_CurrentState.Events(this).OnEnd = () => m_IsEnd = true;
                m_CurrentState.Events(this).SetCallback(m_CanExitEventName, () =>
                {
                    Debug.Log("avoidBack CanExit");
                    m_CanExitState = true;
                    /*BugFix: */
                    // m_MoveSpeed = 0f;
                });
                // avoidBack.Events.SetCallback(m_CanExitEventName, () =>
                // {
                //     Debug.Log("avoidBack CanExit");
                //     m_CanExitState = true;
                //     m_MoveSpeed = 0f;
                // });
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up); //立刻转向
                m_ActualMoveDir = transform.TransformDirection(new Vector3(0f, 0f, 1f));
                m_CurrentState = animPlayer.Play(roll);
                m_CurrentState.Events(this).OnEnd = () => m_IsEnd = true;
                m_CurrentState.Events(this).SetCallback(m_CanExitEventName, () =>
                {
                    Debug.Log("roll CanExit");
                    m_CanExitState = true;
                    // m_MoveSpeed = 0f;
                });
                // animPlayer.Play(roll).Events(this).OnEnd = () => m_IsEnd = true; //翻滚完就结束
                // roll.Events.SetCallback(m_CanExitEventName, () =>
                // {
                //     Debug.Log("roll CanExit");
                //     m_CanExitState = true;
                //     m_MoveSpeed = 0f;
                // });
            }

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Move();
        }

        public override void OnExitState()
        {
            base.OnExitState();

            m_CurrentState = null;
        }

        private void Move()
        {
            Ease(m_CurrentState.NormalizedTime);
            // transform.position += m_ActualMoveDir * m_ActualMoveSpeed * Time.deltaTime;
            m_PhysicsHandler.Move(m_ActualMoveDir * m_ActualMoveSpeed * Time.deltaTime);
        }

        private void Ease(float progress)
        {
            Debug.Log("progress:" + progress);
            if (progress <= 0f || progress >= 1f) return;
            if (progress <= 0.5f)
            {
                // m_ActualMoveSpeed = Mathf.Lerp(0f, m_MoveSpeed, progress * 2f);
                // m_ActualMoveSpeed = -Mathf.Pow((progress - 0.5f), 2) + m_MoveSpeed;
                m_ActualMoveSpeed = Mathf.Max(-Mathf.Pow(7 * (progress), 2) + m_MoveSpeed, 0f);
            }
            else
            {
                // m_ActualMoveSpeed = Mathf.Lerp(m_MoveSpeed, 0f, (progress - 0.5f) * 2f);
                m_ActualMoveSpeed = Mathf.Max(-Mathf.Pow(7 * (progress), 2) + m_MoveSpeed, 0f);
            }
        }
    }
}