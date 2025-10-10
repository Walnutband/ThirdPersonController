
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Editor
{
    [CustomPropertyDrawer(typeof(SerializedPropertyTestAttribute))]
    public class SerializedPropertyTestDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Debug.Log("SerializedPropertyTestDrawer");
            return base.CreatePropertyGUI(property);
        }
    }
}