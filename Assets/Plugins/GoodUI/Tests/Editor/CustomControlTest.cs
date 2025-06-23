using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(CustomControl))]
public class CustomControlEditor : Editor
{


    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        CustomControl target = this.target as CustomControl;

        SerializedProperty property = serializedObject.FindProperty("m_Length");
        PropertyField propertyField = new PropertyField(property);
        root.Add(propertyField);
        propertyField.RegisterValueChangeCallback(evt => target.UpdateHandleSize());

        property = serializedObject.FindProperty("m_Value");
        propertyField = new PropertyField(property);
        root.Add(propertyField);
        propertyField.RegisterValueChangeCallback(evt => target.UpdateHandlePosition());


        return root;

    }
}