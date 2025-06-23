using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[InitializeOnLoad]
public class DisableShortcutsInPlayMode
{//发现单独该键不好使，就只好运行和编辑分别用一套了
    private const string ShortcutId = "Main Menu/File/Save";
    private const string PlayProfile = "DefaultRuntime";
    private const string EditProfile = "DefaultEditor";

    static DisableShortcutsInPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) //进入运行模式
        {
            // 移除保存场景的快捷键
            //ShortcutManager.instance.ClearShortcutOverride(ShortcutId); //该方法清除在当前活跃的配置（active profile）中给定ID绑定的快捷键
            //不知为何这Clear方法没用
            ShortcutManager.instance.activeProfileId = PlayProfile;
            //Debug.Log("运行时配置：" + ShortcutManager.instance.activeProfileId);
            //Debug.Log("Removed Save Scene shortcut in Play Mode");
        }
        else if (state == PlayModeStateChange.EnteredEditMode) //进入编辑模式
        {
            // 重新启用保存场景的快捷键
            //ShortcutBinding(KeyCombination(实际键，修饰键))
            //ShortcutBinding saveBinding = new ShortcutBinding(new KeyCombination(KeyCode.S, ShortcutModifiers.Control));
            //ShortcutManager.instance.RebindShortcut(ShortcutId, saveBinding);
            ShortcutManager.instance.activeProfileId = EditProfile;
            //Debug.Log("编辑时配置：" + ShortcutManager.instance.activeProfileId);
            //Debug.Log("Re-enabled Save Scene shortcut in Edit Mode");
        }
    }
}
