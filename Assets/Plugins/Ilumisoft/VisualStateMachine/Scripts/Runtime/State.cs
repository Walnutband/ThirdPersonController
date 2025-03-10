namespace Ilumisoft.VisualStateMachine
{
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class State : Node
    {
        [SerializeField]
        private UnityEvent onEnterState = new UnityEvent(); //UnityEvent即可以与场景一起保存的零参数的持久化回调

        [SerializeField]
        private UnityEvent onExitState = new UnityEvent();

        [SerializeField]
        private UnityEvent onUpdateState = new UnityEvent();

        /// <summary>
        /// Gets the event which is invoked when the state is entered
        /// </summary>
        public UnityEvent OnEnterState => this.onEnterState;

        /// <summary>
        /// c
        /// </summary>
        public UnityEvent OnExitState => this.onExitState;

        /// <summary>
        /// Gets the event which is invoked when the state is udpated by teh state machine
        /// </summary>
        public UnityEvent OnUpdateState => this.onUpdateState;
    }
}