using UnityEngine;
using System;

namespace ARPGDemo.CustomAttributes
{
    /// <summary>
    /// 自定义列表显示属性
    /// </summary>
    // [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class CustomListInspectorAttribute : PropertyAttribute
    {
        public string DisplayName { get; private set; }

        public CustomListInspectorAttribute(string displayName = null)
        {
            DisplayName = displayName;
        }
    }
}