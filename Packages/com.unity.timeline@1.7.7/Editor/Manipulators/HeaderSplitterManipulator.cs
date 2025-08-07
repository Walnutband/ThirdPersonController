using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// 就是在Track和Clip两个区域中间的竖线，可以拖拽改变两个区域的大小，也就是在整个TimelineWindow中的占比。
    /// </summary>
    class HeaderSplitterManipulator : Manipulator
    {
        bool m_Captured;

        protected override bool MouseDown(Event evt, WindowState state)
        {
            Rect headerSplitterRect = state.GetWindow().headerSplitterRect; //就是中间竖线的Rect。
            // Debug.Log($"HeaderSplitterManipulator.MouseDown: \nheaderSplitterRect: {headerSplitterRect}, mousePosition: {evt.mousePosition}");
            //Tip：这里的mousePosition和Rect都是相对于窗口坐标系，左上角为原点，不包含窗口标题那一栏。
            if (headerSplitterRect.Contains(evt.mousePosition))
            {
                m_Captured = true;
                state.AddCaptured(this);
                return true;
            }

            return false;
        }

        protected override bool MouseDrag(Event evt, WindowState state)
        {
            if (!m_Captured)
                return false;
            //Tip：虽然竖线很窄，但终究有一段宽度，而鼠标可以在竖线范围内，所以不能认为sequencerHeaderWidth就严格等于竖线左边界左边的区域宽度。
            state.sequencerHeaderWidth = evt.mousePosition.x;
            return true;
        }

        protected override bool MouseUp(Event evt, WindowState state)
        {
            if (!m_Captured)
                return false;

            state.RemoveCaptured(this);
            m_Captured = false;

            return true;
        }

        public override void Overlay(Event evt, WindowState state)
        {
            Rect rect = state.GetWindow().sequenceRect;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SplitResizeLeftRight);
        }
    }
}
