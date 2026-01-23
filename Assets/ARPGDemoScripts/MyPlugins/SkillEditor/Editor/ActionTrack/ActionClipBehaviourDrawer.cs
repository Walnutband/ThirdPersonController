
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyPlugins.SkillEditor.Editor
{
    [CustomPropertyDrawer(typeof(ActionClipBehaviour))]
    public class ActionClipBehaviourDrawer : PropertyDrawer
    {
        float singleLineHeight => EditorGUIUtility.singleLineHeight + 5f; 

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty clipProp = property.FindPropertyRelative("clip");
            SerializedProperty fadeInProp = property.FindPropertyRelative("fadeIn");

            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, clipProp);

            // EditorGUILayout.Space(5);

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, fadeInProp);

        }

        // public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        // {

        //     VisualElement container = new VisualElement();
        //     var clipProp = _property.FindPropertyRelative("clip");
        //     var fadeIn = _property.FindPropertyRelative("fadeIn");
        //     container.Add(new PropertyField(clipProp));
        //     container.Add(new PropertyField(fadeIn));

        //     return container;

        //     // VisualElement root = new VisualElement();
        //     // while (_property.Next(true))
        //     // {
        //     //     root.Add(new PropertyField(_property));
        //     // }
        //     // return root;
        // }
    }
}