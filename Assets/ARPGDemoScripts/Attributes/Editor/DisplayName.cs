using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ARPGDemo
{
    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
    public class DisplayName : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            PropertyField field = new PropertyField(property);
            field.label = ((DisplayNameAttribute)attribute).Name;
            return field;
        }
    }

}