using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MyPlugins.BehaviourTree
{
    public class Timeout : DecoratorNode, ISerializationCallbackReceiver
    {
        public float duration = 1.0f;
        float startTime;

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            if (Time.time - startTime > duration) {
                return State.Failure;
            }

            return child.Update();
        }

        public void OnBeforeSerialize()
        {
            // Debug.Log("tree OnBeforeSerialize");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("Timeout OnAfterDeserialize");
        }
    }

    // [CustomEditor(typeof(Timeout))]
    // public class TimeoutEditor : Editor
    // {
    //     public override VisualElement CreateInspectorGUI()
    //     {
    //         Undo.undoRedoPerformed += () => {
    //             serializedObject.ApplyModifiedProperties();
    //         };
    //         // BehaviourTreeView.undoableOperation += () => serializedObject.Update();
            
    //         serializedObject.Update();
    //         // Debug.Log("调用了Update");
    //         VisualElement root = new VisualElement();

    //         // 添加默认的 Inspector GUI
    //         IMGUIContainer defaultInspector = new IMGUIContainer(() =>
    //         {
    //             DrawDefaultInspector(); //就是onGUIHandler委托
    //         });
    //         root.Add(defaultInspector);

    //         // Timeout timeout = target as Timeout;
    //         // Label childTypeName = new Label($"{(timeout.child == null ? "child为空" : timeout.child.GetType().Name)}");
    //         // root.Add(childTypeName);
    //         // EditorUtility.SetDirty(timeout);
    //         // serializedObject.ApplyModifiedProperties();
    //         // Debug.Log("调用了ApplyModifiedProperties");
    //         // timeout.child = serializedObject.FindProperty("child").objectReferenceValue as NodeData;
    //         return root;
    //     }
        
    // }

}