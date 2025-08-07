using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace UnityEditor.Timeline
{
    internal interface IWindowStateProvider //提供窗口状态（以组合的方式）
    {
        IWindowState windowState { get; }
    }

    //Tip：这里指定编辑窗口的标题和图标，第二个参数就是将该特性标记的类型名作为Icon名，也就是路径了，不过在C#库中没有找到，可能存在隐藏。
    [EditorWindowTitle(title = "Timeline", useTypeNameAsIconName = true)]
    //没有指定修饰符，对于这种直接定义在命名空间下的顶层类型，就是internal。
    partial class TimelineWindow : TimelineEditorWindow, IHasCustomMenu, IWindowStateProvider
    {
        [Serializable]
        public class TimelineWindowPreferences
        {
            public EditMode.EditType editType = EditMode.EditType.Mix;
            public TimeReferenceMode timeReferenceMode = TimeReferenceMode.Local;
        }

        [SerializeField] TimelineWindowPreferences m_Preferences = new TimelineWindowPreferences();
        public TimelineWindowPreferences preferences { get { return m_Preferences; } } //只读

        [SerializeField] //EditorLockTracker是internal类型，也就在这种官方库中可以借鉴一下用法了。
        EditorGUIUtility.EditorLockTracker m_LockTracker = new EditorGUIUtility.EditorLockTracker();

        readonly PreviewResizer m_PreviewResizer = new PreviewResizer();
        bool m_LastFrameHadSequence;
        bool m_ForceRefreshLastSelection;
        int m_CurrentSceneHashCode = -1;

        [NonSerialized]
        bool m_HasBeenInitialized;

        [SerializeField]
        SequenceHierarchy m_SequenceHierarchy;
        static SequenceHierarchy s_LastHierarchy;

        public static TimelineWindow instance { get; private set; }
        public Rect clientArea { get; set; }
        public bool isDragging { get; set; }
        public static DirectorStyles styles { get { return DirectorStyles.Instance; } }
        public List<TimelineTrackBaseGUI> allTracks
        {
            get
            {
                return treeView != null ? treeView.allTrackGuis : new List<TimelineTrackBaseGUI>();
            }
        }

        public WindowState state { get; private set; } //在OnEnable中初始化创建实例

        IWindowState IWindowStateProvider.windowState => state;

        public override bool locked
        {
            get
            {
                // we can never be in a locked state if there is no timeline asset
                if (state.editSequence.asset == null)
                    return false;

                return m_LockTracker.isLocked;
            }
            set { m_LockTracker.isLocked = value; }
        }

        public bool hierarchyChangedThisFrame { get; private set; }

        /*Tip：在调用GetWindow时，其中就会调用ScriptableObject.CreateInstance方法构造窗口实例，该方法的逻辑虽然在C++层，但是会调用C#层的无参数构造函数来创建托管对象。
        在调用了构造函数之后，才是后面的周期方法，OnEnable、OnGUI、OnDisable等等。*/
        public TimelineWindow()
        {
            InitializeManipulators();
            m_LockTracker.lockStateChanged.AddPersistentListener(OnLockStateChanged, UnityEventCallState.EditorAndRuntime);
        }

        void OnLockStateChanged(bool locked)
        {
            // Make sure that upon unlocking, any selection change is updated
            // Case 1123119 -- only force rebuild if not recording
            if (!locked)
                RefreshSelection(state != null && !state.recording);
        }

        void OnEnable()
        {
            if (m_SequencePath == null)
                m_SequencePath = new SequencePath();

            if (m_SequenceHierarchy == null)
            {
                // The sequence hierarchy will become null if maximize on play is used for in/out of playmode
                // a static var will hang on to the reference
                if (s_LastHierarchy != null)
                    m_SequenceHierarchy = s_LastHierarchy;
                else
                    m_SequenceHierarchy = SequenceHierarchy.CreateInstance();

                state = null;
            }
            s_LastHierarchy = m_SequenceHierarchy;

            titleContent = GetLocalizedTitleContent();

            UpdateTitle();

            m_PreviewResizer.Init("TimelineWindow");

            // Unmaximize fix : when unmaximizing, a new window is enabled and disabled. Prevent it from overriding the instance pointer.
            if (instance == null)
                instance = this;

            AnimationClipCurveCache.Instance.OnEnable();
            TrackAsset.OnClipPlayableCreate += m_PlayableLookup.UpdatePlayableLookup;
            TrackAsset.OnTrackAnimationPlayableCreate += m_PlayableLookup.UpdatePlayableLookup;

            if (state == null)
            {
                state = new WindowState(this, s_LastHierarchy);
                Initialize();
                RefreshSelection(true);
                m_ForceRefreshLastSelection = true;
            }
        }

        void OnDisable()
        {
            if (instance == this)
                instance = null;

            if (state != null)
                state.Reset();

            if (instance == null)
                SelectionManager.RemoveTimelineSelection();

            AnimationClipCurveCache.Instance.OnDisable();
            TrackAsset.OnClipPlayableCreate -= m_PlayableLookup.UpdatePlayableLookup;
            TrackAsset.OnTrackAnimationPlayableCreate -= m_PlayableLookup.UpdatePlayableLookup;
            TimelineWindowViewPrefs.SaveAll();
            TimelineWindowViewPrefs.UnloadAllViewModels();
        }

        void OnDestroy()
        {
            if (state != null)
            {
                state.OnDestroy();
            }
            m_HasBeenInitialized = false;
            RemoveEditorCallbacks();
            AnimationClipCurveCache.Instance.Clear();
            TimelineAnimationUtilities.UnlinkAnimationWindow();
        }

        void OnLostFocus()
        {
            isDragging = false;

            if (state != null)
                state.captured.Clear();

            Repaint();
        }

        void OnHierarchyChange()
        {
            hierarchyChangedThisFrame = true;
            Repaint();
        }

        void OnStateChange()
        {
            state.UpdateRecordingState();
            state.editSequence.InvalidateChildAssetCache();
            if (treeView != null && state.editSequence.asset != null)
                treeView.Reload();
            if (m_MarkerHeaderGUI != null)
                m_MarkerHeaderGUI.Rebuild();
        }

        //从输入采集、状态更新、事件分发、布局绘制到最终重建与重绘
        void OnGUI()
        {
            // Debug.Log($@"在{Time.time}调用OnGUI
            // 当前事件类型：{Event.current.type}
            // 窗口当前position: position.x:{position.x};position.x:{position.x};position.y:{position.y}; position.xMin:{position.xMin};position.xMax:{position.xMax};
            // position.yMin:{position.yMin}; position.yMax:{position.yMax}; position.width:{position.width}; position.height:{position.height};");

            //测试Event的Use方法
            // if (Event.current.type == EventType.Layout)
            // {
            //     Event.current.Use();
            //     Debug.Log($@"在{Time.time}调用OnGUI, 事件类型为{Event.current.type}");
            //     Event.current.type = EventType.Layout;
            // }
            
            /*Tip：下面这三个以及后面一些方法，共同特征都是无参数、无返回值、私有、只在OnGUI这里被调用过一次，因为它们成为方法的目的并非为了复用，只是为了突出逻辑链条，像这里一样，
                给一段逻辑加上函数名作为标识，表示出了这段逻辑的目的是什么，一系列这样的处理，就可以在OnGUI中看到清晰的逻辑链条，而不用被每个部分中具体的处理细节所干扰。
                这些方法也是典型的可内联函数，不过似乎C#的编译器会自动进行内联处理，甚至C++的编译器也是会自动尝试将函数内联。*/

                //确保内置的 GUIStyle、各类图标与资源只初始化一次。避免在每帧重复加载样式，提升性能。
            InitializeGUIIfRequired();
            //根据当前窗口尺寸和 Editor DPI 重新计算时间尺、轨道 header、缩放手柄等静态布局常量。
            UpdateGUIConstants(); //实际上更新了m_HorizontalScrollBarSize和m_VerticalScrollBarSize
            //为 WindowState 计算一个哈希值，用于对比前后状态是否改变，决定后续是否需要重建可交互元素或刷新布局。
            UpdateViewStateHash(); //就是调用WindowState自己的计算Hash的方法

            //处理“编辑模式锁存”（Mode Clutch）逻辑，允许在轨道剪辑编辑与播放头拖拽之间快速切换工具模式。
            EditMode.HandleModeClutch(); // TODO We Want that here?

            //当编辑器皮肤、字体或 DPI 发生改变时重载样式
            DetectStylesChange();
            //切换场景时，同步 Timeline 的可绑定对象、预设更新。
            DetectActiveSceneChanges();
            //监控 WindowState 中如播放、录制、循环模式等字段变化，触发必要的 UI 刷新。
            DetectStateChanges();

            //执行上一帧末尾挂起的状态修改（如播放头跳转、剪辑添加／删除）以保证一致性。
            state.ProcessStartFramePendingUpdates();

            var clipRect = new Rect(0.0f, 0.0f, position.width, position.height);

            //抛出扩展事件，供外部插件或自定义工具在 GUI 早期阶段注入控件、调整状态。
            using (new GUIViewportScope(clipRect))
                state.InvokeWindowOnGuiStarted(Event.current);

            //如果当前为拖拽延迟阶段（MouseDrag + mouseDragLag > 0），先减少延迟计时并 return，避开重绘和逻辑处理，减轻拖拽时的卡顿感。
            if (Event.current.type == EventType.MouseDrag && state != null && state.mouseDragLag > 0.0f)
            {
                state.mouseDragLag -= Time.deltaTime;
                return;
            }

            //检查并响应 Editor 的 Undo/Redo 操作，一旦发生则立刻 return 触发一次重绘，以更新撤销后的状态。
            //Tip：这里只是在处理Ctrl+Z的事件中不再执行后续逻辑，至于具体的撤销逻辑，是通过Unity编辑器本身的撤销系统以及在Timeline库中注册的撤销行为来实现的，与此处无关。
            if (PerformUndo())
                return;

            /*当处于播放且忽略预览（state.ignorePreview）时：
                如果同时在录制，则终止录制，防止录入无效数据。
                强制 Repaint()，保证时间轴和播放状态保持同步。*/
            if (state != null && state.ignorePreview && state.playing)
            {//在TimelineAsset的检视器中可以看到ScenePreview的开关。
                if (state.recording)
                    state.recording = false;
                Repaint();
            }

            //将整个窗口矩形记录到 clientArea，为后续事件命中测试提供参考。
            clientArea = position; //获取到窗口的Rect信息

            //当播放头接近视口边缘时自动滚动时间轴，保证播放头始终可见。
            PlaybackScroller.AutoScroll(state);
            /*主体布局函数，内部通常会依次执行：
                清理并重建空间分区四叉树（QuadTree）
                调用 m_PreTreeViewControl.HandleManipulatorsEvents(state) 让各 Manipulator 抢先处理 MouseDown/Drag/Up
                绘制轨道树（TreeView）、时间尺（Ruler）、行内曲线、剪辑（Clips）
                调用 m_PostTreeViewControl.HandleManipulatorsEvents(state) 处理右键菜单、快捷键、清除选中等*/
            DoLayout();

            // overlays
            /*如果有 Manipulator 将自身注册到 state.captured（例如拖拽时需要绘制拖动影子），则在主布局之后：
                通过新的 GUIViewportScope 进入同一剪裁区域
                遍历每个 captured 对象的 Overlay(Event.current, state)，完成如剪辑拖影、时间框选高亮等叠加绘制
                结束后 Repaint()，保证持续覆盖渲染*/
            if (state.captured.Count > 0)
            {
                using (new GUIViewportScope(clipRect))
                {
                    foreach (var o in state.captured)
                    {
                        o.Overlay(Event.current, state);
                    }
                    Repaint();
                }
            }

            //当 state.showQuadTree 为真时，用半透明矩形和线框绘制时间轴与 header 的空间分区，用于性能分析与命中测试验证。
            if (state.showQuadTree)
            {
                var fillColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                state.spacePartitioner.DebugDraw(fillColor, Color.yellow);
                state.headerSpacePartitioner.DebugDraw(fillColor, Color.green);
            }

            // attempt another rebuild -- this will avoid 1 frame flashes
            /*
            在 EventType.Repaint 阶段：
                RebuildGraphIfNecessary()——检查 viewStateHash 或状态标志，决定是否重建内部的 PlayableGraph、空间分区树等昂贵结构。
                state.ProcessEndFramePendingUpdates()——执行本帧末尾挂起的更新，以便下一帧从干净状态开始。
            */
            if (Event.current.type == EventType.Repaint)
            {
                RebuildGraphIfNecessary();
                state.ProcessEndFramePendingUpdates();
            }


            using (new GUIViewportScope(clipRect))
            {
                if (Event.current.type == EventType.Repaint)
                    EditMode.inputHandler.OnGUI(state, Event.current);
            }

            //在 Repaint 事件里重置 hierarchyChangedThisFrame = false，为下一次变化检测做准备。
            if (Event.current.type == EventType.Repaint)
            {
                hierarchyChangedThisFrame = false;
            }

            //在 Layout 事件里调用 UpdateTitle()，把当前编辑序列名称、播放状态等信息同步到窗口标题栏。
            if (Event.current.type == EventType.Layout)
            {
                UpdateTitle();
            }
        }

        void UpdateTitle()
        {
#if UNITY_2020_2_OR_NEWER
            bool dirty = false;
            List<Object> children = state?.editSequence.cachedChildAssets;
            if (children != null)
            {
                foreach (var child in children)
                {
                    dirty = EditorUtility.IsDirty(child);
                    if (dirty)
                    {
                        break;
                    }
                }
            }

            hasUnsavedChanges = dirty;
#endif
        }

        static void DetectStylesChange()
        {
            DirectorStyles.ReloadStylesIfNeeded();
        }

        void DetectActiveSceneChanges()
        {
            if (m_CurrentSceneHashCode == -1)
            {
                m_CurrentSceneHashCode = SceneManager.GetActiveScene().GetHashCode();
            }

            if (m_CurrentSceneHashCode != SceneManager.GetActiveScene().GetHashCode())
            {
                bool isSceneStillLoaded = false;
                for (int a = 0; a < SceneManager.sceneCount; a++)
                {
                    var scene = SceneManager.GetSceneAt(a);
                    if (scene.GetHashCode() == m_CurrentSceneHashCode && scene.isLoaded)
                    {
                        isSceneStillLoaded = true;
                        break;
                    }
                }

                if (!isSceneStillLoaded)
                {
                    if (!locked)
                        ClearTimeline();
                    m_CurrentSceneHashCode = SceneManager.GetActiveScene().GetHashCode();
                }
            }
        }

        void DetectStateChanges()
        {
            if (state != null)
            {
                foreach (var sequenceState in state.allSequences)
                {
                    sequenceState.ResetIsReadOnly();
                }
                // detect if the sequence was removed under our feet
                if (m_LastFrameHadSequence && state.editSequence.asset == null)
                {
                    ClearTimeline();
                }
                m_LastFrameHadSequence = state.editSequence.asset != null;

                // the currentDirector can get set to null by a deletion or scene unloading so polling is required
                if (state.editSequence.director == null)
                {
                    state.recording = false;
                    state.previewMode = false;

                    if (locked)
                    {
                        //revert lock if the original context was not asset mode
                        if (!state.masterSequence.isAssetOnly)
                            locked = false;
                    }

                    if (!locked && m_LastFrameHadSequence)
                    {
                        // the user may be adding a new PlayableDirector to a selected GameObject, make sure the timeline editor is shows the proper director if none is already showing
                        var selectedGameObject = Selection.activeObject != null ? Selection.activeObject as GameObject : null;
                        var selectedDirector = selectedGameObject != null ? selectedGameObject.GetComponent<PlayableDirector>() : null;
                        if (selectedDirector != null)
                        {
                            SetTimeline(selectedDirector);
                        }
                        else
                        {
                            state.masterSequence.isAssetOnly = true;
                        }
                    }
                }
                else
                {
                    // the user may have changed the timeline associated with the current director
                    if (state.editSequence.asset != state.editSequence.director.playableAsset)
                    {
                        if (!locked)
                        {
                            SetTimeline(state.editSequence.director);
                        }
                        else
                        {
                            // Keep locked on the current timeline but set the current director to null since it's not the timeline owner anymore
                            SetTimeline(state.editSequence.asset);
                        }
                    }
                }
            }
        }

        void Initialize()
        {
            if (!m_HasBeenInitialized)
            {
                InitializeStateChange();
                InitializeEditorCallbacks();
                m_HasBeenInitialized = true;
            }
        }

        void RefreshLastSelectionIfRequired()
        {
            // case 1088918 - workaround for the instanceID to object cache being update during Awake.
            // This corrects any playableDirector ptrs with the correct cached version
            // This can happen when going from edit to playmode
            if (m_ForceRefreshLastSelection)
            {
                m_ForceRefreshLastSelection = false;
                RestoreLastSelection(true);
            }
        }

        void InitializeGUIIfRequired()
        {
            RefreshLastSelectionIfRequired();
            InitializeTimeArea();
            //依靠树状视图来展示Track和Clip
            if (treeView == null && state.editSequence.asset != null)
            {
                treeView = new TimelineTreeViewGUI(this, state.editSequence.asset, position);
            }
        }

        void UpdateGUIConstants()
        {
            m_HorizontalScrollBarSize =
                GUI.skin.horizontalScrollbar.fixedHeight + GUI.skin.horizontalScrollbar.margin.top;
            //水平滚动条始终都会存在，因为控制时间轴区域，但是竖直滚动条需要再轨道的总高度超过窗口相关区域的高度之后才会出现。
            m_VerticalScrollBarSize = (treeView != null && treeView.showingVerticalScrollBar)
                ? GUI.skin.verticalScrollbar.fixedWidth + GUI.skin.verticalScrollbar.margin.left
                : 0;
        }

        void UpdateViewStateHash()
        {
            if (Event.current.type == EventType.Layout)
                state.UpdateViewStateHash();
        }

        //按下Ctrl
        static bool PerformUndo()
        {
            if (!Event.current.isKey) //该事件是否是键盘事件
                return false;

            if (Event.current.keyCode != KeyCode.Z)
                return false;

            if (!EditorGUI.actionKey) //Windows上Ctrl键
                return false;

            return true;
        }

        public void RebuildGraphIfNecessary(bool evaluate = true)
        {
            if (state == null || currentMode.mode != TimelineModes.Active || state.editSequence.director == null || state.editSequence.asset == null)
                return;

            if (state.rebuildGraph)
            {
                // rebuilding the graph resets the time
                double time = state.editSequence.time;

                var wasPlaying = false;

                // disable preview mode,
                if (!state.ignorePreview)
                {
                    wasPlaying = state.playing;

                    state.previewMode = false;
                    state.GatherProperties(state.masterSequence.director);
                }
                state.RebuildPlayableGraph();
                state.editSequence.time = time;

                if (wasPlaying)
                    state.Play();

                if (evaluate)
                {
                    // put the scene back in the correct state
                    state.EvaluateImmediate();

                    // this is necessary to see accurate results when inspector refreshes
                    // case 1154802 - this will property re-force time on the director, so
                    //  the play head won't snap back to the timeline duration on rebuilds
                    if (!state.playing)
                        state.Evaluate();
                }
                Repaint();
            }

            state.rebuildGraph = false;
        }

        // for tests
        public new void RepaintImmediately()
        {
            base.RepaintImmediately();
        }

        internal static bool IsEditingTimelineAsset(TimelineAsset timelineAsset)
        {
            return instance != null && instance.state != null && instance.state.editSequence.asset == timelineAsset;
        }

        internal static void RepaintIfEditingTimelineAsset(TimelineAsset timelineAsset)
        {
            if (IsEditingTimelineAsset(timelineAsset))
                instance.Repaint();
        }

        internal class DoCreateTimeline : ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var timeline = TimelineUtility.CreateAndSaveTimelineAsset(pathName);
                ProjectWindowUtil.ShowCreatedAsset(timeline);
            }
        }

        //创建TimelineAsset
        [MenuItem("Assets/Create/Timeline", false, 450)]
        public static void CreateNewTimeline()
        {
            var icon = EditorGUIUtility.IconContent("TimelineAsset Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateTimeline>(), "New Timeline.playable", icon, null);
        }
        //打开TimelineWindow
        [MenuItem("Window/Sequencing/Timeline", false, 1)]
        public static void ShowWindow()
        {//如果已经有了就不会生成新的，所以无法多开（编辑窗口几乎都不会设置多开功能），并且重复点击并不会对当前存在的对应窗口产生任何影响
            GetWindow<TimelineWindow>(typeof(SceneView)); //将新打开的窗口TimelineWindow停靠在SceneView窗口旁边
            instance.Focus(); //聚焦窗口
        }

        [OnOpenAsset(1)] //1是Validation，0是Execute
        //Tip：在手册中已经没有相关介绍，不过可以猜测，在Unity编辑器中只要是双击Assets文件夹下的UI元素（文件夹与文件？）就会调用该方法，并且传入其实例ID（这样看来应该是针对资产文件）
        public static bool OnDoubleClick(int instanceID, int line)
        {
            var assetDoubleClicked = EditorUtility.InstanceIDToObject(instanceID) as TimelineAsset;
            if (assetDoubleClicked == null)
                return false;
            //双击TimelineAsset就会打开Timeline窗口，
            ShowWindow(); 
            //将TimelineAsset绑定到TimelineWindow上，也就是所谓的“Set”
            instance.SetTimeline(assetDoubleClicked);

            return true;
        }

        //IHasCustomMenu的接口方法，为窗口的三点菜单添加自定义菜单项。
        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            bool disabled = state == null || state.editSequence.asset == null;
            //这是Unity内置的Lock菜单项，是所有EditorWindow共有的更加底层的选项，只需要处理好disabled是否可以选中的状态逻辑，然后像这里调用它的AddItemsToMenu方法即可
            m_LockTracker.AddItemsToMenu(menu, disabled); 
        }

        protected virtual void ShowButton(Rect r)
        {
            bool disabled = state == null || state.editSequence.asset == null;
            //小锁图标，更加快捷进行上锁和解锁操作。这里对应的是Timeline窗口三点菜单左边的小锁图标，显然也是所有EditorWindow共同的一个接口。
            m_LockTracker.ShowButton(r, DirectorStyles.Instance.timelineLockButton, disabled); //如果点击了图标其实会返回true。
        }
    }
}
