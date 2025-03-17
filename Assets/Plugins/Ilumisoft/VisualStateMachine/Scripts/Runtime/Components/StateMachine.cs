namespace Ilumisoft.VisualStateMachine
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    [DefaultExecutionOrder(-1)] //执行顺序设为-1即优先于所有普通组件（normal components）
    [DisallowMultipleComponent] //不允许添加多于一个的该脚本到同一个游戏对象上
    public class StateMachine : MonoBehaviour
    {
        [SerializeField]
        private Graph graph = new Graph();

        /// <summary>
        /// Returns the ID of the previously active state. Empty if the state machine has not changed states yet
        /// 返回上一个活跃状态的ID
        /// </summary>
        public string PreviousState { get; protected set; } = string.Empty;

        /// <summary>
        /// Returns the ID of the currently active state or string. Empty if none is active
        /// 返回当前活跃的状态（ID）
        /// </summary>
        public string CurrentState { get; protected set; } = string.Empty;

        /// <summary>
        /// Gets the graph of the state machine, which contains all nodes and transitions
        /// 获取状态机的graph即图形化显示，其包含所有的节点和转换
        /// </summary>
        public Graph Graph => graph;
        //=>：这个符号称为 "表达式主体定义" 操作符，用于简化属性或方法的定义。它的作用是将属性的值绑定到一个表达式或字段。
        //就相当于public Graph Graph{get{return graph;}}这是一个只读属性

        /// <summary>
        /// Gets the ID of the entry state获取进入状态的ID
        /// </summary>
        public string EntryState => this.graph.EntryStateID;

        /// <summary>
        /// Returns true if the state machine has switched states yet
        /// 是否存在之前（上一个）状态
        /// </summary>
        public bool HasPreviousState => PreviousState != string.Empty;

        public bool IsPaused { get; protected set; } = false;

        // The transition handler executes transitions
        TransitionHandler transitionHandler;

        #region Callbacks
        /*注意以下定义的是UnityAction，区别于UnityEvent
        定义方式：
        UnityAction 是一个委托，用于定义回调方法。
        UnityEvent 是一个类，可以在编辑器中配置回调方法，并支持序列化。
        使用场景：
        UnityAction 适用于代码中直接设置和调用的简单回调。
        UnityEvent 适用于需要在编辑器中设置和管理的复杂事件系统。*/

        /// <summary>
        /// Callback invoked when a state is entered
        /// </summary>
        public UnityAction<State> OnEnterState = null;

        /// <summary>
        /// Callback invoked when a state is left
        /// </summary>
        public UnityAction<State> OnExitState = null;

        /// <summary>
        /// Callback invoked when a transition is triggered.
        /// Event Order: OnTriggerTransition->OnExitState->OnEnterTransition->OnExitTransition->OnEnterState
        /// </summary>
        public UnityAction<Transition> OnTriggerTransition = null;

        /// <summary>
        /// Callback invoked when a transition is entered
        /// </summary>
        public UnityAction<Transition> OnEnterTransition = null;

        /// <summary>
        /// Callback invoked when a transition is left
        /// </summary>
        public UnityAction<Transition> OnExitTransition = null;
        #endregion

        private void Awake()
        {
            transitionHandler = new TransitionHandler(this);
        }

        public void Start() //注意这里是公开方法，则可以从其他脚本中调用（因为用于游戏中的个体，就需要这样的功能）
        {
            // Don't reset state machine, if it has already been started.
            // When the state machine is created at runtime, this method might get called multiple times
            // or from anopther script
            if (CurrentState == string.Empty)
            {
                Restart();
            }
        }

        private void Update()
        {
            // When the state machine is paused, states and running transitions won't be updated
            if (IsPaused)
            {
                return;
            }
            //转换过程
            transitionHandler.Update();

            // Trigger the OnUpdateState event of the currently active state
            //当前状态的主循环
            if (CurrentState != string.Empty && graph.TryGetState(CurrentState, out State state))
            {
                state?.OnUpdateState?.Invoke();
            }
        }

        /// <summary>
        /// Restarts the state machine by entering the entry state.
        /// Remark: The OnExitState event of the currently active state will not be triggered, when calling this method!
        /// 通过进入entry状态来重启状态机
        /// 注意：调用该方法并不会调用当前方法的OnExitState事件。其实这更加符合“重启”的含义
        /// </summary>
        public void Restart()
        {
            IsPaused = false;

            transitionHandler.Stop();

            this.PreviousState = string.Empty;
            this.CurrentState = this.EntryState;

            if (this.graph.TryGetNode(this.CurrentState, out Node node))
            {
                if (node is State state) //State继承自Node
                {
                    OnEnterState?.Invoke(state);
                    state.OnEnterState.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("Could not start state machine, because no entry state has been set", this);
            }
        }

        /// <summary>
        /// Instantly enters the state with the given ID.
        /// Please Note: Using this method will not call OnExit on the current state or require a transition between the current state and the given one.
        /// </summary>
        /// <param name="stateID"></param>
        public void ForceEnterState(string stateID)
        {
            if (this.graph.TryGetNode(stateID, out Node targetNode))
            {
                if (targetNode is State state)
                {
                    this.PreviousState = CurrentState;
                    this.CurrentState = state.ID;

                    OnEnterState?.Invoke(state);

                    state.OnEnterState.Invoke();

                    return;
                }
            }

            Debug.LogWarning($"Failed to enter state '{stateID}', because there is no state with the given ID on this state machine", this);
        }

        /// <summary>
        /// Tries to trigger the first transition found, which has the given label and goes out from the currently active state. 
        /// Returns true on success, false if none could be found.
        /// </summary>
        /// <param name="transitionLabel"></param>
        /// <returns></returns>
        public bool TryTriggerByLabel(string transitionLabel)
        {
            // Cancel if triggering a transition is currently not possible
            if (!CanTriggerTransitions())
            {
                return false;
            }

            // Check if there is any transition with the given label and the current state as its origin
            foreach (var transition in graph.Transitions)
            {//标签和原始状态
                if (transition.Label == transitionLabel && transition.OriginID == CurrentState)
                {
                    Trigger(transition);
                    return true;
                }
            }

            // Check if there is any transition with the given label and any state as its origin
            foreach (var transition in graph.Transitions)
            {
                if (transition.Label == transitionLabel)
                {//AnyState的存在就是将在某些情况下想用和不想用的部分给分开，而不是捆绑在一起

                    if (graph.TryGetNode(transition.OriginID, out Node node) && node is AnyState)
                    {
                        Trigger(transition);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Triggers the first transition found, which has the given label and goes out from the currently active state.
        /// 触发第一个找到的符合的transition，符合的条件是其Label与给定的label相同，并且从当前活跃状态转出
        /// </summary>
        /// <param name="transitionLabel"></param>
        public void TriggerByLabel(string transitionLabel)
        {
            // Cancel if triggering a transition is currently not possible
            if (!CanTriggerTransitions()) //无法触发转换
            {
                return;
            }
            //在该方法中还会调用CanTriggerTransitions，因为有可能在其他引用的地方的前面并没有调用CanTriggerTransitions，这就是一般性
            //所以在这里为啥不直接调用这个方法？？？
            if (TryTriggerByLabel(transitionLabel) == false)
            {
                Debug.LogWarning($"There is no transition with label {transitionLabel}, which could be triggered in the current context", this);
            }
        }

        /// <summary>
        /// Tries to trigger the first transition found which goes to the given state id from the currently active state. 
        /// </summary>
        /// <param name="stateID">ID of the state we're transitioning to.</param>
        public void TriggerByState(string stateID)
        {
            if (TryTriggerByState(stateID) == false)
            {
                Debug.LogWarning($"There is no transition from the current node to state {stateID}.", this);
            }
        }

        /// <summary>
        /// Tries to trigger the first transition found which goes to the given state id from the currently active state. 
        /// 通过给定的状态ID触发第一个找到的转换
        /// </summary>
        /// <param name="stateID">ID of the state we're transitioning to.</param>
        /// <returns>True on success, false if a suitable transition could not be found.</returns>
        public bool TryTriggerByState(string stateID)
        {
            // Cancel if triggering a transition is currently not possible
            if (!CanTriggerTransitions())
            {
                return false;
            }

            // Check if there is any transition to the given state with the current state as its origin
            foreach (var transition in graph.Transitions)
            {//出入ID均符合
                if (transition.OriginID == CurrentState && transition.TargetID == stateID)
                {
                    Trigger(transition);
                    return true;
                }
            }

            // Check if there is any transition to the given state with any state as its origin
            foreach (var transition in graph.Transitions)
            {//AnyState的含义就是其所指向的状态可以在任何状态下通过指定其ID而转入，当然其他普通状态更优先。其实Animator状态机中就有AnyState
                if (transition.TargetID == stateID && graph.TryGetNode(transition.OriginID, out Node node) && node is AnyState)
                {
                    Trigger(transition);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to trigger the transition with the given ID and returns true on success, false otherwise.
        /// 通过给定的transitionID触发相应的转换
        /// </summary>
        /// <param name="transitionID"></param>
        /// <returns></returns>
        public bool TryTrigger(string transitionID)
        {
            // Cancel if triggering a transition is currently not possible
            if (!CanTriggerTransitions())
            {
                return false;
            }

            bool success = false;

            if (graph.TryGetTransition(transitionID, out Transition transition))
            {
                if (this.graph.TryGetNode(transition.OriginID, out Node originNode))
                {
                    if (originNode is State state && state.ID == this.CurrentState)
                    {
                        success = true;
                    }//该转换从AnyState转出
                    else if (originNode is AnyState anyState)
                    {
                        success = true;
                    }
                }
            }

            if (success)
            {
                Trigger(transition);
            }

            return success;
        }

        /// <summary>
        /// Triggers the transition with the given id, if it exists and if its origin state is the current state or an any state
        /// </summary>
        /// <param name="transitionID"></param>
        public void Trigger(string transitionID)
        {
            // When the state machine is paused, all transitions will be ignored
            if (IsPaused)
            {
                return;
            }

            if (TryTrigger(transitionID) == false)
            {
                if (graph.TryGetTransition(transitionID, out Transition transition))
                {
                    if (transition.OriginID != CurrentState)
                    {
                        Debug.LogWarningFormat("Failed to trigger transition with id {0}, because the current state is not its origin", transitionID);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Failed to trigger transition with id {0}, because no transition with this id exists", transitionID);
                }
            }
        }

        string loopOrigin = string.Empty;

        /// <summary>
        /// Returns true if transitions can be triggered, false if not (e.g. because the state machine is paused or currently in a transition with transition mode being set to 'locked')
        /// 无法触发转换的两种情况：状态机暂停（IsPaused=true）；有转换正在进行且TransitionMode为Locked
        /// </summary>
        /// <returns></returns>
        bool CanTriggerTransitions()
        {
            // When the state machine is paused, all transitions will be ignored
            //状态机暂停时，所有转换都会被忽略
            if (IsPaused)
            {
                return false;
            }

            // If a (timed) transition is currently being executed and the transition mode is set to 'Locked', triggering another transition will be ignored
            //如果是Locked模式，应该就是无法打断正在转换的状态
            if (StateMachineManager.Instance.Configuration.TransitionMode == TransitionMode.Locked) //非锁定就不管有无正在进行的转换，总之注意逻辑上的先后关系
            {
                if (transitionHandler.IsRunning) //正在进行转换
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Triggers the given transition触发所给定的转换（在前置条件判断完成之后才调用该方法）
        /// </summary>
        /// <param name="transition"></param>
        public void Trigger(Transition transition)
        {
            // Cancel if triggering a transition is currently not possible
            if (!CanTriggerTransitions())
            {
                return;
            }

            if (transition == null)
            {
                Debug.LogWarningFormat("Failed to trigger the given transition, because it is null");
                return;
            }
            //传入非空，但是列表记录中没有
            if (transition != null && graph.HasTransition(transition.ID) == false)
            {
                Debug.LogWarningFormat("Failed to trigger transition with name {0}, because no transition with this id exists", transition.ID);
                return;
            }

            // Make sure the transition is not in an infinite call loop.
            // This can happen for transitions between states, which are called instantly and from OnEnterState, e.g.
            // StateA->StateB-StateC->StateA
            // If each of these states uses OnEnterState to trigger the next, this would cause an infite loop of method calls...
            if (loopOrigin == string.Empty)
            {//记录下触发转换的起点。大概是因为有可能在一帧中多处调用该方法？
                loopOrigin = transition.OriginID;
            }
            else if (transition.OriginID == loopOrigin)
            {
                Debug.LogWarning("Stopped executing transition '" + transition.ID + "' due to a transition loop outgoing from state '" + loopOrigin + "'. Make sure you have no circular transitions triggered by OnEnterState, e.g. StateA->StateB-StateC->StateA", this);
                return;
            }

            OnTriggerTransition?.Invoke(transition); //状态机，触发转换

            // Execute the transition
            transitionHandler.Execute(transition);

            loopOrigin = string.Empty;
        }

        /// <summary>
        /// Tries to exit the origin state of the given transition and returns true on success, false otherwise
        /// 尝试退出所给转换的原始状态
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        private bool TryExitTransitionOriginState(Transition transition)
        {
            if (this.graph.TryGetNode(transition.OriginID, out Node originNode))
            {
                //对应状态存在，且为当前状态
                if (originNode is State state && state.ID == this.CurrentState)
                {
                    OnExitState?.Invoke(state); //状态机本身的UnityAction
                    state.OnExitState.Invoke();
                }
                else if (originNode is AnyState anyState)
                {
                    if (this.graph.TryGetNode(this.CurrentState, out Node activeState))
                    {
                        if (activeState is State)
                        {
                            OnExitState?.Invoke((State)activeState);
                            ((State)activeState).OnExitState.Invoke();
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Failed to trigger transition with name {0}, because its origin is not the current state", transition.ID, this);

                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to enter the target state of the given transition and returns true on success, false otherwise.
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        private bool TryEnterTransitionTargetState(Transition transition)
        {
            if (this.graph.TryGetNode(transition.TargetID, out Node targetNode))
            {
                if (targetNode is State state)
                {
                    this.PreviousState = this.CurrentState;
                    this.CurrentState = state.ID;

                    OnEnterState?.Invoke(state);
                    state.OnEnterState.Invoke();

                    return true;
                }
            }

            return false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        //在计算机编程和软件工程中，handler 是一个广泛使用的术语，通常指的是用于处理特定事件、操作或请求的代码块。
        class TransitionHandler
        {
            readonly StateMachine stateMachine; //需要通过状态机查找当前状态等等（直接调用状态机的公开方法）
            readonly TransitionTimer timer; //转换计时器

            // The transition being executed正在执行的转换
            Transition transition;

            public bool IsRunning { get; private set; } = false; //标记是否执行Update

            public TransitionHandler(StateMachine stateMachine)
            {
                this.stateMachine = stateMachine;

                timer = new TransitionTimer();
            }

            /// <summary>
            /// Executes the given transition执行所传入的转换
            /// </summary>
            /// <param name="transition"></param>
            public void Execute(Transition transition) //传入要执行的转换（要用到其）
            {
                this.transition = transition;

                //瞬时或延时
                // If the transition has a duration it will not be completed after a given amount of time
                if (transition.Duration > 0.0f)
                {
                    IsRunning = true; //有duration才running
                    timer.Start(transition.Duration, transition.TimeMode);
                    StartTransition();
                }
                // Otherwise it will be executed instantly立即完成转换
                else
                {//没有duration，直接完成
                    IsRunning = false;
                    StartTransition(); //退出旧状态，进入转换
                    CompleteTransition(); //退出转换，进入新状态
                }
            }

            public void Update()
            {
                if (IsRunning == false)
                {
                    return;
                }

                timer.Update(); //计时

                if (timer.IsCompleted) //计时结束
                {
                    CompleteTransition();
                    IsRunning = false;
                }
            }

            public void Stop()
            {
                IsRunning = false;
            }

            /// <summary>
            /// 开始转换（核心在于transition提供的信息：原始状态和目标状态、转换时间和转换模式，以及出入事件）
            /// </summary>
            void StartTransition()
            {
                stateMachine.TryExitTransitionOriginState(transition);
                stateMachine.OnEnterTransition?.Invoke(transition);
                transition.OnEnterTransition.Invoke();
            }

            void CompleteTransition()
            {
                stateMachine.OnExitTransition?.Invoke(transition);
                transition.OnExitTransition.Invoke();
                stateMachine.TryEnterTransitionTargetState(transition);
            }
        }

        class TransitionTimer
        {
            TimeMode timeMode; //Scaled、Unscaled
            float elapsed = 0.0f; //计时器
            float duration = 0.0f; //总时间

            public bool IsCompleted = false;
            //初始化，传入转换时间和模式
            public void Start(float duration, TimeMode timeMode)
            {
                this.timeMode = timeMode;
                this.duration = duration;
                elapsed = 0.0f;
                IsCompleted = false;
            }

            public void Update()
            {
                if (IsCompleted)
                {
                    return;
                }
                //未完成，每帧计时
                elapsed += (timeMode == TimeMode.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

                if (elapsed >= duration)
                {
                    elapsed = duration;
                    IsCompleted = true; //标记已完成
                }
            }
        }

        #region Obsolete

        /// <summary>
        /// Returns true if the graph contains a state with the given id, false otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.HasState instead.")]
        public bool HasState(string id) => graph.HasState(id);

        /// <summary>
        /// return true if the graph contains a transition with the given id, false otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.HasTransition instead.")]
        public bool HasTransition(string id) => graph.HasTransition(id);

        /// <summary>
        /// Returns the OnEnterState event of the state with the given name, null if the state does not exist 
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.GetState instead.")]
        public UnityEvent GetOnEnterStateEvent(string stateName)
        {
            if (graph.TryGetNode(stateName, out Node node))
            {
                if (node is State state)
                {
                    return state.OnEnterState;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the OnExitState event of the state with the given name, null if the state does not exist 
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.GetState instead.")]
        public UnityEvent GetOnExitStateEvent(string stateName)
        {
            if (graph.TryGetNode(stateName, out Node node))
            {
                if (node is State state)
                {
                    return state.OnExitState;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the OnEnterTransition event of the transition with the given name, null if the transition does not exist 
        /// </summary>
        /// <param name="transitionName"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.GetTransition instead.")]
        public UnityEvent GetOnEnterTransitionEvent(string transitionName)
        {
            if (graph.TryGetTransition(transitionName, out Transition transition))
            {
                return transition.OnEnterTransition;
            }

            return null;
        }

        /// <summary>
        /// Returns the OnExitTransition event of the transition with the given name, null if the transition does not exist 
        /// </summary>
        /// <param name="transitionName"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete. Call Graph.GetTransition instead.")]
        public UnityEvent GetOnExitTransitionEvent(string transitionName)
        {
            if (graph.TryGetTransition(transitionName, out Transition transition))
            {
                return transition.OnExitTransition;
            }

            return null;
        }
        #endregion
    }
}