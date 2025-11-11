
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{//Tip：AnimationStateBase的派生类会被外界获取，以便了解当前动画状态的相关信息，所以必须保证与Playable本身的信息是同步的。
    public abstract class AnimationStateBase : IFadeTarget 
    {
        protected AnimationGraph m_Graph; //对于PlayableGraph的封装
        protected Playable m_Playable;
        public Playable playable => m_Playable;
        //父节点，但只有一部分节点值得记录这个数据。还涉及到节点之间的连接和断连。
        // protected Playable m_Parent;
        protected AnimationLayer m_Layer;
        internal AnimationLayer layer { get => m_Layer; set => m_Layer = value; }

        //记录索引，就是自己连接到父节点的输入端口的端口号，在Playabbles系统中对指定子节点进行操作时都需要指定这个端口号。
        protected int m_Index;
        internal int index { get => m_Index; set => m_Index = value; }

        //节点的权重，不过并非Playble节点公开属性，这里只是记录，最终通过SetInputWeight落实到权重影响，而是是调用父节点的SetInputWeight，这里的Weight主要是记录给父节点访问的。
        public float weight
        {
            get => m_Layer.GetStateWeight(this);
            set
            {
                m_Layer.SetStateWeight(this, value);
            }
        }

        internal float fadeDuration = 0f;

        // protected float m_TimeF;
        // protected double m_TimeD;
        public double time
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

        internal bool isValid => m_Playable.IsValid();

        protected AnimationStateBase(AnimationGraph _graph)
        {
            m_Graph = _graph;
            m_Index = -1;
        }

        /*Tip：开始播放时执行一些逻辑、类似于OnEnable方法的作用，而结束播放时可能也会有一些逻辑类似OnDisable。*/
        internal virtual void EnterPlaying()
        {
            time = 0; //复用时就会体会到这里的用处了。
        }
        internal virtual void ExitPlaying()
        {

        }

        public void Play(int _layerIndex = 0)
        {
            if (!isValid)
            {
                Debug.LogError("正在尝试播放无效的动画状态，请检查");
                return;
            }
            m_Graph.Play(_layerIndex, this);
        }

        public void Stop()
        {
            if (!isValid)
            {
                Debug.LogError("正在尝试停止无效的动画状态，请检查");
                return;
            }
            m_Graph.Stop(m_Layer, this);
        }

        void IFadeTarget.StartFadeOut()
        {

        }
        void IFadeTarget.StartFadeIn()
        {

        }
    }
}