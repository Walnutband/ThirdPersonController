
using UnityEngine;

namespace MyPlugins.GoodUI
{
    // public class SceneNameAttribute : PropertyAttribute
    // {//Property参数，代表inspector窗口中那每一行的变量

    // }

    /// <summary>
    /// 标记 string 字段，该字段存储场景名称，
    /// 编辑器中会显示为 Build Settings 中启用的场景下拉选项
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SceneNameAttribute : PropertyAttribute
    {
        // 可根据需要添加额外参数
    }

}
