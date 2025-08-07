using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
    /*用于在轨道区域做框选（Marquee Select），通常与某个 Manipulator（如 SelectAndMoveItem）配合，实现按住拖拽选中多个剪辑／轨道。*/
    class RectangleSelect : RectangleTool
    {
        protected override bool enableAutoPan { get { return false; } }

        protected override bool CanStartRectangle(Event evt)
        {
            if (evt.button != 0 || evt.alt)
                return false;

            return PickerUtils.pickedElements.All(e => e is IRowGUI);
        }

        protected override bool OnFinish(Event evt, WindowState state, Rect rect)
        {
            var selectables = state.spacePartitioner.GetItemsInArea<ISelectable>(rect).ToList();

            if (!selectables.Any())
                return false;

            if (ItemSelection.CanClearSelection(evt))
                SelectionManager.Clear();

            foreach (var selectable in selectables)
            {
                ItemSelection.HandleItemSelection(evt, selectable);
            }

            return true;
        }
    }
}
