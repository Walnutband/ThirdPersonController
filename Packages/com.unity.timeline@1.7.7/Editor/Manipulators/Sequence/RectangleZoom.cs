using UnityEngine;

namespace UnityEditor.Timeline
{
    /*支持在标尺或轨道上按键＋拖拽做时间轴缩放（类似“区域缩放”），配合 TimelineZoomManipulator 或 TrackZoom 使用。
    在Clip区域按下Alt+右键就可以直接用鼠标滑动来改变时间轴缩放，就像在SceneView中同样的按键可以代替滚轮来实现视图缩放，更加方便有效，因为鼠标滚轮其实只适合小范围的缓慢变化*/
    class RectangleZoom : RectangleTool
    {
        protected override bool enableAutoPan { get { return true; } }

        protected override bool CanStartRectangle(Event evt)
        {
            return evt.button == 1 && evt.modifiers == (EventModifiers.Alt | EventModifiers.Shift);
        }

        protected override bool OnFinish(Event evt, WindowState state, Rect rect)
        {
            var x = state.PixelToTime(rect.xMin);
            var y = state.PixelToTime(rect.xMax);
            state.SetTimeAreaShownRange(x, y);

            return true;
        }
    }
}
