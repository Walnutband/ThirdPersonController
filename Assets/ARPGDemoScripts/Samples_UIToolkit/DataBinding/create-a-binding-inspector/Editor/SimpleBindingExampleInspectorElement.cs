using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;

namespace UIToolkitExamples
{
    public class SimpleBindingExampleInspectorElement : EditorWindow
    {
        [MenuItem("Window/UI Toolkit/数据绑定示例/Simple Binding Example Inspector Element")]
        public static void ShowDefaultWindow()
        {
            var wnd = GetWindow<SimpleBindingExampleInspectorElement>();
            wnd.titleContent = new GUIContent("Simple Binding with Inspector Element");
        }

        TankScript m_Tank;
        public void OnEnable()
        {
            // m_Tank = FindObjectOfType<TankScript>();
            m_Tank = FindFirstObjectByType<TankScript>();
            if (m_Tank == null)
                return;
            //InspectorElement可以在自定义编辑窗口中显示SerializedObject的属性，就像Unity的Inspector窗口一样
            //注意自定义的编辑器类会影响构造的InspectorElement，或者说就是直接定义其检视面板的渲染方式
            var inspector = new InspectorElement(m_Tank);
            rootVisualElement.Add(inspector);
        }
    }
}