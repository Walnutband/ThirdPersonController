using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TankScript))]
public class TankEditor : Editor
{
    [SerializeField]
    VisualTreeAsset visualTree;

    public override VisualElement CreateInspectorGUI()
    {//在引用的UXML文件中的两个PropertyField的Binding Path分别设置为了tankName和tankSize，这样就会自动绑定对象（TankScript）的属性
        var uxmlVE = visualTree.CloneTree();
        return uxmlVE;
    }
}