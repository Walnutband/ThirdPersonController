using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MyPlugins.GoodUI;
using System;

namespace MyPlugins.GoodUI
{
    [CustomEditor(typeof(AccordionElement))]
    [CanEditMultipleObjects] //Tip：一旦标记可以进行多对象编辑，就必须要保证相适配的逻辑。
    public class AccordionElementEditor : Editor
    {
        SerializedProperty m_IsExpand;
        SerializedProperty m_Group;
        SerializedProperty m_MinHeight;
        SerializedProperty headerRect;
        SerializedProperty contentRect;
        SerializedProperty contentPreferredHeight;
        SerializedProperty expandFlag;

        private void OnEnable()
        {
            // Debug.Log("AccordionElementEditor__OnEnable");
            m_IsExpand = serializedObject.FindProperty("m_IsExpand");
            m_Group = serializedObject.FindProperty("m_Group");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
            headerRect = serializedObject.FindProperty("headerRect");
            contentRect = serializedObject.FindProperty("contentRect");
            contentPreferredHeight = serializedObject.FindProperty("contentPreferredHeight");
            expandFlag = serializedObject.FindProperty("expandFlag");
            
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Debug.Log("AccordionElementEditor__OnEnable__CreateInspectorGUI");
            serializedObject.Update();
            AccordionElement te = this.target as AccordionElement;
            VisualElement root = new VisualElement();

            PropertyField propertyField = new PropertyField(m_IsExpand);
            //通过检视面板修改会提供一个条件即，必然发生了值改变。
            //BUG：我发现只要选中目标组件所在的对象，就会首先触发这里注册的方法，至今没想通为什么。
            propertyField.RegisterValueChangeCallback(evt => { te.onValueChanged.Invoke(m_IsExpand.boolValue); });
            //如果位于组中，并且在运行模式下（因为AccordionGroup只在运行时起作用）,就禁止通过检视面板改变状态，否则逻辑会混乱，这样就限制只能通过点击交互来切换状态
            if (m_Group.objectReferenceValue == true && Application.isPlaying)
                propertyField.SetEnabled(false);

            root.Add(propertyField);

            propertyField = new PropertyField(m_Group);
            propertyField.SetEnabled(false);
            root.Add(propertyField);

            propertyField = new PropertyField(headerRect);
            root.Add(propertyField);

            propertyField = new PropertyField(m_MinHeight)
            {
                style = {
                    left = 20
                }
            };
            // propertyField.RegisterValueChangeCallback(evt => target.UpdateHeaderHeight());
            propertyField.RegisterValueChangeCallback(evt =>
            {//由于加入了CanEditMultipleObjects，所以在此就要给所有正在检视的对象都调用更新标题高度的方法，才能真正实现同步更新，而当检视单个目标时也照样通用
             // Debug.Log("minHeight Change");
                // te.UpdateHeaderHeight();
                // Debug.Log($"{te.minHeight}")
                // if (targets != null && targets.Length <= 1 && te != null) te.UpdateHeaderHeight();
                // else if (targets != null)
                // {
                foreach (UnityEngine.Object t in this.targets)
                {
                // Debug.Log("进入循环");
                    AccordionElement te = t as AccordionElement;
                    if (te != null) te.UpdateHeaderHeight();
                    
                }
            });
            root.Add(propertyField);

            propertyField = new PropertyField(contentRect);
            root.Add(propertyField);

            propertyField = new PropertyField(contentPreferredHeight, "Preferred Height")
            {
                style = { 
                    left = 20
                }
            };
            root.Add(propertyField);

            propertyField = new PropertyField(expandFlag);
            root.Add(propertyField);

            //TODO：在检视面板中直接设置ExpandFlag的尺寸，并且不在AccordionElement类中定义相应字段，其实RectTransform的检视面板就是如此的，这在开发效率上绝对有用。
            // FloatField floatField = new FloatField("")

            serializedObject.ApplyModifiedProperties();
            return root;
        }

    }
}