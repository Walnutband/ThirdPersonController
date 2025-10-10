
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{//Tip：AnimationStateBase的派生类会被外界获取，以便了解当前动画状态的相关信息，所以必须保证与Playable本身的信息是同步的。
    public abstract class AnimationStateBase
    {
        //所在的Graph
        protected AnimationGraph m_Graph; //对于PlayableGraph的封装
        //自己所封装的节点，具体来说就是AnimationClipPlayable、AnimationMixerPlayable、AnimationLayerMixerPlayable这三种可能。
        protected Playable m_Playable;
        public Playable playable => m_Playable;
        //父节点，但只有一部分节点值得记录这个数据。还涉及到节点之间的连接和断连。
        // protected Playable m_Parent;
        protected AnimationLayer m_Layer;
        internal AnimationLayer layer { get => m_Layer; set => m_Layer = value; }

        /*Tip：发现设置为internal可以在达到被设置的目的的同时避免外部类修改。*/

        //记录索引，就是自己连接到父节点的输入端口的端口号，在Playabbles系统中对指定子节点进行操作时都需要指定这个端口号。
        protected int m_Index;
        internal int index { get => m_Index; set => m_Index = value; }

        //节点的权重，不过并非Playble节点公开属性，这里只是记录，最终通过SetInputWeight落实到权重影响，而是是调用父节点的SetInputWeight，这里的Weight主要是记录给父节点访问的。
        protected float m_Weight;
        internal float weight { get => m_Weight; set => m_Weight = value; }

        // protected float m_TimeF;
        // protected double m_TimeD;
        internal double time
        {
            get
            {
                return m_Playable.GetTime();
            }
            set
            {
                // m_TimeD = value;
                m_Playable.SetTime(value);
                m_Playable.SetTime(value);
            }
        }

        protected object m_Key;
        internal object key { get => m_Key; set => m_Key = value; }

        // //这个结束事件似乎没有什么信息。
        // //默认结束，就是片段长度为触发时刻
        // public event Action EndedEvent; //感觉事件用大驼峰命名更好。
        // //自定义结束，可以自行设置触发时刻，用于衔接动画。
        // public event Action CustomEndedEvent;

        protected AnimationStateBase(AnimationGraph _graph)
        {
            m_Graph = _graph;
            // m_Playable = AnimationClipPlayable.Create(_graph.graph, _clip);
            // // m_Key = _clip.GetInstanceID(); //使用唯一的实例ID作为Key
            // m_Key = AnimationStateDictionary.StateID(_clip); //Ques：将转换到ID的逻辑方法统一放到字典中是否合适呢？
            m_Weight = 1f;
            /*TODO：-1就代表这是个游离状态，没有连接到（层级）节点上，即没有参与运行，但是仍然被记录着，并且随时可以启用
            当然这有很多处理方式，比如专门设置一个bool标记变量，比如在AnimationGraph中专门设置一个容器存储CurrentStates当前正在运行的状态，等等
            */
            m_Index = -1;
        }

        // protected Playable CreatePlayable(AnimationGraph _graph, )

        /*Tip：在Animancer中这个方法是使用的基类，但是我的Layer和LayerMixer并没有设置为基类，因为我发现其实不需要基类提供的变化，这里其实就必然是连接到AnimationLayer的节点。*/
        // public void SetParent(AnimationLayer _layer) //传入的就只会是所属层级
        // {
        //     m_Layer = _layer;
        //     m_Graph.graph.Connect(m_Playable, 0, )
        // }

        //重置
        // internal void Reset()
        // {
        //     Debug.Log("重置State");
        //     time = 0;
        // }

        /*Tip：开始播放时执行一些逻辑、类似于OnEnable方法的作用，而结束播放时可能也会有一些逻辑类似OnDisable。*/
        internal virtual void EnterPlaying()
        {
            // Reset();
            time = 0;
        }
        internal virtual void ExitPlaying()
        {

        }

        public void Play(int _layerIndex = 0)
        {
            m_Graph.Play(_layerIndex, this);
            // Reset();
        }

        public void Stop()
        {
            m_Graph.Stop(m_Layer, this);
        }
    }
}