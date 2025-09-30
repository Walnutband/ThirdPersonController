using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DragAndDropWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/示例/Drag And Drop")]
    public static void ShowExample()
    {
        DragAndDropWindow wnd = GetWindow<DragAndDropWindow>();
        wnd.titleContent = new GUIContent("Drag And Drop");
    }

    [SerializeField] VisualTreeAsset m_VisualTreeAsset;
    [SerializeField] StyleSheet m_StyleSheet;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

          // Import UXML
        //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/DragAndDropWindow.uxml");
        // VisualElement labelFromUXML = visualTree.Instantiate();
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DragAndDropWindow.uss");
        var styleSheet = m_StyleSheet;
        DragAndDropManipulator manipulator = new(rootVisualElement.Q<VisualElement>("object"));


    }
}