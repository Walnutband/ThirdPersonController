
namespace ARPGDemo.CustomAttributes
{
    // 自定义Attribute
    using UnityEngine;

    public class ContainerDisplayAttribute : PropertyAttribute
    {
        public string DisplayName { get; private set; }
        public string ElementName { get; private set; }
        public bool StartFromOne { get; private set; }

        public ContainerDisplayAttribute(string displayName = null, string elementName = null, bool startFromOne = true)
        {
            DisplayName = displayName;
            ElementName = elementName;
            StartFromOne = startFromOne;
        }
    }
}