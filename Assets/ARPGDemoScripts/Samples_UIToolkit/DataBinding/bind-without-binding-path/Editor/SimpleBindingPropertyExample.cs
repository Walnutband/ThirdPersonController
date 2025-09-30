using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    public class SimpleBindingPropertyExample : EditorWindow
    {
        TextField m_ObjectNameBinding;

        [MenuItem("Window/UI Toolkit/数据绑定示例/（序列化属性）绑定游戏对象名称")]
        public static void ShowDefaultWindow()
        {
            var wnd = GetWindow<SimpleBindingPropertyExample>();
            wnd.titleContent = new GUIContent("Simple Binding Property");
        }
            
        public void CreateGUI()
        {
            m_ObjectNameBinding = new TextField("Object Name Binding");
            rootVisualElement.Add(m_ObjectNameBinding);
            OnSelectionChange();
        }

        public void OnSelectionChange()
        {
            m_ObjectNameBinding.Unbind();
            GameObject selectedObject = Selection.activeObject as GameObject;
            if (selectedObject != null)
            {
                // Create the SerializedObject from the current selection
                SerializedObject so = new SerializedObject(selectedObject);

                // Note: the "name" property of a GameObject is actually named "m_Name" in serialization.
                SerializedProperty property = so.FindProperty("m_Name");
                // Bind the property to the field directly直接绑定指定的序列化属性，无需binding path
                m_ObjectNameBinding.BindProperty(property);
            }
            else
            {
                // Unbind any binding from the field
                // m_ObjectNameBinding.Unbind();
                m_ObjectNameBinding.value = "";
            }
        }
    }
}