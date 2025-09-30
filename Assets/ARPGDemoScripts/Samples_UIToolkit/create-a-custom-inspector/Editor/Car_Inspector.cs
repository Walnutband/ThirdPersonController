using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Car))]
public class Car_Inspector : Editor
{
    public VisualTreeAsset m_InspectorXML;
    /*
    如果你重写了CreateInspectorGUI，Unity会优先使用UI Toolkit的方法，且默认会忽略OnInspectorGUI的内容。
    OnInspectorGUI仅在未实现CreateInspectorGUI的情况下会被调用。因此，当你完全转向UI Toolkit进行定制时，OnInspectorGUI通常不再需要。
    */
    public override VisualElement CreateInspectorGUI() //由此将m_InspectorXML中设置了binding path的元素和Car的序列化对象中的序列化属性绑定了起来
    {
        // Create a new VisualElement to be the root of the Inspector UI.
        //创建一个新的VisualElement作为检视面板的根容器
        VisualElement myInspector = new VisualElement();

        // Load from default reference.将指定的UXML文件内容克隆到指定容器中
        m_InspectorXML.CloneTree(myInspector); 
    
        // Get a reference to the default Inspector Foldout control.获取对于折叠控件的引用
        VisualElement InspectorFoldout = myInspector.Q("Default_Inspector");

        // Attach a default Inspector to the Foldout.将Car类的检视面板内容添加到一个标签为“Default_Inspector”的折叠控件(作为容器)中。
        InspectorElement.FillDefaultInspector(InspectorFoldout, serializedObject, this);

        // Return the finished Inspector UI.
        return myInspector;
    }

    // public override void OnInspectorGUI()
    // {

    // }
}

