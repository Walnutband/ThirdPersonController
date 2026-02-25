// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace Animancer.Samples.Jobs
{
    /// <summary>
    /// A wrapper that manages an Animation Job (the <see cref="Job"/> struct nested inside this class)
    /// which rotates a set of bones to allow the character to dynamically lean over independantly of their animations.
    /// </summary>
    /// 
    /// <remarks>
    /// The axis around which the bones are rotated can be set to achieve several different effects:
    /// <list type="number">
    /// <item>The right axis allows bending forwards and backwards.</item>
    /// <item>The up axis allows turning to either side.</item>
    /// <item>The forward axis allows leaning to either side.</item>
    /// </list>
    /// <see cref="https://github.com/KybernetikGames/animancer/issues/48#issuecomment-632336377">
    /// This script is based on an implementation by ted-hou on GitHub.</see>
    /// <para></para>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/jobs/hit-impacts">
    /// Hit Impacts</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Jobs/SimpleLean
    /// 
    public class SimpleLean : AnimancerJob<SimpleLean.Job>, IDisposable //通过泛型传入Job类型，也就是作为数据类型，实现了IAnimationJob。
    {
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        public SimpleLean(
            AnimancerGraph animancer,
            Vector3 axis,
            NativeArray<TransformStreamHandle> leanBones)
        {
            Animator animator = animancer.Component.Animator;

            _Job = new()
            {
                root = animator.BindStreamTransform(animator.transform), //以此为根。
                bones = leanBones,
                axis = axis,
                angle = AnimancerUtilities.CreateNativeReference<float>(),
            };

            //创建AnimationScriptPlayable节点，并且插入到AnimationPlayableOutput的前面。
            CreatePlayable(animancer);

            animancer.Disposables.Add(this);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Control
        /************************************************************************************************************************/
        // The Axis probably won't change often so the setter can just get the job data and change it.
        /************************************************************************************************************************/

        public Vector3 Axis
        {
            get => _Job.axis;
            set
            {
                if (_Job.axis == value)
                    return;

                _Job.axis = value;
                _Playable.SetJobData(_Job);
            }
        }

        /************************************************************************************************************************/
        // But since the Angle could change all the time, we can exploit the fact that arrays are actualy references to avoid
        // copying the entire struct out of the job playable then back in every time.
        /************************************************************************************************************************/

        public float Angle
        {
            get => _Job.angle[0];
            set => _Job.angle[0] = value;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Clean Up
        /************************************************************************************************************************/

        void IDisposable.Dispose() => Dispose();

        //Tip：NativeArray原生数组，自己管理内存，所以记得销毁。
        /// <summary>Cleans up the <see cref="NativeArray{T}"/>s.</summary>
        /// <remarks>Called by <see cref="AnimancerGraph.OnPlayableDestroy"/>.</remarks>
        private void Dispose()
        {
            if (_Job.angle.IsCreated)
                _Job.angle.Dispose();

            if (_Job.bones.IsCreated)
                _Job.bones.Dispose();
        }

        /// <summary>Destroys the <see cref="_Playable"/> and restores the graph connection it was intercepting.</summary>
        public override void Destroy()
        {
            Dispose();
            base.Destroy();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Job
        /************************************************************************************************************************/

        /// <summary>An <see cref="IAnimationJob"/> that applies a lean effect to an <see cref="AnimationStream"/>.</summary>
        /// 
        /// <remarks>
        /// <strong>Sample:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/samples/jobs/hit-impacts">
        /// Hit Impacts</see>
        /// </remarks>
        /// 
        /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Jobs/Job
        /// 
        public struct Job : IAnimationJob
        {
            /************************************************************************************************************************/

            public TransformStreamHandle root;
            public NativeArray<TransformStreamHandle> bones;
            public Vector3 axis;
            public NativeArray<float> angle;

            /************************************************************************************************************************/

            //readonly表示：此方法不会修改结构体的任何字段
            public readonly void ProcessRootMotion(AnimationStream stream) { }

            /************************************************************************************************************************/

            //Tip：就是这里导致的Hit效果。
            public void ProcessAnimation(AnimationStream stream)
            {
                float angle = this.angle[0] / bones.Length; //平均分配角度。
                Vector3 worldAxis = root.GetRotation(stream) * axis; //这里将axis做root的旋转，本质上就是从root的局部转换到世界。
                Quaternion offset = Quaternion.AngleAxis(angle, worldAxis);

                //Tip：虽然没完全看懂，但是从下面这个式子可以直观发现，获取原始的数据，乘以offset也就是进行一个偏移，这个偏移就是Hit的效果，然后SetRotation写回到AnimationStream中，最终输出到Animator。
                for (int i = bones.Length - 1; i >= 0; i--)
                {
                    TransformStreamHandle bone = bones[i];
                    bone.SetRotation(stream, offset * bone.GetRotation(stream));
                }
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}
