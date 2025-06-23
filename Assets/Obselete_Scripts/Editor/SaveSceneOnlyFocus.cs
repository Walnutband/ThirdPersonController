using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CustomSaveHandler
{
    static CustomSaveHandler()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Hierarchy")//Hierarchy窗口获得焦点
        //if (!EditorApplication.isPlaying) //非播放模式下才可以保存场景，其实本来就是，只是在这里直接把运行模式下的按键响应都给禁止了
        {
            // 检查Ctrl+S是否被按下
            if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            //if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                // 保存场景
                //Debug.Log("Ctrl+S pressed in Hierarchy window");
                SaveCurrentScene();
            }
        }
    }

    private static void SaveCurrentScene()
    {
        // 保存当前场景
        EditorApplication.ExecuteMenuItem("File/Save"); //就是执行指定的菜单项命令
    }
}