
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：对于PlayableGraph的封装*/
    public class AnimationGraph
    {
        //所封装的PlayableGraph
        private PlayableGraph m_Graph;
        internal PlayableGraph graph => m_Graph;
        //状态字典，帮助Graph管理图中的状态，非常重要
        private AnimationStateManager m_StateManager;
        //层级混合节点，就是对各个层级做最后的遮罩过滤处理，混合之后就传递给输出节点输出到Animator中了。
        private AnimationLayerMixer m_LayerMixer;
        internal AnimationLayerMixer layers => m_LayerMixer;
        private Preprocessor preprocessor;
        internal Preprocessor pre => preprocessor;
        private Postprocessor postprocessor;
        internal Postprocessor post => postprocessor;

        public AnimationGraph(Animator _Animator)
        {
            //首先创建真正的PlayableGraph
            m_Graph = CreateGraph(_Animator.gameObject.name);
            // m_Graph.Play(); //其实多此一举，本来创建之后就会自动开始播放。

            // Debug.Log("创建Graph");

            //创建状态管理器
            m_StateManager = new AnimationStateManager(this);
            //然后是各个节点
            //输出节点。应该都不需要保存引用，虽然确实可以直接通过PlayableGraph获取，但是运行过程中大概是不需要获取的。
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(m_Graph, "AnimationOutput", _Animator);
            var prePlayable = ScriptPlayable<Preprocessor>.Create(m_Graph, 1);
            output.SetSourcePlayable(prePlayable);
            preprocessor = prePlayable.GetBehaviour();
            
            m_LayerMixer = new AnimationLayerMixer(this);
            m_Graph.Connect(m_LayerMixer.playable, 0, prePlayable, 0);
            prePlayable.SetInputWeight(0, 1f);

            //后处理器，由于需要调用ProcessFrame（在动画更新之后调用），所以需要连接到ScriptPlayableOutput节点
            ScriptPlayableOutput scriptOutput = ScriptPlayableOutput.Create(m_Graph, "ScriptOutput");
            var postPlayable = ScriptPlayable<Postprocessor>.Create(m_Graph);
            scriptOutput.SetSourcePlayable(postPlayable);

            postprocessor = postPlayable.GetBehaviour();
            postprocessor.GetStates = AllPlayingStates;

        }

        public void Destroy()
        {
            m_Graph.Destroy();
        }

        public void Play() => m_Graph.Play();
        public void Stop() => m_Graph.Stop();

        //创建PlayableGraph
        private PlayableGraph CreateGraph(string _name = null)
        {
            if (string.IsNullOrEmpty(_name))
            {
                return PlayableGraph.Create();
            }
            else
            {
                return PlayableGraph.Create(_name);
            }
        }

        public AnimationClipState Play(int _layerIndex, AnimationClip _clip, PlayOption option)
        {

#if UNITY_EDITOR //Ques：这种写法如何？因为在编辑时需要这种判断信息，就是为了将这些前提条件在编辑时做好，而在运行时就不用进行这些判断来浪费性能了。
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            if (_clip == null)
            {
                Debug.LogError("在播放动画时传入的动画Clip为空");
                return null;
            }
#endif

            AnimationClipState state = m_StateManager.GetOrCreateState(_clip);
            Play(_layerIndex, state, option);

            return state;

        }

        //需要过渡的话，那么可能自己转向自己了，因为默认是就算正在播放，也要过渡，也就是产生一个新的状态。
        public AnimationClipState Play(int _layerIndex, FadeAnimation _fadeAnimation)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                // return null;
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            if (_fadeAnimation.fadeDuration <= 0.001f)
            {
                Debug.Log("！注意！传入的过渡动画指定的过渡时间过小，无法进行过渡过程");
                return Play(_layerIndex, _fadeAnimation.clip, PlayOption.FromStart);
            }
            else
            {
                //过渡必执行。
                AnimationClipState state = m_StateManager.GetOrCreateState(_fadeAnimation);
                m_LayerMixer.Play(_layerIndex, state, _fadeAnimation.fadeDuration);
                return state;
            }
        }

        public AnimationMixerState Play(int _layerIndex, MixerAnimation _mixer)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            AnimationMixerState state = m_StateManager.GetOrCreateState(_mixer);

            if (_mixer.fadeDuration <= 0.001f)
            {
                Debug.Log("！注意！传入的过渡动画指定的过渡时间过小，无法进行过渡过程");
                m_LayerMixer.Play(_layerIndex, state);
            }
            else
            {
                m_LayerMixer.Play(_layerIndex, state, _mixer.fadeDuration);
            }

            return state;
        }

        //直接播放状态。
        public AnimationStateBase Play(int _layerIndex, AnimationStateBase _state, PlayOption _option = PlayOption.None)
        {//能成功调用该方法，就说明该状态
         //state不可能为空，就根据其是否游离而分支。
            if (_state.index < 0) //游离状态
            {
                m_LayerMixer.Play(_layerIndex, _state);
            }
            else //非游离状态，也就是正在播放
            {
                if (_option == PlayOption.None)
                {
                    //正在播放，那么什么都不做。
                }
                else if (_option == PlayOption.FromStart)
                {
                    m_LayerMixer.Play(_layerIndex, _state);
                }
            }
            return _state;
        }


        public void Stop(AnimationLayer _layer, AnimationStateBase _state)
        {
            m_LayerMixer.Stop(_layer.index, _state);
        }

        public List<AnimationStateBase> AllPlayingStates()
        {
            // List<AnimationStateBase> result = new List<AnimationStateBase>();
            return m_LayerMixer.AllPlayingStates();
        }

        public void SetLayerCount(int _count) => m_LayerMixer.SetLayers(_count);
        public void SetLayerMask(uint _index, AvatarMask _mask) => m_LayerMixer.SetLayerMask(_index, _mask);
        public void SetLayerAdditive(uint _index, bool _additive) => m_LayerMixer.SetLayerAdditive(_index, _additive);

        //将State连接到Layer
        internal void Connect(AnimationStateBase _state, AnimationLayer _layer, int _index)
        {
            m_Graph.Connect(_state.playable, 0, _layer.playable, _index);
            _state.index = _index; //记录自己的索引
            // _layer.SetStateWeight(_state, _state.weight);
        }
        //将layer连接到layerMixer
        internal void Connect(AnimationLayer _layer, AnimationLayerMixer _layerMixer, int _index)
        {
            m_Graph.Connect(_layer.playable, 0, _layerMixer.playable, _index);
            _layer.index = _index;

            //索引为0就是Base Layer，否则大于0的话就是附加的层级了，这是动画系统的底层条件，就是这样特殊处理。
            // if (_layer.index == 0)
            // {
            //     _layerMixer.SetLayerWeight(_layer, 1f);
            // }
            //为附加层级的话，大概是让LayerMixer自己处理权重等等逻辑。
            // else //保证大于0
            // {
            //     _layerMixer.SetLayerWeight(_layer, 0f);
            // }
        }

        internal void Disconnect(AnimationStateBase _state, AnimationLayer _layer)
        {
            m_Graph.Disconnect(_layer.playable, _state.index);
            _state.index = -1; //游离状态

            /*BUG：打个补丁，在断开连接的时候，如果发现该状态已经没有记录了，那么就直接销毁其节点，同时其他地方的逻辑保证也已经清除了对该状态的引用，那么就可以等待GC自动回收了。
            也就是说，实际上，
            */
            if (m_StateManager.ExistState(_state) == false)
            {
                DestroySubgraph(_state.playable);    
            }
        }

        internal void DestroySubgraph(Playable _playable)
        {
            m_Graph.DestroySubgraph(_playable);
        }

        //销毁游离状态
        internal void DestroyFreeState(AnimationStateBase _state)
        {
            Debug.Log($"尝试销毁游离状态{_state.key}");
            if (_state.index >= 0)
            {
                Debug.LogError("尝试删除非游离状态，请检查");
                return;
            }

            /*Tip：一个重要情况是，此时存在两个key相同的状态，一个是游离的，一个是非游离的且记录在StateManager中，这样的话必然有除了StateManager之外的对象（其实就是FadeHandler）
            保存着对它的引用，那么就要由它来清除引用，然后让GC回收。*/
            m_StateManager.RemoveState(_state);
            m_Graph.DestroySubgraph(_state.playable);
        }

        public void SetSpeed(double _speed)
        {
            m_LayerMixer.SetSpeed(_speed);
        }
    }
}