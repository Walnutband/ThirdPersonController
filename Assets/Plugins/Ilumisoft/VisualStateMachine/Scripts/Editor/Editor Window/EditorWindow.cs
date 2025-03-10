namespace Ilumisoft.VisualStateMachine.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The State Machine Editor Window used to edit a StateMachine Monobhevaiour
    /// 基于该类自定义的状态机编辑窗口用于编辑状态机组件
    /// </summary>
    public class EditorWindow : UnityEditor.EditorWindow
    {
        /// <summary>
        /// Data container of the window窗口的数据容器
        /// </summary>
        public Context Context { get; private set; } = new Context();

        /// <summary>
        /// The GUI of the editor window
        /// </summary>
        [System.NonSerialized] private EditorWindowGUI editorWindowGUI;

        /// <summary>
        /// Returns true if the window has been enabled
        /// </summary>
        private bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the rect of the view in its parent
        /// </summary>
        public Rect Rect
        {
            get { return new Rect(0, 0, this.position.width, this.position.height); }
        }

        /*关于EditorWindow的OnEnable方法何时被调用
        EditorWindow 的 OnEnable 方法会在以下情况下被调用：
        窗口首次打开时：当你通过菜单或代码打开一个 EditorWindow 时，会调用 OnEnable 方法。此时可以进行一些初始化操作。
        进入 Play 模式或编辑模式：当你在 Unity 编辑器中切换到 Play 模式或返回编辑模式时，已经打开的 EditorWindow 的 OnEnable 方法会被调用。这适用于需要在不同模式下重新初始化一些内容的情况。
        重新编译脚本后：当你在 Unity 编辑器中更改并重新编译代码时，OnEnable 方法也会被调用。这在脚本重新加载后可以重新进行一些初始化操作。
        */

        /// <summary>
        /// Initializes the editor window初始化编辑窗口
        /// </summary>
        private void OnEnable()
        {
            this.wantsMouseMove = true; //窗口内是否接受鼠标移动事件

            this.editorWindowGUI = new EditorWindowGUI(this);

            Context.Reload();

            IsEnabled = true;
        }

        /// <summary>
        /// Repaint the window regularly（定期） when Unity is in Playmode
        /// </summary>
        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }

        /// <summary>
        /// Repaint the window on inspector updates
        /// </summary>
        private void OnInspectorUpdate() //每秒10帧调用该方法
        {
            Repaint();
        }

        /// <summary>
        /// Reloads the context if the hierarchy changed 
        /// </summary>
        private void OnHierarchyChange() //其实没看懂为何层级视图变化时要重载这个，大概是因为有些联系我还没发现
        {
            Context.Reload();
        }

        /// <summary>
        /// Updates the context if a state machine is selected
        /// 选中一个状态机时更新context数据
        /// </summary>
        private void OnSelectionChange()
        {
            if (IsEnabled)
            {
                Context.UpdateSelection();
            }
        }

        /// <summary>
        /// Reloads the context if the editor window is enabled and gets focused
        /// </summary>
        private void OnFocus()
        {
            if (IsEnabled)
            {
                Context.Reload();
            }
        }

        /// <summary>
        /// Draws the Graph and processes Input
        /// </summary>
        private void OnGUI()
        {
            this.editorWindowGUI.OnGUI();
        }
    }
}