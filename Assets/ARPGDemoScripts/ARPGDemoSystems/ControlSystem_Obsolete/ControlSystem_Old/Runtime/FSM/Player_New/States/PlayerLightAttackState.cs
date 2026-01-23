using Animancer;
using UnityEngine;
using ARPGDemo.BattleSystem;
using MyPlugins.AnimationPlayer; 

namespace ARPGDemo.ControlSystem_Old
{
    // [AddComponentMenu("ARPGDemo/ControlSystem/Player/States/PlayerLightAttackState", 40)]
    public class PlayerLightAttackState : PlayerStateBehaviour
    {
        /*TODO：暂时以魂游为例的动画不可打断（除了外部情况以外）来编写此处状态逻辑。*/
        [SerializeField] protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;
        // [SerializeField] protected bool m_CanExitState;
        // public override bool canExitState => m_CanExitState;
        public override bool canExitState => isEnd;
        [SerializeField] protected bool m_CanTransitionToSelf;
        public override bool canTransitionToSelf => m_CanTransitionToSelf;
        public override int tempPriority => 10;

        [SerializeField] protected bool m_Restart = true;
        public bool restart { get => m_Restart; set => m_Restart = value; }
        [SerializeField] protected Vector3 m_MoveDir;
        public Vector3 moveDir { get => m_MoveDir; set => m_MoveDir = value; }
        [SerializeField] protected CollisionDetectionMonitor m_Monitor;
        public CollisionDetectionMonitor monitor { get => m_Monitor; set => m_Monitor = value; }

        //TODO：这里的计算连招段数的combo还需要改进计算方式
        [SerializeField] protected int combo = 1;
        [SerializeField] protected FadeAnimation m_AttackOne;
        [SerializeField] protected FadeAnimation m_AttackTwo;
        [SerializeField] protected FadeAnimation m_AttackThree;

        protected AnimationClipState m_CurrentState;

        public override void OnEnterState()
        {
            base.OnEnterState();
            /*BugFix：通常有m_IsEnd字段的都需要在Enter方法中设置为false*/
            /*Tip：isEnd代表自然结束，canExitState代表主动结束，用在这种具有连招的状态中，也就是在一段动画的某个节点之后就可以接上下一个连段了，这就是canExitState，但是
            如果没有主动接上下一段的话，就应该等这一段执行完成之后才是isEnd。
            从这一点理解的话，canExitstate应该是isEnd的必要不充分条件*/
            m_IsEnd = false;
            // m_CanExitState = false;
            m_CanTransitionToSelf = false;

            m_Monitor.EnableCollider();

            // animPlayer.Animator.ApplyBuiltinRootMotion
            // Debug.Log("PlayerLightAttackState.OnEnterState");
            /*Tip：可以控制在每段攻击开始时的方向，才是通常的设计。
            不过如下，貌似一般的设计是在第一段攻击开始可以立刻转向，连段不可以，单从动画的角度来看都会感觉很奇怪（不过动画效果很大程度上都是取决于原本的动画素材），*/
            // transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
            // if (stateMachine.previousState == this)
            if (m_Restart == true)
            {
                combo = 1;
                //仍然别忘了，在没有方向输入的时候就维持原本的朝向不变
                if (m_MoveDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up);
            }
            m_AnimPlayer.GetComponent<RootMotionController>().ApplyRootMotion(true);
            switch (combo)
            {
                case 1:
                    // animPlayer.Play(m_AttackOne).Events(this).OnEnd = () => m_IsEnd = true;
                    //调整OnEnd触发的时机。
                    // animPlayer.Play(m_AttackOne).Events(this).OnEnd = () => m_IsEnd = true;
                    // m_AttackOne.Events.OnEnd = () =>
                    // {
                    //     Debug.Log("AttackOne End");
                    //     m_IsEnd = true;
                    // };
                    /*Tip：在这里将m_CanExitState改为m_CanTransitionToSelf意义在于，如果是主动接上下一段攻击，那么就可以在设置的提前位置接上下一段，而
                    如果主动的操作并非接上下一段攻击，而是其他状态，则只能等到动画结束才能接上。这当然牵扯到游戏设计，但是促使我如此改变的原因是，在
                    提前位置接上比如翻滚的话，动画插值表现会很奇怪，因为连段攻击的各个片段之间是衔接好的，但是攻击动画和翻滚动画又没有衔接好，所以就会出现
                    很难看的动画效果。*/
                    // m_AttackOne.Events.SetCallback(m_CanExitEventName, () => m_CanExitState = true);
                    // m_AttackOne.Events.SetCallback(m_CanExitEventName, () => m_CanTransitionToSelf = true);
                    // animPlayer.Play(m_AttackOne);
                    m_CurrentState = animPlayer.Play(m_AttackOne);
                    m_CurrentState.EndedEvent += () =>
                    {
                        // Debug.Log("AttackOne End");
                        m_IsEnd = true;
                    };
                    m_CurrentState.CustomEndedEvent += () => m_CanTransitionToSelf = true;
                    combo = 2;
                    break;
                case 2:
                    m_CurrentState = animPlayer.Play(m_AttackTwo);
                    m_CurrentState.EndedEvent += () =>
                    {
                        // Debug.Log("AttackOne End");
                        m_IsEnd = true;
                    };
                    m_CurrentState.CustomEndedEvent += () => m_CanTransitionToSelf = true;
                    combo = 3;
                    break;
                case 3:
                    m_CurrentState = animPlayer.Play(m_AttackThree);
                    m_CurrentState.EndedEvent += () =>
                    {
                        m_IsEnd = true;
                        m_CanTransitionToSelf = true;
                    };
                    combo = 1;
                    break;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            // Debug.Log("Exit Light Attack State");
            m_Monitor.DisableCollider();
            /*TODO: 对于计时器的控制还需要进一步优化*/
            m_Monitor.ClearTimers();

            m_AnimPlayer.GetComponent<RootMotionController>().ApplyRootMotion(false);
        }

        // public override void OnUpdate()
        // {
        //     base.OnUpdate();


        // }
    }
}