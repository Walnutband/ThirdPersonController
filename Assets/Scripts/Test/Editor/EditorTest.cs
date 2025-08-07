using UnityEngine;
using UnityEditor;

public class EditorTest : EditorWindow {
    [MenuItem("Tests/EditorWindowTest")]
    private static void ShowWindow() {
        var window = GetWindow<EditorTest>();
        window.titleContent = new GUIContent("Test");
        window.Show();
    }

    private void OnGUI()
    {
        Debug.Log($@"在{Time.time}调用OnGUI
            当前事件类型：{Event.current.type}
            窗口当前position: position.x:{position.x};position.x:{position.x};position.y:{position.y}; position.xMin:{position.xMin};position.xMax:{position.xMax};
            position.yMin:{position.yMin}; position.yMax:{position.yMax}; position.width:{position.width}; position.height:{position.height};");

        if (Event.current.type == EventType.MouseDown)
        {
            Repaint();  
        }
    }
}