using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /*Tip：TrackAsset就是运行时轨道的资产类，*/

    /// <summary>
    /// A PlayableAsset representing a track inside a timeline.
    /// </summary>
    ///
    /// <remarks>
    /// Derive from TrackAsset to implement custom timeline tracks. TrackAsset derived classes support the following attributes:
    /// <seealso cref="UnityEngine.Timeline.HideInMenuAttribute"/>
    /// <seealso cref="UnityEngine.Timeline.TrackColorAttribute"/>
    /// <seealso cref="UnityEngine.Timeline.TrackClipTypeAttribute"/>
    /// <seealso cref="UnityEngine.Timeline.TrackBindingTypeAttribute"/>
    /// <seealso cref="System.ComponentModel.DisplayNameAttribute"/>
    /// </remarks>
    ///
    /// <example>
    /// <code source="../../DocCodeExamples/TrackAssetExamples.cs" region="declare-trackAssetExample" title="TrackAssetExample"/>
    /// </example>
    [Serializable]
    [IgnoreOnPlayableTrack]
    public abstract partial class TrackAsset : PlayableAsset, IPropertyPreview, ICurvesOwner
    { //注意Track包含普通Track以及Group Track，但是实际使用的只有普通Track，因为GroupTrack本质上是一个编辑器属性。
        // Internal caches used to avoid memory allocation during graph construction
        private struct TransientBuildData
        {
            public List<TrackAsset> trackList;
            public List<TimelineClip> clipList;
            public List<IMarker> markerList;

            public static TransientBuildData Create()
            {
                return new TransientBuildData()
                {
                    trackList = new List<TrackAsset>(20),
                    clipList = new List<TimelineClip>(500),
                    markerList = new List<IMarker>(100),
                };
            }

            public void Clear()
            {
                trackList.Clear();
                clipList.Clear();
                markerList.Clear();
            }
        }

        //默认的临时构建数据
        private static TransientBuildData s_BuildData = TransientBuildData.Create();

        internal const string kDefaultCurvesName = "Track Parameters";

        internal static event Action<TimelineClip, GameObject, Playable> OnClipPlayableCreate;
        internal static event Action<TrackAsset, GameObject, Playable> OnTrackAnimationPlayableCreate;

        [SerializeField, HideInInspector] bool m_Locked; //Tip：用于编辑时单独锁定轨道
        //Ques：禁用可以在运行时和编辑时使用，但大概通常只会在编辑时方便调试，不过如果是用作技能编辑器的话，似乎会有开关特效之类的需求？
        [SerializeField, HideInInspector] bool m_Muted; 
        [SerializeField, HideInInspector] string m_CustomPlayableFullTypename = string.Empty;
        [SerializeField, HideInInspector] AnimationClip m_Curves;
        [SerializeField, HideInInspector] PlayableAsset m_Parent;
        [SerializeField, HideInInspector] List<ScriptableObject> m_Children;

        [NonSerialized] int m_ItemsHash;
        [NonSerialized] TimelineClip[] m_ClipsCache;

        DiscreteTime m_Start;
        DiscreteTime m_End;
        bool m_CacheSorted;
        bool? m_SupportsNotifications;

        static TrackAsset[] s_EmptyCache = new TrackAsset[0];
        IEnumerable<TrackAsset> m_ChildTrackCache;

        static Dictionary<Type, TrackBindingTypeAttribute> s_TrackBindingTypeAttributeCache = new Dictionary<Type, TrackBindingTypeAttribute>();

        [SerializeField, HideInInspector] protected internal List<TimelineClip> m_Clips = new List<TimelineClip>();

        [SerializeField, HideInInspector] MarkerList m_Markers = new MarkerList(0);

#if UNITY_EDITOR
        internal int DirtyIndex { get; private set; }
        internal void MarkDirtyTrackAndClips()
        {
            DirtyIndex++;
            foreach (var clip in GetClips())
            {
                if (clip != null)
                    clip.MarkDirty();
            }
        }
#endif

        /// <summary>
        /// The start time, in seconds, of this track
        /// </summary>
        public double start
        {
            get
            {
                UpdateDuration();
                return (double)m_Start;
            }
        }

        /// <summary>
        /// The end time, in seconds, of this track
        /// </summary>
        public double end
        {
            get
            {
                UpdateDuration();
                return (double)m_End;
            }
        }

        /// <summary>
        /// The length, in seconds, of this track
        /// </summary>
        public sealed override double duration
        {
            get
            {
                UpdateDuration();
                return (double)(m_End - m_Start);
            }
        }

        /// <summary>
        /// Whether the track is muted or not.
        /// </summary>
        /// <remarks>
        /// A muted track is excluded from the generated PlayableGraph
        /// </remarks>
        public bool muted
        {
            get { return m_Muted; }
            set { m_Muted = value; }
        }

        /// <summary>
        /// The muted state of a track.
        /// </summary>
        /// <remarks>
        /// A track is also muted when one of its parent tracks are muted.
        /// </remarks>
        public bool mutedInHierarchy
        {
            get
            {
                if (muted)
                    return true;

                TrackAsset p = this;
                while (p.parent as TrackAsset != null)
                {
                    p = (TrackAsset)p.parent;
                    if (p as GroupTrack != null)
                        return p.mutedInHierarchy;
                }

                return false;
            }
        }

        /// <summary>
        /// The TimelineAsset that this track belongs to.
        /// </summary>
        public TimelineAsset timelineAsset
        {
            get
            {//因为有父轨道的存在，所以在此需要向上查找。
                var node = this;
                while (node != null)
                {
                    if (node.parent == null)
                        return null;

                    var seq = node.parent as TimelineAsset;
                    if (seq != null)
                        return seq;

                    node = node.parent as TrackAsset;
                }
                return null;
            }
        }

        /// <summary>
        /// The owner of this track.
        /// </summary>
        /// <remarks>
        /// If this track is a subtrack, the parent is a TrackAsset. Otherwise the parent is a TimelineAsset.
        /// 要么是GroupTrack，要么是TimelineAsset
        /// </remarks>
        public PlayableAsset parent
        {
            get { return m_Parent; }
            internal set { m_Parent = value; }
        }

        /// <summary>
        /// A list of clips owned by this track
        /// </summary>
        /// <returns>Returns an enumerable list of clips owned by the track.</returns>
        public IEnumerable<TimelineClip> GetClips()
        {
            return clips;
        }

        internal TimelineClip[] clips
        {
            get
            {
                if (m_Clips == null)
                    m_Clips = new List<TimelineClip>();

                if (m_ClipsCache == null)
                {
                    m_CacheSorted = false;
                    m_ClipsCache = m_Clips.ToArray();
                }

                return m_ClipsCache;
            }
        }

        /// <summary>
        /// Whether this track is considered empty.
        /// </summary>
        /// <remarks>
        /// A track is considered empty when it does not contain a TimelineClip, Marker, or Curve.
        /// </remarks>
        public virtual bool isEmpty
        {
            get { return !hasClips && !hasCurves && GetMarkerCount() == 0; }
        }

        /// <summary>
        /// Whether this track contains any TimelineClip.
        /// </summary>
        public bool hasClips
        {
            get { return m_Clips != null && m_Clips.Count != 0; }
        }

        /// <summary>
        /// Whether this track contains animated properties for the attached PlayableAsset.
        /// </summary>
        /// <remarks>
        /// This property is false if the curves property is null or if it contains no information.
        /// </remarks>
        public bool hasCurves
        {
            get { return m_Curves != null && !m_Curves.empty; }
        }

        /// <summary>
        /// Returns whether this track is a subtrack
        /// </summary>
        public bool isSubTrack
        {
            get
            {
                //这里不为空的实际含义是有parent，
                var owner = parent as TrackAsset;
                return owner != null && owner.GetType() == GetType();
            }
        }


        /*Tip：其实这是个非常关键的属性，*/
        /// <summary>
        /// Returns a description of the PlayableOutputs that will be created by this track.
        /// </summary>
        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                TrackBindingTypeAttribute attribute;
                //反射获取当前类型上标记的所有TrackBindingTypeAttribute，也就是该轨道可以绑定的对象类型。
                if (!s_TrackBindingTypeAttributeCache.TryGetValue(GetType(), out attribute))
                {
                    attribute = (TrackBindingTypeAttribute)Attribute.GetCustomAttribute(GetType(), typeof(TrackBindingTypeAttribute));
                    s_TrackBindingTypeAttributeCache.Add(GetType(), attribute);
                }
                //默认就是自定义轨道，所以ScriptPlayableBinding，而特殊的如Animation和Audio，就有自己特定的AnimationPlayableBinding和AudioPlayableBinding
                var trackBindingType = attribute != null ? attribute.type : null;
                ///注意，<see cref="TimelinePlayable.CreateTrackOutput"/>，由此会发现这里的作为参数的this就是SetReferenceObject的对象。
                yield return ScriptPlayableBinding.Create(name, this, trackBindingType);
            }
        }

        /// <summary>
        /// The list of subtracks or child tracks attached to this track.
        /// </summary>
        /// <returns>Returns an enumerable list of child tracks owned directly by this track.</returns>
        /// <remarks>
        /// In the case of GroupTracks, this returns all tracks contained in the group. This will return the all subtracks or override tracks, if supported by the track.
        /// </remarks>
        public IEnumerable<TrackAsset> GetChildTracks()
        {
            UpdateChildTrackCache();
            return m_ChildTrackCache;
        }

        internal string customPlayableTypename
        {
            get { return m_CustomPlayableFullTypename; }
            set { m_CustomPlayableFullTypename = value; }
        }

        /// <summary>
        /// An animation clip storing animated properties of the attached PlayableAsset
        /// </summary>
        public AnimationClip curves
        {
            get { return m_Curves; }
            internal set { m_Curves = value; }
        }

        string ICurvesOwner.defaultCurvesName
        {
            get { return kDefaultCurvesName; }
        }

        Object ICurvesOwner.asset
        {
            get { return this; }
        }

        Object ICurvesOwner.assetOwner
        {
            get { return timelineAsset; }
        }

        TrackAsset ICurvesOwner.targetTrack
        {
            get { return this; }
        }

        // for UI where we need to detect 'null' objects
        internal List<ScriptableObject> subTracksObjects
        {
            get { return m_Children; }
        }

        /// <summary>
        /// The local locked state of the track.
        /// </summary>
        /// <remarks>
        /// Note that locking a track only affects operations in the Timeline Editor. It does not prevent other API calls from changing a track or it's clips.
        ///
        /// This returns or sets the local locked state of the track. A track may still be locked for editing because one or more of it's parent tracks in the hierarchy is locked. Use lockedInHierarchy to test if a track is locked because of it's own locked state or because of a parent tracks locked state.
        /// </remarks>
        public bool locked
        {
            get { return m_Locked; }
            set { m_Locked = value; }
        }

        /// <summary>
        /// The locked state of a track. (RO)
        /// </summary>
        /// <remarks>
        /// Note that locking a track only affects operations in the Timeline Editor. It does not prevent other API calls from changing a track or it's clips.
        ///
        /// This indicates whether a track is locked in the Timeline Editor because either it's locked property is enabled or a parent track is locked.
        /// </remarks>
        public bool lockedInHierarchy
        {
            get
            {
                if (locked)
                    return true;

                TrackAsset p = this;
                while (p.parent as TrackAsset != null)
                {
                    p = (TrackAsset)p.parent;
                    if (p as GroupTrack != null)
                        return p.lockedInHierarchy;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if a track accepts markers that implement <see cref="UnityEngine.Playables.INotification"/>.
        /// 这里就是只有使用[TrackBindingType]指定绑定对象为Component或GameObject类型，才支持通知。
        /// </summary>
        /// <remarks>
        /// Only tracks with a bound object of type <see cref="UnityEngine.GameObject"/> or <see cref="UnityEngine.Component"/> can accept notifications.
        /// </remarks>
        public bool supportsNotifications
        {
            get
            {
                if (!m_SupportsNotifications.HasValue)
                {
                    m_SupportsNotifications = NotificationUtilities.TrackTypeSupportsNotifications(GetType());
                }

                return m_SupportsNotifications.Value;
            }
        }

        void __internalAwake() //do not use OnEnable, since users will want it to initialize their class
        {
            if (m_Clips == null)
                m_Clips = new List<TimelineClip>();

            m_ChildTrackCache = null;
            if (m_Children == null)
                m_Children = new List<ScriptableObject>();
#if UNITY_EDITOR
            // validate the array. DON'T remove Unity null objects, just actual null objects
            for (int i = m_Children.Count - 1; i >= 0; i--)
            {
                object o = m_Children[i];
                if (o == null)
                {
                    Debug.LogWarning("Empty child track found while loading timeline. It will be removed.");
                    m_Children.RemoveAt(i);
                }
            }
#endif
        }

        /// <summary>
        /// Creates an AnimationClip to store animated properties for the attached PlayableAsset.
        /// </summary>
        /// <remarks>
        /// If curves already exists for this track, this method produces no result regardless of
        /// the value specified for curvesClipName.
        /// </remarks>
        /// <remarks>
        /// When used from the editor, this method attempts to save the created curves clip to the TimelineAsset.
        /// The TimelineAsset must already exist in the AssetDatabase to save the curves clip. If the TimelineAsset
        /// does not exist, the curves clip is still created but it is not saved.
        /// </remarks>
        /// <param name="curvesClipName">
        /// The name of the AnimationClip to create.
        /// This method does not ensure unique names. If you want a unique clip name, you must provide one.
        /// See ObjectNames.GetUniqueName for information on a method that creates unique names.
        /// </param>
        public void CreateCurves(string curvesClipName)
        {
            if (m_Curves != null)
                return;

            m_Curves = TimelineCreateUtilities.CreateAnimationClipForTrack(string.IsNullOrEmpty(curvesClipName) ? kDefaultCurvesName : curvesClipName, this, true);
        }


        /*Tip：每个轨道都应该重写该方法CreateTrackMixer，决定自己的运行时节点，用以混合轨道上的各个片段的输出。*/
        /// <summary>
        /// Creates a mixer used to blend playables generated by clips on the track.
        /// 用于创建轨道的Playable，（返回的Playable）直接连接到作为中枢的TimelinePlayable，并且创建多个输入端口，每一个端口就是连接一个在该轨道上的Clip所对应的Playable。
        /// 其实就类似于动画系统中的代表层级或混合树的AnimationMixerPlayable节点和代表动画片段的AnimationClipPlayable。
        /// </summary> 
        /// <param name="graph">The graph to inject playables into</param>
        /// <param name="go">The GameObject that requested the graph.</param>
        /// <param name="inputCount">The number of playables from clips that will be inputs to the returned mixer</param>
        /// <returns>A handle to the [[Playable]] representing the mixer.</returns>
        /// <remarks>
        /// Override this method to provide a custom playable for mixing clips on a graph.
        /// </remarks>
        public virtual Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return Playable.Create(graph, inputCount);
        }

        /// <summary>
        /// Overrides PlayableAsset.CreatePlayable(). Not used in Timeline.
        /// </summary>
        /// <param name="graph"><inheritdoc/></param>
        /// <param name="go"><inheritdoc/></param>
        /// <returns><inheritDoc/></returns>
        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            return Playable.Null;
        }

        /// <summary>
        /// Creates a TimelineClip on this track.
        /// </summary>
        /// <returns>Returns a new TimelineClip that is attached to the track.</returns>
        /// <remarks>
        /// The type of the playable asset attached to the clip is determined by TrackClip attributes that decorate the TrackAsset derived class
        /// </remarks>
        public TimelineClip CreateDefaultClip()
        {
            var trackClipTypeAttributes = GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
            Type playableAssetType = null;
            foreach (var trackClipTypeAttribute in trackClipTypeAttributes)
            {
                var attribute = trackClipTypeAttribute as TrackClipTypeAttribute;
                if (attribute != null && typeof(IPlayableAsset).IsAssignableFrom(attribute.inspectedType) && typeof(ScriptableObject).IsAssignableFrom(attribute.inspectedType))
                {
                    playableAssetType = attribute.inspectedType;
                    break;
                }
            }

            if (playableAssetType == null)
            {
                Debug.LogWarning("Cannot create a default clip for type " + GetType());
                return null;
            }
            return CreateAndAddNewClipOfType(playableAssetType);
        }

        /// <summary>
        /// Creates a clip on the track with a playable asset attached, whose derived type is specified by T
        /// </summary>
        /// <typeparam name="T">A PlayableAsset derived type</typeparam>
        /// <returns>Returns a TimelineClip whose asset is of type T</returns>
        /// <remarks>
        /// Throws <exception cref="System.InvalidOperationException"/> if <typeparamref name="T"/> is not supported by the track.
        /// Supported types are determined by TrackClip attributes that decorate the TrackAsset derived class
        /// </remarks>
        public TimelineClip CreateClip<T>() where T : ScriptableObject, IPlayableAsset
        {
            return CreateClip(typeof(T));
        }

        /// <summary>
        /// Delete a clip from this track.
        /// </summary>
        /// <param name="clip">The clip to delete.</param>
        /// <returns>Returns true if the removal was successful</returns>
        /// <remarks>
        /// This method will delete a clip and any assets owned by the clip.
        /// </remarks>
        /// <exception>
        /// Throws <exception cref="System.InvalidOperationException"/> if <paramref name="clip"/> is not a child of the TrackAsset.
        /// </exception>
        public bool DeleteClip(TimelineClip clip)
        {
            if (!m_Clips.Contains(clip))
                throw new InvalidOperationException("Cannot delete clip since it is not a child of the TrackAsset.");

            return timelineAsset != null && timelineAsset.DeleteClip(clip);
        }

        /// <summary>
        /// Creates a marker of the requested type, at a specific time, and adds the marker to the current asset.
        /// </summary>
        /// <param name="type">The type of marker.</param>
        /// <param name="time">The time where the marker is created.</param>
        /// <returns>Returns the instance of the created marker.</returns>
        /// <remarks>
        /// All markers that implement IMarker and inherit from <see cref="UnityEngine.ScriptableObject"/> are supported.
        /// Markers that implement the INotification interface cannot be added to tracks that do not support notifications.
        /// CreateMarker will throw <exception cref="System.InvalidOperationException"/> with tracks that do not support notifications if <paramref name="type"/> implements the INotification interface.
        /// </remarks>
        /// <seealso cref="UnityEngine.Timeline.Marker"/>
        /// <seealso cref="UnityEngine.Timeline.TrackAsset.supportsNotifications"/>
        public IMarker CreateMarker(Type type, double time)
        {
            return m_Markers.CreateMarker(type, time, this);
        }

        /// <summary>
        /// Creates a marker of the requested type, at a specific time, and adds the marker to the current asset.
        /// </summary>
        /// <param name="time">The time where the marker is created.</param>
        /// <typeparam name="T">The type of marker to create.</typeparam>
        /// <returns>Returns the instance of the created marker.</returns>
        /// <remarks>
        /// All markers that implement IMarker and inherit from <see cref="UnityEngine.ScriptableObject"/> are supported.
        /// CreateMarker will throw <exception cref="System.InvalidOperationException"/> with tracks that do not support notifications if <typeparamref name="T"/> implements the INotification interface.
        /// </remarks>
        /// <seealso cref="UnityEngine.Timeline.Marker"/>
        /// <seealso cref="UnityEngine.Timeline.TrackAsset.supportsNotifications"/>
        public T CreateMarker<T>(double time) where T : ScriptableObject, IMarker
        {
            return (T)CreateMarker(typeof(T), time);
        }

        /// <summary>
        /// Removes a marker from the current asset.
        /// </summary>
        /// <param name="marker">The marker instance to be removed.</param>
        /// <returns>Returns true if the marker instance was successfully removed. Returns false otherwise.</returns>
        public bool DeleteMarker(IMarker marker)
        {
            return m_Markers.Remove(marker);
        }

        /// <summary>
        /// Returns an enumerable list of markers on the current asset.
        /// </summary>
        /// <returns>The list of markers on the asset.
        /// </returns>
        public IEnumerable<IMarker> GetMarkers()
        {
            return m_Markers.GetMarkers();
        }

        /// <summary>
        /// Returns the number of markers on the current asset.
        /// </summary>
        /// <returns>The number of markers.</returns>
        public int GetMarkerCount()
        {
            return m_Markers.Count;
        }

        /// <summary>
        /// Returns the marker at a given position, on the current asset.
        /// </summary>
        /// <param name="idx">The index of the marker to be returned.</param>
        /// <returns>The marker.</returns>
        /// <remarks>The ordering of the markers is not guaranteed.
        /// </remarks>
        public IMarker GetMarker(int idx)
        {
            return m_Markers[idx];
        }

        internal TimelineClip CreateClip(System.Type requestedType)
        {
            if (ValidateClipType(requestedType))
                return CreateAndAddNewClipOfType(requestedType);

            throw new InvalidOperationException("Clips of type " + requestedType + " are not permitted on tracks of type " + GetType());
        }

        internal TimelineClip CreateAndAddNewClipOfType(Type requestedType)
        {
            var newClip = CreateClipOfType(requestedType);
            AddClip(newClip);
            return newClip;
        }

        internal TimelineClip CreateClipOfType(Type requestedType)
        {
            if (!ValidateClipType(requestedType))
                throw new System.InvalidOperationException("Clips of type " + requestedType + " are not permitted on tracks of type " + GetType());

            var playableAsset = CreateInstance(requestedType);
            if (playableAsset == null)
            {
                throw new System.InvalidOperationException("Could not create an instance of the ScriptableObject type " + requestedType.Name);
            }
            playableAsset.name = requestedType.Name;
            //添加为该轨道资产的子资产。
            TimelineCreateUtilities.SaveAssetIntoObject(playableAsset, this);
            TimelineUndo.RegisterCreatedObjectUndo(playableAsset, "Create Clip");

            return CreateClipFromAsset(playableAsset);
        }

        /// <summary>
        /// Creates a timeline clip from an existing playable asset.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        internal TimelineClip CreateClipFromPlayableAsset(IPlayableAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset");

            if ((asset as ScriptableObject) == null)
                throw new System.ArgumentException("CreateClipFromPlayableAsset " + " only supports ScriptableObject-derived Types");

            if (!ValidateClipType(asset.GetType()))
                throw new System.InvalidOperationException("Clips of type " + asset.GetType() + " are not permitted on tracks of type " + GetType());

            return CreateClipFromAsset(asset as ScriptableObject);
        }

        private TimelineClip CreateClipFromAsset(ScriptableObject playableAsset)
        {
            TimelineUndo.PushUndo(this, "Create Clip");

            var newClip = CreateNewClipContainerInternal();
            newClip.displayName = playableAsset.name;
            newClip.asset = playableAsset;

            IPlayableAsset iPlayableAsset = playableAsset as IPlayableAsset;
            if (iPlayableAsset != null)
            {
                var candidateDuration = iPlayableAsset.duration;

                if (!double.IsInfinity(candidateDuration) && candidateDuration > 0)
                    newClip.duration = Math.Min(Math.Max(candidateDuration, TimelineClip.kMinDuration), TimelineClip.kMaxTimeValue);
            }

            try
            {
                OnCreateClip(newClip);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message, playableAsset);
                return null;
            }

            return newClip;
        }

        internal IEnumerable<ScriptableObject> GetMarkersRaw()
        {
            return m_Markers.GetRawMarkerList();
        }

        internal void ClearMarkers()
        {
            m_Markers.Clear();
        }

        internal void AddMarker(ScriptableObject e)
        {
            m_Markers.Add(e);
        }

        internal bool DeleteMarkerRaw(ScriptableObject marker)
        {
            return m_Markers.Remove(marker, timelineAsset, this);
        }

        int GetTimeRangeHash()
        {
            double start = double.MaxValue, end = double.MinValue;
            int count = m_Markers.Count;
            for (int i = 0; i < m_Markers.Count; i++)
            {
                var marker = m_Markers[i];
                if (!(marker is INotification))
                {
                    continue;
                }

                if (marker.time < start)
                    start = marker.time;
                if (marker.time > end)
                    end = marker.time;
            }

            return start.GetHashCode().CombineHash(end.GetHashCode());
        }

        internal void AddClip(TimelineClip newClip)
        {
            if (!m_Clips.Contains(newClip))
            {
                m_Clips.Add(newClip);
                m_ClipsCache = null;
            }
        }

        Playable CreateNotificationsPlayable(PlayableGraph graph, Playable mixerPlayable, GameObject go, Playable timelinePlayable)
        {
            //收集轨道上所有 Marker/Notification
            s_BuildData.markerList.Clear();
            GatherNotifications(s_BuildData.markerList);

            //创建 TimeNotificationBehaviour 负责调度通知
            ScriptPlayable<TimeNotificationBehaviour> notificationPlayable;
            //Ques：不理解这里绑定的游戏对象还可以没有PlayableDirector组件吗？
            if (go.TryGetComponent(out PlayableDirector director))
                notificationPlayable = NotificationUtilities.CreateNotificationsPlayable(graph, s_BuildData.markerList, director);
            else
                notificationPlayable = NotificationUtilities.CreateNotificationsPlayable(graph, s_BuildData.markerList, timelineAsset);

            if (notificationPlayable.IsValid())
            {
                //指定使用根 TimelinePlayable 的时间作为通知参考，也就是采用timeSource的duration和wrapMode
                notificationPlayable.GetBehaviour().timeSource = timelinePlayable;
                //若存在 mixer，则挂载为通知节点的唯一输入。有可能一条轨道上只有通知，而没有设置任何片段，那么就只是一个通知节点和一个Output节点。
                if (mixerPlayable.IsValid())
                {
                    notificationPlayable.SetInputCount(1);
                    graph.Connect(mixerPlayable, 0, notificationPlayable, 0);
                    notificationPlayable.SetInputWeight(mixerPlayable, 1);
                }
            }

            return notificationPlayable;
        }

        internal Playable CreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, Playable timelinePlayable)
        {
            UpdateDuration();

            //mixerPlayable就是直接连接到中枢节点的节点。
            var mixerPlayable = Playable.Null;

            if (CanCreateMixerRecursive())
                mixerPlayable = CreateMixerPlayableGraph(graph, go, tree);
            /*Tip：为当前轨道的所有 Marker/Signal 注册时点通知，将上一步的 mixerPlayable 作为输入，包装在一个 ScriptPlayable<TimeNotificationBehaviour> 中
            如果轨道不含通知，则返回 Playable.Null*/
            Playable notificationsPlayable = CreateNotificationsPlayable(graph, mixerPlayable, go, timelinePlayable);

            // clear the temporary build data to avoid holding references
            // case 1253974
            s_BuildData.Clear();
            if (!notificationsPlayable.IsValid() && !mixerPlayable.IsValid())
            {
                Debug.LogErrorFormat("Track {0} of type {1} has no notifications and returns an invalid mixer Playable", name,
                    GetType().FullName);

                return Playable.Create(graph);
            }
            //有通知就是通知节点与中枢节点相连，否则就是混合节点直接连接到中枢节点。
            return notificationsPlayable.IsValid() ? notificationsPlayable : mixerPlayable;
        }

        //Tip：生成轨道节点以及各个片段的节点，并且将各个片段节点连接到轨道节点上的对应端口上（还没有将轨道节点连接到中枢节点上）。
        internal virtual Playable CompileClips(PlayableGraph graph, GameObject go, IList<TimelineClip> timelineClips, IntervalTree<RuntimeElement> tree)
        {
            //Tip：生成轨道节点。这里的CreateTrackMixer方法就是供派生类重写的，用以生成自己的轨道节点。
            var blend = CreateTrackMixer(graph, go, timelineClips.Count);
            for (var c = 0; c < timelineClips.Count; c++)
            {
                //Tip：注意这里传入给RuntimeClip的Playable是该轨道所使用的片段的PlayableAsset的CreatePlayable方法所返回的Playble，以及轨道自己重写的CreateTrackMixer返回的Playable。
                var source = CreatePlayable(graph, go, timelineClips[c]);
                if (source.IsValid())
                {
                    //显示设置Duration，启用自动Done机制。
                    source.SetDuration(timelineClips[c].duration);
                    var clip = new RuntimeClip(timelineClips[c], source, blend);
                    //Tip：将片段资产的CreatePlayable和轨道资产的CreateTrackMixer返回的节点加入到区间树。这里其实很重要，因为在TimelinePlayable的运行过程中（PrepareFrame）就是以区间树来进行遍历处理的。
                    tree.Add(clip); 
                    graph.Connect(source, 0, blend, c); //片段节点连接到轨道节点。
                    blend.SetInputWeight(c, 0.0f); //初始都是0
                }
            }
            ConfigureTrackAnimation(tree, go, blend);
            return blend;
        }

        void GatherCompilableTracks(IList<TrackAsset> tracks)
        {
            if (!muted && CanCreateTrackMixer())
                tracks.Add(this);

            foreach (var c in GetChildTracks())
            {
                if (c != null)
                    c.GatherCompilableTracks(tracks);
            }
        }

        void GatherNotifications(List<IMarker> markers)
        {
            if (!muted && CanCompileNotifications())
                markers.AddRange(GetMarkers());
            foreach (var c in GetChildTracks())
            {
                if (c != null)
                    c.GatherNotifications(markers);
            }
        }

        internal virtual Playable CreateMixerPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            if (tree == null)
                throw new ArgumentException("IntervalTree argument cannot be null", "tree");

            if (go == null)
                throw new ArgumentException("GameObject argument cannot be null", "go");

            /*Tip：这个s_BuildData在这里使用之前会清空，其实就是为每一个主轨道使用，因为可能会有子轨道，所以对于任意一个主轨道都统一采用这种容器存储，以便应对多轨道的情况。*/
            //收集所有可编译的轨道，也就是要真正生成运行时节点的轨道资产TrackAsset，包括自己以及子轨道。
            //其实就是因为存在子轨道的可能，所以才有这样的逻辑，否则就是直接对当前轨道做些判断就行了。
            s_BuildData.Clear();
            GatherCompilableTracks(s_BuildData.trackList);

            // nothing to compile
            if (s_BuildData.trackList.Count == 0)
                return Playable.Null;

            // check if layers are supported
            //TrackAsset派生类，实现ILayerable接口就可以支持层级，AnimationTrack就是如此，但其实AnimationTrack重写了该方法，所以该方法本身还是支持更广泛的层级混合功能。
            Playable layerMixer = Playable.Null;
            ILayerable layerable = this as ILayerable;
            //传入轨道数量，因为从设计上这个就是用来混合带有子轨道的轨道内容的，不过除了动画轨道以外，我确实想不到还有什么会用到这种功能的。
            if (layerable != null)
                layerMixer = layerable.CreateLayerMixer(graph, go, s_BuildData.trackList.Count);
            /*Tip：这里就是将轨道节点连接到这个层级混合节点，而非通常的直接将轨道节点连接到中枢节点，但其实从结构上来说本质相同，因为这里会将该节点返回，随后照样是连接到中枢节点。
            结合AnimationTrack重写的该方法CreateMixerPlayableGraph就可以发现，这个方法返回的节点就是直接连接到中枢节点TimelinePlayable的节点。
            */
            if (layerMixer.IsValid())
            {
                for (int i = 0; i < s_BuildData.trackList.Count; i++)
                {
                    //CompileClips返回的是轨道节点即此处的mixer。
                    var mixer = s_BuildData.trackList[i].CompileClips(graph, go, s_BuildData.trackList[i].clips, tree);
                    if (mixer.IsValid())
                    {
                        graph.Connect(mixer, 0, layerMixer, i);
                        layerMixer.SetInputWeight(i, 1.0f);
                    }
                }
                return layerMixer;
            }

            // one track compiles. Add track mixer and clips
            if (s_BuildData.trackList.Count == 1)
                return s_BuildData.trackList[0].CompileClips(graph, go, s_BuildData.trackList[0].clips, tree);

            /*Tip：这个应该算是异常情况，最通常的应该是上面判断轨道数量为1的情况，就直接调用轨道的CompileClips方法即可，而这里的情况是，有多个轨道但并没有设置混合逻辑，所以
            这里的处理就是将这些轨道上的片段放到一起，也就是当做位于同一个轨道上的片段，然后调用CompileClips方法，显然这并不符合常理，只是勉强这样处理而已。*/
            // no layer mixer provided. merge down all clips.
            //在这里可以看到clipList纯粹就是用于应对这种情况的，否则的话就是TrackAsset的clips成员就存储了轨道上的所有片段。
            for (int i = 0; i < s_BuildData.trackList.Count; i++)
                s_BuildData.clipList.AddRange(s_BuildData.trackList[i].clips);

#if UNITY_EDITOR
            bool applyWarning = false;
            for (int i = 0; i < s_BuildData.trackList.Count; i++)
                applyWarning |= i > 0 && s_BuildData.trackList[i].hasCurves;

            if (applyWarning)
                Debug.LogWarning("A layered track contains animated fields, but no layer mixer has been provided. Animated fields on layers will be ignored. Override CreateLayerMixer in " + s_BuildData.trackList[0].GetType().Name + " and return a valid playable to support animated fields on layered tracks.");
#endif
            // compile all the clips into a single mixer
            return CompileClips(graph, go, s_BuildData.clipList, tree);
        }

        internal void ConfigureTrackAnimation(IntervalTree<RuntimeElement> tree, GameObject go, Playable blend)
        {
            if (!hasCurves)
                return;

            blend.SetAnimatedProperties(m_Curves);
            tree.Add(new InfiniteRuntimeClip(blend));

            if (OnTrackAnimationPlayableCreate != null)
                OnTrackAnimationPlayableCreate.Invoke(this, go, blend);
        }

        // sorts clips by start time
        internal void SortClips()
        {
            var clipsAsArray = clips; // will alloc
            if (!m_CacheSorted) //没有排过序
            {
                //传入比较函数，会自动根据第一个容器参数的元素类型来确定比较函数的类型
                //默认从小到大的升序，所以实现根据Clip的开始时刻先后来决定在容器中的顺序
                Array.Sort(clips, (clip1, clip2) => clip1.start.CompareTo(clip2.start));
                m_CacheSorted = true;
            }
        }

        // clears the clips after a clone
        internal void ClearClipsInternal()
        {
            m_Clips = new List<TimelineClip>();
            m_ClipsCache = null;
        }

        internal void ClearSubTracksInternal()
        {
            m_Children = new List<ScriptableObject>();
            Invalidate();
        }

        // called by an owned clip when it moves
        internal void OnClipMove()
        {
            m_CacheSorted = false;
        }

        internal TimelineClip CreateNewClipContainerInternal()
        {
            var clipContainer = new TimelineClip(this);
            clipContainer.asset = null;

            // position clip at end of sequence
            var newClipStart = 0.0;
            for (var a = 0; a < m_Clips.Count - 1; a++)
            {
                var clipDuration = m_Clips[a].duration;
                if (double.IsInfinity(clipDuration))
                    clipDuration = TimelineClip.kDefaultClipDurationInSeconds;
                newClipStart = Math.Max(newClipStart, m_Clips[a].start + clipDuration);
            }

            clipContainer.mixInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            clipContainer.mixOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            clipContainer.start = newClipStart;
            clipContainer.duration = TimelineClip.kDefaultClipDurationInSeconds;
            clipContainer.displayName = "untitled";
            return clipContainer;
        }

        internal void AddChild(TrackAsset child)
        {
            if (child == null)
                return;

            m_Children.Add(child);
            child.parent = this;
            Invalidate();
        }

        internal void MoveLastTrackBefore(TrackAsset asset)
        {
            if (m_Children == null || m_Children.Count < 2 || asset == null)
                return;

            var lastTrack = m_Children[m_Children.Count - 1];
            if (lastTrack == asset)
                return;

            for (int i = 0; i < m_Children.Count - 1; i++)
            {
                if (m_Children[i] == asset)
                {
                    for (int j = m_Children.Count - 1; j > i; j--)
                        m_Children[j] = m_Children[j - 1];
                    m_Children[i] = lastTrack;
                    Invalidate();
                    break;
                }
            }
        }

        internal bool RemoveSubTrack(TrackAsset child)
        {
            if (m_Children.Remove(child))
            {
                Invalidate();
                child.parent = null;
                return true;
            }
            return false;
        }

        internal void RemoveClip(TimelineClip clip)
        {
            m_Clips.Remove(clip);
            m_ClipsCache = null;
        }

        // Is this track compilable for the sequence
        // calculate the time interval that this track will be evaluated in.
        internal virtual void GetEvaluationTime(out double outStart, out double outDuration)
        {
            outStart = 0;
            outDuration = 1;

            outStart = double.PositiveInfinity;
            var outEnd = double.NegativeInfinity;

            if (hasCurves)
            {
                outStart = 0.0;
                outEnd = TimeUtility.GetAnimationClipLength(curves);
            }

            foreach (var clip in clips)
            {
                outStart = Math.Min(clip.start, outStart);
                outEnd = Math.Max(clip.end, outEnd);
            }

            if (HasNotifications())
            {
                var notificationDuration = GetNotificationDuration();
                outStart = Math.Min(notificationDuration, outStart);
                outEnd = Math.Max(notificationDuration, outEnd);
            }

            if (double.IsInfinity(outStart) || double.IsInfinity(outEnd))
                outStart = outDuration = 0.0;
            else
                outDuration = outEnd - outStart;
        }

        // calculate the time interval that the sequence will use to determine length.
        // by default this is the same as the evaluation, but subclasses can have different
        // behaviour
        internal virtual void GetSequenceTime(out double outStart, out double outDuration)
        {
            GetEvaluationTime(out outStart, out outDuration);
        }

        /// <summary>
        /// Called by the Timeline Editor to gather properties requiring preview.
        /// </summary>
        /// <param name="director">The PlayableDirector invoking the preview</param>
        /// <param name="driver">PropertyCollector used to gather previewable properties</param>
        public virtual void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            // only push on game objects if there is a binding. Subtracks
            //  will use objects on the stack
            var gameObject = GetGameObjectBinding(director);
            if (gameObject != null)
                driver.PushActiveGameObject(gameObject);

            if (hasCurves)
                driver.AddObjectProperties(this, m_Curves);

            foreach (var clip in clips)
            {
                if (clip.curves != null && clip.asset != null)
                    driver.AddObjectProperties(clip.asset, clip.curves);

                IPropertyPreview modifier = clip.asset as IPropertyPreview;
                if (modifier != null)
                    modifier.GatherProperties(director, driver);
            }

            foreach (var subtrack in GetChildTracks())
            {
                if (subtrack != null)
                    subtrack.GatherProperties(director, driver);
            }

            if (gameObject != null)
                driver.PopActiveGameObject();
        }

        //从PlayableDirector中获取绑定的GameObject，可以发现，轨道本来是可以绑定GameObject或者Component，而这里会统一转换成GameObject。
        internal GameObject GetGameObjectBinding(PlayableDirector director)
        {
            if (director == null)
                return null;

            /*TODO：这里的Binding可以在PlayableDirector的检视器中的Bindings属性中查看，就是TrackAsset与GameObject或者Component的绑定，在PlayableDirector源码中看不到具体细节，
            甚至看不到定义的字段，但是从逻辑上似乎这个Binding就只是一个字典而已*/
            var binding = director.GetGenericBinding(this);

            var gameObject = binding as GameObject;
            if (gameObject != null)
                return gameObject;

            var comp = binding as Component;
            if (comp != null)
                return comp.gameObject;

            return null;
        }

        internal bool ValidateClipType(Type clipType)
        {
            var attrs = GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
            for (var c = 0; c < attrs.Length; ++c)
            {
                var attr = (TrackClipTypeAttribute)attrs[c];
                if (attr.inspectedType.IsAssignableFrom(clipType))
                    return true;
            }

            // special case for playable tracks, they accept all clips (in the runtime)
            return typeof(PlayableTrack).IsAssignableFrom(GetType()) &&
                typeof(IPlayableAsset).IsAssignableFrom(clipType) &&
                typeof(ScriptableObject).IsAssignableFrom(clipType);
        }

        /// <summary>
        /// Called when a clip is created on a track.
        /// </summary>
        /// <param name="clip">The timeline clip added to this track</param>
        /// <remarks>Use this method to set default values on a timeline clip, or it's PlayableAsset.</remarks>
        protected virtual void OnCreateClip(TimelineClip clip) { }

        void UpdateDuration()
        {
            // check if something changed in the clips that require a re-calculation of the evaluation times.
            var itemsHash = CalculateItemsHash();
            if (itemsHash == m_ItemsHash)
                return;
            m_ItemsHash = itemsHash;

            double trackStart, trackDuration;
            GetSequenceTime(out trackStart, out trackDuration);

            m_Start = (DiscreteTime)trackStart;
            m_End = (DiscreteTime)(trackStart + trackDuration);

            // calculate the extrapolations time.
            // TODO Extrapolation time should probably be extracted from the SequenceClip so only a track is aware of it.
            this.CalculateExtrapolationTimes();
        }

        protected internal virtual int CalculateItemsHash()
        {
            return HashUtility.CombineHash(GetClipsHash(), GetAnimationClipHash(m_Curves), GetTimeRangeHash());
        }

        /// <summary>
        /// Constructs a Playable from a TimelineClip.
        /// </summary>
        /// <param name="graph">PlayableGraph that will own the playable.</param>
        /// <param name="gameObject">The GameObject that builds the PlayableGraph.</param>
        /// <param name="clip">The TimelineClip to construct a playable for.</param>
        /// <returns>A playable that will be set as an input to the Track Mixer playable, or Playable.Null if the clip does not have a valid PlayableAsset</returns>
        /// <exception cref="ArgumentException">Thrown if the specified PlayableGraph is not valid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the specified TimelineClip is not valid.</exception>
        /// <remarks>
        /// By default, this method invokes Playable.CreatePlayable, sets animated properties, and sets the speed of the created playable. Override this method to change this default implementation.
        /// </remarks>
        protected virtual Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            if (!graph.IsValid())
                throw new ArgumentException("graph must be a valid PlayableGraph");
            if (clip == null)
                throw new ArgumentNullException("clip");

            var asset = clip.asset as IPlayableAsset;
            if (asset != null)
            {
                var handle = asset.CreatePlayable(graph, gameObject);
                if (handle.IsValid())
                {
                    //该方法是公开的，不过具体何用不清楚。
                    handle.SetAnimatedProperties(clip.curves);
                    handle.SetSpeed(clip.timeScale); //设置播放速度，其实是个很重要的功能，只是通常用不上，但是必备功能。
                    /*TODO：内部事件，主要是给编辑器用的，不过是否应该另外这些时刻设置公开的事件呢？*/
                    if (OnClipPlayableCreate != null)
                        OnClipPlayableCreate(clip, gameObject, handle);
                }
                return handle; //返回创建的Playable节点。
            }
            return Playable.Null;
        }

        internal void Invalidate()
        {
            m_ChildTrackCache = null;
            var timeline = timelineAsset;
            if (timeline != null)
            {
                timeline.Invalidate();
            }
        }

        internal double GetNotificationDuration()
        {
            if (!supportsNotifications)
            {
                return 0;
            }

            var maxTime = 0.0;
            int count = m_Markers.Count;
            for (int i = 0; i < count; i++)
            {
                var marker = m_Markers[i];
                if (!(marker is INotification))
                {
                    continue;
                }
                maxTime = Math.Max(maxTime, marker.time);
            }

            return maxTime;
        }

        /// <summary>
        /// 有片段或曲线即可。
        /// </summary>
        /// <returns></returns>
        internal virtual bool CanCompileClips()
        {
            return hasClips || hasCurves;
        }

        /// <summary>
        /// Whether the track can create a mixer for its own contents.
        /// </summary>
        /// <returns>Returns true if the track's mixer should be included in the playable graph.</returns>
        /// <remarks>A return value of true does not guarantee that the mixer will be included in the playable graph. GroupTracks and muted tracks are never included in the graph</remarks>
        /// <remarks>A return value of false does not guarantee that the mixer will not be included in the playable graph. If a child track returns true for CanCreateTrackMixer, the parent track will generate the mixer but its own playables will not be included.</remarks>
        /// <remarks>Override this method to change the conditions for a track to be included in the playable graph.</remarks>
        public virtual bool CanCreateTrackMixer()
        {
            return CanCompileClips();
        }

        //
        internal bool IsCompilable()
        {
            //是否为GroupTrack
            bool isContainer = typeof(GroupTrack).IsAssignableFrom(GetType()); //就是GetType得到的类型的实例能否分配给GroupTrack类型的引用，

            if (isContainer)
                return false;

            var ret = !mutedInHierarchy && (CanCreateTrackMixer() || CanCompileNotifications());
            if (!ret)
            {//Ques：非GroupTrack的话，怎么会有子轨道呢？似乎是AnimationTrack，就是它最特殊，可以添加多个Override Track，这就是子轨道。
                foreach (var t in GetChildTracks())
                {
                    if (t.IsCompilable())
                        return true;
                }
            }

            return ret;
        }

        private void UpdateChildTrackCache()
        {
            if (m_ChildTrackCache == null)
            {
                if (m_Children == null || m_Children.Count == 0)
                    m_ChildTrackCache = s_EmptyCache;
                else
                {
                    var childTracks = new List<TrackAsset>(m_Children.Count);
                    for (int i = 0; i < m_Children.Count; i++)
                    {
                        var subTrack = m_Children[i] as TrackAsset;
                        if (subTrack != null)
                            childTracks.Add(subTrack);
                    }
                    m_ChildTrackCache = childTracks;
                }
            }
        }

        internal virtual int Hash()
        {
            return clips.Length + (m_Markers.Count << 16);
        }

        int GetClipsHash()
        {
            var hash = 0;
            foreach (var clip in m_Clips)
            {
                hash = hash.CombineHash(clip.Hash());
            }
            return hash;
        }

        /// <summary>
        /// Gets the hash code for an AnimationClip.
        /// </summary>
        /// <param name="clip">The animation clip.</param>
        /// <returns>A 32-bit signed integer that is the hash code for <paramref name="clip"/>. Returns 0 if <paramref name="clip"/> is null or empty.</returns>
        protected static int GetAnimationClipHash(AnimationClip clip)
        {
            var hash = 0;
            if (clip != null && !clip.empty)
                hash = hash.CombineHash(clip.frameRate.GetHashCode())
                    .CombineHash(clip.length.GetHashCode());

            return hash;
        }

        bool HasNotifications()
        {
            return m_Markers.HasNotifications();
        }

        bool CanCompileNotifications()
        {
            return supportsNotifications && m_Markers.HasNotifications();
        }

        /// <summary>
        /// 要求自己或子轨道有片段或曲线
        /// </summary>
        /// <returns></returns>
        bool CanCreateMixerRecursive()
        {
            //有片段或曲线即可。否则就是看子轨道中是否有符合要求的轨道。
            if (CanCreateTrackMixer())
                return true;
            foreach (var track in GetChildTracks())
            {
                if (track.CanCreateMixerRecursive())
                    return true;
            }

            return false;
        }
    }
}
