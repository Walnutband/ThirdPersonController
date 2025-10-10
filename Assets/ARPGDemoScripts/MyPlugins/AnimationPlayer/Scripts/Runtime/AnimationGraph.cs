
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
            m_Graph = CreateGraph();
            // m_Graph.Play(); //其实多此一举，本来创建之后就会自动开始播放。

            // Debug.Log("创建Graph");

            //创建状态管理器
            m_StateManager = new AnimationStateManager(this);
            //然后是各个节点
            //输出节点。应该都不需要保存引用，虽然确实可以直接通过PlayableGraph获取，但是运行过程中大概是不需要获取的。
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(m_Graph, "AnimationOutput", _Animator);
            // AnimationLayerMixerPlayable layerMixer = AnimationLayerMixerPlayable.Create(m_Graph, 1); //默认有一个Layer
            m_LayerMixer = new AnimationLayerMixer(this);
            // m_Graph.Connect(m_Layers.playable, 0, output, 0);
            //TODO：可能中间应该连接到比如Animancer中的UpdatableList节点，以便处理过渡效果。
            // output.SetSourcePlayable(m_Layers.playable); //直接连接到输出节点
            //输出节点连接到预处理器，然后预处理器连接到LayerMixer。
            /*BugFix: 不知道是啥时候，把这里的第二个参数去掉了，结果导致下面的Connect显示尝试连接到一个不存在的端口，然后引出一系列Error日志，给我干麻了。
            当然这体现出Playables系统的屎性，这种端口明明完全可以自动化，结果还必须自己硬编码，而且还没有纠错机制、一出错就是一连串Error，真NM离谱。
            */
            var prePlayable = ScriptPlayable<Preprocessor>.Create(m_Graph, 1);
            output.SetSourcePlayable(prePlayable);
            preprocessor = prePlayable.GetBehaviour();
            //将LayerMixer连接到预处理器
            m_Graph.Connect(m_LayerMixer.playable, 0, prePlayable, 0);
            prePlayable.SetInputWeight(0, 1f);

            //后处理器，由于需要调用ProcessFrame（在动画更新之后调用），所以需要连接到ScriptPlayableOutput节点
            ScriptPlayableOutput scriptOutput = ScriptPlayableOutput.Create(m_Graph, "ScriptOutput");
            var postPlayable = ScriptPlayable<Postprocessor>.Create(m_Graph);
            // m_Graph.Connect(postPlayable, 0, prePlayable, 0);
            // output.SetSourcePlayable(postPlayable, 1);
            // output.SetSourcePlayable(scriptOutput, 1);

            scriptOutput.SetSourcePlayable(postPlayable);

            // scriptOutput.SetUserData(this);
            // postPlayable.SetInputWeight(0, 1f);
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

        // public AnimationState Play(AnimationClip _clip)
        // {
        //     AnimationState state = m_StateDictionary.GetOrCreateState(_clip);
        //     // m_Layers[0].Play

        //     return state;
        // }

        public AnimationClipState Play(int _layerIndex, AnimationClip _clip)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                // return null;
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            //TODO：已经存在。大概会
            // AnimationState state = m_StateManager.GetOrCreateState(_clip);
            if (m_StateManager.TryGetOrCreateState(_clip, out var state) == true)
            {
                m_LayerMixer[_layerIndex].Play(state);
                return state;
            }
            //否则就交给对应Layer播放。
            // Debug.Log("001交给Layer播放");
            // m_Layers[_layerIndex].PlayAdditive(state);
            m_LayerMixer[_layerIndex].Play(state);
            return state;

        }

        public AnimationClipState Play(int _layerIndex, AnimationClip _clip, float _fadeDuration)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                // return null;
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            if (m_StateManager.TryGetOrCreateState(_clip, out var state) == true)
            {
                // m_Layers[_layerIndex].
                m_LayerMixer[_layerIndex].Play(state, _fadeDuration);
                return state;
            }

            m_LayerMixer[_layerIndex].Play(state, _fadeDuration);
            return state; //state的成员会经过AnimationLayer修改，而由于class是引用类型所以能够传递出去。
        }

        public AnimationMixerState Play(int _layerIndex, MixerAnimation _mixer)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                // return null;
                _layerIndex = 0; //感觉直接修正为第一层比直接空处理更好。
            }

            if (m_StateManager.TryGetOrCreateState(_mixer, out var _state))
            {
                m_LayerMixer[_layerIndex].Play(_state, _mixer.fadeDuration);
                return _state;
            }

            m_LayerMixer[_layerIndex].Play(_state, _mixer.fadeDuration);
            return _state;
        }

        //直接播放状态。
        public AnimationStateBase Play(int _layerIndex, AnimationStateBase _state)
        {
            m_LayerMixer[_layerIndex].Play(_state);
            return _state;
        }


        public void Stop(AnimationLayer _layer, AnimationStateBase _state)
        {
            m_LayerMixer[_layer.index].Stop(_state);
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
        public void Connect(AnimationStateBase _state, AnimationLayer _layer, int _index)
        {
            m_Graph.Connect(_state.playable, 0, _layer.playable, _index);
            _state.index = _index; //记录自己的索引
            // _layer.playable.SetInputWeight(_index, _state.weight);
            _layer.SetStateWeight(_state, _state.weight);
        }
        //将layer连接到layerMixer
        public void Connect(AnimationLayer _layer, AnimationLayerMixer _layerMixer, int _index)
        {
            m_Graph.Connect(_layer.playable, 0, _layerMixer.playable, _index);
            _layer.index = _index;
            _layerMixer.playable.SetInputWeight(_index, _layer.weight);
        }

        public void Disconnect(AnimationStateBase _state, AnimationLayer _layer)
        {
            m_Graph.Disconnect(_layer.playable, _state.index);
            _state.index = -1; //游离状态
        }

        public void SetSpeed(double _speed)
        {
            m_LayerMixer.SetSpeed(_speed);
        }
    }
}