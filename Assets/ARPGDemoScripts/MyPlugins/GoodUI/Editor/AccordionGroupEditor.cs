using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MyPlugins.GoodUI
{
    [CustomEditor(typeof(AccordionGroup))]
    public class AccordionGroupEditor : Editor {

        SerializedProperty elements;
        SerializedProperty m_AllowMultiOn;
        SerializedProperty m_Transition;
        SerializedProperty m_TransitionDuration;

        private void OnEnable()
        {
            elements = serializedObject.FindProperty("elements");
            m_AllowMultiOn = serializedObject.FindProperty("m_AllowMultiOn");
            m_Transition = serializedObject.FindProperty("m_Transition");
            m_TransitionDuration = serializedObject.FindProperty("m_TransitionDuration");
        }
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            VisualElement root = new VisualElement();
            AccordionGroup target = this.target as AccordionGroup;

            PropertyField propertyField = new PropertyField(elements);
            propertyField.SetEnabled(false); //注意在设置为false之后就无法使用为该字段添加的菜单项了，就只剩下Copy和Paste
            root.Add(propertyField);

            PropertyField property = new PropertyField(m_AllowMultiOn);
            //BUG:发现在选中AccordionGroup对象即显示其检视面板时即调用该方法时会自动调用一次这里注册的方法，不知道为啥，估计要去了解UI Toolkit检测值改变的底层逻辑才能知道了
            property.RegisterValueChangeCallback(evt => target.EnsureValidState());
            root.Add(property);

            propertyField = new PropertyField(m_Transition);
            root.Add(propertyField);

            propertyField = new PropertyField(m_TransitionDuration);
            root.Add(propertyField);

            serializedObject.ApplyModifiedProperties();
            return root;
        }
    }
    
}