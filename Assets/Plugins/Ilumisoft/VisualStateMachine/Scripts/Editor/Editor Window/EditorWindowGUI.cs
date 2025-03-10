namespace Ilumisoft.VisualStateMachine.Editor
{
    using UnityEngine;

    public class EditorWindowGUI
    {
        [System.NonSerialized] private readonly EditorWindow editorWindow; //构造时传入

        /// <summary>
        /// The Toolbar at the top of the window在窗口顶部的工具栏
        /// </summary>
        private Toolbar Toolbar { get; set; }

        /// <summary>
        /// The StateMachine graph
        /// </summary>
        private GraphView Graph { get; set; }

        public EditorWindowGUI(EditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;

            this.Graph = new GraphView(this.editorWindow);

            this.Toolbar = new Toolbar(this.editorWindow);
        }

        /// <summary>
        /// Draws the Graph and processes Input绘制界面并处理输入
        /// </summary>
        public void OnGUI() //没有继承，并非消息方法，而是被EditorWindow实现的消息方法OnGUI所调用
        {
            var rect = editorWindow.Rect;

            if (Event.current.type == EventType.Repaint)
            {
                this.Graph.Repaint(rect);
            }

            this.Toolbar.OnGUI(editorWindow.Rect);

            if (Event.current.isMouse ||
                Event.current.isKey ||
                Event.current.isScrollWheel ||
                Event.current.rawType == EventType.MouseUp)
            {
                this.Graph.ProcessEvents(editorWindow.Rect);
            }

            if (GUI.changed)
            {
                editorWindow.Repaint();
            }
        }
    }
}