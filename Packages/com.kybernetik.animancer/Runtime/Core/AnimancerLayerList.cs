// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>A list of <see cref="AnimancerLayer"/>s with methods to control their mixing and masking.</summary>
    /// <remarks>
    /// The default implementation of this class is <see cref="AnimancerLayerMixerList"/>.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
    /// Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayerList
    public abstract class AnimancerLayerList :
        IEnumerable<AnimancerLayer>,
        IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> containing this list.</summary>
        public readonly AnimancerGraph Graph;

        /// <summary>The layers which each manage their own set of animations.</summary>
        /// <remarks>This field should never be null so it shouldn't need null-checking.</remarks>
        private AnimancerLayer[] _Layers;

        /// <summary>The number of layers that have actually been created.</summary>
        private int _Count;

        /// <summary>The <see cref="UnityEngine.Playables.Playable"/> which blends the layers.
        /// <para></para>
        /// 混合这些层级的Playable（由内置的AnimationLayerMixerPlayable提供功能）。
        /// </summary>
        public Playable Playable { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerLayerList"/>.</summary>
        /// <remarks>The <see cref="Playable"/> must be assigned by the end of the derived constructor.</remarks>
        protected AnimancerLayerList(AnimancerGraph graph)
        {
            Graph = graph;
            _Layers = new AnimancerLayer[DefaultCapacity];
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region List Operations
        /************************************************************************************************************************/

        
        /// <summary>[Pro-Only] The number of layers in this list.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value is set higher than the <see cref="DefaultCapacity"/>. This is simply a safety measure,
        /// so if you do actually need more layers you can just increase the limit.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">The value is set to a negative number.</exception>
        public int Count
        {
            get => _Count;
            set
            {
                var count = _Count;

                if (value == count)
                    return;

                CheckAgain:

                if (value > count)// Increasing.
                {
                    Add();
                    count++;
                    goto CheckAgain;
                }
                else// Decreasing.
                {
                    while (value < count--)
                    {
                        var layer = _Layers[count];
                        //Tip：使用PlayableGraph提供的方法，删除指定节点及其所有（直接或间接）连接到其输入端的Playable
                        if (layer._Playable.IsValid())
                            Graph._PlayableGraph.DestroySubgraph(layer._Playable);
                        layer.DestroyStates();
                    }
                    //将数组中的元素删除，上面是将元素本身的内容销毁，注意区别。
                    Array.Clear(_Layers, value, _Count - value); //指定开始位置和长度

                    _Count = value;
                    //更新输入端口数量。
                    Playable.SetInputCount(value);
                }
            }
        }

        /************************************************************************************************************************/

        
        /// <summary>[Pro-Only]
        /// If the <see cref="Count"/> is below the specified `min`, this method increases it to that value.
        /// </summary>
        public void SetMinCount(int min)
        {
            if (Count < min)
                Count = min;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
        /// be thrown (default 4).
        /// <para></para>
        /// Lowering this value will not affect layers that have already been created.
        /// </summary>
        /// <remarks>
        /// <strong>Example:</strong>
        /// To set this value automatically when the application starts, place a method like this in any class:
        /// <para></para><code>
        /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// private static void SetMaxLayerCount()
        /// {
        ///     Animancer.AnimancerLayerList.DefaultCapacity = 8;
        /// }
        /// </code>
        /// Otherwise you can set the <see cref="Capacity"/> of each individual list:
        /// <para></para><code>
        /// AnimancerComponent animancer;
        /// animancer.Layers.Capacity = 8;
        /// </code></remarks>
        public static int DefaultCapacity { get; set; } = 4;

        /// <summary>[Pro-Only]
        /// If the <see cref="DefaultCapacity"/> is below the specified `min`, this method increases it to that value.
        /// </summary>
        public static void SetMinDefaultCapacity(int min)
        {
            if (DefaultCapacity < min)
                DefaultCapacity = min;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
        /// be thrown. The initial capacity is determined by <see cref="DefaultCapacity"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Lowering this value will destroy any layers beyond the specified value.
        /// <para></para>
        /// Changing this value will cause the allocation of a new array and garbage collection of the old one,
        /// so you should generally set the <see cref="DefaultCapacity"/> before initializing this list.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
        public int Capacity
        {
            get => _Layers.Length;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), $"must be greater than 0 ({value} <= 0)");

                if (_Count > value)
                    Count = value;

                Array.Resize(ref _Layers, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Creates and returns a new <see cref="AnimancerLayer"/> at the end of this list.</summary>
        /// <remarks>If the <see cref="Capacity"/> would be exceeded, it will be doubled.</remarks>
        public AnimancerLayer Add()
        {
            /*这里需要注意的是，_Count记录的是当前数量，而由于索引差一，所以正好等于新增的元素的索引，在Connect中传入的就是索引，而SetInputCount时传入的是数量，并非索引*，注意区分
            这样新增的AnimancerLayer（AnimationMixerPlayable）就是连接到的AnimancerLayerMixerPlayable的新增的输入端口。*/
            var index = _Count;

            if (index >= _Layers.Length)
                Capacity *= 2;

            var layer = new AnimancerLayer(Graph, index);
            /*Tip：对于实际类型为AnimancerLayerMixerList来说，就是设置AnimancerLayerMixerPlayable的输入端口，并且将AnimationMixerPlayable连接到该Layer节点上。*/
            _Count = index + 1;
            Playable.SetInputCount(_Count);
            //连接到新增的输入端口，并且设置权重为0，就是避免新增的立刻产生任何影响
            Graph._PlayableGraph.Connect(Playable, layer._Playable, index, 0); 

            _Layers[index] = layer;
            return layer;
        }

        /************************************************************************************************************************/

        //AnimancerLayerList数组索引器
        /*Ques：我疑惑的点在于，这里的SetMinCount中的Count的setter，不会导致访问指定Layer时意外地将端口号向上的那些Layer删除吗？
        大概这只是一个很少用到的快捷方法，实际访问Layer还是应该使用GetLayer*/
        /// <summary>Returns the layer at the specified index. If it didn't already exist, this method creates it.</summary>
        /// <remarks>To only get an existing layer without creating new ones, use <see cref="GetLayer"/> instead.
        /// <para></para>因为通过AnimancerLayer是调用的自定义的数组索引器，而GetLayer方法是直接从_Layers数组中获取指定索引的Layer
        /// </remarks>
        public AnimancerLayer this[int index]
        {
            get
            {
                //加1说明传入还是按照从0开始的索引，但是在内部处理时其实是按照从1开始来计算的。
                //似乎也不准确， 还是应该理解为这里 传入的就是数量 ，内部是按照数量来处理，只是在获取的时候还是按照索引惯例来操作。
                SetMinCount(index + 1); 
                return _Layers[index];
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the layer at the specified index.</summary>
        /// <remarks>To create a new layer if the target doesn't exist, use <see cref="this[int]"/> instead.</remarks>
        public AnimancerLayer GetLayer(int index)
            => _Layers[index];

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/

        /// <summary>Returns an enumerator that will iterate through all layers.</summary>
        public FastEnumerator<AnimancerLayer> GetEnumerator()
            => new(_Layers, _Count);

        /// <inheritdoc/>
        IEnumerator<AnimancerLayer> IEnumerable<AnimancerLayer>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in all layers.</summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(_Layers);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Layer Details
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Is the layer at the specified index is set to additive blending?
        /// Otherwise it will override lower layers.
        /// </summary>
        public virtual bool IsAdditive(int index)
            => false;

        /// <summary>[Pro-Only]
        /// Sets the layer at the specified index to blend additively with earlier layers (if true)
        /// or to override them (if false). Newly created layers will override by default.
        /// </summary>
        public virtual void SetAdditive(int index, bool value) { }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Sets an <see cref="AvatarMask"/> to determine which bones the layer at the specified index will affect.
        /// </summary>
        /// <remarks>
        /// Don't assign the same mask repeatedly unless you have modified it.
        /// This property doesn't check if the mask is the same
        /// so repeatedly assigning the same thing will simply waste performance.
        /// </remarks>
        public virtual void SetMask(int index, AvatarMask mask) { }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Sets the Inspector display name of the layer at the specified index.</summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public void SetDebugName(int index, string name)
            => this[index].SetDebugName(name);

        /************************************************************************************************************************/

        /// <summary>
        /// The average velocity of the root motion of all currently playing animations,
        /// taking their current <see cref="AnimancerNode.Weight"/> into account.
        /// </summary>
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = 0; i < _Count; i++)
                {
                    var layer = _Layers[i];
                    velocity += layer.AverageVelocity * layer.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

