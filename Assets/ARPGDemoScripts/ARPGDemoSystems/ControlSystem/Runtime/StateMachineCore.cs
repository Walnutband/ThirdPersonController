using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public interface IState
    {
        bool canEnterState { get; }
        bool canExitState { get; }
        bool canTransitionToSelf { get; }
        /*Tip：标记状态是否结束，这是我所设想的新型状态机的一个核心概念。准确来说代表的是自然结束，比如一段攻击自然结束，那么就正常恢复到默认状态Idle，不过应该会先检查是否有指定
        的下一个状态，如果有的话就转换到这个指定状态，否则就是恢复到默认状态。
        其实如果对于魂游的话，canExitState和isEnd可能作用是完全相同的，因为其在游戏设计上不允许打断动画。但是对于非魂游，应该都会设置可以打断和不可以打断的动画，所以这种基本的扩展性
        还是必要的。这样的话，canExitState应该是包含了isEnd情况的，或者说isEnd是canExitState的充分不必要条件，所以还是需要将isEnd抽象出来，而不是合并在一起。*/
        bool isEnd { get; }
        void OnEnterState();
        void OnExitState();
        void OnFixedUpdate();
        void OnUpdate();
    }

    public abstract class StateBehaviour : MonoBehaviour, IState //作为组件的好处就是便于在编辑器中直接编辑
    {
        /*Tip：加入了canTransitionToSelf之后，大概可以认为，canTransitionToSelf是canExitState的必要不充分条件，而canExitState又是isEnd的必要不充分条件，
        canTransitionToSelf是开放给自己的，而canExitState是开放给其他状态的，而isEnd是开放给状态机的。*/
        //默认为true，因为通常都是指定转换状态、然后执行转换，如果存在不为true的情况的话，代表有所阻碍，这就是具体状态自身要做的事了。
        public virtual bool canEnterState => true;
        public virtual bool canExitState => true;
        public virtual bool canTransitionToSelf => false; //按照通常情况来设置默认值。
        /*Tip：似乎也不需要在基类这里定义一个字段，因为字段就是存储数据的变量，是为了应对变化，而对于很多状态的isEnd可能就是固定为true或者false，完全用不到专门的字段，
        如果需要的话，自己内部定义就行了。*/
        // [SerializeField] protected bool m_IsEnd;
        public abstract bool isEnd { get; }
        public abstract int tempPriority { get; }
        // public abstract int priority { get; } //TODO:状态优先级，应该叫切换优先级，可以作为控制状态转换的额外机制。
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
    }

    /*TODO：这里的设计还有待改进，现在就是对于具体的状态，如果是个体的那种，比如玩家、怪物之类的，状态会拥有对于其所属状态机的引用，就是为了获取一些状态本身之外的信息，
    比如上一个状态是什么，因为比如多段连招，归为一个状态，而每次进入状态都只会播放一段，那么就需要知道上次进入状态时是否是该状态，如果是的话那么证明是连招，就会
    播放下一段，如果不是的话，那么就应该播放第一段。要做到这种功能就必须让状态能够获取到状态机所存储的关于上一个状态的信息，当然也可以将这个信息设置为进入状态时
    传入的参数，但是这样一来就会增加额外的复杂度和依赖性，而且其他状态的逻辑也可能受此影响。
    也可以尝试从TryResetState方法入手？？*/
    [Serializable]
    // public class StateMachine<TState> where TState : class, IState
    public class StateMachine<TState> where TState : StateBehaviour //泛型也是一种多态，可以使得其中一些基本类型在编译时就可以确定具体类型
    {//TODO: 关于End结束，然后状态机自动转换到默认状态，这个机制，应该还可以加上一个栈机制，也就是结束后自动转换不仅限于就是默认状态，而且本来也有“栈式状态机”。
        //默认状态
        [SerializeField] protected TState m_DefaultState;
        public TState defaultState { get => m_DefaultState; }
        //当前状态
        [SerializeField] protected TState m_CurrentState;
        // public TState currentState { get => m_CurrentState; private set => m_CurrentState = value; } //既然是私有set的话，直接赋值私有的字段就行了。
        /*Tip：开放给控制器，得知此时处于什么状态，这样会影响如何响应输入命令。*/
        public TState currentState { get => m_CurrentState; }
        //上一个状态，这个记录很重要。
        [SerializeField] protected TState m_PreviousState;
        public TState previousState { get => m_PreviousState; }
        //TODO：下一个状态，该成员有何用处还有待测试
        [SerializeField] protected TState m_TempNextState;
        public TState tempNextState { get => m_TempNextState; }

        private bool m_HasInitialized = false;

        public virtual void Initialize(TState _defaultState = null)
        {
            // Debug.Log("初始化状态机");
            //设计上，传入默认状态就肯定是非空的，要不然就不传入则默认为空。
            if (_defaultState != null) m_DefaultState = _defaultState;
            /*TODO：要么进入指定的当前状态，要么进入默认状态。但是从这个方法的过程，我想到状态机从逻辑上来看算是一个不断运行的机器，那么是否会加入暂停运行和恢复运行的功能？
            这样的话，可能在初始化的逻辑中就需要有所变化了。*/
            if (m_CurrentState != null) //这种就是在开始时进入指定状态，但实际上应该不会提前指定CurrentState，应该刚开始时就是自动进入DefaultState
            {
                m_CurrentState.OnEnterState();
            }
            else if (m_DefaultState != null)
            {
                m_DefaultState.OnEnterState();
                m_CurrentState = m_DefaultState;
            }
        }

        public virtual void OnFixedUpdate()
        {
            if (m_CurrentState != null)
            {/*TODO：至于是否要在FixedUpdate中检查当前状态是否运行结束，暂时没想到明确的原因。*/
                m_CurrentState.OnFixedUpdate();
            }
        }

        public virtual void OnUpdate()
        {
            if (m_HasInitialized == false)
            {
                Initialize();
                m_HasInitialized = true;
            }

            // Debug.Log($"当前的NextState：{(m_TempNextState != null ? m_TempNextState.GetType().Name : "null")}");
            /*假设一种情况，某一帧在控制器中接收命令，通知状态机要切换状态，发现暂时切换不了，所以就缓存到了TempNextState，然后下一帧在这里的监测方法OnUpdate中一开始就
            尝试切换到TempNextState*/
            if (m_TempNextState != null)
            {
                /*TODO：其实TryReset这个东西也就是是否能够从当前状态转换到当前状态，应该根据具体状态自己的逻辑来判断，比如设置一个属性canTransitionToSelf，而不是搞一个
                TryReset非常别扭，完全可以融入到TrySetState的逻辑中。*/
                // TryResetState(m_TempNextState);
                TrySetState(m_TempNextState);
            }

            if (m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(); //在这一帧运行之后，检查。
                //引入一个机制，如果当前状态结束了，就自动切换。其实AnimatorController中就有这种机制（Exit Time）而且感觉对于状态机来说确实挺重要的
                if (m_CurrentState.isEnd == true) //运行完毕（自然结束，回归到默认状态）
                {
                    // if (m_TempNextState != null)
                    // {//TODO：是否使用Force，还是Try，有待考虑
                    //     ForceSetState(m_TempNextState);
                    // }
                    // else
                    // {
                    // ForceSetState(m_DefaultState);
                    TrySetState(m_DefaultState);
                    // }
                }
            }

            // m_TempNextState = null;
        }

        public bool CanSetState(TState state)
        {
            //可退出以及可进入，两个if语句都是排除指定的不符合情况，其他情况都是符合的。
            if (m_CurrentState != null && !m_CurrentState.canExitState)
                return false;

            if (state != null && !state.canEnterState)
                return false;

            return true;
        }

        //使用IList而不是List，有一个扩展性好处是，可以使用自定义的实现了IList接口的容器类。
        public TState CanSetState(IList<TState> states)
        {
            var count = states.Count;
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (CanSetState(state))
                    return state;
            }

            return null;
        }

        public bool TrySetState(TState state)
        {
            // if (state == null) return false;
            // if (state != null) m_TempNextState = state;
            if (state == null) return false;

            UpdateTempNextState(state);

            //已经是当前状态，并且不能自转换
            // if (m_CurrentState == state && m_CurrentState.canTransitionToSelf == false)
            if (m_CurrentState == state)
            {
                //特殊状态可以通过控制canTransitionSelf的值来实现抢先机制，不需要考虑canExitState，因为canExitState从逻辑上是对所有状态开放，而canTransitionToSelf是对自己开放。
                if (m_CurrentState.canTransitionToSelf == true)
                {
                    ForceSetState(state);
                }
                return false;
            }
            // Debug.Log($"TrySetState: {state.GetType().Name}");
            return TryResetState(state);
        }

        public bool TrySetState(IList<TState> states)
        {
            var count = states.Count;
            for (int i = 0; i < count; i++)
                if (TrySetState(states[i]))
                    return true;

            return false;
        }

        //TODO：可能会增加一个参数，标示是否要将转换目标缓存起来。
        public bool TryResetState(TState state, bool temp = false)
        {
            //TODO：判空主要是因为在比如控制器中的状态字段没有正确赋值，导致传入空，然后引起一系列bug，但实际编辑时肯定是要求把状态都赋值好，但测试时确实不会考虑那么多。
            if (state == null) return false;

            UpdateTempNextState(state);

            if (!CanSetState(state))
                return false;
            // m_TempNextState = null; //可以转换，那就不存储了，所以本质上是TempNextState。
            // Debug.Log($"TryResetState: {state.GetType().Name}");
            ForceSetState(state);
            return true;
        }

        public bool TryResetState(IList<TState> states)
        {
            var count = states.Count;
            for (int i = 0; i < count; i++)
                if (TryResetState(states[i]))
                    return true;

            return false;
        }

        //强制切换状态，也就是不进行状态切换前的那些判断。
        public void ForceSetState(TState state)
        {
            /*由于?.属于C#本身的语法，而UntiyEngine.Object的派生类的==和!=是经过重载的，两者判断是否为空的逻辑不同，其实就是后者可以将销毁状态视为null，而前者却不能，这样可能
            会在某些情况下出现一些奇怪的问题，所以此处选择使用!=而不是?.*/
            // m_CurrentState?.OnExitState();
            m_PreviousState = m_CurrentState;

            if (m_CurrentState != null)
            {
                m_CurrentState.OnExitState();
            }

            m_CurrentState = state;

            // state?.OnEnterState();
            if (state != null)
            {
                // Debug.Log("ForceSetState: " + state.GetType().Name);
                state.OnEnterState();
                //消耗掉TempNextState
                if (state == m_TempNextState) m_TempNextState = null;
            }
        }

        //保证了传入的state非空
        private bool UpdateTempNextState(TState state)
        {
            //将这一种情况排除掉，其他都是要更新TempNextState的。
            //如果优先级相同的话，则后来的会替换先来的，也就是预输入中的覆盖机制，当然这里的实现比较简单。
            // if (!(m_TempNextState != null && state.tempPriority < m_TempNextState.tempPriority))
            // {
            //     m_TempNextState = state;
            //     return true;
            // }
            // else return false;
            // Debug.Log("尝试设置NextState：" + state.GetType().Name);
            if (m_TempNextState != null && state.tempPriority < m_TempNextState.tempPriority)
                return false;
            else
            {
                m_TempNextState = state;
                return true;
            }
        }

    }


    public class ActorStateMachine
    {
        
    }

}