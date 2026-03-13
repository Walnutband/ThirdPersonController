

using UnityEngine;

namespace ARPGDemo.CustomAttributes
{
    public class ExpandInlinePropertiesAttribute : PropertyAttribute
    {
        public string label; // 可选的显示名称

        public ExpandInlinePropertiesAttribute(string label = null)
        {
            this.label = label;
        }
    }
}