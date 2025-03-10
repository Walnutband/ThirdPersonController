namespace Ilumisoft.VisualStateMachine.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class GridButton : ToolbarElement
    {
        public GridButton(EditorWindow window) : base(window)
        {
            this.Width = 80;
        }

        public override void OnGUI(Rect rect)
        {//EditorGUI.BeginDisabledGroup 方法用于在 Unity 编辑器中禁用一组 GUI 控件（显示就是变灰，无法点击）。这在需要暂时禁用某些用户界面元素时非常有用，比如当某些条件未满足时，防止用户进行某些操作。
            EditorGUI.BeginDisabledGroup(!Context.IsStateMachineLoaded); //只要工具栏最左边状态机字段没有引用状态机，则会看到右边所有工具都变灰，且无法点击
            {//传入的rect.y为0，这里就是从窗口右上角水平左移Width个单位作为起点。
                rect = new Rect(rect.width - Width, rect.y, Width, rect.height);
                //竟然还能在传入参数中使用三元运算符（条件运算符）？
                if (GUI.Button(rect, Context.IsGridEnabled ? "Grid On" : "Grid Off", EditorStyles.toolbarButton))
                {//点开点关。这里的实现非常巧妙
                    Context.IsGridEnabled = !Context.IsGridEnabled;
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}