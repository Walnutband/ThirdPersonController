using System.Text;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyLogger
{
    /// <summary>
    /// 打印传入 SerializedProperty 及其所有可见子属性（递归）。
    /// </summary>
    /// <param name="prop">目标 SerializedProperty（通常是 SerializedObject.FindProperty 返回的）</param>
    public static void LogAllProperties(SerializedProperty prop)
    {
        if (prop == null)
        {
            Debug.Log("SerializedProperty is null");
            return;
        }

        // 我们使用一个复制品来遍历，这样不会改变原始 prop 的位置
        SerializedProperty iter = prop.Copy();
        int startDepth = iter.depth;
        Debug.Log($"startDepth开始深度: {startDepth}");
        StringBuilder sb = new StringBuilder();

        // 打印根属性信息
        sb.AppendLine($"--- 原属性路径: {prop.propertyPath}   原属性类型: {prop.propertyType}) ---");

        bool first = true;
        // 使用 NextVisible 来遍历可见属性。第一次调用 NextVisible(true) 会进入子属性。
        // 我们先处理当前节点本身，然后遍历它的可见子属性直到回到同一或更浅深度。
        /*使用do-while循环就是会处理原元素本身，而通常情况下对于这种要进入子元素的元素是不会处理元素本身的。*/
        do
        {
            // 跳过第一次循环自动移动到第一个子属性时的重复打印（用 first 标志控制）
            if (!first)
            {
                // 若已经爬回到与起始同级或更浅位置，则结束（说明子树遍历完了）
                if (iter.depth <= startDepth && iter.propertyPath != prop.propertyPath)
                    break;
            }
            first = false;

            // 构建缩进以体现层级关系
            string indent = new string(' ', iter.depth * 2);
            string line = BuildPropertyLine(iter, indent);
            sb.AppendLine(line);

            // 移动到下一个可见属性
        } while (iter.NextVisible(true));
        // } while (iter.Next(true));

        sb.AppendLine($"--- end of {prop.propertyPath} ---");
        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// 根据 SerializedPropertyType 读取并格式化当前属性的显示文本。
    /// </summary>
    private static string BuildPropertyLine(SerializedProperty prop, string indent)
    {
        string name = prop.displayName;
        string path = prop.propertyPath;
        string type = prop.propertyType.ToString();
        string value = GetPropertyValueString(prop);
        return @$"{indent}displayName:{name} name: {prop.name}| depth: {prop.depth} | propertyPath: {path} | propertyTypeName: {prop.propertyType} type: {prop.type}| value: {value} | isArray : {prop.isArray}
        arrayElementType: {prop.arrayElementType} | arraysize: {prop.arraySize} | floatValue: {prop.floatValue} | hasChildren: {prop.hasChildren} hasVisibleChildren: {prop.hasVisibleChildren}
        | isExpanded: {prop.isExpanded} | intValue: {prop.intValue} | floatValue: {prop.floatValue} | 
        EndProperty: {prop.GetEndProperty().displayName}";
    }

    /// <summary>
    /// 获取属性的值字符串表示，覆盖常见类型。
    /// </summary>
    private static string GetPropertyValueString(SerializedProperty prop)
    {/*Tip：穷举SerializedPropertyType，有一些类型会进行特殊处理、就是提取有效信息，比如ObjectReference。*/
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return prop.boolValue.ToString();
            case SerializedPropertyType.Float:
                return prop.floatValue.ToString();
            case SerializedPropertyType.String:
                return $"\"{prop.stringValue}\"";
            case SerializedPropertyType.Color:
                return prop.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                //类型是UnityEngine.Object的派生类。
                return prop.objectReferenceValue != null ? $"{prop.objectReferenceValue.GetType().Name} ({prop.objectReferenceValue.name})" : "null";
            case SerializedPropertyType.LayerMask:
                return prop.intValue.ToString();
            case SerializedPropertyType.Enum:
                return prop.enumNames[prop.enumValueIndex];
            case SerializedPropertyType.Vector2:
                return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return prop.vector3Value.ToString();
            case SerializedPropertyType.Vector4:
                return prop.vector4Value.ToString();
            case SerializedPropertyType.Rect:
                return prop.rectValue.ToString();
            case SerializedPropertyType.ArraySize:
                return prop.intValue.ToString();
            case SerializedPropertyType.Character:
                return ((char)prop.intValue).ToString();
            case SerializedPropertyType.AnimationCurve:
                return prop.animationCurveValue != null ? "AnimationCurve" : "null";
            case SerializedPropertyType.Bounds:
                return prop.boundsValue.ToString();
            case SerializedPropertyType.Gradient:
                return "Gradient"; // Gradient 无法直接读取内容
            case SerializedPropertyType.Quaternion:
                return prop.quaternionValue.eulerAngles.ToString();
            case SerializedPropertyType.ExposedReference:
                return prop.exposedReferenceValue != null ? prop.exposedReferenceValue.ToString() : "null";
            case SerializedPropertyType.FixedBufferSize:
                return prop.intValue.ToString();
            case SerializedPropertyType.ManagedReference:
                return prop.managedReferenceFullTypename;
            case SerializedPropertyType.Generic:
            default:
                // 对于 Generic（复杂类型、结构体、类、列表等），打印其子元素个数或简单描述
                if (prop.hasVisibleChildren)
                {
                    // 统计直接子属性数量（可见）
                    int count = CountVisibleDirectChildren(prop);
                    return $"Generic with {count} visible children";
                }
                return "<unhandled generic>";
        }
    }

    /// <summary>
    /// 统计传入属性下直接可见子属性数量（不递归深度更深的孙子）。
    /// </summary>
    private static int CountVisibleDirectChildren(SerializedProperty prop)
    {
        int count = 0;
        var iter = prop.Copy();
        if (!iter.NextVisible(true)) return 0; // 没有子属性
        int parentDepth = prop.depth;
        do
        {
            // 只统计直接子属性
            if (iter.depth == parentDepth + 1) count++;
            if (iter.depth <= parentDepth) break;
        } while (iter.NextVisible(false)); // 不进入子子层（false）
        return count;
    }
}

/// <summary>
/// 示例 EditorWindow：在窗口中选择目标对象并打印其第一个根级 SerializedProperty（方便测试）
/// </summary>
public class SerializedPropertyLoggerWindow : EditorWindow
{
    private Object targetObject;
    private string propertyPath = "";

    [MenuItem("Tools/SerializedProperty Logger")]
    public static void OpenWindow()
    {
        GetWindow<SerializedPropertyLoggerWindow>("SerializedProperty Logger");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("选择一个对象并输入要打印的属性路径（例如: m_Script 或 m_SomeField）", EditorStyles.wordWrappedLabel);
        targetObject = EditorGUILayout.ObjectField("Target", targetObject, typeof(Object), true);

        propertyPath = EditorGUILayout.TextField("Property Path", propertyPath);

        if (GUILayout.Button("Log Property"))
        {
            if (targetObject == null)
            {
                Debug.Log("请先选择一个目标对象");
                return;
            }

            SerializedObject so = new SerializedObject(targetObject);
            so.Update();
            SerializedProperty prop;
            if (string.IsNullOrEmpty(propertyPath))
            {
                // 若未指定路径，则打印第一个可见的根属性
                prop = so.GetIterator();
                prop.NextVisible(true); // move to first visible
            }
            else
            {
                prop = so.FindProperty(propertyPath);
            }

            if (prop == null)
            {
                Debug.Log($"找不到属性: {propertyPath}");
                return;
            }

            SerializedPropertyLogger.LogAllProperties(prop);
        }
    }
}

/*Tip：NMD这里用户所有MonoBehaviour及其派生类，会直接覆盖掉CustomPropertyDrawer*/

// /// <summary>
// /// 示例 CustomEditor：在任意组件的 Inspector 中添加一个按钮，点击打印整个组件的 serializedObject 根属性
// /// </summary>
// [CustomEditor(typeof(MonoBehaviour), true)]
// public class SerializedPropertyLoggerExampleEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         if (GUILayout.Button("Log whole SerializedObject (root)"))
//         {
//             // 获取 serializedObject 的第一个可见属性（通常是 script，然后是字段）
//             var iter = serializedObject.GetIterator();
//             if (iter.NextVisible(true))
//             {
//                 // 传入根属性以打印其所有可见子属性
//                 SerializedPropertyLogger.LogAllProperties(iter);
//             }
//             else
//             {
//                 Debug.Log("No visible properties found on this object.");
//             }
//         }
//     }
// }
