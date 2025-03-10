namespace Ilumisoft.VisualStateMachine
{
    using UnityEngine;

    /// <summary>
    /// Base class to create custom behaviours for states为状态创造自定义行为的基类
    /// </summary>
    public abstract class StateBehaviour : MonoBehaviour //抽象类，不能被实例化，只能被继承然后实现这里声明的抽象内容
    {
        [SerializeField]
        StateMachine stateMachine;

        State state = null;

        /// <summary>
        /// ID of the state this behaviour is belonging to该行为所属状态的ID
        /// </summary>
        public abstract string StateID { get; } //只读的抽象字符串属性

        /// <summary>
        /// The State Machine owning the state this behaviour is belonging to
        /// </summary>
        public StateMachine StateMachine { get => this.stateMachine; set => this.stateMachine = value; }
        //tips:虚拟成员和抽象成员不能是私有的。不过虚拟成员在派生类中不是必须被重写，而抽象成员则必须被重写即实现。
        protected virtual void Awake()
        {
            if (StateMachine != null)
            {
                // Get the state
                state = StateMachine.Graph.GetState(StateID);

                // Add listeners to enter, exit and update events of the state
                if (state != null)
                {
                    state.OnEnterState.AddListener(OnEnterState);
                    state.OnExitState.AddListener(OnExitState);
                    state.OnUpdateState.AddListener(OnUpdateState);
                }
                else
                {
                    Debug.Log($"Could not find state with the id '{StateID}'", this);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            // Stop listening to state events when the behaviour gets destroyed
            if (StateMachine != null && state != null)
            {
                state.OnEnterState.RemoveListener(OnEnterState);
                state.OnExitState.RemoveListener(OnExitState);
                state.OnUpdateState.RemoveListener(OnUpdateState);
            }
        }

        /// <summary>
        /// Automatically tries to assign a state machine, when the component gets created or reset
        /// </summary>
        private void Reset()
        {
            if (StateMachine == null)
            {
                StateMachine = GetComponentInParent<StateMachine>(); //状态机要作为父物体？
            }
        }

        /// <summary>
        /// Returns true if the state is the currently active one为当前活跃状态
        /// </summary>
        public bool IsActiveState => StateMachine.CurrentState == StateID;

        /// <summary>
        /// Callback invoked when the state is entered
        /// </summary>
        protected virtual void OnEnterState() { }

        /// <summary>
        /// Callback invoked when the state is exit
        /// </summary>
        protected virtual void OnExitState() { }

        /// <summary>
        /// Callback invoked when the state is active and updated
        /// </summary>
        protected virtual void OnUpdateState() { }
    }
}