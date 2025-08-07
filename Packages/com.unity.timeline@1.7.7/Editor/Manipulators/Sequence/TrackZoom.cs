using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// 按住 Ctrl＋滚轮，实现垂直方向上轨道行高的批量缩放
    /// </summary>
    class TrackZoom : Manipulator
    {
        // only handles 'vertical' zoom. horizontal is handled in timelineGUI
        protected override bool MouseWheel(Event evt, WindowState state)
        {
            if (EditorGUI.actionKey)
            {
                state.trackScale = Mathf.Min(Mathf.Max(state.trackScale + (evt.delta.y * 0.1f), 1.0f), 100.0f);
                return true;
            }

            return false;
        }
    }
}
