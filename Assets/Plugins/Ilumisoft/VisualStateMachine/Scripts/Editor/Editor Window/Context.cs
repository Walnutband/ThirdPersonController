namespace Ilumisoft.VisualStateMachine.Editor
{
    using Ilumisoft.VisualStateMachine.Editor.Extensions;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;


    [System.Serializable]
    public class Context
    {
        //[System.NonSerialized] 特性用于指示序列化机制（例如，Unity 的序列化系统或 JSON 序列化）不应序列化特定字段
        //缩放设置
        [System.NonSerialized] private ZoomSettings zoomSettings = new ZoomSettings();
        //拖拽设置
        [System.NonSerialized] private DragSettings dragSettings = new DragSettings();
        //网格（显示）设置
        [System.NonSerialized] private GridSettings gridSettings = new GridSettings();
        //标签（显示）设置
        [System.NonSerialized] private LabelSettings labelSettings = new LabelSettings();

        /// <summary>
        /// The instance ID of the last loaded StateMachine上一个加载的状态机的实例ID
        /// </summary>
        [SerializeField]
        private int instanceID = 0;

        [System.NonSerialized]
        private StateMachine stateMachine = null;

        public bool IsPrefabAsset
        {
            get => IsStateMachineLoaded && PrefabUtility.IsPartOfAnyPrefab(StateMachine.gameObject);
        }

        public bool IsPlayMode => EditorApplication.isPlaying;

        /// <summary>
        /// Gets the currently loaded StateMachine获取当前加载的状态机
        /// </summary>
        public StateMachine StateMachine
        {
            get => stateMachine;
            private set
            {
                if (value != null)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(value))
                    {
                        return;
                    }

                    Graph = value.GetStateMachineGraph();
                }

                LoadSettings(value);

                stateMachine = value;
            }
        }

        public Graph Graph { get; private set; }

        /// <summary>
        /// Gets or sets the selection rect
        /// </summary>
        public SelectionRect SelectionRect { get; set; } = new SelectionRect();

        [System.NonSerialized]
        private Node transitionPreview = null;

        /// <summary>
        /// Returns true if a StateMachine is loaded by the EditorWindow, false otherwise
        /// </summary>
        public bool IsStateMachineLoaded => (this.StateMachine != null);

        [System.NonSerialized]
        public GraphSelection GraphSelection;

        public Context()
        {
            GraphSelection = new GraphSelection(this);
        }

        /// <summary>
        /// The list of selected states
        /// </summary>
        public List<Node> SelectedNodes { get; } = new List<Node>();

        /// <summary>
        /// The temporary preview of a not completely created transition
        /// </summary>
        public Node TransitionPreview
        {
            get => transitionPreview;
            set => transitionPreview = value;
        }

        /// <summary>
        /// Enables or disables the grid of the graph
        /// </summary>
        public bool IsGridEnabled
        {
            get => this.gridSettings.IsEnabled;
            set => this.gridSettings.IsEnabled = value;
        }

        public bool ShowLabels
        {
            get => this.labelSettings.IsEnabled;
            set => this.labelSettings.IsEnabled = value;
        }

        /// <summary>
        /// Gets or sets the ZoomFactor of the graph.
        /// If the value has changed, an event is fired.
        /// </summary>
        public float ZoomFactor
        {
            get => this.zoomSettings.ZoomFactor;
            set => this.zoomSettings.ZoomFactor = value;
        }

        /// <summary>
        /// Gets or sets the DragOffset of the graph.
        /// If the value has changed, an event is fired.
        /// </summary>
        public Vector2 DragOffset
        {
            get => this.dragSettings.DragOffset;
            set => this.dragSettings.DragOffset = value;
        }

        /// <summary>
        /// Loads the given StateMachine
        /// </summary>
        /// <param name="stateMachine"></param>
        public void LoadStateMachine(StateMachine stateMachine)
        {
            //为空则为0，否则获取该组件的内置实例ID（Unity管理）
            this.instanceID = (stateMachine != null) ? stateMachine.GetInstanceID() : 0;
            //更新实例ID和实例
            this.StateMachine = stateMachine;

            SelectedNodes.Clear(); //清空被选择的状态
            TransitionPreview = null;
            SelectionRect.Reset(); //重置选择区域

            Reload();
        }

        private void LoadSettings(StateMachine stateMachine)
        {
            if (stateMachine != null)
            {
                var preferences = stateMachine.GetPreferences();

                zoomSettings = preferences.ZoomSettings;
                dragSettings = preferences.DragSettings;
                gridSettings = preferences.GridSettings;
                labelSettings = preferences.LabelSettings;
            }
            else
            {
                zoomSettings = new ZoomSettings();
                dragSettings = new DragSettings();
                gridSettings = new GridSettings();
                labelSettings = new LabelSettings();
            }
        }

        /// <summary>
        /// Reloads all cached data from the currently loaded StateMachine
        /// 重新加载来自于当前加载的状态机的所有缓存数据
        /// </summary>
        public void Reload()
        {
            if (IsStateMachineLoaded == false)
            {
                //通过制定的实例ID
                if (TryFind(this.instanceID, out StateMachine stateMachine))
                {
                    this.StateMachine = stateMachine;
                }
            }
        }


        /// <summary>
        /// Searches for the state machine with the given instanceID
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        private bool TryFind(int instanceID, out StateMachine stateMachine)
        {
            stateMachine = null;
            //查找项目中所有类型为 StateMachine 的对象
            var machines = Resources.FindObjectsOfTypeAll<StateMachine>();
            //遍历比较
            foreach (StateMachine machine in machines)
            {
                if (machine.GetInstanceID() == instanceID)
                {
                    stateMachine = machine;
                    return true;
                }
            }

            return false;
        }

        public void UpdateSelection()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject selection = Selection.activeGameObject;

                var stateMachine = selection.GetComponent<StateMachine>();
                //判断如果挂载了StateMachine组件即为状态机，就更新相关数据。
                //就和在层级视图中选中不同的Animator就会在Aniamtor窗口中自动切换显示选中对象的信息一样
                if (stateMachine != null)
                {
                    LoadStateMachine(stateMachine);
                }
            }
        }
    }
}