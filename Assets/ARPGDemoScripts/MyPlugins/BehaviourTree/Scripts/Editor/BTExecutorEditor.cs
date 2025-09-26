using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MyPlugins.BehaviourTree.EditorSection  //发现命名空间设置为Editor的话会与UnityEditor的Editor类型名冲突。
{
    // [CustomEditor(typeof(BehaviourTreeExecutor))]
    public class BTExecutorEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            Button openButton = new Button(BehaviourTreeEditor.OpenWindow);
            openButton.text = "打开编辑窗口";
            root.Add(openButton);

            PropertyField scriptField = new PropertyField(serializedObject.FindProperty("m_Script"), "脚本");
            scriptField.Bind(serializedObject);
            scriptField.SetEnabled(false);
            root.Add(scriptField);

            PropertyField treeField = new PropertyField(serializedObject.FindProperty("tree"), "行为树");
            treeField.Bind(serializedObject);
            root.Add(treeField);

            PropertyField blackboardField = new PropertyField(serializedObject.FindProperty("blackboard"), "黑板");
            blackboardField.Bind(serializedObject);
            root.Add(blackboardField);
            treeField.RegisterValueChangeCallback((evt) => {    

            });
            return root;
        }
    }
}