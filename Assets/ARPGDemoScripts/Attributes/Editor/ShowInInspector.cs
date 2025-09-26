
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ARPGDemo.Editor
{
    [CustomEditor(typeof(ShowInInspectorAttribute))]
    public class ShowInInspectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            PropertyField field = new PropertyField(property);
            field.SetEnabled(false); //只读
            return field;
        }
    }
}