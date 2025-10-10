
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：这感觉并不需要模仿Animancer，其实说白了这就是个普通但是完全够用的动画系统，如果是对于独立游戏或者非超大型游戏都是肯定够用的，而且动画系统真的没什么区别，
    真正有区别的是那种专门编辑动画的，比如专门的IK插件、动画编辑器等等，而像这样只是基于Playables系统的实现播放、过渡、混合动画的动画系统，真的都差不多。*/
    //TODO：其实也可以巧妙使用泛型来将这些节点的类型统一化。
    // public class AnimationLayerMixerNode : AnimationNodeBase
    public class AnimationLayerMixer
    {
        private AnimationGraph m_Graph;
        private AnimationLayerMixerPlayable m_Playable;
        public AnimationLayerMixerPlayable playable => m_Playable; //公开主要还是为了连接节点，也没必要为Connect封装方法，实在犯不上。
        private List<AnimationLayer> m_Layers;
        // private AnimationLayer[] m_Layers;
        public int layerCount => m_Layers.Count; //就是提供给AnimationLayer的，避免让它直接访问到Layers列表。
        // public int layerCapacity => m_Layers.Capacity;

        public AnimationLayerMixer(AnimationGraph _graph, int _layerCount = 0)
        // public AnimationLayerMixer(AnimationGraph _graph, int _layerCount = 4)
        {
            //首要确定所在Graph以及创建Playable，才能开展后续。
            m_Graph = _graph;
            m_Playable = AnimationLayerMixerPlayable.Create(_graph.graph, _layerCount);
            SetLayers(_layerCount);
            // m_Layers = new List<AnimationLayer>(_layerCount);
            // AddLayer();
            //默认有一个Layer
            // m_Playable = AnimationLayerMixerPlayable.Create(_graph.graph, 1);
            // for (int i = 0; i < _layerCount; i++)
            // {
            //     AddLayer();
            // }

        }

        /*Ques：真的需要指定索引添加Layer的方法吗？我感觉完全用不上，就按照顺序Add就行了*/
        // public void AddLayer(int _index)
        // {
        //     Debug.Log("添加层级， index：" + _index);
        //     AnimationLayer layer = new AnimationLayer(m_Graph, this);
        //     // m_Layers.Add(layer);
        //     m_Layers[_index] = layer;
        //     //将层级连接到该LayerMixer。
        //     // m_Graph.graph.Connect(layer.playable, 0, m_Playable, _index);
        //     m_Graph.Connect(layer, this, _index);
        // }

        public void AddLayer(int _index)
        {
            Debug.Log("layerCount : " + layerCount + "index: " + _index);

            // AddLayer(layerCount);
            AnimationLayer layer = new AnimationLayer(m_Graph, this);
            m_Layers.Add(layer); //一定要放在前面，因为layerCount返回的就是List的Count成员。
            // m_Graph.graph.Connect(layer.playable, 0, m_Playable, layerCount);
            //将层级连接到该LayerMixer。注意要放在添加容器之前，因为要使用List的Count成员。
            /*Tip：其实一直觉得应该在AnimationLayer的构造函数中连接，但是又很别扭。*/
            m_Graph.Connect(layer, this, _index);
            // m_Playable.SetInputWeight(layer.playable, 1f);
        }

        public AnimationLayer this[int index]
        {
            get
            {
                if (index < 0)
                {
                    index = 0;
                    // return null;
                }
                // else if (index >= layerCount) //超过当前层级数量（端口数量）
                else if (index >= layerCount) 
                {
                    // Debug.Log("003访问层级，扩容");
                    index = layerCount;
                    //分配足够的输入端口
                    // m_Playable.SetInputCount(index + 1);
                    /*直接按顺序就行了，不管它有多大，因为毫无意义。CNMD*/
                    m_Playable.SetInputCount(layerCount + 1);
                    // AddLayer(index); //添加新的层级，注意这中途可能留下空端口，但实际上不会让这种情况发生。
                    AddLayer(index);
                    return m_Layers[index];
                }
                // Debug.Log("003访问层级");
                return m_Layers[index];
            }
            // set => m_Layers[index] = value;
        }

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

        public void SetSpeed(double _speed)
        {
            m_Playable.SetSpeed(_speed);
        }

        /*Tip：注意，设置层级数量意味着分配对应数量的输入端口的同时还要创建代表层级的AnimationMixerPlayable节点连接。
        经过考虑，这个方法就是直接清空之前层级，重新构建，所以按理来说只应该在初始化时调用。
        */
        public void SetLayers(int _count)
        {
            m_Playable.SetInputCount(_count);
            //注意这里的count实际对应的是List的Capacity容量。
            m_Layers = new List<AnimationLayer>(_count);
            // m_Layers.ForEach(x => AddLayer());
            for (int i = 0; i < _count; i++)
            {
                // AddLayer(i);
                AddLayer(i);
            }
        }

        //设置层级遮罩
        public void SetLayerMask(uint _index, AvatarMask _mask)
        {
            m_Playable.SetLayerMaskFromAvatarMask(_index, _mask);
        }

        //要么Addtive要么Override（AnimatorController中的Layer也是同样的设置）
        public void SetLayerAdditive(uint _index, bool _additive)
        {
            m_Playable.SetLayerAdditive(_index, _additive);
        }
    }
}