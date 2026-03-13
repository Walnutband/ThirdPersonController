
namespace ARPGDemo.CustomAttributes.EditorSection
{
    // 编辑器绘制类
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ContainerDisplayAttribute))]
public class ContainerDisplayDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();
        
        if (!property.isArray)
        {
            container.Add(new PropertyField(property));
            return container;
        }

        var attr = attribute as ContainerDisplayAttribute;
        
        // 第一行：标题和数量
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.justifyContent = Justify.SpaceBetween;
        headerRow.style.marginBottom = 4;
        
        string displayName = string.IsNullOrEmpty(attr.DisplayName) ? property.displayName : attr.DisplayName;
        var titleLabel = new Label(displayName);
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerRow.Add(titleLabel);
        
        var countLabel = new Label($"Count: {property.arraySize}");
        countLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        headerRow.Add(countLabel);
        
        container.Add(headerRow);
        
        // 元素列表
        for (int i = 0; i < property.arraySize; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            
            // 元素容器
            var elementContainer = new VisualElement();
            elementContainer.style.marginBottom = 4;
            
            // 元素标题
            string elementTitle;
            if (string.IsNullOrEmpty(attr.ElementName))
            {
                elementTitle = $"Element {i}";
            }
            else
            {
                int index = attr.StartFromOne ? i + 1 : i;
                elementTitle = $"{attr.ElementName} {index}";
            }
            
            var elementLabel = new Label(elementTitle);
            elementLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            elementLabel.style.marginBottom = 2;
            elementContainer.Add(elementLabel);
            
            // 元素内容 - 使用PropertyField，不显示标签
            var elementField = new PropertyField(element, "");
            elementField.style.marginLeft = 15; // 轻微缩进让内容居中
            elementContainer.Add(elementField);
            
            container.Add(elementContainer);
        }
        
        return container;
    }
}
#endif
}