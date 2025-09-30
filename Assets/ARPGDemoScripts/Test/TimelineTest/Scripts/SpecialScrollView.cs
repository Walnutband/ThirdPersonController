using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecialScrollView : VisualElement
{
    // Factory so it can be used from UXML if wanted
    public new class UxmlFactory : UxmlFactory<SpecialScrollView, UxmlTraits> { }

    // 内部元素
    private VisualElement viewport;          // 裁剪区
    private VisualElement contentRoot;       // 所有被控制元素的容器（水平控制通常移动此容器）
    private Scroller horizontalScrollbar;
    private Scroller verticalScrollbar;

    // 控制集合
    private List<VisualElement> horizontalTargets = new List<VisualElement>();
    private List<VisualElement> verticalTargets = new List<VisualElement>();

    // 滚动范围缓存（每个 target 的真实尺寸与偏移由外部预设）
    private float horizontalMax = 0f;
    private float verticalMax = 0f;

    public SpecialScrollView()
    {
        // 样式基础
        style.flexDirection = FlexDirection.Column;
        style.alignItems = Align.Stretch;

        // 创建viewport并开启裁剪
        viewport = new VisualElement { name = "viewport" };
        viewport.style.flexGrow = 1;
        viewport.style.position = Position.Relative;
        viewport.style.overflow = Overflow.Hidden; // 关键：裁剪
        Add(viewport);

        // contentRoot 放在 viewport 内，用于水平同时移动多个元素
        contentRoot = new VisualElement { name = "contentRoot" };
        contentRoot.style.position = Position.Absolute;
        contentRoot.style.left = 0;
        contentRoot.style.top = 0;
        viewport.Add(contentRoot);

        // 滚动条
        // horizontalScrollbar = new Scroller(default, default, default, SliderDirection.Horizontal) { name = "hScroll" };
        horizontalScrollbar = new Scroller(default, default, OnHorizontalValueChanged, SliderDirection.Horizontal) { name = "hScroll" };
        horizontalScrollbar.style.height = 14;
        // horizontalScrollbar.RegisterValueChangedCallback(OnHorizontalValueChanged);
        Add(horizontalScrollbar);

        verticalScrollbar = new Scroller(default, default, OnVerticalValueChanged, SliderDirection.Horizontal) { name = "vScroll" };
        verticalScrollbar.style.width = 14;
        // verticalScrollbar.RegisterValueChangedCallback(OnVerticalValueChanged);
        // 将竖直滚动条放到控件右上角，overlay 形式
        Add(verticalScrollbar);

        // 给竖直滚动条设置绝对定位，使其覆盖在右侧
        verticalScrollbar.style.position = Position.Absolute;
        verticalScrollbar.style.right = 0;
        verticalScrollbar.style.top = 0;
        verticalScrollbar.style.height = StyleKeyword.Auto; // 由外层高度决定

        // 避免默认大小导致遮挡滚条本身的内容
        this.RegisterCallback<GeometryChangedEvent>(evt => UpdateLayoutOnSizeChanged());
    }

    // 外部 API：把元素添加到 contentRoot（由用户调用或控件内部创建示例）
    public VisualElement AddControlledElement(VisualElement element)
    {
        element.style.position = Position.Absolute;
        contentRoot.Add(element);
        UpdateContentBounds();
        return element;
    }

    // Assign targets for horizontal movement (can be multiple)
    public void AssignHorizontalTargets(IEnumerable<VisualElement> targets)
    {
        horizontalTargets.Clear();
        horizontalTargets.AddRange(targets);
        UpdateHorizontalRange();
    }

    // Assign targets for vertical movement (can be multiple; typical场景为单个元素)
    public void AssignVerticalTargets(IEnumerable<VisualElement> targets)
    {
        verticalTargets.Clear();
        verticalTargets.AddRange(targets);
        UpdateVerticalRange();
    }

    // 设置 viewport 的“可视区域”高度（用于实现你描述的切换行为）
    // 当 horizontal mode active 时，你可以把 viewport 高度设置成上下两个元素总高度
    // 当 vertical mode active 时，你可以把 viewport 高度设置成下面元素高度（实现遮挡）
    public void SetViewportHeight(float height)
    {
        viewport.style.height = height;
        UpdateLayoutOnSizeChanged();
    }

    // 更新 contentRoot、scrollbar 范围
    private void UpdateContentBounds()
    {
        // 计算 contentRoot 的最小包围盒，根据 children 的 layout（top/left/width/height）
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        bool any = false;
        foreach (var ch in contentRoot.Children())
        {
            any = true;
            var left = ch.resolvedStyle.left;
            var top = ch.resolvedStyle.top;
            var w = ch.resolvedStyle.width;
            var h = ch.resolvedStyle.height;
            minX = Math.Min(minX, left);
            minY = Math.Min(minY, top);
            maxX = Math.Max(maxX, left + w);
            maxY = Math.Max(maxY, top + h);
        }

        if (!any)
        {
            contentRoot.style.width = 0;
            contentRoot.style.height = 0;
        }
        else
        {
            contentRoot.style.width = maxX - minX;
            contentRoot.style.height = maxY - minY;
        }

        UpdateHorizontalRange();
        UpdateVerticalRange();
    }

    private void UpdateLayoutOnSizeChanged()
    {
        // 当控件大小变更时重算滚动范围
        UpdateHorizontalRange();
        UpdateVerticalRange();

        // 垂直滚动条高度匹配 viewport 高度
        verticalScrollbar.style.height = viewport.resolvedStyle.height;
        verticalScrollbar.style.top = viewport.style.top;
    }

    // 计算水平最大滚动量：以被控制元素的最右边 - viewport.width
    private void UpdateHorizontalRange()
    {
        if (horizontalTargets.Count == 0)
        {
            horizontalMax = 0;
            horizontalScrollbar.highValue = 0;
            horizontalScrollbar.SetEnabled(false);
            return;
        }

        float leftMost = float.MaxValue, rightMost = float.MinValue;
        foreach (var t in horizontalTargets)
        {
            leftMost = Math.Min(leftMost, t.resolvedStyle.left);
            rightMost = Math.Max(rightMost, t.resolvedStyle.left + t.resolvedStyle.width);
        }

        float visibleW = viewport.resolvedStyle.width;
        horizontalMax = Mathf.Max(0f, rightMost - leftMost - visibleW);
        horizontalScrollbar.highValue = horizontalMax;
        horizontalScrollbar.SetEnabled(horizontalMax > 0f);
    }

    // 计算竖直最大滚动量：这里按 target 的包围盒计算
    private void UpdateVerticalRange()
    {
        if (verticalTargets.Count == 0)
        {
            verticalMax = 0;
            verticalScrollbar.highValue = 0;
            verticalScrollbar.SetEnabled(false);
            return;
        }

        float topMost = float.MaxValue, bottomMost = float.MinValue;
        foreach (var t in verticalTargets)
        {
            topMost = Math.Min(topMost, t.resolvedStyle.top);
            bottomMost = Math.Max(bottomMost, t.resolvedStyle.top + t.resolvedStyle.height);
        }

        float visibleH = viewport.resolvedStyle.height;
        verticalMax = Mathf.Max(0f, bottomMost - topMost - visibleH);
        verticalScrollbar.highValue = verticalMax;
        verticalScrollbar.SetEnabled(verticalMax > 0f);
    }

    // 当水平滚动值变化：同时移动 horizontalTargets（为了性能，这里我们移动 contentRoot）
    private void OnHorizontalValueChanged(ChangeEvent<float> evt)
    {
        float value = evt.newValue;
        // 将 contentRoot 的 translate.x 设置为 -value（向左移动）
        // contentRoot.style.translate = new Translate(-value, contentRoot.style.translate.y ?? 0f, 0f);
        contentRoot.style.translate = new Translate(-value, contentRoot.style.translate.value.y, 0f);
    }

    private void OnHorizontalValueChanged(float _newValue)
    {
        float value = _newValue;
        // 将 contentRoot 的 translate.x 设置为 -value（向左移动）
        // contentRoot.style.translate = new Translate(-value, contentRoot.style.translate.y ?? 0f, 0f);
        contentRoot.style.translate = new Translate(-value, contentRoot.style.translate.value.y, 0f);
    }

    // 当垂直滚动值变化：移动 verticalTargets 中的每个元素（或只移动其中单个）
    private void OnVerticalValueChanged(ChangeEvent<float> evt)
    {
        float value = evt.newValue;
        // 我们将竖直滚动直接应用到每个 vertical target 的 translate.y（相对其初始 top）
        foreach (var t in verticalTargets)
        {
            // translate.y = -value 保持 top 不变但视觉上滚动
            // t.style.translate = new Translate(t.style.translate.value.x ?? 0f, -value, 0f);
            t.style.translate = new Translate(t.style.translate.value.x, -value, 0f);
        }
    }
    private void OnVerticalValueChanged(float _newValue)
    {
        float value = _newValue;
        // 我们将竖直滚动直接应用到每个 vertical target 的 translate.y（相对其初始 top）
        foreach (var t in verticalTargets)
        {
            // translate.y = -value 保持 top 不变但视觉上滚动
            // t.style.translate = new Translate(t.style.translate.x ?? 0f, -value, 0f);
            t.style.translate = new Translate(t.style.translate.value.x, -value, 0f);
        }
    }

    // helper: 允许外部设置水平滚动值
    public void SetHorizontalValue(float v)
    {
        horizontalScrollbar.value = Mathf.Clamp(v, 0, horizontalMax);
    }

    // helper: 允许外部设置竖直滚动值
    public void SetVerticalValue(float v)
    {
        verticalScrollbar.value = Mathf.Clamp(v, 0, verticalMax);
    }

    // 示例初始化辅助（在运行时用来创建两个示例元素）
    public void CreateDemoElements()
    {
        // Clear existing
        contentRoot.Clear();

        var topBox = new VisualElement { name = "topBox" };
        topBox.style.left = 0;
        topBox.style.top = 0;
        topBox.style.width = 600;
        topBox.style.height = 150;
        topBox.style.backgroundColor = new Color(0.8f, 0.8f, 1f);
        contentRoot.Add(topBox);

        var bottomBox = new VisualElement { name = "bottomBox" };
        bottomBox.style.left = 0;
        bottomBox.style.top = 160; // 放在下面，跟 topBox 有间隔
        bottomBox.style.width = 600;
        bottomBox.style.height = 300;
        bottomBox.style.backgroundColor = new Color(0.8f, 1f, 0.8f);
        contentRoot.Add(bottomBox);

        // 把contentRoot的尺寸重算并初始viewport高度为上下两者高度（示例：水平时）
        UpdateContentBounds();

        // 默认配置：水平控制两个元素同时移动；竖直只控制 bottomBox
        AssignHorizontalTargets(new[] { topBox, bottomBox });
        AssignVerticalTargets(new[] { bottomBox });

        // 初始 viewport 高度示例：水平滚动时可视区域等于上下两个元素原本所占的区域
        float combinedHeight = (bottomBox.resolvedStyle.top + bottomBox.resolvedStyle.height);
        SetViewportHeight(combinedHeight);

        // 把竖直滚动条放在右侧，横条在底部（布局可能需微调）
        UpdateLayoutOnSizeChanged();
    }
}
