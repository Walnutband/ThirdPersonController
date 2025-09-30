using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    public class SimpleBindingPropertyTrackingExample : EditorWindow
    {
        TextField m_ObjectNameBinding;

        [MenuItem("Window/UI Toolkit/数据绑定示例/Simple Binding Property Tracking Example")]
        public static void ShowDefaultWindow()
        {
            var wnd = GetWindow<SimpleBindingPropertyTrackingExample>();
            wnd.titleContent = new GUIContent("Simple Binding Property Tracking");
        }

        public void CreateGUI()
        {
            m_ObjectNameBinding = new TextField("Object Name Binding");
            rootVisualElement.Add(m_ObjectNameBinding);
            OnSelectionChange();
        }

        public void OnSelectionChange()
        {
            GameObject selectedObject = Selection.activeObject as GameObject;
            if (selectedObject != null)
            {
                // Create the SerializedObject from the current selection
                SerializedObject so = new SerializedObject(selectedObject);

                // Note: the "name" property of a GameObject is actually named "m_Name" in serialization.
                SerializedProperty property = so.FindProperty("m_Name");

                // Ensure to use Unbind() before tracking a new property
                m_ObjectNameBinding.Unbind(); //先解绑，再绑定
                //以固定间隔检测序列化属性是否变化，如果变化则执行回调。
                m_ObjectNameBinding.TrackPropertyValue(property, CheckName);

                // Bind the property to the field directly
                m_ObjectNameBinding.BindProperty(property);

                CheckName(property);
            }
            else
            {
                // Unbind any binding from the field
                m_ObjectNameBinding.Unbind();
            }
        }

        void CheckName(SerializedProperty property)
        {
            //如果游戏对象名改为了GameObject，就将字段背景色改为红色
            if (property.stringValue == "GameObject")
            {
                m_ObjectNameBinding.style.backgroundColor = Color.red * 0.5f;
            }
            else
            {
                m_ObjectNameBinding.style.backgroundColor = StyleKeyword.Null;
            }
        }
    }
}