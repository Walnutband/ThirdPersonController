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

        public bool IsPrefabAsset //窗口当前加载的状态机所挂载的对象是否是预制体
        {
            get => IsStateMachineLoaded && PrefabUtility.IsPartOfAnyPrefab(StateMachine.gameObject);
        }

        public bool IsPlayMode => EditorApplication.isPlaying; //是否处于运行模式

        /// <summary>
        /// Gets the currently loaded StateMachine获取当前加载的状态机
        /// </summary>
        public StateMachine StateMachine
        {
            get => stateMachine;
            private set
            {//非空且非预制体，则会做三件事：获取给定状态机的Graph，获取首选项，最后获取状态机本身
                if (value != null)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(value))
                    {//不能是预制体？
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
        /// The list of selected states所选中状态的列表
        /// </summary>
        public List<Node> SelectedNodes { get; } = new List<Node>();

        /// <summary>
        /// The temporary preview of a not completely created transition
        /// 对于一个还没有完全创建的转换的临时预览
        /// </summary>
        public Node TransitionPreview
        {
            get => transitionPreview;
            set => transitionPreview = value;
        }

        //在渲染图形化界面时会根据以下对应的布尔变量来决定是否绘制相应的内容
        /// <summary>
        /// Enables or disables the grid of the graph启用或禁用网格
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
        /// If the value has changed, an event is fired.如果值发生了改变则触发一个事件
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

        /// <summary>
        /// 加载状态机的首选项
        /// </summary>
        /// <param name="stateMachine"></param>
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
        /// 搜索带有给定的实例ID的状态机
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        private bool TryFind(int instanceID, out StateMachine stateMachine)
        {
            stateMachine = null;
            //查找项目中所有类型为 StateMachine 的对象
            /*查找范围
            所有加载的场景：包括当前活动场景和所有已经加载的场景中的对象。
            所有资源：不仅包括场景中的对象，还包括在项目资源（Resources）文件夹中的资源。
            未激活的对象：包括那些在场景中未激活的对象（例如隐藏的游戏对象）。
            不可见的对象：包括那些在场景中不可见的对象。
            */
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
            if (Selection.activeGameObject != null) //点击空白处就会取消之前的选中，所以就会为null
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