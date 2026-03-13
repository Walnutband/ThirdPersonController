
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ARPGDemo.CustomAttributes.EditorSection
{
    //TODO：现在对容器字段还无用。
    [CustomPropertyDrawer(typeof(ExpandInlinePropertiesAttribute))]
    public class ExpandInlinePropertiesDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // if (property.hasVisibleChildren == false) return null;
            if (property.hasVisibleChildren == false)
            {
                VisualElement v = new VisualElement();
                v.style.flexDirection = FlexDirection.Row;
                v.style.justifyContent = Justify.Center;
                Label label = new Label("没有事件数据");
                v.Add(label);
                return v;
            }

            ExpandInlinePropertiesAttribute inlineAttribute = (ExpandInlinePropertiesAttribute)attribute; 

            // 创建根容器
            var container = new VisualElement();
            container.style.marginBottom = 2;

            // 设置显示名称（优先使用Attribute中指定的名称）
            string displayName = string.IsNullOrEmpty(inlineAttribute.label)
                ? property.displayName
                : inlineAttribute.label;

            // 创建标题Label（加粗样式）
            var headerLabel = new Label(displayName);
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.marginBottom = 2;
            headerLabel.style.marginTop = 2;
            // headerLabel.style.fontSize = 16f;
            container.Add(headerLabel);

            // 创建缩进容器
            var indentedContainer = new VisualElement();
            indentedContainer.style.marginLeft = 15; // 缩进量

            // 遍历并添加所有子字段
            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();
            iterator.NextVisible(true); // 进入第一个子字段

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                // 为每个子字段创建PropertyField
                var field = new PropertyField(iterator);
                field.BindProperty(iterator);
                indentedContainer.Add(field);

                if (!iterator.NextVisible(false)) // 移动到下一个可见字段
                    break;
            }

            container.Add(indentedContainer);

            return container;
        }
    }
}