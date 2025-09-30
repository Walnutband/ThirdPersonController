
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using APRGDemo.SkillSystemtest;
using System.Collections.Generic;
using System;

namespace ARPGDemo.Test.Timeline
{
    public class Obsolete_SkillEditorWindow : EditorWindow
    {

        #region 缓存特定节点元素引用
        private VisualElement m_GroupView;
        private VisualElement m_TimelineView;
        private VisualElement m_TimelineLeftView;
        private VisualElement m_TimelineRightView;
        private VisualElement m_TrackView;

        //轨道容器和片段容器
        private ListView m_TrackContainer;
        private ScrollView m_ClipContainer;
        private VisualElement m_ClipContentContainer;
        /*Tip：突然想到对于这种不太想要用专门字段引用的节点（感觉有点冗余），就可以直接用一个属性来返回、逻辑量可以忽略不计*/
        private ScrollView leftScroll => m_TrackContainer.Q<ScrollView>();
        private ScrollView rightScroll => m_ClipContainer;
        //时间尺
        private RulerElement timeRuler => m_TimelineRightView.Q<RulerElement>();
        // private VisualElement endFlag => m_TimelineRightView.Q("EndFlag");
        private VisualElement m_EndFlag;

        #endregion

        private bool syncingScroll;
        private Vector2 m_LastScrollOffset; //右视图的ScrollView的上一次的偏移量。
        private double m_ClipContentPreEndTime;
        private float m_ClipContentPreEndPosX;
        /*TODO：暂时将就，这就是避免在缩放时间尺的时候还会触发水平滚动条的valueChanged导致时间尺进行一个额外的移动.
        其实感觉是够了，因为这个是完全确定的行为*/
        private bool rulerScaled;
        private bool horizontalSynced;
        private int currentEditorFrameCount;


        // [MenuItem("MyPlugins/SkillEditor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SkillEditorWindow>();
            window.titleContent = new GUIContent("SkillEditor");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            /*Tip：记录帧数以及其他一些在编辑模式下使用的，编辑模式下不适合使用Time.frameCount*/
            currentEditorFrameCount++;
            // Debug.Log($"当前帧：{currentFrame}\nrulerScaled: {rulerScaled}, horizontalSynced: {horizontalSynced}");
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            /*Tip：首先加载整个资产文件，添加到窗口的根节点下*/
            VisualTreeAsset timelineEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ARPGDemoScripts/Test/TimelineTest/SkillEditor.uxml");
            var timelineEditorTree = timelineEditorAsset.CloneTree();
            timelineEditorTree.style.flexGrow = 1f;
            root.Add(timelineEditorTree);

            //时间轴视图，作为父对象，带有左右两个子对象。
            m_TimelineView = timelineEditorTree.Q<SplitView>();
            m_TimelineLeftView = m_TimelineView.Q("TimelineLeftView");
            m_TimelineRightView = m_TimelineView.Q("TimelineRightView");

            //获取轨道和片段各自的容器，注意轨道使用的是ListView、片段使用的是ScrollView
            m_TrackContainer = m_TimelineLeftView.Q<ListView>();
            m_ClipContainer = m_TimelineRightView.Q<ScrollView>();
            //注意这是位于contentContainer下的子对象，这才是真正存放片段内容的容器，而且滚动条的长度也是基于该对象的尺寸与可视区域尺寸的比值来决定的，并非默认的contentContainer对象
            m_ClipContentContainer = m_ClipContainer.Q("ClipContentContainer");
            //结束标志
            m_EndFlag = m_TimelineRightView.Q("EndFlag");

            leftScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;

            /*Tip：调整片段视图自带contentContainer与自定义的contentContainer相关参数*/
            m_ClipContentContainer.style.flexGrow = 1f;
            // m_ClipContentContainer.style.flexShrink = 1f;
            rightScroll.contentContainer.style.flexGrow = 0f;
            rightScroll.contentContainer.style.flexShrink = 0f;
            rightScroll.contentContainer.style.height = new StyleLength(StyleKeyword.Auto);

            /*TODO：初步生成轨道和片段内容*/
            TrackViewContentForTest();
            ClipViewContentForTest();

            var hScroller = m_ClipContainer.Q<Scroller>(className: "unity-scroller--horizontal");

            //Tip：同步底部占位符。
            var placeholder = m_TimelineLeftView.Q("Placeholder");
            placeholder.RegisterCallback<GeometryChangedEvent>(e =>
            {//匿名函数捕获局部变量。
                // placeholder.style.height = m_ClipContainer.Q<Scroller>(className: "unity-scroller--horizontal").worldBound.height;
                placeholder.style.height = hScroller.resolvedStyle.height;
            });

            //调整标志线的bottom，以免出现在水平滚动条表面
            hScroller.RegisterCallback<GeometryChangedEvent>(e => m_EndFlag.style.bottom = hScroller.resolvedStyle.height);

            /*Tip：同步左右侧的垂直滚动
            同步滚动就是两部分：直接用鼠标滚轮控制、以及拖拽滚动条Scroller，这都需要各自单独注册方法。
            */
            leftScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            leftScroll.RegisterCallback<WheelEvent>(e => OnSyncVerticalScroll(e, leftScroll, rightScroll));
            rightScroll.RegisterCallback<WheelEvent>(e => OnSyncVerticalScroll(e, rightScroll, leftScroll));
            rightScroll.verticalScroller.valueChanged += value => OnSyncVerticalScroll(null, rightScroll, leftScroll);

            /*Tip：同步时间轴右视图的片段视图与时间尺的水平滚动
            经测试contentContainer移动不会触发GeometryChangedEvent
            */
            m_LastScrollOffset = rightScroll.scrollOffset; //从记录初始值开始
            rightScroll.RegisterCallback<WheelEvent>(e =>
            {
                OnSyncHorizontalScroll(e);
                horizontalSynced = true;
                //Tip：使用调度器实现帧末（还是帧首？）清理工作
                rightScroll.schedule.Execute(() => horizontalSynced = false).StartingIn(0);
            });
            /*BugFix：我草，我在这里注册了水平滚动条的值改变事件之后，竟然意外地把下面说到的Bug给修好了。大概是因为拉动分割线的时候导致内容区域与可视区域宽度比值变化，导致
            水平滚动条的值发生变化，而且变化量正好就是分割线的偏移量也就是时间尺的移动量*/
            rightScroll.horizontalScroller.valueChanged += value =>
            {//如果滚动已经执行同步了，那么就不在值改变事件中执行了。
                if (horizontalSynced == true)
                {
                    horizontalSynced = false;
                    return;
                }
                OnSyncHorizontalScroll(null);
            };
            // rightScroll.horizontalScroller.slider.RegisterValueChangedCallback(value => OnSyncHorizontalScroll(null));
            //TODO：时间尺的移动和缩放都会牵涉到片段视图，但这方面逻辑必须要落实到各个片段、无法单独以片段容器来设计逻辑
            /*BUG：有个Bug在于，当内容区域大于可视区域、且此时右边界对齐、左边界超出，那么向左拉动分割线会带动时间尺变宽、但此时内容区域位置不变，所以刻度线就与之前对应内容区域
            的位置错位了。但其他情况下是正常同步移动的。
            不过这种情况下，时间尺会触发GeometryChangedEvent，所以能够利用该回调来让内容区域与时间尺同步*/
            timeRuler.RegisterCallback<GeometryChangedEvent>(e => Debug.Log("时间尺GeometryChangedEvent"));

            // rightScroll.contentContainer.RegisterCallback<GeometryChangedEvent>(e => OnSyncHorizontalScroll(e));

            /*Tip：同步片段视图与时间尺的缩放*/
            //在缩放之前保存一下当前的结束时刻，因为缩放后要通过该时刻计算缩放后的宽度，而且缩放后就访问不到缩放前的_pixelsPerSecond了
            timeRuler.preRulerScaled += (_visibleStartTime, _pixelsPerSecond) =>
            {
                Rect rect = m_ClipContentContainer.contentRect;
                m_ClipContentPreEndTime = rect.width / _pixelsPerSecond;
                m_ClipContentPreEndPosX = timeRuler.PixelOfTime(m_ClipContentPreEndTime);
            };
            timeRuler.postRulerScaled += (a, b) =>
            {
                rulerScaled = true;
                OnSyncRulerScale(a, b);
                timeRuler.schedule.Execute(() => rulerScaled = false).StartingIn(0);
            };

            // EditorApplication.update += () => Debug.Log("");

            /*Tip：在Timeline右视图生成结束标志线
            其实算是穷举，关键也就两种情况：时间尺移动和缩放
            */
            timeRuler.rulerMoved += OnAdjustEndFlag;
            //拖动分割线或者窗口边界，直接导致时间尺的宽度变化，这也会对标志线产生影响
            timeRuler.rulerGeometryChanged += OnAdjustEndFlag;
            //Tip：这一步非常重要，保证在对m_ClipContentContainer应用了布局之后再调整结束标记线
            m_ClipContentContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                OnAdjustEndFlag(timeRuler.visibleStartTime, timeRuler.pixelsPerSecond);
            });
            // timeRuler.rulerScaled += OnAdjustEndFlag;
            /*在一开始就调整一下，就是初始化EndFlag*/
            OnAdjustEndFlag(timeRuler.visibleStartTime, timeRuler.pixelsPerSecond);

            // m_TimelineRightView.generateVisualContent += OnGenerateEndFlag;
            // m_ClipContentContainer.RegisterCallback<GeometryChangedEvent>(e =>
            // {   

            // });

        }


        // /*在布局计算完成之后再触发，就是让左边占位形与右边占位形保持高度一致。*/
        // void OnGeometryChanged_LeftBottomPlaceholder(GeometryChangedEvent evt)
        // {
        //     // float height = m_ClipContainer.Q<Scroller>(className: "unity-scroller--horizontal").worldBound.height;
        //     leftBottomPlaceholder.style.height = height;
        //     Debug.Log($"height: {height}");
        // }


        #region 同步滚动、同步缩放

        //竖直滚动是同步轨道视图与片段视图
        void OnSyncVerticalScroll(WheelEvent evt, ScrollView _target, ScrollView _source)
        {
            if (syncingScroll) return;
            try
            {
                syncingScroll = true;
                // rightScroll.scrollOffset 是 Vector2 (x, y)
                Vector2 rOff = _target.scrollOffset;
                // 只把 y 同步给左侧，保留左侧原 x
                _source.scrollOffset = new Vector2(_source.scrollOffset.x, rOff.y);
                // Debug.Log($"rightScroll.scrollOffset={rightScroll.scrollOffset}\nleftScroll.scrollOffset={leftScroll.scrollOffset}");
            }
            finally { syncingScroll = false; }
        }

        //水平滚动是同步片段视图与时间尺
        void OnSyncHorizontalScroll(WheelEvent evt)
        {
            // const bool isWheeling = false;
            if (rulerScaled == true)
            {
                rulerScaled = false;
                return;
            }

            // if (horizontalSynced == true)
            // {
            //     horizontalSynced = false;
            //     return;
            // }

            Debug.Log("水平滚动");

            // if (syncingScroll) return;
            // try
            // {
            //     syncingScroll = true;

            /*Tip：之前搞混了，把scrollOffset当成了坐标*/
            Vector2 offset = rightScroll.scrollOffset;
            // timeRuler.DoMoveRuler(offset.x - m_LastScrollOffset.x);
            // Debug.Log($"移动量：{m_LastScrollOffset.x - offset.x}");
            timeRuler.DoMoveRuler(m_LastScrollOffset.x - offset.x);
            // Debug.Log($"移动量：{m_LastScrollOffset.x - offset.x}");
            m_LastScrollOffset = offset; //更新上一次偏移。
            // }
            // finally { syncingScroll = false; }
        }

        /*Tip：同步片段视图与时间尺的缩放，时间尺本身缩放的同时会触发事件rulerScaled，从而调用注册的该方法实现同步。*/
        private void OnSyncRulerScale(double _visibleStartTime, float _pixelsPerSecond)
        {
            // isRulerWheeling = true;

            // Rect rect = m_ClipContentContainer.contentRect;
            // Debug.Log($"m_ClipContentEndTime: {m_ClipContentEndTime}, _pixelsPerSecond: {_pixelsPerSecond}\n新宽度：{(float)(m_ClipContentEndTime * _pixelsPerSecond)}");
            // m_ClipContentContainer.style.width = (float)Math.Floor(m_ClipContentEndTime * _pixelsPerSecond);

            //首先是调整宽度，然后还要设置位置
            VisualElement contentContainer = rightScroll.contentContainer;
            contentContainer.style.width = m_ClipContentContainer.style.width = (float)(m_ClipContentPreEndTime * _pixelsPerSecond);
            // Debug.Log($"坐标：{m_ClipContentContainer.transform.position}");

            // Vector3 pos = contentContainer.transform.position;
            Vector2 offset = rightScroll.scrollOffset;
            // pos.x = timeRuler.PixelOfTime(m_ClipContentPreEndTime) - m_ClipContentPreEndPosX;
            // contentContainer.transform.position += pos;
            offset.x = (float)(timeRuler.visibleStartTime * _pixelsPerSecond);
            rightScroll.scrollOffset = offset;
            // Debug.Log("contentContainer.transform.position:" + contentContainer.transform.position);

            // Vector3 targetPos = m_ClipContentContainer.transform.position;
            // Vector3 targetPos = rightScroll.contentContainer.transform.position;
            // targetPos.x = timeRuler.PixelOfTime(m_ClipContentEndTime) - (float)(m_ClipContentEndTime * _pixelsPerSecond);
            // rightScroll.contentContainer.transform.position = targetPos;

            /*BugFix；在缩放时，发现标记线会出现在上一次的结束边界，就是因为此时才刚设置样式的width，还没有实际应用到元素的布局中，所以就不应该在这里调用，如果要在这里调用的话则
            需要增加额外逻辑，总之就是换成给m_ClipContentContainer把OnAdjustEndFlag注册到GeometryChangedEvent事件中（看上面CreateGUI），就可以保证在实际应用布局之后才调整结束标记线，这样就准确了*/
            //缩放之后，立刻检查是否需要调整EndFlag
            // OnAdjustEndFlag(_visibleStartTime, _pixelsPerSecond);
        }

        //
        /*Tip：艹了，突然忘了makeItem和itemsSource的区别，我还疑惑了很久，因为感觉作用是重合的，结果才想起来makeItem指的是UI层的元素，而itemSource指的是数据层的元素，而bindItem
        会基于itemSource的Count来决定有多少个元素、然后在绑定的方法中要进行的逻辑就是将来自于itemSource的元素的数据绑定到来自于makeItem的UI元素上（UI Toolkit的DataBinding）*/
        private void TrackViewContentForTest()
        {
            m_TrackContainer.makeItem = () =>
            {
                var item = new Label();
                item.style.unityTextAlign = TextAnchor.MiddleCenter;
                item.style.borderBottomColor = new StyleColor(Color.white);
                item.style.borderBottomWidth = 1;
                return item;
            };

            m_TrackContainer.bindItem = (e, i) =>
            {
                ((Label)e).text = m_TrackContainer.itemsSource[i] as string; //默认为VisualElement和C#的object类型
            };

            m_TrackContainer.itemsSource = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            m_TrackContainer.reorderable = true;
            m_TrackContainer.reorderMode = ListViewReorderMode.Simple;
        }

        private void ClipViewContentForTest()
        {
            m_ClipContentContainer.style.flexDirection = FlexDirection.Column;
            m_ClipContentContainer.style.alignItems = Align.Stretch;
            // m_ClipContentContainer.style.width = 1000;
            m_ClipContainer.contentContainer.style.width = 1000;
            // m_ClipContainer.contentContainer.style.height = 600;

            for (int i = 0; i < 10; ++i)
            {
                VisualElement clipRow = new VisualElement();
                clipRow.style.height = 22;
                clipRow.style.borderBottomColor = Color.white;
                clipRow.style.borderBottomWidth = 1;
                VisualElement clip = new VisualElement();
                clip.style.backgroundColor = new StyleColor(UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1));
                clip.style.left = 200;
                clip.style.width = UnityEngine.Random.Range(100, 300);
                clip.style.flexGrow = 1;
                clipRow.Add(clip);
                m_ClipContentContainer.Add(clipRow);
            }
        }

        // // 当右侧滚动时，同步左侧的垂直偏移
        // void OnRightScroll(WheelEvent e)
        // {
        //     if (syncingScroll) return;
        //     try
        //     {
        //         syncingScroll = true;
        //         // rightScroll.scrollOffset 是 Vector2 (x, y)
        //         var rOff = rightScroll.scrollOffset;
        //         // 只把 y 同步给左侧，保留左侧原 x
        //         leftScroll.scrollOffset = new Vector2(leftScroll.scrollOffset.x, rOff.y);
        //         // Debug.Log($"rightScroll.scrollOffset={rightScroll.scrollOffset}\nleftScroll.scrollOffset={leftScroll.scrollOffset}");
        //     }
        //     finally { syncingScroll = false; }
        // }

        // // 当左侧滚动时，同步右侧的垂直偏移（通常用户不会在左侧滚动多数，但为健壮性处理）
        // void OnLeftScroll(WheelEvent e)
        // {
        //     if (syncingScroll) return;
        //     try
        //     {
        //         syncingScroll = true;
        //         var lOff = leftScroll.scrollOffset;
        //         rightScroll.scrollOffset = new Vector2(Mathf.Max(0, rightScroll.scrollOffset.x), lOff.y);
        //     }
        //     finally { syncingScroll = false; }
        // }

        #endregion

        //调整结束标志线。
        private void OnAdjustEndFlag(double _visibleStartTime, float _pixelsPerSecond)
        {
            /*取内容区域是最严谨的做法，但也要看实际需求和UI设计，因为可能需要给Clip内容容器加上一点点marginRight以便在滑动到最右侧时也能看到结束边界，更符合直觉。*/
            // Rect clipContentRect = m_ClipContentContainer.contentRect;
            Rect clipContentRect = m_ClipContainer.contentContainer.contentRect;
            //时间尺的有限宽度指的是本身宽度除去垂直滚动条的宽度，因为ScrollView的内容区域本身也是去除了这部分宽度的，而且从感觉来看也不应该在滚动条上出现结束标志线。
            float rulerEffectiveWidth = timeRuler.contentRect.width - m_ClipContainer.Q<Scroller>(className: "unity-scroller--vertical").resolvedStyle.width;
            //比较右边界时刻，注意时间尺和Clip内容容器左边界都是从0开始的。
            //Tip：这个时间比较肯定存在误差，只不过表现到UI界面上也就一两个像素，没啥区别
            //Ques：可能限制在时间尺的左边界和（有效）右边界之间是最严谨的，不过缩放会带来很多意外情况，还需要进一步测试
            // if (_visibleStartTime + rulerEffectiveWidth / _pixelsPerSecond >= clipContentRect.width / _pixelsPerSecond)
            if (_visibleStartTime + rulerEffectiveWidth / _pixelsPerSecond >= clipContentRect.width / _pixelsPerSecond
            && clipContentRect.width / _pixelsPerSecond > _visibleStartTime)
            {
                m_EndFlag.style.visibility = Visibility.Visible;
                //前提是保持同步。那么通过时间与像素的映射就可以直接计算出各部分的宽度或各位置的时刻。
                // m_EndFlag.style.left = (clipContentRect.width / _pixelsPerSecond - _visibleStartTime) * _pixelsPerSecond;
                m_EndFlag.style.left = clipContentRect.width - (float)(_visibleStartTime * _pixelsPerSecond);
                // m_EndFlag.style.left = Mathf.Floor(Mathf.Floor(clipContentRect.width) - (float)Math.Floor(_visibleStartTime * _pixelsPerSecond));
                // m_EndFlag.style.bottom = 
            }
            else
            {
                m_EndFlag.style.visibility = Visibility.Hidden;
            }
        }

        // /*绘制结束时刻的标记线*/
        // private void OnGenerateEndFlag(MeshGenerationContext mgc)
        // {
        //     var painter = mgc.painter2D;
        //     painter.lineWidth = 1f;
        //     painter.strokeColor = Color.blue;
        // }
    }
}