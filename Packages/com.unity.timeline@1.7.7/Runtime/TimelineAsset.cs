using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A PlayableAsset that represents a timeline.
    /// </summary>
    [ExcludeFromPreset] //避免从该类实例创建Preset
    [Serializable]
    [TimelineHelpURL(typeof(TimelineAsset))]
    public partial class TimelineAsset : PlayableAsset, ISerializationCallbackReceiver, ITimelineClipAsset, IPropertyPreview
    {
        /// <summary>
        /// How the duration of the timeline is determined.
        /// </summary>
        public enum DurationMode
        {
            /// <summary>
            /// The duration of the timeline is determined based on the clips present.
            /// </summary>
            BasedOnClips,
            /// <summary>
            /// The duration of the timeline is a fixed length.
            /// </summary>
            FixedLength
        }

        /// <summary>
        /// Properties of the timeline that are used by the editor
        /// </summary>
        [Serializable]
        public class EditorSettings
        {
            internal static readonly double kMinFrameRate = TimeUtility.kFrameRateEpsilon;
            internal static readonly double kMaxFrameRate = 1000.0;
            internal static readonly double kDefaultFrameRate = 60.0;
            //帧率，默认60，可以通过齿轮菜单调节
            [HideInInspector, SerializeField, FrameRateField] double m_Framerate = kDefaultFrameRate;
            [HideInInspector, SerializeField] bool m_ScenePreview = true;

            /// <summary>
            /// The frames per second used for snapping and time ruler display
            /// </summary>
            [Obsolete("EditorSettings.fps has been deprecated. Use editorSettings.frameRate instead.", false)]
            public float fps
            {
                get
                {
                    return (float)m_Framerate;
                }
                set
                {
                    m_Framerate = Mathf.Clamp(value, (float)kMinFrameRate, (float)kMaxFrameRate);
                }
            }

            /// <summary>
            /// The frames per second used for framelocked preview, frame snapping and time ruler display,
            /// </summary>
            /// <remarks>
            /// If frameRate is set to a non-standard custom frame rate, Timeline playback
            /// may give incorrect results when playbackLockedToFrame is true.
            /// </remarks>
            /// <seealso cref="UnityEngine.Timeline.TimelineAsset"/>
            public double frameRate
            {
                get { return m_Framerate; }
                set { m_Framerate = GetValidFrameRate(value); }
            }

            /// <summary>
            /// Sets the EditorSetting frameRate to one of the provided standard frame rates.
            /// </summary>
            /// <param name="enumValue"> StandardFrameRates value, used to set the current EditorSettings frameRate value.</param>
            /// <remarks>
            /// When specifying drop frame values, it is recommended to select one of the provided standard frame rates.
            /// Specifying a non-standard custom frame rate may give incorrect results when playbackLockedToFrame
            /// is enabled during Timeline playback.
            /// </remarks>
            /// <exception cref="ArgumentException">Thrown when the enumValue is not a valid member of StandardFrameRates.</exception>
            /// <seealso cref="UnityEngine.Timeline.TimelineAsset"/>
            public void SetStandardFrameRate(StandardFrameRates enumValue) //在齿轮菜单的FrameRate中就可以看到这些常量对应的选项
            {//Tip：这里应该是在菜单项的逻辑中通过反射调用该方法，所以显示0个引用
                FrameRate rate = TimeUtility.ToFrameRate(enumValue);
                if (!rate.IsValid()) //只要不为0就是有效。
                    throw new ArgumentException(String.Format("StandardFrameRates {0}, is not defined",
                        enumValue.ToString()));
                m_Framerate = rate.rate;
            }

            /// <summary>
            /// Set to false to ignore scene preview when this timeline is played by the Timeline window.
            /// </summary>
            /// <remarks>
            /// When set to false, this setting will
            /// - Disable scene preview when this timeline is played by the Timeline window.
            /// - Disable recording for all recordable tracks.
            /// - Disable play range in the Timeline window.
            /// - `Stop()` is not called on the `PlayableDirector` when switching between different `TimelineAsset`s in the TimelineWindow.
            ///
            /// `scenePreview` will only be applied if the asset is the master timeline.
            /// </remarks>
            /// <seealso cref="UnityEngine.Timeline.TimelineAsset"/>
            public bool scenePreview
            {
                get => m_ScenePreview;
                set => m_ScenePreview = value;
            }
        }

        //Ques：这里为何不写成List<TrackAsset>呢？
        [HideInInspector, SerializeField] List<ScriptableObject> m_Tracks; 
        [HideInInspector, SerializeField] double m_FixedDuration; // only applied if duration mode is Fixed
        [HideInInspector, NonSerialized] TrackAsset[] m_CacheOutputTracks;
        [HideInInspector, NonSerialized] List<TrackAsset> m_CacheRootTracks;
        [HideInInspector, NonSerialized] TrackAsset[] m_CacheFlattenedTracks;
        [HideInInspector, SerializeField] EditorSettings m_EditorSettings = new EditorSettings();
        [SerializeField] DurationMode m_DurationMode;

        [HideInInspector, SerializeField] MarkerTrack m_MarkerTrack;

        /// <summary>
        /// Settings used by timeline for editing purposes
        /// </summary>
        public EditorSettings editorSettings
        {
            get { return m_EditorSettings; }
        }

        /// <summary>
        /// The length, in seconds, of the timeline
        /// 以秒s作为单位，实际的总时间，在编辑时就会自动计算，根据Duration Mode，基于Clips或者是直接指定固定值。
        /// </summary>
        public override double duration
        {
            get
            {
                // @todo cache this value when rebuilt
                if (m_DurationMode == DurationMode.BasedOnClips)
                {
                    //avoid having no clip evaluated at the end by removing a tick from the total duration
                    var discreteDuration = CalculateItemsDuration();
                    if (discreteDuration <= 0)
                        return 0.0;
                    return (double)discreteDuration.OneTickBefore();
                }

                return m_FixedDuration;
            }
        }

        /// <summary>
        /// The length of the timeline when durationMode is set to fixed length.
        /// 以s为单位，可以自由设置的固定时间，至于最终时间，要看DurationMode
        /// </summary>
        public double fixedDuration
        {
            get
            {
                DiscreteTime discreteDuration = (DiscreteTime)m_FixedDuration;
                if (discreteDuration <= 0)
                    return 0.0;

                //avoid having no clip evaluated at the end by removing a tick from the total duration
                return (double)discreteDuration.OneTickBefore();
            }
            set { m_FixedDuration = Math.Max(0.0, value); }
        }

        /// <summary>
        /// The mode used to determine the duration of the Timeline
        /// </summary>
        public DurationMode durationMode
        {
            get { return m_DurationMode; }
            set { m_DurationMode = value; }
        }

        /// <summary>
        /// A description of the PlayableOutputs that will be created by the timeline when instantiated.
        /// </summary>
        /// <remarks>
        /// Each track will create an PlayableOutput
        /// 每一个轨道Track都会创建一个PlayableOutput
        /// </remarks>
        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                foreach (var outputTracks in GetOutputTracks())
                    foreach (var output in outputTracks.outputs)
                        yield return output;
            }
        }

        /// <summary>
        /// The capabilities supported by all clips in the timeline.
        /// </summary>
        public ClipCaps clipCaps
        {
            get
            {
                var caps = ClipCaps.All;
                foreach (var track in GetRootTracks())
                {//Ques：这里从RootTrack取Clip，但这不是遗漏了在GroupTrack内部的普通Track的Clip吗？
                    foreach (var clip in track.clips)
                        caps &= clip.clipCaps;
                }
                return caps;
            }
        }

        /// <summary>
        /// Returns the the number of output tracks in the Timeline.
        /// </summary>
        /// <remarks>
        /// An output track is a track the generates a PlayableOutput. In general, an output track is any track that is not a GroupTrack, a subtrack, or override track.
        /// </remarks>
        public int outputTrackCount
        {
            get
            {
                UpdateOutputTrackCache(); // updates the cache if necessary
                return m_CacheOutputTracks.Length;
            }
        }

        /// <summary>
        /// Returns the number of tracks at the root level of the timeline.
        /// </summary>
        /// <remarks>
        /// A root track refers to all tracks that occur at the root of the timeline. These are the outmost level GroupTracks, and output tracks that do not belong to any group
        /// </remarks>
        public int rootTrackCount
        {
            get
            {
                UpdateRootTrackCache();
                return m_CacheRootTracks.Count;
            }
        }

        void OnValidate()
        {
            editorSettings.frameRate = GetValidFrameRate(editorSettings.frameRate);
        }

        /// <summary>
        /// Retrieves at root track at the specified index.
        /// </summary>
        /// <param name="index">Index of the root track to get. Must be between 0 and rootTrackCount</param>
        /// <remarks>
        /// A root track refers to all tracks that occur at the root of the timeline. These are the outmost level GroupTracks, and output tracks that do not belong to any group.
        /// </remarks>
        /// <returns>Root track at the specified index.</returns>
        public TrackAsset GetRootTrack(int index)
        {
            UpdateRootTrackCache();
            return m_CacheRootTracks[index];
        }

        /// <summary>
        /// Get an enumerable list of all root tracks.
        /// </summary>
        /// <returns>An IEnumerable of all root tracks.</returns>
        /// <remarks>A root track refers to all tracks that occur at the root of the timeline. These are the outmost level GroupTracks, and output tracks that do not belong to any group.</remarks>
        public IEnumerable<TrackAsset> GetRootTracks()
        {
            UpdateRootTrackCache();
            return m_CacheRootTracks;
        }

        /// <summary>
        /// Retrives the output track from the given index.
        /// </summary>
        /// <param name="index">Index of the output track to retrieve. Must be between 0 and outputTrackCount</param>
        /// <returns>The output track from the given index</returns>
        public TrackAsset GetOutputTrack(int index)
        {
            UpdateOutputTrackCache();
            return m_CacheOutputTracks[index];
        }

        /// <summary>
        /// Gets a list of all output tracks in the Timeline.
        /// </summary>
        /// <returns>An IEnumerable of all output tracks</returns>
        /// <remarks>
        /// An output track is a track the generates a PlayableOutput. In general, an output track is any track that is not a GroupTrack or subtrack.
        /// </remarks>
        public IEnumerable<TrackAsset> GetOutputTracks()
        {
            UpdateOutputTrackCache();
            return m_CacheOutputTracks;
        }

        static double GetValidFrameRate(double frameRate)
        {//Max就是定最小值，Min就是定最大值
            return Math.Min(Math.Max(frameRate, EditorSettings.kMinFrameRate), EditorSettings.kMaxFrameRate);
        }

        void UpdateRootTrackCache()
        {
            if (m_CacheRootTracks == null)
            {
                if (m_Tracks == null)
                    m_CacheRootTracks = new List<TrackAsset>();
                else
                {
                    m_CacheRootTracks = new List<TrackAsset>(m_Tracks.Count);
                    if (markerTrack != null)
                    {//marker单独处理，没有包含在m_Tracks中，但是包含在返回的m_CacheRootTracks中
                        m_CacheRootTracks.Add(markerTrack);
                    }

                    foreach (var t in m_Tracks)
                    {//因为m_Tracks就是用来存储RootTrack的。
                        var trackAsset = t as TrackAsset;
                        if (trackAsset != null)
                            m_CacheRootTracks.Add(trackAsset);
                    }
                }
            }
        }

        //更新作为输出节点的各个轨道的原始资产TrackAsset实例。
        void UpdateOutputTrackCache()
        {
            if (m_CacheOutputTracks == null)
            {
                var outputTracks = new List<TrackAsset>();
                foreach (var flattenedTrack in flattenedTracks)
                {//确定是普通Track，才会生成对应的PlayableOutput
                    if (flattenedTrack != null && flattenedTrack.GetType() != typeof(GroupTrack) && !flattenedTrack.isSubTrack)
                        outputTracks.Add(flattenedTrack); 
                }
                //List操作方便，Array计算快捷。
                m_CacheOutputTracks = outputTracks.ToArray();
            }
        }

        /// <summary>
        /// 这里是当前TimelineAsset中所有的普通Track，即除了GroupTrack以外的Track。
        /// </summary>
        internal TrackAsset[] flattenedTracks
        {
            get
            {
                if (m_CacheFlattenedTracks == null)
                {
                    //提前准备2倍的容量，当然也可能不够用，那么就动态扩容，总之预设一定容量都是为了避免或减少动态扩容造成的性能影响
                    var list = new List<TrackAsset>(m_Tracks.Count * 2); 
                    UpdateRootTrackCache();

                    list.AddRange(m_CacheRootTracks);
                    for (int i = 0; i < m_CacheRootTracks.Count; i++)
                    {//Tip：从RootTrack入手，递归添加普通Track，就是因为有GroupTrack的存在。
                        AddSubTracksRecursive(m_CacheRootTracks[i], ref list);
                    }

                    m_CacheFlattenedTracks = list.ToArray();
                }
                return m_CacheFlattenedTracks;
            }
        }

        /// <summary>
        /// Gets the marker track for this TimelineAsset.
        /// </summary>
        /// <returns>Returns the marker track.</returns>
        /// <remarks>
        /// Use <see cref="TrackAsset.GetMarkers"/> to get a list of the markers on the returned track.
        /// </remarks>
        public MarkerTrack markerTrack
        {
            get { return m_MarkerTrack; }
        }

        // access to the track list as scriptable object
        //TODO：感觉命名很模糊，而且m_Tracks本来也是指的RootTrack，命名也很不准确。
        internal List<ScriptableObject> trackObjects
        {
            get { return m_Tracks; }
        }

        //直接添加RootTrack
        internal void AddTrackInternal(TrackAsset track)
        {
            m_Tracks.Add(track);
            track.parent = this; //RootTrack。
            Invalidate();
        }


        internal void RemoveTrack(TrackAsset track)
        {
            m_Tracks.Remove(track);
            Invalidate();
            var parentTrack = track.parent as TrackAsset;
            if (parentTrack != null) //通知父Track。
            {
                parentTrack.RemoveSubTrack(track);
            }
        }

        //Tip：CreatePlayable创建TimelinePlayable的起始方法，应该说这是整个Timeline的起点。
        /// <summary>
        /// （根据TimelineAsset创建TimelinePlayable）Creates an instance of the timeline
        /// </summary>
        /// <param name="graph">PlayableGraph that will own the playable</param>
        /// <param name="go">（触发构建Graph的游戏对象，就是PlayableDirector组件所挂载的对象）The gameobject that triggered the graph build</param>
        /// <returns>The Root Playable of the Timeline</returns>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            bool autoRebalanceTree = false; 
#if UNITY_EDITOR
            autoRebalanceTree = true;
#endif

            // only create outputs if we are not nested
            /*graph.GetPlayableCount() == 0 表明这是 PlayableGraph 的根节点（非嵌套调用）。
            仅在根 Graph 才创建输出（PlayableOutput），避免重复嵌套时生成多余的输出端口。*/
            //Tip：这里是为了“嵌套”这样的高级功能，通常大概不会用到该功能。
            bool createOutputs = graph.GetPlayableCount() == 0; //Playable
            //该方法会创建Root即TimelinePlayable，以及各个Track对应的Output节点
            //Tip：创建作为整个Timeline中枢的Playable节点。
            var timeline = TimelinePlayable.Create(graph, GetOutputTracks(), go, autoRebalanceTree, createOutputs);
            /*作用：显式指定根 Playable 的播放时长，覆盖默认的“无限时长”行为。因为这是一个明确的整体时间轴，TimelinePlayable就代表了整个时间轴的时长。
            效果：
                控制 Timeline 播放到末尾时何时触发完成、循环或停止。
                驱动 PlayableGraph.Evaluate() 时，只在 [0, duration] 范围内生效。*/
            //Tip：显式指定Duration，就代表会有Done。这里就是将中枢节点TimelinePlayable的Duration设置为整个时间轴的时长，Graph自动播放，直到Duration也就标志着Timeline播放结束。
            timeline.SetDuration(this.duration);
            /*
            文档说明：开启后，根 Playable 在执行 SetTime() 或者进行时间跳跃（如编辑器拖拽进度）时，会自动将新的本地时间“向下”传播到所有输入节点（轨道 Mixer、Clip Playable 等）。
            为何需要：
                Timeline 在编辑器模式下允许即时预览、精准拖拽。当根时间被修改时，所有子节点必须同步更新当前帧，否则轨道混合和剪辑播放会错位。
                若不启用时间传播，子节点依旧保持旧时间，需要手动调用 Evaluate() 或其它机制才能同步，导致体验不连贯。*/
            //Tip：保证在对TimelinePlayable调用SetTime时（通常是跳转操作），所有作为子节点的轨道也能同步时间。
            timeline.SetPropagateSetTime(true);
            return timeline.IsValid() ? timeline : Playable.Null; //返回中枢节点
        }

        /// <summary>
        /// Called before Unity serializes this object.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Version = k_LatestVersion;
        }

        /// <summary>
        /// Called after Unity deserializes this object.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // resets cache on an Undo
            //在进行一次撤销操作Undo之后，如果某个（序列化）对象在Undo前和Undo后发生了值改变的话，就会触发反序列化
            Invalidate(); // resets cache on an Undo
            if (m_Version < k_LatestVersion)
            {
                UpgradeToLatestVersion();
            }
        }

#if UNITY_EDITOR
        internal event Action AssetModifiedOnDisk;
#endif
        void __internalAwake()
        {
            if (m_Tracks == null)
                m_Tracks = new List<ScriptableObject>();

#if UNITY_EDITOR
            // case 1280331 -- embedding the timeline asset inside a prefab will create a temporary non-persistent version of an asset
            // setting the track parents to this will change persistent tracks
            if (!UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif

            // validate the array. DON'T remove Unity null objects, just actual null objects
            for (int i = m_Tracks.Count - 1; i >= 0; i--)
            {
                TrackAsset asset = m_Tracks[i] as TrackAsset;
                if (asset != null)
                    asset.parent = this;
#if UNITY_EDITOR
                object o = m_Tracks[i];
                if (o == null)
                {
                    Debug.LogWarning("Empty track found while loading timeline. It will be removed.");
                    m_Tracks.RemoveAt(i);
                }
#endif
            }

#if UNITY_EDITOR
            AssetModifiedOnDisk?.Invoke();
#endif
        }

        /// <summary>
        /// Called by the Timeline Editor to gather properties requiring preview.
        /// </summary>
        /// <param name="director">The PlayableDirector invoking the preview</param>
        /// <param name="driver">PropertyCollector used to gather previewable properties</param>
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var outputTracks = GetOutputTracks();
            foreach (var track in outputTracks)
            {
                if (!track.mutedInHierarchy)
                    track.GatherProperties(director, driver);
            }
        }

        /// <summary>
        /// Creates a marker track for the TimelineAsset.
        /// </summary>
        /// In the editor, the marker track appears under the Timeline ruler.
        /// <remarks>
        /// This track is always bound to the GameObject that contains the PlayableDirector component for the current timeline.
        /// The marker track is created the first time this method is called. If the marker track is already created, this method does nothing.
        /// </remarks>
        public void CreateMarkerTrack()
        {
            if (m_MarkerTrack == null)
            {
                m_MarkerTrack = CreateInstance<MarkerTrack>();
                TimelineCreateUtilities.SaveAssetIntoObject(m_MarkerTrack, this);
                m_MarkerTrack.parent = this;
                m_MarkerTrack.name = "Markers"; // This name will show up in the bindings list if it contains signals
                Invalidate();
            }
        }

        // Invalidates the asset, call this if changing the asset data
        /*Tip：将缓存的数据失效，以便随后再次访问对应的属性时就会重新读取数据，也就是读取最新数据。
        因为编辑器是基于序列化系统的，而只有字段能够进行序列化，所以在编辑窗口中进行的编辑操作都会直接应用到运行时实例的字段值上，
        而在运行时代码中通常都是访问属性而不是直接访问字段，因为需要经过属性的getter或setter处理，为了保证运行时代码中访问到的
        是最新数据，同时又因为使用了缓存的技巧，所以才有了这样一个Invalidate方法*/
        internal void Invalidate()
        {
            m_CacheRootTracks = null;
            m_CacheOutputTracks = null;
            m_CacheFlattenedTracks = null;
        }

        internal void UpdateFixedDurationWithItemsDuration()
        {
            m_FixedDuration = (double)CalculateItemsDuration();
        }

        DiscreteTime CalculateItemsDuration()
        {
            var discreteDuration = new DiscreteTime(0);
            foreach (var track in flattenedTracks)
            {
                if (track.muted)//跳过静音（muted）轨道
                    continue;

                discreteDuration = DiscreteTime.Max(discreteDuration, (DiscreteTime)track.end);
            }

            if (discreteDuration <= 0)
                return new DiscreteTime(0);

            return discreteDuration;
        }

        static void AddSubTracksRecursive(TrackAsset track, ref List<TrackAsset> allTracks)
        {
            if (track == null)
                return;

            allTracks.AddRange(track.GetChildTracks());
            foreach (TrackAsset subTrack in track.GetChildTracks())
            {
                AddSubTracksRecursive(subTrack, ref allTracks);
            }
        }
    }
}
