namespace Ilumisoft.VisualStateMachine.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class Toolbar
    {
        private readonly List<ToolbarElement> elements; //只读的工具栏元素

        /// <summary>
        /// Creates a new layer for the Toolbar
        /// </summary>
        /// <param name="editorWindow"></param>
        public Toolbar(EditorWindow editorWindow)
        {
            this.elements = new List<ToolbarElement>
            {
                new GridButton(editorWindow), //网格（开关）按钮
                new LabelButton(editorWindow), //标签（开关）按钮
                new ZoomSlider(editorWindow), //缩放滑动条
                new StateMachineField(editorWindow) //状态机引用字段
            };
        }

        /// <summary>
        /// Draws the toolbar
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        {
            if (!Event.current.isKey)
            {
                //窗口左上角开始，窗口宽度，固定高度（Unity内置工具栏的固定高度）
                GUI.BeginGroup(new Rect(0, 0, rect.width, EditorStyles.toolbar.fixedHeight), EditorStyles.toolbar);
                {
                    Rect toolbarRect = new Rect(0, 0, rect.width, EditorStyles.toolbar.fixedHeight);

                    //遍历调用每个工具栏元素各自实现的OnGUI方法
                    foreach (var element in this.elements)
                    {
                        element.OnGUI(toolbarRect);
                    }
                }
                GUI.EndGroup();
            }
        }
    }
}