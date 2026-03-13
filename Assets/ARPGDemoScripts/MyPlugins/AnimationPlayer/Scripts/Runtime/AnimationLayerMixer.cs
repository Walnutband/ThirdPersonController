using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public interface IAnimationMixer
    {
        Playable playable { get; }
    }

    /*Tip：这感觉并不需要模仿Animancer，其实说白了这就是个普通但是完全够用的动画系统，如果是对于独立游戏或者非超大型游戏都是肯定够用的，而且动画系统真的没什么区别，
    真正有区别的是那种专门编辑动画的，比如专门的IK插件、动画编辑器等等，而像这样只是基于Playables系统的实现播放、过渡、混合动画的动画系统，真的都差不多。*/
    //TODO：其实也可以巧妙使用泛型来将这些节点的类型统一化。
    // public class AnimationLayerMixerNode : AnimationNodeBase
    public class AnimationLayerMixer : IAnimationMixer
    {
        private AnimationGraph m_Graph;
        private AnimationLayerMixerPlayable m_Playable;
        // public AnimationLayerMixerPlayable playable => m_Playable; //公开主要还是为了连接节点，也没必要为Connect封装方法，实在犯不上。
        public Playable playable => m_Playable; //公开主要还是为了连接节点，也没必要为Connect封装方法，实在犯不上。
        private List<AnimationLayer> m_Layers;
        public List<AnimationLayer> layers => m_Layers;
        public int layerCount => m_Layers.Count; //就是提供给AnimationLayer的，避免让它直接访问到Layers列表。

        private FadeHandler m_FadeHandler;

        /*Tip：初始必有一层，此即为底层Base Layer*/
        public AnimationLayerMixer(AnimationGraph _graph)
        {
            //首要确定所在Graph以及创建Playable，才能开展后续。
            m_Graph = _graph;
            m_Playable = AnimationLayerMixerPlayable.Create(_graph.graph);
            m_Layers = new List<AnimationLayer>();
        }

        public void AddLayer(int _index)
        {
            //在添加层级时要保证已经设置了足够的输入端口，同时容器m_Layers与其同步。
            if (_index < 0 || _index >= m_Playable.GetInputCount())
            {
                Debug.LogError("在添加层级时传入的索引超出了当前的输入端口数量，请检查。");
                return;
            }
            
            //说明该位置已经有了层级，提示一下，不能重复添加，也默认不能覆盖。这就是系统刻意设计的底层机制。
            if (m_Layers[_index] != null)
            {
                Debug.LogError($"尝试在索引{_index}添加层级时发现该位置已经有了层级，默认不能覆盖，请检查。");
                return;
            }
            // Debug.Log($"AddLayer，此时index为{_index}");
            AnimationLayer layer = new AnimationLayer(m_Graph); 
            m_Graph.Connect(layer, this, _index);
            if (_index == 0)
            {//Base Layer直接权重设置为1
                layer.weight = 1f;
            }
            else
            {
                layer.weight = 0f;
            }
        }

        public AnimationLayer this[int index]
        {
            get
            {
                // Debug.Log($"index: {index}， layerCount: {layerCount}");
                if (index < 0)
                {
                    Debug.LogError("在访问层级时传入的索引小于0，请检查");
                    return null;
                }
                else if (index >= layerCount)
                {
                    Debug.LogError("在访问层级时传入的索引超出了当前的输入端口数量范围，请检查。");
                    return null;
                }
                return m_Layers[index];
            }
            set
            {
                if (index >= 0 && index < layerCount)
                {
                    m_Layers[index] = value;
                }
            }
        }

        public void Play(int _layerIndex, AnimationStateBase _state)
        {
            AnimationLayer layer = this[_layerIndex];

            //Tip：没有过渡，确实就没啥麻烦事了
            layer.Play(_state);
            // layer.weight = 1f;
        }

        public void Play(int _layerIndex, AnimationStateBase _state, float _fadeDuration)
        {
            AnimationLayer layer = this[_layerIndex];
            layer.Play(_state, _fadeDuration);
        }

        // public void Play(int _layerIndex, AnimationStateBase _state, float _fadeDuration)
        // {
        //     AnimationLayer layer = this[_layerIndex];

        //     if (layer.isPlaying)
        //     {
        //         layer.Play(_state, _fadeDuration);
        //         if (_layerIndex > 0) //非Base层
        //         {
        //             m_FadeHandler?.Complete();
        //             // m_FadeHandler?.Cancel();
        //             layer.weight = 1f;
        //         }
        //     }
        //     //该层级没有正在播放的动画
        //     else
        //     {
        //         //Base层直接播放，不进行过渡
        //         if (_layerIndex == 0)
        //         {
        //             layer.weight = 1f;
        //             layer.Play(_state);
        //         }
        //         //Tip: 非Base层，需要过渡，但注意是过渡层级权重而非状态权重。
        //         else
        //         {
        //             m_FadeHandler?.Complete();
        //             layer.weight = 0f;
        //             layer.Play(_state); //层级上直接播放，而层级之间进行过渡。
        //             m_FadeHandler = new FadeHandler(null, layer, _fadeDuration, () => { m_FadeHandler = null; });
        //             m_Graph.pre.AddUpdatable(m_FadeHandler);
        //         }
        //     }
        // }

        public void Stop(int _layerIndex, AnimationStateBase _state, float _duration)
        {
            AnimationLayer layer = this[_layerIndex];
            layer.Stop(_state, _duration); 
        }

        // public void Stop(int _layerIndex, AnimationStateBase _state)
        // {
        //     AnimationLayer layer = this[_layerIndex];

        //     //没有在播放的动画，就不可能存在传入的状态了
        //     if (layer.isPlaying == false) return;

        //     if (_layerIndex == 0)
        //     {
        //         layer.Stop(_state);
        //     }
        //     else //非Base层
        //     {
        //         //就是取出
        //         float baseDuration = this[0].currentFadeDuration;
        //         if (baseDuration <= 0.001f)
        //         {
        //             layer.Stop(_state);
        //             layer.weight = 0f; //直接设置为0，否则的话就是按照下面进行一个过渡。
        //         }
        //         else
        //         {
        //             Debug.Log("非Base层过渡到0");
        //             //放入容器中，注意使用初始化器简化。
        //             List<IFadeTarget> outs = new List<IFadeTarget>(1) { layer };
        //             //因为本来就是Override，计算就是根据后面一层的权重临时改变前一层的权重，使得两者权重和为1，而以后面一层权重优先。
        //             m_FadeHandler = new FadeHandler(outs, null, baseDuration, () =>
        //             {
        //                 m_FadeHandler = null;
        //                 layer.Stop(_state);
        //             });
        //             m_Graph.pre.AddUpdatable(m_FadeHandler);
        //         }
        //     }
        // }

        public List<AnimationStateBase> AllPlayingStates()
        {
            List<AnimationStateBase> result = new List<AnimationStateBase>();
            foreach (var layer in m_Layers)
            {
                result.AddRange(layer.states);
            }
            //本质上是清理层级节点中的那些空的输入端口。
            result.RemoveAll(item => item == null);
            return result;
        }

        public float GetLayerWeight(AnimationLayer _layer)
        {
            if (_layer == null || _layer.index < 0)
            {
                Debug.LogError("在获取层级权重时传入的Layer为空，或者是索引小于0");
                return -1f;
            }
            return m_Playable.GetInputWeight(_layer.index);
        }

        public void SetLayerWeight(AnimationLayer _layer, float _weight)
        {
            if (_layer == null || _layer.index < 0)
            {
                Debug.LogError("在设置层级权重时传入的Layer为空，或者是索引小于0");
                return;
            }
            m_Playable.SetInputWeight(_layer.index, _weight);
        }

        public void SetSpeed(double _speed)
        {
            m_Playable.SetSpeed(_speed);
        }

        /*Tip：注意，设置层级数量意味着分配对应数量的输入端口的同时还要创建代表层级的AnimationMixerPlayable节点连接。
        经过考虑，这个方法就是直接清空之前层级，重新构建，所以按理来说只应该在初始化时调用。
        */
        public void SetLayers(int _count)
        {
            if (_count <= 0)
            {
                Debug.LogError("设置层级数量时传入的数量小于等于0，请检查。");
                return;
            }

            ClearLayers();

            m_Playable.SetInputCount(_count);
            m_Layers.Capacity = _count; //直接分配容量，没必要直接new，但其实我也不确定性能有什么本质区别。
            for (int i = 0; i < _count; i++)
            {
                /*BugFix：很尬，List访问索引比如保证其位置被显式添加过元素，尽管其实际上就是null，但如果没有添加过元素的话，也会无法访问、直接报错。*/
                m_Layers.Add(null);
                AddLayer(i); //在该索引位置添加层级。
            }

            // Debug.Log($"SetLayers结束之后，layerCount：{layerCount}");
        }

        public void ClearLayers()
        {
            int count = m_Playable.GetInputCount();
            if (count <= 0)
            {
                // Debug.Log("清空层级时此时输入端口数量实际为0");
                return;
            }
            for (int i = 0; i < count; i++)
            {//逐个取出，然后销毁
                m_Graph.DestroySubgraph(m_Playable.GetInput(i));
            }
            //注意同步，清空Playable节点的同时清空对应的状态（GC自动清理内存，只要负责清除引用即可）。
            m_Layers.Clear();
        }

        //设置层级遮罩
        public void SetLayerMask(uint _index, AvatarMask _mask)
        {
            if (_index >= m_Playable.GetInputCount())
            {
                Debug.LogError("设置层级遮罩时传入的索引大于等于输入端口数量，请检查。");
                return;
            }
            m_Playable.SetLayerMaskFromAvatarMask(_index, _mask);
        }

        //要么Addtive要么Override（AnimatorController中的Layer也是同样的设置）
        public void SetLayerAdditive(uint _index, bool _additive)
        {
            if (_index >= m_Playable.GetInputCount())
            {
                Debug.LogError("设置层级的混合模式时传入的索引大于等于输入端口数量，请检查。");
                return;
            }
            m_Playable.SetLayerAdditive(_index, _additive);
        }

    }
}