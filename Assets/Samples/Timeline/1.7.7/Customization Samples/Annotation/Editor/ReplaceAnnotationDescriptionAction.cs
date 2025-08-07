using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Adds an additional item in context menus that will replace an annotation's description field
    // with the clipboard's contents.
    [MenuEntry("Replace description with clipboard contents")]
    public class ReplaceAnnotationDescriptionAction : MarkerAction
    {
        // Specifies the action's prerequisites:
        // - Invalid (grayed out in the menu) if no text content is in the clipboard;
        // - NotApplicable (not shown in the menu) if the current marker is not an Annotation.
        public override ActionValidity Validate(IEnumerable<IMarker> markers)
        {
            //必须保证选中的元素全部都是Annotation才会显示该菜单项。
            if (!markers.All(marker => marker is AnnotationMarker))
            {
                return ActionValidity.NotApplicable;
            }

            // get the current text content of the clipboard
            string clipboardTextContent = EditorGUIUtility.systemCopyBuffer;
            if (clipboardTextContent.Length == 0)
            {
                return ActionValidity.Invalid;
            }

            return ActionValidity.Valid;
        }

        // Sets the Annotation's description based on the contents of the clipboard.
        public override bool Execute(IEnumerable<IMarker> markers)
        {
            // get the current text content of the clipboard
            string clipboardTextContent = EditorGUIUtility.systemCopyBuffer;
            //将所有选中的Annotation的描述都设置为剪贴版内容
            foreach (AnnotationMarker annotation in markers.Cast<AnnotationMarker>()) //Cast可以将容器中所有元素转换为目标类型
            {
                annotation.description = clipboardTextContent;
            }

            return true;
        }

        // Assigns a shortcut to the action.
        [TimelineShortcut("Replace annotation description with clipboard", KeyCode.D)]
        public static void InvokeShortcut()
        {
            Invoker.InvokeWithSelectedMarkers<ReplaceAnnotationDescriptionAction>();
        }
    }
}
