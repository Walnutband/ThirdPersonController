namespace Ilumisoft.VisualStateMachine.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public static class GUILayoutUtils
    {
        /// <summary>
        /// 在垂直布局中添加一个指定高度的空间
        /// </summary>
        /// <param name="pixels"></param>
        public static void VerticalSpace(float pixels)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(pixels);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 在面板中临时缩进传入的委托所绘制的内容。
        /// </summary>
        /// <param name="action">传入一个委托，将被执行并应用缩进效果</param>
        public static void Ident(Action action)
        {//增加和减少 EditorGUI.indentLevel 的值，用于在编辑器中临时缩进内容
            EditorGUI.indentLevel++;

            action();

            EditorGUI.indentLevel--;
        }

        public static void HorizontalGroup(Action action)
        {
            GUILayout.BeginHorizontal();

            action();

            GUILayout.EndHorizontal();
        }
    }
}