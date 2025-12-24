using CrashKonijn.Goap.Runtime;
using UnityEditor;
using UnityEngine;

namespace CrashKonijn.Goap.Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //通过元数据，反射获取方法然后调用。
            var methodName = (this.attribute as ButtonAttribute).MethodName;
            var target = property.serializedObject.targetObject;
            var type = target.GetType();
            var method = type.GetMethod(methodName);
            
            if (method == null)
            {
                GUI.Label(position, "Method could not be found. Is it public?");
                return;
            }
            
            if (method.GetParameters().Length > 0)
            {
                GUI.Label(position, "Method cannot have parameters.");
                return;
            }
            
            if (GUI.Button(position, method.Name))
            {
                method.Invoke(target, null);
            }
        }
    }
#endif
}