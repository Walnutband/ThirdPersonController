using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ARPGDemo.CustomAttributes;

namespace ARPGDemo.CustomAttributes.EditorSection
{
    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
    public class DisplayName : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            // 创建支持Rich Text的Label
            var label = new Label(((DisplayNameAttribute)attribute).Name);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginLeft = 3;
            label.style.marginBottom = 2;
            label.enableRichText = true;  // 启用富文本

            // 创建属性字段
            var field = new PropertyField(property, ""); //注意指定标签为空，因为创建了专门的Label。

            container.Add(label);
            container.Add(field);

            return container;
        }
    }

}