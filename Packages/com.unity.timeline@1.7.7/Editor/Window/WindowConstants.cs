namespace UnityEditor.Timeline
{
    static class WindowConstants
    {
        public const float timeAreaYPosition = 19.0f; //数值上等于顶部playback高度，其实本意是时间轴横排区域的左上角坐标。
        public const float timeAreaHeight = 22.0f; //时间轴横排区域的高度。
        public const float timeAreaMinWidth = 50.0f; //缩小窗口最小能到达的宽度，因为timeArea水平上扩展到了整个窗口。
        public const float timeAreaShownRangePadding = 5.0f;

        public const float markerRowHeight = 18.0f; //Markers轨道的高度。
        public const float markerRowYPosition = timeAreaYPosition + timeAreaHeight; //固有的Markers一行区域，这里是其左上角点的Y坐标，相对于窗口坐标系，原点在左上角
        
        public const float defaultHeaderWidth = 315.0f; //左半边默认宽度，其实就是初始化宽度，因为初始总需要有一个值，然后就可以自由拖拽进行改变了。
        public const float defaultBindingAreaWidth = 40.0f;

        public const float minHeaderWidth = 195.0f;
        public const float maxHeaderWidth = 650.0f;
        public const float headerSplitterWidth = 6.0f;
        public const float headerSplitterVisualWidth = 2.0f;

        public const float maxTimeAreaScaling = 90000.0f;
        public const float timeCodeWidth = 100.0f; // Enough space to display up to 9999 without clipping

        public const float sliderWidth = 15;
        public const float shadowUnderTimelineHeight = 15.0f;
        public const float createButtonWidth = 70.0f;

        public const float selectorWidth = 23.0f;
        public const float cogButtonWidth = 25.0f;

        public const float trackHeaderBindingHeight = 18.0f;
        public const float trackHeaderButtonSize = 16.0f;
        public const float trackHeaderButtonPadding = 2.0f;
        public const float trackBindingMaxSize = 300.0f;
        public const float trackBindingPadding = 5.0f;

        public const float trackInsertionMarkerHeight = 1f;
        public const float trackResizeHandleHeight = 7f;
        public const float inlineCurveContentPadding = 2.0f;

        public const float playControlsWidth = 300;

        public const int autoPanPaddingInPixels = 50;

        public const float overlayTextPadding = 40.0f;
    }
}
