using System;
using System.Collections.Generic;
using UnityEngine.Animations;
#if !UNITY_2020_2_OR_NEWER
using UnityEngine.Experimental.Animations;
#endif

using UnityEngine.Playables;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Flags specifying which offset fields to match
    /// </summary>
    [Flags]
    public enum MatchTargetFields
    {
        /// <summary>
        /// Translation X value
        /// </summary>
        PositionX = 1 << 0,
        /// <summary>
        /// Translation Y value
        /// </summary>
        PositionY = 1 << 1,
        /// <summary>
        /// Translation Z value
        /// </summary>
        PositionZ = 1 << 2,
        /// <summary>
        /// Rotation Euler Angle X value
        /// </summary>
        RotationX = 1 << 3,
        /// <summary>
        /// Rotation Euler Angle Y value
        /// </summary>
        RotationY = 1 << 4,
        /// <summary>
        /// Rotation Euler Angle Z value
        /// </summary>
        RotationZ = 1 << 5
    }

    /// <summary>
    /// Describes what is used to set the starting position and orientation of each Animation Track.
    /// </summary>
    /// <remarks>
    /// By default, each Animation Track uses ApplyTransformOffsets to start from a set position and orientation.
    /// To offset each Animation Track based on the current position and orientation in the scene, use ApplySceneOffsets.
    /// </remarks>
    public enum TrackOffset
    {
        /// <summary>
        /// Use this setting to offset each Animation Track based on a set position and orientation.
        /// </summary>
        ApplyTransformOffsets,
        /// <summary>
        /// Use this setting to offset each Animation Track based on the current position and orientation in the scene.
        /// </summary>
        ApplySceneOffsets,
        /// <summary>
        /// Use this setting to offset root transforms based on the state of the animator.
        /// </summary>
        /// <remarks>
        /// Only use this setting to support legacy Animation Tracks. This mode may be deprecated in a future release.
        ///
        /// In Auto mode, when the animator bound to the animation track contains an AnimatorController, it offsets all animations similar to ApplySceneOffsets.
        /// If no controller is assigned, then all offsets are set to start from a fixed position and orientation, similar to ApplyTransformOffsets.
        /// In Auto mode, in most cases, root transforms are not affected by local scale or Animator.humanScale, unless the animator has an AnimatorController and Animator.applyRootMotion is set to true.
        /// </remarks>
        Auto
    }

    // offset mode
    enum AppliedOffsetMode
    {
        NoRootTransform,
        TransformOffset,
        SceneOffset,
        TransformOffsetLegacy,
        SceneOffsetLegacy,
        SceneOffsetEditor, // scene offset mode in editor
        SceneOffsetLegacyEditor,
    }

    // separate from the enum to hide them from UI elements
    static class MatchTargetFieldConstants
    {
        public static MatchTargetFields All = MatchTargetFields.PositionX | MatchTargetFields.PositionY |
            MatchTargetFields.PositionZ | MatchTargetFields.RotationX |
            MatchTargetFields.RotationY | MatchTargetFields.RotationZ;

        public static MatchTargetFields None = 0;

        public static MatchTargetFields Position = MatchTargetFields.PositionX | MatchTargetFields.PositionY |
            MatchTargetFields.PositionZ;

        public static MatchTargetFields Rotation = MatchTargetFields.RotationX | MatchTargetFields.RotationY |
            MatchTargetFields.RotationZ;

        public static bool HasAny(this MatchTargetFields me, MatchTargetFields fields)
        {
            return (me & fields) != None;
        }

        public static MatchTargetFields Toggle(this MatchTargetFields me, MatchTargetFields flag)
        {
            return me ^ flag;
        }
    }

    /// <summary>
    /// A Timeline track used for playing back animations on an Animator.
    /// </summary>
    [Serializable]
    [TrackClipType(typeof(AnimationPlayableAsset), false)]
    [TrackBindingType(typeof(Animator))]
    [ExcludeFromPreset]
    [TimelineHelpURL(typeof(AnimationTrack))]
    public partial class AnimationTrack : TrackAsset, ILayerable
    {
        const string k_DefaultInfiniteClipName = "Recorded";
        const string k_DefaultRecordableClipName = "Recorded";

        [SerializeField, FormerlySerializedAs("m_OpenClipPreExtrapolation")]
        TimelineClip.ClipExtrapolation m_InfiniteClipPreExtrapolation = TimelineClip.ClipExtrapolation.None;

        [SerializeField, FormerlySerializedAs("m_OpenClipPostExtrapolation")]
        TimelineClip.ClipExtrapolation m_InfiniteClipPostExtrapolation = TimelineClip.ClipExtrapolation.None;

        [SerializeField, FormerlySerializedAs("m_OpenClipOffsetPosition")]
        Vector3 m_InfiniteClipOffsetPosition = Vector3.zero;

        [SerializeField, FormerlySerializedAs("m_OpenClipOffsetEulerAngles")]
        Vector3 m_InfiniteClipOffsetEulerAngles = Vector3.zero;

        [SerializeField, FormerlySerializedAs("m_OpenClipTimeOffset")]
        double m_InfiniteClipTimeOffset;

        [SerializeField, FormerlySerializedAs("m_OpenClipRemoveOffset")]
        bool m_InfiniteClipRemoveOffset; // cached value for remove offset

        [SerializeField]
        bool m_InfiniteClipApplyFootIK = true;

        [SerializeField, HideInInspector]
        AnimationPlayableAsset.LoopMode mInfiniteClipLoop = AnimationPlayableAsset.LoopMode.UseSourceAsset;

        [SerializeField]
        MatchTargetFields m_MatchTargetFields = MatchTargetFieldConstants.All;
        [SerializeField]
        Vector3 m_Position = Vector3.zero;
        [SerializeField]
        Vector3 m_EulerAngles = Vector3.zero;


        [SerializeField] bool m_ApplyRootMotion;

        [SerializeField] AvatarMask m_AvatarMask;
        [SerializeField] bool m_ApplyAvatarMask = true;

        [SerializeField] TrackOffset m_TrackOffset = TrackOffset.ApplyTransformOffsets;

        [SerializeField, HideInInspector] AnimationClip m_InfiniteClip;


#if UNITY_EDITOR
        private AnimationClip m_DefaultPoseClip;
        private AnimationClip m_CachedPropertiesClip;
        private int           m_CachedHash;
        private EditorCurveBinding[] m_CachedBindings;

        AnimationOffsetPlayable m_ClipOffset;

        private Vector3 m_SceneOffsetPosition = Vector3.zero;
        private Vector3 m_SceneOffsetRotation = Vector3.zero;

        private bool m_HasPreviewComponents = false;
#endif

        /// <summary>
        /// The translation offset of the entire track.
        /// </summary>
        public Vector3 position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        /// <summary>
        /// The rotation offset of the entire track, expressed as a quaternion.
        /// </summary>
        public Quaternion rotation
        {
            get { return Quaternion.Euler(m_EulerAngles); }
            set { m_EulerAngles = value.eulerAngles; }
        }

        /// <summary>
        /// The euler angle representation of the rotation offset of the entire track.
        /// </summary>
        public Vector3 eulerAngles
        {
            get { return m_EulerAngles; }
            set { m_EulerAngles = value; } 
        }

        /// <summary>
        /// Specifies whether to apply track offsets to all clips on the track.
        /// </summary>
        /// <remarks>
        /// This can be used to offset all clips on a track, in addition to the clips individual offsets.
        /// </remarks>
        [Obsolete("applyOffset is deprecated. Use trackOffset instead", true)]
        public bool applyOffsets
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Specifies what is used to set the starting position and orientation of an Animation Track.
        /// </summary>
        /// <remarks>
        /// Track Offset is only applied when the Animation Track contains animation that modifies the root Transform.
        /// </remarks>
        public TrackOffset trackOffset
        {
            get { return m_TrackOffset; }
            set { m_TrackOffset = value; }
        }

        /// <summary>
        /// Specifies which fields to match when aligning offsets of clips.
        /// </summary>
        public MatchTargetFields matchTargetFields
        {
            get { return m_MatchTargetFields; }
            set { m_MatchTargetFields = value & MatchTargetFieldConstants.All; }
        }

        /// <summary>
        /// An AnimationClip storing the data for an infinite track.
        /// </summary>
        /// <remarks>
        /// The value of this property is null when the AnimationTrack is in Clip Mode.
        /// </remarks>
        public AnimationClip infiniteClip
        {
            get { return m_InfiniteClip; }
            internal set { m_InfiniteClip = value; }
        }

        // saved value for converting to/from infinite mode
        internal bool infiniteClipRemoveOffset
        {
            get { return m_InfiniteClipRemoveOffset; }
            set { m_InfiniteClipRemoveOffset = value; }
        }

        /// <summary>
        /// Specifies the AvatarMask to be applied to all clips on the track.
        /// </summary>
        /// <remarks>
        /// Applying an AvatarMask to an animation track will allow discarding portions of the animation being applied on the track.
        /// </remarks>
        public AvatarMask avatarMask
        {
            get { return m_AvatarMask; }
            set { m_AvatarMask = value; }
        }

        /// <summary>
        /// Specifies whether to apply the AvatarMask to the track.
        /// </summary>
        public bool applyAvatarMask
        {
            get { return m_ApplyAvatarMask; }
            set { m_ApplyAvatarMask = value; }
        }

        // is this track compilable

        internal override bool CanCompileClips()
        {
            return !muted && (m_Clips.Count > 0 || (m_InfiniteClip != null && !m_InfiniteClip.empty));
        }

        /// <inheritdoc/>
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AnimationPlayableBinding.Create(name, this); }
        }


        /// <summary>
        /// Specifies whether the Animation Track has clips, or is in infinite mode.
        /// 只要有clip就返回true即处于ClipMode
        /// </summary>
        public bool inClipMode 
        {
            get { return clips != null && clips.Length != 0; }
        }

        /// <summary>
        /// The translation offset of a track in infinite mode.
        /// </summary>
        public Vector3 infiniteClipOffsetPosition
        {
            get { return m_InfiniteClipOffsetPosition; }
            set { m_InfiniteClipOffsetPosition = value; }
        }

        /// <summary>
        /// The rotation offset of a track in infinite mode.
        /// </summary>
        public Quaternion infiniteClipOffsetRotation
        {
            get { return Quaternion.Euler(m_InfiniteClipOffsetEulerAngles); }
            set { m_InfiniteClipOffsetEulerAngles = value.eulerAngles; }
        }

        /// <summary>
        /// The euler angle representation of the rotation offset of the track when in infinite mode.
        /// </summary>
        public Vector3 infiniteClipOffsetEulerAngles
        {
            get { return m_InfiniteClipOffsetEulerAngles; }
            set { m_InfiniteClipOffsetEulerAngles = value; }
        }

        internal bool infiniteClipApplyFootIK
        {
            get { return m_InfiniteClipApplyFootIK; }
            set { m_InfiniteClipApplyFootIK = value; }
        }

        internal double infiniteClipTimeOffset
        {
            get { return m_InfiniteClipTimeOffset; }
            set { m_InfiniteClipTimeOffset = value; }
        }

        /// <summary>
        /// The saved state of pre-extrapolation for clips converted to infinite mode.
        /// </summary>
        public TimelineClip.ClipExtrapolation infiniteClipPreExtrapolation
        {
            get { return m_InfiniteClipPreExtrapolation; }
            set { m_InfiniteClipPreExtrapolation = value; }
        }

        /// <summary>
        /// The saved state of post-extrapolation for clips when converted to infinite mode.
        /// </summary>
        public TimelineClip.ClipExtrapolation infiniteClipPostExtrapolation
        {
            get { return m_InfiniteClipPostExtrapolation; }
            set { m_InfiniteClipPostExtrapolation = value; }
        }

        /// <summary>
        /// The saved state of animation clip loop state when converted to infinite mode
        /// </summary>
        internal AnimationPlayableAsset.LoopMode infiniteClipLoop
        {
            get { return mInfiniteClipLoop; }
            set { mInfiniteClipLoop = value; }
        }

        [ContextMenu("Reset Offsets")]
        void ResetOffsets()
        {
            m_Position = Vector3.zero;
            m_EulerAngles = Vector3.zero;
            UpdateClipOffsets();
        }

        /// <summary>
        /// Creates a TimelineClip on this track that uses an AnimationClip.
        /// </summary>
        /// <param name="clip">Source animation clip of the resulting TimelineClip.</param>
        /// <returns>A new TimelineClip which has an AnimationPlayableAsset asset attached.</returns>
        public TimelineClip CreateClip(AnimationClip clip)
        {
            if (clip == null)
                return null;

            var newClip = CreateClip<AnimationPlayableAsset>();
            AssignAnimationClip(newClip, clip);
            return newClip;
        }

        /// <summary>
        /// Creates an AnimationClip that stores the data for an infinite track.
        /// </summary>
        /// <remarks>
        /// If an infiniteClip already exists, this method produces no result, even if you provide a different value
        /// for infiniteClipName.
        /// </remarks>
        /// <remarks>
        /// This method can't create an infinite clip for an AnimationTrack that contains one or more Timeline clips.
        /// Use AnimationTrack.inClipMode to determine whether it is possible to create an infinite clip on an AnimationTrack.
        /// </remarks>
        /// <remarks>
        /// When used from the editor, this method attempts to save the created infinite clip to the TimelineAsset.
        /// The TimelineAsset must already exist in the AssetDatabase to save the infinite clip. If the TimelineAsset
        /// does not exist, the infinite clip is still created but it is not saved.
        /// </remarks>
        /// <param name="infiniteClipName">
        /// The name of the AnimationClip to create.
        /// This method does not ensure unique names. If you want a unique clip name, you must provide one.
        /// See ObjectNames.GetUniqueName for information on a method that creates unique names.
        /// </param>
        public void CreateInfiniteClip(string infiniteClipName)
        {
            if (inClipMode)
            {
                Debug.LogWarning("CreateInfiniteClip cannot create an infinite clip for an AnimationTrack that contains one or more Timeline Clips.");
                return;
            }

            if (m_InfiniteClip != null)
                return;

            m_InfiniteClip = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(infiniteClipName) ? k_DefaultInfiniteClipName : infiniteClipName, this, false);
        }

        /// <summary>
        /// Creates a TimelineClip, AnimationPlayableAsset and an AnimationClip. Use this clip to record in a timeline.
        /// </summary>
        /// <remarks>
        /// When used from the editor, this method attempts to save the created recordable clip to the TimelineAsset.
        /// The TimelineAsset must already exist in the AssetDatabase to save the recordable clip. If the TimelineAsset
        /// does not exist, the recordable clip is still created but it is not saved.
        /// </remarks>
        /// <param name="animClipName">
        /// The name of the AnimationClip to create.
        /// This method does not ensure unique names. If you want a unique clip name, you must provide one.
        /// See ObjectNames.GetUniqueName for information on a method that creates unique names.
        /// </param>
        /// <returns>
        /// Returns a new TimelineClip with an AnimationPlayableAsset asset attached.
        /// </returns>
        public TimelineClip CreateRecordableClip(string animClipName)
        {
            var clip = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(animClipName) ? k_DefaultRecordableClipName : animClipName, this, false);

            var timelineClip = CreateClip(clip);
            timelineClip.displayName = animClipName;
            timelineClip.recordable = true;
            timelineClip.start = 0;
            timelineClip.duration = 1;

            var apa = timelineClip.asset as AnimationPlayableAsset;
            if (apa != null)
                apa.removeStartOffset = false;

            return timelineClip;
        }

#if UNITY_EDITOR
        internal Vector3 sceneOffsetPosition
        {
            get { return m_SceneOffsetPosition; }
            set { m_SceneOffsetPosition = value; }
        }

        internal Vector3 sceneOffsetRotation
        {
            get { return m_SceneOffsetRotation; }
            set { m_SceneOffsetRotation = value; }
        }

        internal bool hasPreviewComponents
        {
            get
            {
                if (m_HasPreviewComponents)
                    return true;

                var parentTrack = parent as AnimationTrack;
                if (parentTrack != null)
                {
                    return parentTrack.hasPreviewComponents;
                }

                return false;
            }
        }
#endif

        /// <summary>
        /// Used to initialize default values on a newly created clip
        /// </summary>
        /// <param name="clip">The clip added to the track</param>
        protected override void OnCreateClip(TimelineClip clip)
        {
            var extrapolation = TimelineClip.ClipExtrapolation.None;
            if (!isSubTrack)
                extrapolation = TimelineClip.ClipExtrapolation.Hold;
            clip.preExtrapolationMode = extrapolation;
            clip.postExtrapolationMode = extrapolation;
        }

        protected internal override int CalculateItemsHash()
        {
            return GetAnimationClipHash(m_InfiniteClip).CombineHash(base.CalculateItemsHash());
        }

        internal void UpdateClipOffsets()
        {
#if UNITY_EDITOR
            if (m_ClipOffset.IsValid())
            {
                m_ClipOffset.SetPosition(position);
                m_ClipOffset.SetRotation(rotation);
            }
#endif
        }

        //Tip：该方法的作用类似于TrackAsset的CompileClips，都是创建轨道节点以及轨道上的各个片段节点，并且连接，然后返回轨道节点。
        //创建轨道的AnimationMixerPlayable节点，遍历调用轨道上的各个片段的CreatePlayable方法，并将返回的节点与Mixer节点相连，之后返回Mixer节点或在Mixer和LayerMixer之间的偏移节点。
        Playable CompileTrackPlayable(PlayableGraph graph, AnimationTrack track, GameObject go, IntervalTree<RuntimeElement> tree, AppliedOffsetMode mode)
        {
            //每条轨道都有一个AnimationMixerPlayable节点，并且输入端口数就是该轨道中的片段个数。
            var mixer = AnimationMixerPlayable.Create(graph, track.clips.Length);
            for (int i = 0; i < track.clips.Length; i++)
            {
                var c = track.clips[i];
                var asset = c.asset as PlayableAsset;
                if (asset == null)
                    continue;

                //这里就是因为有一个appliedOffsetMode属性需要赋值，所以进行转换。后面在AnimationPlayableAsset的ShouldApplyOffset会基于该属性判断。
                var animationAsset = asset as AnimationPlayableAsset;
                if (animationAsset != null)
                    animationAsset.appliedOffsetMode = mode;

                var source = asset.CreatePlayable(graph, go);
                if (source.IsValid())
                {//Tip：这里可以肯定，RuntimeClip中的Mixer节点就是AnimationMixerPlayable节点。
                    var clip = new RuntimeClip(c, source, mixer);
                    tree.Add(clip);
                    graph.Connect(source, 0, mixer, i);
                    mixer.SetInputWeight(i, 0.0f);
                }
            }

            /*Tip：如果AnimatesRootTransform为true的话，也就是进入后续ApplyTrackOffset，则会发现在轨道的AnimationMixerPlayable节点和AnimationLayerMixerPlayable节点之间连接了一个
            AnimationOffsetPlayable偏移节点，而没有连接该节点的轨道就没有RootMotion。
            */
            if (!track.AnimatesRootTransform())
                return mixer;

            return ApplyTrackOffset(graph, mixer, go, mode);
        }

        /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
        /// <returns>Returns <c>Playable.Null</c></returns>
        Playable ILayerable.CreateLayerMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return Playable.Null;
        }

        //返回AnimationLayerMixerPlayable或AnimationMotionXToDeltaPlayable节点，直接连接到TimelinePlayable。
        internal override Playable CreateMixerPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            if (isSubTrack)
                throw new InvalidOperationException("Nested animation tracks should never be asked to create a graph directly");

            //准备好容器，存储所有子轨道（包括自己，因为本质上对于AnimationTrack来说，父轨道与Override Track没有本质区别，如果父轨道没有任何片段的话，那么它就不会生成节点）
            List<AnimationTrack> flattenTracks = new List<AnimationTrack>();
            if (CanCompileClips())
                flattenTracks.Add(this);

            //Tip：就是这里，我之前一直疑惑为何自动给我开启RootMotion，原来是因为只要判断AnimationClip可以进行RootMotion（含有相关曲线）那么就自动开启RootMotion模式。
            var genericRoot = GetGenericRootNode(go); //获取Generic（通用）根节点（Transform）
            var animatesRootTransformNoMask = AnimatesRootTransform(); //这就是没有考虑遮罩的情况下，单纯根据轨道上的片段做的判断。
            // if (animatesRootTransformNoMask == true) Debug.Log("AnimatesRootTransform为true");
            var animatesRootTransform = animatesRootTransformNoMask && !IsRootTransformDisabledByMask(go, genericRoot);
            foreach (var subTrack in GetChildTracks())
            {
                var child = subTrack as AnimationTrack;
                if (child != null && child.CanCompileClips())
                {//Tip：其实这里的判断和上面的逻辑完全一样，就是站在主轨道和子轨道的层面，只要有轨道的animatesRootTransform为true，那么就会让后续参与逻辑的animatesRootTransform为true。
                    var childAnimatesRoot = child.AnimatesRootTransform();
                    animatesRootTransformNoMask |= child.AnimatesRootTransform(); //Ques：为何不直接写childAnimatesRoot，不是相同值吗？
                    animatesRootTransform |= (childAnimatesRoot && !child.IsRootTransformDisabledByMask(go, genericRoot));
                    flattenTracks.Add(child);
                }
            }

            // figure out which mode to apply
            AppliedOffsetMode mode = GetOffsetMode(go, animatesRootTransform);
            int defaultBlendCount = GetDefaultBlendCount();
            //创建AnimationLayerMixerPlayable节点。
            var layerMixer = CreateGroupMixer(graph, go, flattenTracks.Count + defaultBlendCount);
            for (int c = 0; c < flattenTracks.Count; c++)
            {//Tip：在这里发现，其实对于主轨道和子轨道都是一视同仁的，都是按照同样的条件来决定是否驱动RootTransform。只是在实际编辑的时候，确实对于主轨道会有一个特殊倾向。
                int blendIndex = c + defaultBlendCount;
                // if the child is masking the root transform, compile it as if we are non-root mode
                var childMode = mode;
                if (mode != AppliedOffsetMode.NoRootTransform && flattenTracks[c].IsRootTransformDisabledByMask(go, genericRoot))
                    childMode = AppliedOffsetMode.NoRootTransform;

                Debug.Log($"索引{c}的轨道IsRootTransformDisabledByMask为{flattenTracks[c].IsRootTransformDisabledByMask(go, genericRoot)}");

                //返回Mixer节点或偏移节点。
                var compiledTrackPlayable = flattenTracks[c].inClipMode ?
                    CompileTrackPlayable(graph, flattenTracks[c], go, tree, childMode) :
                    flattenTracks[c].CreateInfiniteTrackPlayable(graph, go, tree, childMode);
                //把轨道节点连接到LayerMixer节点。
                graph.Connect(compiledTrackPlayable, 0, layerMixer, blendIndex);
                //注意此时只是在构图，而inClipMode就是轨道上有片段，那么就设置权重为0，也就是初始状态下权重默认为0。
                layerMixer.SetInputWeight(blendIndex, flattenTracks[c].inClipMode ? 0 : 1);
                //设置层级遮罩。
                if (flattenTracks[c].applyAvatarMask && flattenTracks[c].avatarMask != null)
                {
                    layerMixer.SetLayerMaskFromAvatarMask((uint)blendIndex, flattenTracks[c].avatarMask);
                }
            }

            //Tip：上面其实都是在说偏移，根节点在初始位置的偏移，而这里才是指的RootMotion根运动。
            //检查Animator的RootMotion选项。
            var requiresMotionXPlayable = RequiresMotionXPlayable(mode, go);

            // In the editor, we may require the motion X playable if we are animating the root transform but it is masked out, because the default poses
            //  need to properly update root motion
            requiresMotionXPlayable |= (defaultBlendCount > 0 && RequiresMotionXPlayable(GetOffsetMode(go, animatesRootTransformNoMask), go));

            //创建默认的那两个片段并且做好连接。
            // Attach the default poses
            AttachDefaultBlend(graph, layerMixer, requiresMotionXPlayable);

            // motionX playable not required in scene offset mode, or root transform mode
            Playable mixer = layerMixer;
            //将AnimationLayerMixerPlayable连接到AnimationMotionXToDeltaPlayable（该节点就是用来处理根运动RootMotion的），并将其返回，随后会与TimelinePlayable直接相连。
            if (requiresMotionXPlayable)
            {
                // If we are animating a root transform, add the motionX to delta playable as the root node
                var motionXToDelta = AnimationMotionXToDeltaPlayable.Create(graph);
                graph.Connect(mixer, 0, motionXToDelta, 0);
                motionXToDelta.SetInputWeight(0, 1.0f);
                motionXToDelta.SetAbsoluteMotion(UsesAbsoluteMotion(mode));
                mixer = (Playable)motionXToDelta; //注意Playable节点都是定义为的结构体，所以这里需要强制转换，具体转换逻辑已经定义在底层了。
            }


#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var animator = GetBinding(go != null ? go.GetComponent<PlayableDirector>() : null);
                if (animator != null)
                {
                    GameObject targetGO = animator.gameObject;
                    IAnimationWindowPreview[] previewComponents = targetGO.GetComponents<IAnimationWindowPreview>();

                    m_HasPreviewComponents = previewComponents.Length > 0;
                    if (m_HasPreviewComponents)
                    {
                        foreach (var component in previewComponents)
                        {
                            mixer = component.BuildPreviewGraph(graph, mixer);
                        }
                    }
                }
            }
#endif

            return mixer;
        }

        /*Tip：在编辑模式下会看到有两个默认的片段，分别引用了DefaultPose和HumanoidDefault。*/
        private int GetDefaultBlendCount()
        {
#if  UNITY_EDITOR
            if (Application.isPlaying)
                return 0;

            return ((m_CachedPropertiesClip != null) ? 1 : 0) + ((m_DefaultPoseClip != null) ? 1 : 0);
#else
            return 0;
#endif
        }

        // Attaches the default blends to the layer mixer
        // the base layer is a default clip of all driven properties
        // the next layer is optionally the desired default pose (in the case of humanoid, the TPose)
        private void AttachDefaultBlend(PlayableGraph graph, AnimationLayerMixerPlayable mixer, bool requireOffset)
        {
#if  UNITY_EDITOR
            if (Application.isPlaying)
                return;

            int mixerInput = 0;
            if (m_CachedPropertiesClip)
            {
                var cachedPropertiesClip = AnimationClipPlayable.Create(graph, m_CachedPropertiesClip);
                cachedPropertiesClip.SetApplyFootIK(false);
                var defaults = (Playable)cachedPropertiesClip;
                if (requireOffset)
                    defaults = AttachOffsetPlayable(graph, defaults, m_SceneOffsetPosition, Quaternion.Euler(m_SceneOffsetRotation));
                graph.Connect(defaults, 0, mixer, mixerInput);
                mixer.SetInputWeight(mixerInput, 1.0f);
                mixerInput++;
            }

            if (m_DefaultPoseClip)
            {
                var defaultPose = AnimationClipPlayable.Create(graph, m_DefaultPoseClip);
                defaultPose.SetApplyFootIK(false);
                var blendDefault = (Playable)defaultPose;
                if (requireOffset)
                    blendDefault = AttachOffsetPlayable(graph, blendDefault, m_SceneOffsetPosition, Quaternion.Euler(m_SceneOffsetRotation));
                graph.Connect(blendDefault, 0, mixer, mixerInput);
                mixer.SetInputWeight(mixerInput, 1.0f);
            }
#endif
        }

        private Playable AttachOffsetPlayable(PlayableGraph graph, Playable playable, Vector3 pos, Quaternion rot)
        {
            var offsetPlayable = AnimationOffsetPlayable.Create(graph, pos, rot, 1);
            offsetPlayable.SetInputWeight(0, 1.0f);
            graph.Connect(playable, 0, offsetPlayable, 0);
            return offsetPlayable;
        }

#if UNITY_EDITOR
        private static string k_DefaultHumanoidClipPath = "Packages/com.unity.timeline/Editor/StyleSheets/res/HumanoidDefault.anim";
        private static AnimationClip s_DefaultHumanoidClip = null;

        AnimationClip GetDefaultHumanoidClip()
        {
            if (s_DefaultHumanoidClip == null)
            {
                s_DefaultHumanoidClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(k_DefaultHumanoidClipPath);
                if (s_DefaultHumanoidClip == null)
                    Debug.LogError("Could not load default humanoid animation clip for Timeline");
            }

            return s_DefaultHumanoidClip;
        }

#endif

        //Ques：似乎这个方法才是决定是否启用RootMotion？而非AnimatesRootTransform，似乎这是决定是否应用偏移的。
        bool RequiresMotionXPlayable(AppliedOffsetMode mode, GameObject gameObject)
        {
            /*TODO：主动取消RootMotion，下面的模式判断应该属于“是否具备进行RootMotion的能力”。按理来说，没有能力那当然就返回false，而如果有，再看主观意见，所以貌似放在下面更合适。*/
            // if (m_ApplyRootMotion == false) return false;

            //过了这个条件，应该就是具备进行RootMotion的能力。
            if (mode == AppliedOffsetMode.NoRootTransform)
                return false;
            if (mode == AppliedOffsetMode.SceneOffsetLegacy)
            {
                var animator = GetBinding(gameObject != null ? gameObject.GetComponent<PlayableDirector>() : null);
                return animator != null && animator.hasRootMotion;
            }

            //TODO: 隐含了就是看主轨道的该选项，是否合理呢？
            //这个选项属性就是主动取消，说白了就是有能力就默认开启，但可以主动取消，否则的话这个选项为true，意思就是完全看有没有这个能力，有就开启，没有自然就关闭。
            if (m_ApplyRootMotion == false) return false;

            return true;
        }

        static bool UsesAbsoluteMotion(AppliedOffsetMode mode)
        {
#if UNITY_EDITOR
            // in editor, previewing is always done in absolute motion
            if (!Application.isPlaying)
                return true;
#endif
            return mode != AppliedOffsetMode.SceneOffset &&
                mode != AppliedOffsetMode.SceneOffsetLegacy;
        }

        bool HasController(GameObject gameObject)
        {
            var animator = GetBinding(gameObject != null ? gameObject.GetComponent<PlayableDirector>() : null);

            return animator != null && animator.runtimeAnimatorController != null;
        }

        //传入自己（轨道资产）从PlayableDirector获取绑定的对象Animator。
        internal Animator GetBinding(PlayableDirector director)
        {
            if (director == null)
                return null;

            UnityEngine.Object key = this;
            if (isSubTrack)
                key = parent;

            //传入轨道即自己，获取所绑定的对象。（在PlayableDirector的Scene Bindings可以看到）
            UnityEngine.Object binding = null;
            if (director != null)
                binding = director.GetGenericBinding(key);

            Animator animator = null;
            if (binding != null) // the binding can be an animator or game object
            {
                animator = binding as Animator;
                var gameObject = binding as GameObject;
                if (animator == null && gameObject != null)
                    animator = gameObject.GetComponent<Animator>();
            }

            return animator;
        }

        static AnimationLayerMixerPlayable CreateGroupMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
#if UNITY_2022_2_OR_NEWER
            return AnimationLayerMixerPlayable.Create(graph, inputCount, false);
#else
            return AnimationLayerMixerPlayable.Create(graph, inputCount);
#endif
        }

        Playable CreateInfiniteTrackPlayable(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, AppliedOffsetMode mode)
        {
            if (m_InfiniteClip == null)
                return Playable.Null;

            var mixer = AnimationMixerPlayable.Create(graph, 1);

            // In infinite mode, we always force the loop mode of the clip off because the clip keys are offset in infinite mode
            //  which causes loop to behave different.
            // The inline curve editor never shows loops in infinite mode.
            var playable = AnimationPlayableAsset.CreatePlayable(graph, m_InfiniteClip, m_InfiniteClipOffsetPosition, m_InfiniteClipOffsetEulerAngles, false, mode, infiniteClipApplyFootIK, AnimationPlayableAsset.LoopMode.Off);
            if (playable.IsValid())
            {
                tree.Add(new InfiniteRuntimeClip(playable));
                graph.Connect(playable, 0, mixer, 0);
                mixer.SetInputWeight(0, 1.0f);
            }

            if (!AnimatesRootTransform())
                return mixer;

            var rootTrack = isSubTrack ? (AnimationTrack)parent : this;
            return rootTrack.ApplyTrackOffset(graph, mixer, go, mode);
        }

        Playable ApplyTrackOffset(PlayableGraph graph, Playable root, GameObject go, AppliedOffsetMode mode)
        {
#if UNITY_EDITOR
            m_ClipOffset = AnimationOffsetPlayable.Null;
#endif

            //Tip：这些模式也就是无偏移。
            // offsets don't apply in scene offset, or if there is no root transform (globally or on this track)
            if (mode == AppliedOffsetMode.SceneOffsetLegacy ||
                mode == AppliedOffsetMode.SceneOffset ||
                mode == AppliedOffsetMode.NoRootTransform
            )
                return root;

            var pos = position;
            var rot = rotation;

#if UNITY_EDITOR
            // in the editor use the preview position to playback from if available
            if (mode == AppliedOffsetMode.SceneOffsetEditor)
            {
                pos = m_SceneOffsetPosition;
                rot = Quaternion.Euler(m_SceneOffsetRotation);
            }
#endif

            var offsetPlayable = AnimationOffsetPlayable.Create(graph, pos, rot, 1);
#if UNITY_EDITOR
            m_ClipOffset = offsetPlayable;
#endif
            graph.Connect(root, 0, offsetPlayable, 0);
            offsetPlayable.SetInputWeight(0, 1);

            return offsetPlayable;
        }

        // the evaluation time is large so that the properties always get evaluated
        internal override void GetEvaluationTime(out double outStart, out double outDuration)
        {
            if (inClipMode)
            {
                base.GetEvaluationTime(out outStart, out outDuration);
            }
            else
            {
                outStart = 0;
                outDuration = TimelineClip.kMaxTimeValue;
            }
        }

        internal override void GetSequenceTime(out double outStart, out double outDuration)
        {
            if (inClipMode)
            {
                base.GetSequenceTime(out outStart, out outDuration);
            }
            else
            {
                outStart = 0;
                outDuration = Math.Max(GetNotificationDuration(), TimeUtility.GetAnimationClipLength(m_InfiniteClip));
            }
        }

        void AssignAnimationClip(TimelineClip clip, AnimationClip animClip)
        {
            if (clip == null || animClip == null)
                return;

            if (animClip.legacy)
                throw new InvalidOperationException("Legacy Animation Clips are not supported");

            AnimationPlayableAsset asset = clip.asset as AnimationPlayableAsset;
            if (asset != null)
            {
                asset.clip = animClip;
                asset.name = animClip.name;
                var duration = asset.duration;
                if (!double.IsInfinity(duration) && duration >= TimelineClip.kMinDuration && duration < TimelineClip.kMaxTimeValue)
                    clip.duration = duration;
            }
            clip.displayName = animClip.name;
        }

        /// <summary>
        /// Called by the Timeline Editor to gather properties requiring preview.
        /// </summary>
        /// <param name="director">The PlayableDirector invoking the preview</param>
        /// <param name="driver">PropertyCollector used to gather previewable properties</param>
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            m_SceneOffsetPosition = Vector3.zero;
            m_SceneOffsetRotation = Vector3.zero;

            var animator = GetBinding(director);
            if (animator == null)
                return;

            var animClips = new List<AnimationClip>(this.clips.Length + 2);
            GetAnimationClips(animClips);

            var hasHumanMotion = animClips.Exists(clip => clip.humanMotion);
            // case 1174752 - recording root transform on humanoid clips clips cause invalid pose. This will apply the default T-Pose, only if it not already driven by another track
            if (!hasHumanMotion && animator.isHuman && AnimatesRootTransform() &&
                !DrivenPropertyManagerInternal.IsDriven(animator.transform, "m_LocalPosition.x") &&
                !DrivenPropertyManagerInternal.IsDriven(animator.transform, "m_LocalRotation.x"))
                hasHumanMotion = true;

            m_SceneOffsetPosition = animator.transform.localPosition;
            m_SceneOffsetRotation = animator.transform.localEulerAngles;

            // Create default pose clip from collected properties
            if (hasHumanMotion)
                animClips.Add(GetDefaultHumanoidClip());

            m_DefaultPoseClip = hasHumanMotion ? GetDefaultHumanoidClip() : null;
            var hash = AnimationPreviewUtilities.GetClipHash(animClips);
            if (m_CachedBindings == null || m_CachedHash != hash)
            {
                m_CachedBindings = AnimationPreviewUtilities.GetBindings(animator.gameObject, animClips);
                m_CachedPropertiesClip = AnimationPreviewUtilities.CreateDefaultClip(animator.gameObject, m_CachedBindings);
                m_CachedHash = hash;
            }

            AnimationPreviewUtilities.PreviewFromCurves(animator.gameObject, m_CachedBindings); // faster to preview from curves then an animation clip
#endif
        }

        /// <summary>
        /// Gather all the animation clips for this track
        /// </summary>
        /// <param name="animClips"></param>
        private void GetAnimationClips(List<AnimationClip> animClips)
        {
            foreach (var c in clips)
            {
                var a = c.asset as AnimationPlayableAsset;
                if (a != null && a.clip != null)
                    animClips.Add(a.clip);
            }

            if (m_InfiniteClip != null)
                animClips.Add(m_InfiniteClip);

            foreach (var childTrack in GetChildTracks())
            {
                var animChildTrack = childTrack as AnimationTrack;
                if (animChildTrack != null)
                    animChildTrack.GetAnimationClips(animClips);
            }
        }

        // calculate which offset mode to apply
        AppliedOffsetMode GetOffsetMode(GameObject go, bool animatesRootTransform)
        {
            //首先要（主动）支持RootTransform，然后才是看在检视器中选择的模式。
            //所以另一面，就是当（主动）不支持时，就直接设置为NoRootTransform，不看其他。
            if (!animatesRootTransform)
                return AppliedOffsetMode.NoRootTransform;

            if (m_TrackOffset == TrackOffset.ApplyTransformOffsets)
                return AppliedOffsetMode.TransformOffset;

            //在检视器中直接选择的模式，在此根据运行模式还是编辑模式进行二分。
            if (m_TrackOffset == TrackOffset.ApplySceneOffsets)
                return (Application.isPlaying) ? AppliedOffsetMode.SceneOffset : AppliedOffsetMode.SceneOffsetEditor;

            //上面两个都没选择的话，可选的那就只有Auto了，其实也就是执行这里的逻辑。
            if (HasController(go)) //查找绑定的Animator是否有AnimatorController。
            {
                if (!Application.isPlaying)
                    return AppliedOffsetMode.SceneOffsetLegacyEditor;
                return AppliedOffsetMode.SceneOffsetLegacy;
            }

            return AppliedOffsetMode.TransformOffsetLegacy;
        }

        private bool IsRootTransformDisabledByMask(GameObject gameObject, Transform genericRootNode)
        {
            //有遮罩再看后面
            if (avatarMask == null || !applyAvatarMask)
                return false;

            //传入GameObject就是为了获取绑定的Animator。
            var animator = GetBinding(gameObject != null ? gameObject.GetComponent<PlayableDirector>() : null);
            if (animator == null)
                return false;

            //Tip：其实这里的根节点，就是在AvatarMask检视器中的Humanoid看到的脚底的那个遮罩，这个就是用来控制是否启用RootMotion。
            //人形骨骼的根节点如果Active，那么就返回false，即没有被遮。
            if (animator.isHuman)
                return !avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Root);

            if (avatarMask.transformCount == 0)
                return false;

            /*Tip：注意这里检查第一个变换路径是否为空或空字符串，也就是判断其是否代表根节点，这是AvatarMask的变换路径的机制。然后就是该根节点没有Active则返回true。
            意思就是，遮罩接管了根节点，但是又没有Active，所以就相当于根运动被遮盖了。
            */
            // no special root supplied
            if (genericRootNode == null)
                return string.IsNullOrEmpty(avatarMask.GetTransformPath(0)) && !avatarMask.GetTransformActive(0);

            // walk the avatar list to find the matching transform
            for (int i = 0; i < avatarMask.transformCount; i++)
            {
                if (genericRootNode == animator.transform.Find(avatarMask.GetTransformPath(i)))
                    return !avatarMask.GetTransformActive(i);
            }

            return false;
        }

        // Returns the generic root transform node. Returns null if it is the root node, OR if it not a generic node
        private Transform GetGenericRootNode(GameObject gameObject)
        {
            //首先获取绑定的Animator。
            var animator = GetBinding(gameObject != null ? gameObject.GetComponent<PlayableDirector>() : null);
            if (animator == null)
                return null;

            //因为是获取Generic（通用）节点，所以不能是Humanoid（人形）
            if (animator.isHuman)
                return null;
            //此时就要求具有Generic类型的Avatar
            if (animator.avatar == null)
                return null;

            // this returns the bone name, but not the full path
            var rootName = animator.avatar.humanDescription.m_RootMotionBoneName; //内部记录好的根骨骼名称。
            //根骨骼名称是否与Animator所在游戏对象的名称相同。说明不能与其相同，应当是在其以下层级的对象作为根节点。
            if (rootName == animator.name || string.IsNullOrEmpty(rootName))
                return null;

            // walk the hierarchy to find the first bone with this name
            return FindInHierarchyBreadthFirst(animator.transform, rootName);
        }

        //Tip：简单来说，这里判断的就是是否具备RootTransform的能力（其实主要指的是根节点的初始偏移，而非RootMotion，但貌似也没完全分开，有点混合的意思），具备则默认开启。
        internal bool AnimatesRootTransform()
        {
            // if (m_ApplyRootMotion == false) return false;

            //TODO：暂时可以不管这个infiniteClip，就看后面的。
            // infinite mode
            if (AnimationPlayableAsset.HasRootTransforms(m_InfiniteClip))
                return true;

            //Tip：理解这里的核心是理解在AnimationPlayableAsset的HasRootTransforms。
            // clip mode
            foreach (var c in GetClips())
            {//只要存在带有RootMotion的片段，就要驱动RootMotion，因为没有的也自然表现为没有，也就是兼容的。
                var apa = c.asset as AnimationPlayableAsset;
                if (apa != null && apa.hasRootTransforms)
                    return true;
            }

            return false;
        }

        //Tip：迭代法查找指定名称的Transform（游戏对象）
        private static readonly Queue<Transform> s_CachedQueue = new Queue<Transform>(100);
        private static Transform FindInHierarchyBreadthFirst(Transform t, string name)
        {
            s_CachedQueue.Clear();
            s_CachedQueue.Enqueue(t);
            //就是遍历树，查找指定名称的节点。
            while (s_CachedQueue.Count > 0)
            {
                var r = s_CachedQueue.Dequeue();
                if (r.name == name)
                    return r;
                for (int i = 0; i < r.childCount; i++)
                    s_CachedQueue.Enqueue(r.GetChild(i));
            }

            return null;
        }
    }
}
