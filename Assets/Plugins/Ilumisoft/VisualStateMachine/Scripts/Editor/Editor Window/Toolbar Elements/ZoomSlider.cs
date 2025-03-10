namespace Ilumisoft.VisualStateMachine.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class ZoomSlider : ToolbarElement
    {
        float zoomFactor;

        public ZoomSlider(EditorWindow window) : base(window)
        {
            this.Width = 185;
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginDisabledGroup(!Context.IsStateMachineLoaded);
            {
                zoomFactor = Context.ZoomFactor;

                var zoomSliderRect = new Rect((rect.width - this.Width - 50) / 2 + 50, rect.y, this.Width, rect.height);
                //为标签空出了50个单位长
                GUI.Label(new Rect((rect.width - this.Width - 50) / 2, rect.y, 50, rect.height), "Zoom");
                //默认设置为0.5~1.5
                zoomFactor = GUI.HorizontalSlider(zoomSliderRect, zoomFactor, ZoomSettings.MinZoomFactor, ZoomSettings.MaxZoomFactor);
                //返回最近整数。这里的简单处理非常巧妙，就可以保证每次滑动都是以10%为间隔
                zoomFactor = Mathf.Round(zoomFactor * 10) / 10;
                //以百分比形式显示，更加直观
                GUI.Label(new Rect(zoomSliderRect.xMax + 10, rect.y, 50, rect.height), $"{zoomFactor * 100}%");

                Context.ZoomFactor = zoomFactor;
            }
            EditorGUI.EndDisabledGroup();

            //Zoom in/out when the scroll wheel has been moved
            //当 Event.current.type == EventType.ScrollWheel 时，表示当前事件类型是滚轮滚动事件。这通常用于检测用户 在GUI界面上 使用鼠标滚轮进行滚动的操作。
            if (Context.IsStateMachineLoaded && Event.current.type == EventType.ScrollWheel)
            {
                Context.ZoomFactor -= Mathf.Sign(Event.current.delta.y) * ZoomSettings.MaxZoomFactor / 20.0f;

                Event.current.Use();
            }
        }
    }
}