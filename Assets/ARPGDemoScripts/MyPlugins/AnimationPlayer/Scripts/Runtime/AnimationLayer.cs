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
    public class AnimationLayer
    {
        private AnimationGraph m_Graph;

        private AnimationMixerPlayable m_Playable;
        public AnimationMixerPlayable playable => m_Playable;
        private AnimationLayerMixer m_LayerMixer;

        /*Tip：我发现在AnimationLayerMixerPlayable的有关Layer设置的方法中，索引的参数类型设置为了uint而非int。*/
        private int m_Index;
        internal int index { get => m_Index; set => m_Index = value; }
        private float m_Weight;
        internal float weight { get => m_Weight; set => m_Weight = value; }
        /*Tip：从结构上来看，应当是标识为子节点而非State，但是我突然想到，这个动画系统是对于Playables的封装，其实并不一定要完全按照Playables系统的结构来组织，完全可以通过封装形成
        另一种独立的结构。
        这里注意必须保持容器元素与输入端口的一致性，因为Playable无法直接访问存储输入端口的容器。
        */
        private List<AnimationStateBase> m_States;
        public List<AnimationStateBase> states => m_States;
        private AnimationStateBase m_CurrentState;

        // public int stateCount => 

        public AnimationLayer(AnimationGraph _graph, AnimationLayerMixer _layerMixer)
        {
            m_Weight = 1f;
            m_States = new List<AnimationStateBase>();
            m_Graph = _graph;
            m_LayerMixer = _layerMixer;
            //作为一个Layer，默认没有子节点，因为它的子节点就直接代表当前所播放的动画内容。
            // m_Playable = AnimationMixerPlayable.Create(_graph.graph, 8); //不如直接一开始就设置为8个端口算了
            m_Playable = AnimationMixerPlayable.Create(_graph.graph, 2); //不如直接一开始就设置为8个端口算了
            // m_States = new List<AnimationStateBase>(8);
            m_States = new List<AnimationStateBase>(2);
            //直接连接到对应的LayerMixer，正好将数量作为连接到的下标。
            //Tip：还是让LayerMixer来处理连接逻辑吧。
            // _graph.graph.Connect(m_Playable, 0, _layerMixer.playable, _layerMixer.layerCount);
            // m_Graph.Connect(this, _layerMixer, _layerMixer.layerCount);
        }

        /*Tip：由于结构非常固定，Layer就必然是连接到LayerMixer节点上，而且随后就不会变化、主要是也不需要变化，所以甚至连SetParent这种方法都完全不需要，只要在构造函数中处理好即可。*/
        // public void SetParent()

        /*Tip：附加播放，就是不断开其他State节点。*/
        // public void Play(AnimationClip _clip)
        // public void PlayAdditive(AnimationStateBase _state)
        // {//连接即播放。
        //     int index = FindNullIndex();
        //     m_States[index] = _state;
        //     // m_Graph.graph.Connect(_state.playable, 0, m_Playable, index);
        //     m_Graph.Connect(_state, this, index);
        //     // m_Playable.SetInputWeight(index, _state.weight);
        //     // _state.index = index; //TODO：其实感觉index应该让state自己设置。。。

        // }

        public void Play(AnimationStateBase _state)
        {
            // _state.Reset();
            _state.EnterPlaying();

            m_Graph.pre.EndFading(); //直接结束过渡过程
            ClearStates();
            // PlayAdditive(_state);
            int index = FindNullIndex();
            m_States[index] = _state;
            _state.weight = 1f; //直接权重初始化为1
            _state.layer = this;
            m_Graph.Connect(_state, this, index);
            m_CurrentState = _state;
        }

        public void Play(AnimationStateBase _fadeIn, float _fadeDuration)
        {
            // _fadeIn.Reset();
            _fadeIn.EnterPlaying();

            if (_fadeIn == m_CurrentState || m_CurrentState == null || (_fadeDuration - 0.0001f) < 0f)
            {
                Play(_fadeIn); //不进行过渡
                return;
            }

            int index = FindNullIndex(); //找出槽位
            //从当前状态过渡到指定状态
            FadeHandler fade = new FadeHandler(this, m_CurrentState, _fadeIn, _fadeDuration);
            //虽然在过渡，但逻辑上已经是处于转入状态了。
            m_States[index] = _fadeIn;
            _fadeIn.weight = 0f; //从0开始。
            _fadeIn.layer = this;
            m_Graph.Connect(_fadeIn, this, index);
            m_Graph.pre.RegisterFadeHandler(fade); //注册过渡处理器，即开始过渡过程。
            m_CurrentState = _fadeIn;
        }

        private void ClearStates()
        {
            for (int i = 0; i < m_States.Count; i++)
            {
                // m_States[i] = null;
                if (m_States[i] != null)
                {
                    //断开，然后置空
                    m_Graph.Disconnect(m_States[i], this);
                    m_States[i] = null;
                }
            }
            //大概也没必要处理空位。
        }

        //Tip：都应该通过该方法获取到应该连接到哪个端口。
        private int FindNullIndex()
        {
            int nullIndex = -1;
            // int nullIndex = m_States.IndexOf(null); //这会返回第一个为null的元素的索引。
            // int nullIndex = m_States.FindIndex(x => x == null); //这会返回第一个为null的元素的索引。
            for (int i = 0; i < m_States.Count; i++)
            {
                if (m_States[i] == null)
                {
                    nullIndex = i;
                    break;
                }
            }
            if (nullIndex < 0) //为-1的话就说明没有
            {
                //说明还有没使用过的位置。
                if (m_States.Count < m_States.Capacity)
                {
                    nullIndex = m_States.Count;
                    m_States.Add(null);
                }
                else
                {//扩展一个位置
                    nullIndex = m_States.Count;
                    ExpandInputCount(m_States.Count + 1);
                    /*Tip：注意List的索引访问仅限于Count范围内，也就是说必须要用Add填充才能够访问，尽管对应位置的元素本来就是默认值null。*/
                    m_States.Add(null);
                }
            }

            return nullIndex;
        }

        /*TODO：感觉没啥用，如果运行时设置的话就需要处理一些额外逻辑，而其实端口数量本来就是固定的，实际根本用不上好多个。*/
        private void ExpandInputCount(int _count) //输入的是绝对量
        {
            //如果为负（无意义）或者小于当前拥有的输入端口数量，直接返回
            if (_count <= 0 || _count == m_Playable.GetInputCount()) return;

            m_Playable.SetInputCount(_count);
            List<AnimationStateBase> states = new List<AnimationStateBase>(_count);
            states.AddRange(m_States); //将之前的全部添加进去。
            m_States = states;
        }

        /*Tip：连接就意味着播放，所以按理来说应当放在一起，而不是分开。*/
        // private void AddState(AnimationStateBase)

        public void SetStateWeight(AnimationStateBase _state, float _weight)
        {
            m_Playable.SetInputWeight(_state.index, _weight);
        }

        public void RemoveState(AnimationStateBase _state)
        {
            if (_state == null) return;
            int index = m_States.IndexOf(_state);
            if (index >= 0) //找到，置空，断连。
            {
                m_States[index] = null;
                m_Graph.Disconnect(_state, this);
            }
        }

        public void Stop(AnimationStateBase _state)
        {
            RemoveState(_state);
        }

    }
}