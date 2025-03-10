namespace Ilumisoft.VisualStateMachine.Editor
{
    using Ilumisoft.VisualStateMachine.Editor.Extensions;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(StateMachine))]
    public class StateMachineInspector : Editor
    {
        //Foldout values
        private bool showStates = false; //（展开）显示各状态（默认不展开，下同）
        private bool showTransitions = false; //（展开）显示各转换
        private string stateSearchFilter = string.Empty; //状态搜索框
        private string transitionSearchFilter = string.Empty; //转换搜索框

        //Reference to the inspected state machine对于检视对象（StateMachine）的引用
        private StateMachine stateMachine = null;

        private Graph graph = null;

        //GUI Contents of the buttons
        private GUIContent selectButtonContent = new GUIContent("Select");
        private GUIContent openButtonContent = new GUIContent("Open");
        //折叠面板的标题
        private GUIContent stateListFoldoutContent = new GUIContent("States", "All states of the state machine");

        private void OnEnable()
        {//该事件用于在场景的层级视图发生变化时触发。这包括任何影响层级视图结构的操作，比如添加、删除或重新排列 GameObject等等。
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            UpdateCache();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// Gets all states and transitions of the inspected State Machine
        /// </summary>
        private void UpdateCache()
        {
            //强制转换，因为target原本是Object类型。这里省略this也是一样的结果。
            this.stateMachine = (StateMachine)this.target;

            if (this.stateMachine != null)
            {
                this.graph = this.stateMachine.GetStateMachineGraph();
            }
        }

        /// <summary>
        /// Updates the cache and triggers a repaint when the hierarchy has changed
        /// </summary>
        private void OnHierarchyChanged()
        {
            UpdateCache();

            Repaint(); //重绘检视面板
        }

        public override void OnInspectorGUI()
        {
            if (this.stateMachine == null)
            {
                return;
            }

            this.serializedObject.Update(); //获取最新序列化数据

            GUILayoutUtils.VerticalSpace(8);

            DrawGraphButton();

            GUILayoutUtils.VerticalSpace(4);

            DrawStateList();
            DrawTransitionList();

            this.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the button to open the State Machine Graph Editor
        /// </summary>
        private void DrawGraphButton()
        {
            GUILayoutUtils.HorizontalGroup(() =>
            {
                EditorGUILayout.LabelField("Graph Editor");

                if (GUILayout.Button(this.openButtonContent, EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    var castedTarget = (this.target as StateMachine);

                    EditorWindowCommands.OpenStateMachineGraph(castedTarget);
                }
            });
        }

        /// <summary>
        /// Draws a list of all states of the state machine
        /// </summary>
        private void DrawStateList()
        {
            this.showStates = EditorGUILayout.Foldout(this.showStates, this.stateListFoldoutContent);

            if (this.showStates)
            {
                GUILayoutUtils.Ident(() =>
                {
                    //返回输入的字符串
                    this.stateSearchFilter = EditorGUILayout.TextField("Search", this.stateSearchFilter);

                    EditorGUILayout.Space();

                    string filterKeyword = this.stateSearchFilter.ToLower(); //转换为小写（就是默认英文？），说明这里不区分大小写

                    foreach (var node in this.graph.Nodes)
                    {
                        //is模式匹配，检查对象 node 是否为类型 State 的实例。如果是，将 node 转换为 State 类型并赋值给变量 state
                        if (node is State state)
                        {
                            GUILayoutUtils.HorizontalGroup(() =>
                            {
                                //包含该部分连续字符串
                                if (state.ID.ToLower().Contains(filterKeyword))
                                {
                                    EditorGUILayout.LabelField(state.ID); //在左边绘制状态ID
                                    //在同一行右边绘制按钮
                                    if (GUILayout.Button(this.selectButtonContent, EditorStyles.miniButton, GUILayout.Width(50)))
                                    {
                                        EditorWindowCommands.OpenStateMachineGraph(this.stateMachine).SelectState(state);
                                    }
                                }
                            });
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Daws a list of all transitions of the state machine
        /// </summary>
        private void DrawTransitionList()
        {
            this.showTransitions = EditorGUILayout.Foldout(this.showTransitions, "Transitions");

            if (this.showTransitions)
            {
                GUILayoutUtils.Ident(() =>
                {
                    this.transitionSearchFilter = EditorGUILayout.TextField("Search", this.transitionSearchFilter);

                    EditorGUILayout.Space();

                    string filterKeyword = this.transitionSearchFilter.ToLower();

                    foreach (var transition in this.graph.Transitions)
                    {
                        GUILayoutUtils.HorizontalGroup(() =>
                        {
                            if (transition != null && transition.ID.ToLower().Contains(filterKeyword))
                            {
                                EditorGUILayout.LabelField(transition.ID);

                                if (GUILayout.Button(this.selectButtonContent, EditorStyles.miniButton, GUILayout.Width(50)))
                                {
                                    EditorWindowCommands.OpenStateMachineGraph(this.stateMachine).SelectTransition(transition);
                                }
                            }
                        });
                    }
                });
            }
        }
    }
}