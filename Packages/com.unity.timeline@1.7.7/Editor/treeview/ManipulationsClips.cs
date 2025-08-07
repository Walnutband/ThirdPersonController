using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// 双击某些类型的剪辑（比如子 Timeline 的 Clip）时，进入该子序列做深层编辑：切换 state.editSequence
    /// </summary>
    class DrillIntoClip : Manipulator
    {
        protected override bool DoubleClick(Event evt, WindowState state)
        {
            if (evt.button != 0)
                return false;

            var guiClip = PickerUtils.TopmostPickedItem() as TimelineClipGUI;

            if (guiClip == null)
                return false;
            //现在总共两种情况，
            // 双击AnimationTrack上的Clip会打开Animation窗口编辑该Animation
            // 双击SubTimeline的Clip，会进入到该子Timeline进行编辑。
            if (!TimelineWindow.instance.state.editSequence.isReadOnly && (guiClip.clip.curves != null || guiClip.clip.animationClip != null))
                Invoker.Invoke<EditClipInAnimationWindow>(new[] { guiClip.clip });

            if (guiClip.supportsSubTimelines)
                Invoker.Invoke<EditSubTimeline>(new[] { guiClip.clip });

            return true;
        }
    }

    /// <summary>
    /// 右键点击空白或轨道头部，弹出全局上下文菜单（新建轨道、清空等命令）。
    /// </summary>
    class ContextMenuManipulator : Manipulator
    {
        protected override bool MouseDown(Event evt, WindowState state)
        {
            if (evt.button == 1) //鼠标右键
                ItemSelection.HandleSingleSelection(evt);

            return false;
        }

        protected override bool ContextClick(Event evt, WindowState state)
        {
            if (evt.alt)
                return false;

            var selectable = PickerUtils.TopmostPickedItem() as ISelectable;

            if (selectable != null && selectable.IsSelected())
            {
                SequencerContextMenu.ShowItemContextMenu(evt.mousePosition);
                return true;
            }

            return false;
        }
    }
}
