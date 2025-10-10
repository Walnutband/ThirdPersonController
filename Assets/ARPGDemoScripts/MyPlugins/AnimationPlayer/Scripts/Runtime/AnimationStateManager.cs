
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    public class AnimationStateManager
    {
        private AnimationGraph m_Graph;

        private Dictionary<object, AnimationStateBase> m_StateDic;
        

        public AnimationStateManager(AnimationGraph _graph)
        {
            m_Graph = _graph;
            m_StateDic = new Dictionary<object, AnimationStateBase>();
        }

        /*Tip：相当固定，根据Clip那就是创建播放单个片段的AnimationState，不需要设置为基类AnimationStateBase*/
        public AnimationClipState GetOrCreateState(AnimationClip _clip)
        {
            var stateID = StateID(_clip);
            if (!m_StateDic.TryGetValue(stateID, out var state))
            {
                state = new AnimationClipState(m_Graph, _clip);
                m_StateDic.Add(stateID, state);
            }
            return state as AnimationClipState;
        }

        /*TODO：这里的代码真的一坨。*/

        //总之都会有，但是返回表示是否已存在（true）还是新创建（false）
        public bool TryGetOrCreateState(AnimationClip _clip, out AnimationClipState _state)
        {
            var stateID = StateID(_clip);
            if (!m_StateDic.TryGetValue(stateID, out AnimationStateBase state))
            {
                _state = new AnimationClipState(m_Graph, _clip);
                m_StateDic.Add(stateID, _state);
                return false;
            }
            _state = state as AnimationClipState;
            return true;
        }
        
        public bool TryGetOrCreateState(MixerAnimation _mixer, out AnimationMixerState _state)
        {
            var stateID = StateID(_mixer);
            if (!m_StateDic.TryGetValue(stateID, out AnimationStateBase state))
            {
                _state = new AnimationMixerState(m_Graph, _mixer);
                m_StateDic.Add(stateID, _state);
                return false;
            }
            _state = state as AnimationMixerState;
            return true;
        }

        // public

        /*Tip：不同类型的ID，由于Key是object类型，所以能够引用这些不同类型的ID。*/
        public static int StateID(AnimationClip _clip) => _clip.GetInstanceID();
        public static int StateID(MixerAnimation _mixer) => _mixer.key;
    }


    /*Tip：之前就感觉Animancer中直接使用object类型作为State的标识符过于宽泛了，虽然实际上默认都是用AnimationClip作为标识符，但总感觉不够严谨，现在想到
    可以专门设置一个类型，还是用object作为引用类型，但是限制通道，虽然是object，但其实限制了只有几种确定类型可以作为ID，但是这样的话似乎就无法通过传入AnimationClip
    直接从字典查询对应状态了，因为在字典外部其实直接存储的并非这里的StateID，而是比如AnimationClip这种可以直接作为ID的类型，所以此时这个封装外壳反而成了阻碍。*/
    /*TODO：还是从Animancer参考，我能想到的是在要个体类中声明字段时，就将字段设置为专门的类型，其实Animancer中除了直接AnimationClip以外就常用ClipTransition指定动画片段以及
    过渡参数、还有LinearMixerTransition实现混合动画（就像AnimatorController中的混合树一样），在这个专门的类型中就定义了字段存储自己的ID信息或者说Key信息。*/
    public class StateID
    {
        private object m_ID;
        public StateID(AnimationClip _id)
        {
            m_ID = _id;
        }
    }
}