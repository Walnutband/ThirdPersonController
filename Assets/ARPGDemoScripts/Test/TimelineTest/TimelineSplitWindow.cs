
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{

    /// <summary>
    /// 最小示例：左右两栏轨道视图对齐与垂直滚动同步（UI Toolkit，适配 Unity 2022.3）.
    /// - 左侧固定宽度，显示轨道名行
    /// - 右侧可水平滚动，显示对应的轨道内容行（此处用彩色条模拟片段）
    /// - 两个垂直滚动视图（leftScroll, rightScroll）垂直偏移实时同步
    /// </summary>
    public class TimelineSplitWindow : EditorWindow
    {
        // 模拟数据模型：若干轨道，每个轨道若干片段（仅用于布局示例）
        class ClipModel { public double start; public double duration; public Color color; }
        class TrackModel { public string name; public float height = 28f; public List<ClipModel> clips = new List<ClipModel>(); }

        List<TrackModel> tracks = new List<TrackModel>();

        // UI 元素引用
        ScrollView leftScroll;    // 左侧垂直滚动（只垂直）
        ScrollView rightScroll;   // 右侧可水平+垂直滚动（右侧为主内容）
        VisualElement leftContent; // 左侧行容器
        VisualElement rightContent;// 右侧行容器（每一行包含 background + clips container）

        VisualElement leftBottomPlaceholder;

        // 防止同步时互相触发回调的标志
        bool syncingScroll = false;

        // 窗口菜单
        [MenuItem("Window/Timeline Split Demo")]
        public static void Open()
        {
            var wnd = GetWindow<TimelineSplitWindow>();
            wnd.titleContent = new GUIContent("Timeline Split Demo");
            wnd.minSize = new Vector2(700, 300);
        }

        // void OnEnable()
        void CreateGUI()
        {
            // 生成示例数据
            BuildSampleTracks();

            // 清理并构建根 UI
            rootVisualElement.Clear();

            // 顶层横向容器，放左右两列
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexGrow = 1;
            rootVisualElement.Add(container);

            // ------- 左侧面板（轨道名） -------
            leftScroll = new ScrollView(ScrollViewMode.Vertical);
            // leftScroll.style.width = 300; // 固定宽度
            // leftScroll.style.overflow = Overflow.Hidden; 
            // leftScroll.showHorizontal = false;
            leftScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            // leftScroll.showVertical = true;
            // leftScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            leftScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            // leftScroll.verticalScrollerVisibility = ScrollerVisibility.VisibleAlways;
            leftScroll.AddToClassList("left-scroll");


            //Tip：设置一个容器对象LeftView
            VisualElement leftView = new VisualElement() { name = "LeftView" };
            leftView.Add(leftScroll);
            container.Add(leftView);
            // container.Add(leftScroll);
            
            leftView.style.width = 300;
            leftView.style.flexShrink = 0f;
            leftScroll.style.flexGrow = 1f;

            leftContent = new VisualElement();
            leftContent.style.flexDirection = FlexDirection.Column;
            leftScroll.contentContainer.Add(leftContent);
            // leftScroll.style.flexGrow = 1f;

            // ------- 右侧面板（时间 / 轨道内容） -------
            // 我们把水平滚动放在右侧 ScrollView，右侧既有水平也有垂直滚动
            rightScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rightScroll.style.flexGrow = 1; // 占据剩余空间
            // rightScroll.showHorizontal = true;
            // rightScroll.showVertical = true;
            rightScroll.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            rightScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            // rightScroll.horizontalScrollerVisibility = ScrollerVisibility.VisibleAsNeeded;
            // rightScroll.verticalScrollerVisibility = ScrollerVisibility.VisibleAlways;
            rightScroll.AddToClassList("right-scroll");

            //Tip：设置一个容器对象RightView
            VisualElement rightView = new VisualElement() { name = "RightView" };
            rightView.Add(rightScroll);
            container.Add(rightView);
            // container.Add(rightScroll);

            // tracks container 包含每一行（背景 + clips）
            rightContent = new VisualElement();
            rightContent.style.flexDirection = FlexDirection.Column;
            rightScroll.contentContainer.Add(rightContent);

            // 右侧内容容器：先放一个时间尺占位（高度），然后放轨道行容器
            // var rulerPlaceholder = new Label("Ruler area (placeholder)");
            // rulerPlaceholder.style.height = 26;
            // rulerPlaceholder.style.unityTextAlign = TextAnchor.MiddleLeft;
            // rulerPlaceholder.style.paddingLeft = 6;
            // rulerPlaceholder.style.backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f));
            // rightScroll.contentContainer.Add(rulerPlaceholder);

            /*Tip: 为左右视图上方添加占位形*/
            VisualTreeAsset placeholder = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ARPGDemoScripts/Test/TimelineTest/Placeholder.uxml");
            VisualElement left = placeholder.CloneTree();
            /*Tip：注意这里的flexShrink设置非常关键，目标就是让上下的占位形不减小自己的高度，而是让中间的ScrollView减小高度。*/
            left.style.flexShrink = 0f;
            VisualElement right = placeholder.CloneTree();
            right.style.flexShrink = 0f;
            // rightScroll.Add(placeholder.CloneTree());
            // leftScroll.Insert(0, placeholder.CloneTree());
            // leftScroll.hierarchy.Insert(0, placeholder.CloneTree());
            leftView.hierarchy.Insert(0, left);
            leftView.style.borderRightWidth = 2; leftView.style.borderRightColor = new StyleColor(new Color(1f, 1f, 1f));
            // rightScroll.hierarchy.Insert(0, placeholder.CloneTree());
            rightView.hierarchy.Insert(0, right);

            /*Tip：偶然发现，tm的同一个VisualElement使用Add添加到其他VisualElement中，竟然是同一个，并不会克隆，所以上面用了rulerPlaceholder，下面再使用Add添加的话，就相当于
            把上面的rulerPlaceholder添加到了下面，就是直接抢过来了，而且VisualElement也没有提供克隆的方法，要么重复写一遍构造的代码，要么最好的方式就是制作成Uxml文件，那么
            就可以利用Instantiate或CloneTree克隆UI元素了。*/

            // Label placeholder = new Label("Track rows (placeholder)");
            // placeholder.style = rulerPlaceholder.style;
            // leftScroll.contentContainer.Add(rulerPlaceholder).SendToBack();
            // leftScroll.contentContainer.Insert(0, rulerPlaceholder.);



            /*Tip：给左边滚动视图下方添加一个占位矩形，因为右边滚动视图下方有水平滚动条而左边没有，所以要保持竖直一致。*/
            // Label label = new Label();
            leftBottomPlaceholder = new VisualElement() { name = "Left Placeholder" };
            // label.style.height = rightScroll.Q<Scroller>(className: "unity-scroller--horizontal").style.height;
            // label.style.height = rightScroll.hierarchy[1].style.height;
            // Debug.Log($"{rightScroll.hierarchy[1]}\nheight : {rightScroll.hierarchy[1].style.height}");

            // leftBottomPlaceholder.style.height = 30; //测试出来的值，发现能够实现像素对齐。

            // label.style.height = rightScroll.Q<Scroller>(className: "unity-scroller--horizontal").worldBound.height;
            // label.style.borderTopColor = new StyleColor(new Color(1f, 1f, 1f));
            // label.style.borderTopWidth = 1;
            // label.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f));
            leftBottomPlaceholder.style.backgroundColor = new StyleColor(Color.gray);
            leftBottomPlaceholder.style.flexShrink = 0f;
            leftScroll.hierarchy.Add(leftBottomPlaceholder);
            // leftView.hierarchy.Add(leftBottomPlaceholder);
            leftBottomPlaceholder.RegisterCallback<GeometryChangedEvent>(e => OnGeometryChanged_LeftBottomPlaceholder(e));

            // 构建行（左右分别创建对应行，保证高度一致）
            BuildRows();

            // 注册滚动同步回调（通过 ScrollEvent 监听）
            // 当右侧垂直滚动时，把左侧垂直偏移同步过去；反之亦然
            // rightScroll.RegisterCallback<ScrollEvent>(e => OnRightScroll(e));
            rightScroll.RegisterCallback<WheelEvent>(e => OnRightScroll(e));
            // leftScroll.RegisterCallback<ScrollEvent>(e => OnLeftScroll(e));
            leftScroll.RegisterCallback<WheelEvent>(e => OnLeftScroll(e));

            // 基本样式（可按需扩展）
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TimelineSplitStyles.uss");
            if (styleSheet != null) rootVisualElement.styleSheets.Add(styleSheet);
        }

        void OnDisable()
        {
            // 清理（移除回调等）
        }

        // 构造模拟轨道数据
        void BuildSampleTracks()
        {
            tracks.Clear();
            System.Random r = new System.Random(12345);
            for (int i = 0; i < 30; ++i)
            {
                var t = new TrackModel
                {
                    name = $"Track {i}",
                    height = 32 + (i % 3 == 0 ? 8 : 0) // 不同高度演示
                };
                int clipCount = 1 + (i % 4);
                for (int k = 0; k < clipCount; ++k)
                {
                    t.clips.Add(new ClipModel
                    {
                        start = k * 1.5 + (i % 3) * 0.2,
                        duration = 0.6 + (k * 0.3),
                        color = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1f)
                    });
                }
                tracks.Add(t);
            }
        }

        // 构建左右两边的行，保证 index 对应
        void BuildRows()
        {
            leftContent.Clear();
            rightContent.Clear();

            for (int i = 0; i < tracks.Count; ++i)
            {
                var track = tracks[i];

                // --- 左侧行：轨道名 ---
                var leftRow = new VisualElement();
                leftRow.style.height = track.height;
                // leftRow.style.flexDirection = FlexDirection.Row;
                leftRow.style.flexDirection = FlexDirection.Column;
                // leftRow.style.alignItems = Align.Center;
                leftRow.style.alignItems = Align.Stretch;
                leftRow.style.paddingLeft = 8;
                leftRow.style.unityTextAlign = TextAnchor.MiddleLeft;

                var label = new Label(track.name) { name = "TrackName" };
                // label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.flexGrow = 1;
                // label.style.borderBottomColor = new StyleColor(new Color(1f, 1f, 1f));
                // label.style.borderBottomWidth = 1;
                leftRow.Add(label);

                // 加一个分割线底部
                var leftDivider = new VisualElement() { name = "LeftDivider" };
                // leftDivider.style.height = 1;
                leftDivider.style.height = 2;
                // leftDivider.style.flexGrow = 1;
                // leftDivider.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                leftDivider.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f));
                // leftDivider.style.marginTop = 2;
                leftDivider.style.left = 0;
                leftDivider.style.right = 0;
                leftDivider.style.bottom = 0;
                leftDivider.style.position = Position.Absolute;
                leftRow.Add(leftDivider);

                leftContent.Add(leftRow);

                // --- 右侧行：轨道内容容器 ---
                var rightRow = new VisualElement() { name = "RightRow" };
                rightRow.style.height = track.height;
                rightRow.style.flexDirection = FlexDirection.Row;
                rightRow.style.alignItems = Align.FlexStart;
                rightRow.style.position = Position.Relative;
                // rightRow.style.backgroundColor = new StyleColor(new Color(0.08f, 0.08f, 0.08f)); //就是黑色
                // rightRow.style.backgroundColor = new StyleColor(Color.gray); //就是黑色

                // clipsContainer 存放 clip 的 VE（每个 clip 为一个子 VisualElement，使用 style.left/width 布局）
                var clipsContainer = new VisualElement() { name = "ClipsContainer" };
                clipsContainer.style.position = Position.Absolute;
                // 为演示，我们把右侧内容宽度设置得很大，模拟时间轴的横向可滚动区域（例如 2000 px）
                float contentWidth = 2000f;
                clipsContainer.style.left = 0;
                clipsContainer.style.top = 0;
                clipsContainer.style.height = track.height;
                clipsContainer.style.width = contentWidth;
                rightRow.Add(clipsContainer);

                // 为可视化每个 clip，计算 left/width（这里简单把时间映射到像素，假设 100 px / sec）
                float pxPerSecond = 100f;
                foreach (var clip in track.clips)
                {
                    var ce = new VisualElement();
                    ce.style.position = Position.Absolute;
                    ce.style.left = (float)(clip.start * pxPerSecond);
                    ce.style.top = 2;
                    ce.style.height = track.height - 6;
                    ce.style.width = (float)(clip.duration * pxPerSecond);
                    ce.style.backgroundColor = new StyleColor(clip.color);
                    ce.style.borderTopLeftRadius = 4;
                    ce.style.borderTopRightRadius = 4;
                    ce.style.borderBottomLeftRadius = 4;
                    ce.style.borderBottomRightRadius = 4;
                    ce.tooltip = $"start {clip.start:F2}s dur {clip.duration:F2}s";
                    clipsContainer.Add(ce);
                }

                // 在每个右行上添加一个细分竖线（作为分隔）
                var rowDivider = new VisualElement();
                /*Tip：之前很疑惑为啥rigihtRow的FlexDirection为Row，但是这里加入的rowDivider竟然是位于竖直下面的，这就是Column的表现，然后才发现这里的
                position设置为了Absolute，所以才会如此，而且这样还真的更合适，因为可以让片段的高度就作为整行轨道的高度，避免这里的分割线影响到像素对齐。*/
                rowDivider.style.position = Position.Absolute;
                rowDivider.style.left = 0;
                rowDivider.style.bottom = 0;
                rowDivider.style.width = contentWidth;
                // rowDivider.style.height = 1;
                rowDivider.style.height = 2;
                // rowDivider.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                rowDivider.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f));
                rightRow.Add(rowDivider);

                rightContent.Add(rightRow);
            }
        }

        // 当右侧滚动时，同步左侧的垂直偏移
        void OnRightScroll(WheelEvent e)
        {
            if (syncingScroll) return;
            try
            {
                syncingScroll = true;
                // rightScroll.scrollOffset 是 Vector2 (x, y)
                var rOff = rightScroll.scrollOffset;
                // 只把 y 同步给左侧，保留左侧原 x
                leftScroll.scrollOffset = new Vector2(leftScroll.scrollOffset.x, rOff.y);
                // Debug.Log($"rightScroll.scrollOffset={rightScroll.scrollOffset}\nleftScroll.scrollOffset={leftScroll.scrollOffset}");
            }
            finally { syncingScroll = false; }
        }

        // 当左侧滚动时，同步右侧的垂直偏移（通常用户不会在左侧滚动多数，但为健壮性处理）
        void OnLeftScroll(WheelEvent e)
        {
            if (syncingScroll) return;
            try
            {
                syncingScroll = true;
                var lOff = leftScroll.scrollOffset;
                rightScroll.scrollOffset = new Vector2(Mathf.Max(0, rightScroll.scrollOffset.x), lOff.y);
            }
            finally { syncingScroll = false; }
        }

        // 可选：在窗口大小变化或外部事件时刷新行高度/布局
        void RefreshRows()
        {
            // 如果你的 track height 动态变化，需要重新构建或只更新高度样式
            for (int i = 0; i < tracks.Count; ++i)
            {
                var leftRow = leftContent.ElementAt(i);
                var rightRow = rightContent.ElementAt(i);
                leftRow.style.height = tracks[i].height;
                rightRow.style.height = tracks[i].height;
            }
        }

        /*在布局计算完成之后再触发，就是让左边占位形与右边占位形保持高度一致。*/
        void OnGeometryChanged_LeftBottomPlaceholder(GeometryChangedEvent evt)
        {
            // StyleLength height = rightScroll.Q<Scroller>(className: "unity-scroller--horizontal").style.height;
            float height = rightScroll.Q<Scroller>(className: "unity-scroller--horizontal").worldBound.height;
            leftBottomPlaceholder.style.height = height;
            Debug.Log($"height: {height}");
        }
    }
}

