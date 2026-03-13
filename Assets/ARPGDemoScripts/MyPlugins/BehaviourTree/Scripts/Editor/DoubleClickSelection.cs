using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MyPlugins.BehaviourTree.EditorSection
{
    public class DoubleClickSelection : MouseManipulator {
        double time;
        double doubleClickDuration = 0.3; //双击时间

        public DoubleClickSelection() {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget() {

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt) {
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration) { //在tap时间内即判定为双击
                SelectChildren(evt); //选中该节点及其所有子节点
            }

            time = EditorApplication.timeSinceStartup; //每次点击记录当前时间。
        }

        void SelectChildren(MouseDownEvent evt) {

            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;
            //满足停止条件，则返回true，确保不会意外终止操作。
            if (!CanStopManipulation(evt))
                return;

            NodeView clickedElement = evt.target as NodeView; //这是点击的视图节点，也就是作为父节点，然后选中其所有子节点
            if (clickedElement == null) {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
                Debug.Log("双击目标为NodeView的");
            }

            // Add children to selection so the root element can be moved
            BehaviourTree.Traverse(clickedElement.node, node => {
                var view = graphView.FindNodeView(node);
                graphView.AddToSelection(view);
            });
        }
    }
}