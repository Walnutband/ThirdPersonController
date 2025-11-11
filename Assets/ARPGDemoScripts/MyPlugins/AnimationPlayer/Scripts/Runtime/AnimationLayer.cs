using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：封装AnimationMixerPlayable，其实Mixer就代表一个层级，也可以命名为AnimationLayerNode，感觉差不多。
    并非，准确来说，AnimationMixerPlayable可以代表一个Layer，也可以代表一个混合树，在Animancer中的混合动画就是使用AnimationMixerPlayable连接若干个AnimationClipPlayable节点
    作为一个整体节点连接到作为一个Layer的AnimationMixerPlayable上来实现的，混合效果就是根据参数值来实时计算并调整连接的各个ClipPlayable节点的权重，
    实质上是调用AnimationMixerPlayable的SetInputWeight方法传入index设置对应端口上的子节点的权重占比。*/
    // public class AnimationMixerNode : AnimationNodeBase
    public class AnimationLayer : IFadeTarget
    {
        private AnimationGraph m_Graph;

        private AnimationMixerPlayable m_Playable;
        public AnimationMixerPlayable playable => m_Playable;
        private AnimationLayerMixer m_LayerMixer;

        /*Tip：我发现在AnimationLayerMixerPlayable的有关Layer设置的方法中，索引的参数类型设置为了uint而非int。*/
        private int m_Index;
        internal int index { get => m_Index; set => m_Index = value; }
        public float weight
        {
            get => m_LayerMixer.GetLayerWeight(this);
            set
            {
                m_LayerMixer.SetLayerWeight(this, value);
            }
        }
        /*Tip：从结构上来看，应当是标识为子节点而非State，但是我突然想到，这个动画系统是对于Playables的封装，其实并不一定要完全按照Playables系统的结构来组织，完全可以通过封装形成
        另一种独立的结构。
        这里注意必须保持容器元素与输入端口的一致性，因为Playable无法直接访问存储输入端口的容器。
        */
        private List<AnimationStateBase> m_States;
        internal List<AnimationStateBase> states => m_States;
        private AnimationStateBase m_CurrentState;
        internal AnimationStateBase currentState => m_CurrentState;
        internal bool isPlaying => m_CurrentState != null;

        private FadeHandler m_FadeHandler = null;
        private bool m_IsFading => m_FadeHandler != null;

        //专门为了层级混合使用的，因为在非Base层结束播放时应当逐渐过渡到Base层，而过渡时间则是依赖于Base层当前状态设定的过渡时间，但是这个时间是在Layer之外处理的
        // ，所以这里专门定义一个字段来存储，以便LayerMixer调用。
        // private float m_CurrentFadeDuration;
        internal float currentFadeDuration => m_CurrentState.fadeDuration;


        public AnimationLayer(AnimationGraph _graph, AnimationLayerMixer _layerMixer)
        {
            m_States = new List<AnimationStateBase>();
            m_Graph = _graph;
            m_Playable = AnimationMixerPlayable.Create(_graph.graph);
            m_CurrentState = null;
            m_LayerMixer = _layerMixer;
        }

        //就是要播放该状态。
        private void SetState(AnimationStateBase _state, int _index)
        {
            if (_index < 0)
            {
                Debug.Log("在设置状态所在Layer时传入的索引小于0，请检查。");
                return;
            }
            _state.index = _index;
            _state.layer = this;
            m_Graph.Connect(_state, this, _index);
            m_States[_index] = _state;
            m_CurrentState = _state; //Ques：按理来说，既然设置状态了，那么就是当前要播放的状态了？
            _state.time = 0; //直接默认从头开始。
            _state.EnterPlaying();
        }

        /*Tip：如果播放的是同一个动画的话，由于一开始就会全部清空，所以不会出现重复之类的问题。*/
        public void Play(AnimationStateBase _state)
        {
            if (m_IsFading)
            {
                Debug.Log("在播放新动画时正处于过渡状态，请检查是否符合意愿");
                m_FadeHandler.Complete(); //首先结束过渡
                // m_FadeHandler = null; //利用注册的方法顺带就置空了。
            }

            ClearStates();

            // int index = 0;
            int index = FindNullIndex(); //Tip：统一通道，因为除了最后的值以外，还有不可或缺的额外逻辑必须执行。
            SetState(_state, index);
            _state.weight = 1f; //直接权重初始化为1
        }

        public void Play(AnimationStateBase _fadeIn, float _fadeDuration)
        {
            if (m_CurrentState == null || (_fadeDuration - 0.001f) <= 0f)
            {
                Play(_fadeIn); //不进行过渡
                return;
            }

            if (m_IsFading)
            {
                Debug.Log("在播放新动画时正处于过渡状态，请检查是否符合意愿");
                m_FadeHandler.Complete(); //首先结束过渡
                // m_FadeHandler = null; //利用注册的方法顺带就置空了。
            }

            //当前存在的元素都作为fadeOut，然后新的即要播放的元素作为fadeIn
            List<IFadeTarget> fadeOuts = new List<IFadeTarget>();
            fadeOuts.AddRange(m_States.FindAll(x => x != null && x != _fadeIn));

            int index = FindNullIndex(); //找出槽位
            SetState(_fadeIn, index);
            _fadeIn.weight = 0f;
            m_FadeHandler = new FadeHandler(fadeOuts, _fadeIn, _fadeDuration, () =>
            {
                m_FadeHandler = null; //预处理器以及这里的引用清理之后，就等GC回收了。
                // ClearStates(_fadeIn);
                /*Ques：改成成员变量，可以避免捕获局部变量，因为捕获的话会将局部变量从栈提升到堆，浪费性能。但是这样的话有可能会出现逻辑问题？因为m_CurrenState不一定和fadeIn就
                保持一致，这需要前面的代码做基础。*/
                ClearStates(m_CurrentState);
            });
            m_Graph.pre.AddUpdatable(m_FadeHandler);

            // m_CurrentFadeDuration = _fadeDuration;

        }

        //Tip：正常情况下应该是不会调用Stop的，因为在播放其他状态时就会自动停止之前的状态了。可能用的多的还是在非Base层上。
        public void Stop(AnimationStateBase _state)
        {
            if (m_IsFading)
            {
                Debug.Log("在播放新动画时正处于过渡状态，请检查是否符合意愿");
                m_FadeHandler.Complete(); //首先结束过渡
                // m_FadeHandler = null; //利用注册的方法顺带就置空了。
            }

            RemoveState(_state);
        }

        //只是清空连接的节点，仍然保持输入端口数量
        private void ClearStates(AnimationStateBase _exclude = null)
        {
            Debug.Log($"ClearStates清空状态，除开{(_exclude == null ? "空" : _exclude.key)}");
            if (m_States.Count <= 0) return;

            for (int i = 0; i < m_States.Count; i++)
            {
                AnimationStateBase state = m_States[i];
                if (state != null && state != _exclude)
                {
                    Debug.Log($"清理槽位{i}上的{state.key}");
                    //断开，然后置空
                    m_Graph.Disconnect(state, this);
                    /*BUG：注意这是C#的一个常见错误，因为没有使用指针，而是所谓的“引用类型”，这里的m_States[i]与state指向的并非同一块内存，所以要直接对m_States[i]置空。*/
                    // state = null;
                    m_States[i] = null;

                    /*Ques：发现在Graph的Disconnect方法中*/
                    // if (_exclude != null && state.key == _exclude.key)
                    // {//说明是基于相同动画的状态，也就是自己转入自己，所以要将老状态直接销毁掉。
                    //     m_Graph.DestroyFreeState(state);
                    // }
                }
            }
        }

        //Tip：都应该通过该方法获取到应该连接到哪个端口。
        private int FindNullIndex()
        {
            int nullIndex = -1;
            for (int i = 0; i < m_States.Count; i++)
            {
                if (m_States[i] == null)
                {
                    nullIndex = i;
                    // break;
                    return nullIndex;
                }
#if UNITY_EDITOR
                else
                {
                    Debug.Log($"槽位{i}已被占用");
                }
#endif
            }
            if (nullIndex < 0) //为-1的话就说明没有
            {
                //说明还有没使用过的位置。
                /*Tip：其实按照正常流程，应该是Count和Capacity始终保持一致。*/
                if (m_States.Count < m_States.Capacity)
                {
                    nullIndex = m_States.Count;
                    m_States.Add(null);
                }
                else
                {//扩展一个位置
                    nullIndex = m_States.Count;
                    // ExpandInputCount(m_States.Count + 1);
                    ExpandInputCount(1);
                    /*Tip：注意List的索引访问仅限于Count范围内，也就是说必须要用Add填充才能够访问，尽管对应位置的元素本来就是默认值null。*/
                    m_States.Add(null);
                }
            }
            Debug.Log($"找到槽位：{nullIndex}");
            return nullIndex;
        }

        /*TODO：感觉没啥用，如果运行时设置的话就需要处理一些额外逻辑，而其实端口数量本来就是固定的，实际根本用不上好多个。*/
        private void ExpandInputCount(int _count) //注意输入的是绝对量还是相对量
        {
            //如果为负（无意义）或者小于当前拥有的输入端口数量，直接返回
            // if (_count <= 0 || _count == m_Playable.GetInputCount()) return;
            if (_count <= 0)
            {
                Debug.LogError("在尝试扩容时，传入的增加量小于等于0，请检查。");
                return;
            }

            m_Playable.SetInputCount(m_Playable.GetInputCount() + _count);
            List<AnimationStateBase> states = new List<AnimationStateBase>(_count);
            states.AddRange(m_States); //将之前的全部添加进去。
            m_States = states;
        }


        public float GetStateWeight(AnimationStateBase _state)
        {
            if (_state == null || _state.index < 0)
            {
                Debug.LogError("在获取状态权重时传入的State为空，或者是索引小于0");
                return -1f;
            }
            return m_Playable.GetInputWeight(_state.index);
        }
        public void SetStateWeight(AnimationStateBase _state, float _weight)
        {
            if (_state == null || _state.index < 0)
            {
                Debug.LogError("在获取状态权重时传入的State为空，或者是索引小于0");
                return;
            }
            m_Playable.SetInputWeight(_state.index, _weight);
        }

        public void RemoveState(AnimationStateBase _state)
        {
            //这一段条件判断就已经排除大量的不必要的情况了。
            if (_state == null || _state.index < 0 || _state.layer != this) return;

            if (m_States[_state.index] != _state)
            {//Tip：异常情况，正常来说经过上面的条件过滤之后，_state就必然是当然Layer上连接的某个状态，但是在此处检查索引发现与Layer自己记录的不一致，这就是该动画系统本身逻辑出了问题。
                Debug.LogError("传入状态索引与Layer自己记录的不一致，请检查。");
                return;
            }
            m_States[_state.index] = null;
            m_Graph.Disconnect(_state, this);
            /*Tip:如果移除的是当前状态。。。但其实按照这个动画系统的运行逻辑来看，调用该方法时，必然移除的就是当前状态。。。*/
            if (m_CurrentState == _state)
            {
                m_CurrentState = null;
            }
        }

        
        void IFadeTarget.StartFadeOut() { }
        void IFadeTarget.StartFadeIn() {}
        
    }
}