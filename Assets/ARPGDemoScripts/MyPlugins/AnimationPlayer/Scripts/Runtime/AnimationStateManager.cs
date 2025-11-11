
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

        public bool ExistState(AnimationStateBase _state)
        {

            if (!m_StateDic.ContainsKey(_state.key) || m_StateDic[_state.key] != _state) return false;
            return true;
        }
        
        //Ques：其实可以将判定是否存在也塞在这里面，但是我感觉应该分开，逻辑意义更明确，
        public bool RemoveState(AnimationStateBase _state)
        {
            if (ExistState(_state) == true)
            {
                m_StateDic.Remove(_state.key);
                return true;
            }
            return false;
        }


        public AnimationClipState GetOrCreateState(AnimationClip _clip)
        {
            if (m_StateDic.TryGetValue(StateID(_clip), out var _state))
            {
                return _state as AnimationClipState;
            }
            //没有就新建
            AnimationClipState state = new AnimationClipState(m_Graph, _clip);
            state.key = StateID(_clip);
            m_StateDic.Add(state.key, state);
            return state;

        }

        //对于FadeAnimation，默认是必须播放，而如果存在同片段的状态，就看是否游离，游离的话就直接复用即可
        public AnimationClipState GetOrCreateState(FadeAnimation _anim)
        {
            int id = StateID(_anim.clip);
            //存在且处于游离状态才能复用。
            if (m_StateDic.TryGetValue(id, out var _state) && _state.index < 0)
            {
                _state.fadeDuration = _anim.fadeDuration;
                return _state as AnimationClipState;
            }
            AnimationClipState state = new AnimationClipState(m_Graph, _anim);
            state.key = id;
            if (m_StateDic.ContainsKey(id))
            {
                m_StateDic[id] = state; //直接覆盖，而其所在Layer还存储着对其的引用。
            }
            else //Add添加的键如果已存在的话，那么就会抛出异常，所以要先检查一下。
            {
                m_StateDic.Add(state.key, state);
            }

            state.fadeDuration = _anim.fadeDuration;
            return state;
        }

        public AnimationMixerState GetOrCreateState(MixerAnimation _mixer)
        {
            int id = StateID(_mixer);
            //存在且处于游离状态才能复用。
            if (m_StateDic.TryGetValue(id, out var _state) && _state.index < 0)
            {
                _state.fadeDuration = _mixer.fadeDuration;
                return _state as AnimationMixerState;
            }
            AnimationMixerState state = new AnimationMixerState(m_Graph, _mixer);
            state.key = id;
            if (m_StateDic.ContainsKey(id))
            {
                m_StateDic[id] = state; //直接覆盖，而其所在Layer还存储着对其的引用。
            }
            else //Add添加的键如果已存在的话，那么就会抛出异常，所以要先检查一下。
            {
                m_StateDic.Add(state.key, state);
            }

            state.fadeDuration = _mixer.fadeDuration;
            return state;
        }

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