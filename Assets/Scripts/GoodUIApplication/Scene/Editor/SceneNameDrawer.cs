using UnityEditor;
using UnityEngine;

namespace MyPlugins.GoodUI.Editor
{

    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // 创建容器，作为根VisualElement
            var container = new VisualElement();

            // 判断该属性是否为string类型
            if (property.propertyType != SerializedPropertyType.String)
            {
                container.Add(new Label("SceneNameAttribute 只支持 string 类型字段"));
                return container;
            }

            // 从 EditoBuildSettings 中提取所有启用的场景
            var buildScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            if (buildScenes.Length == 0)
            {
                container.Add(new Label("构建设置中无场景"));
                return container;
            }

            // 提取每个场景的文件名（不包含扩展名）作为下拉选项
            string[] sceneOptions = buildScenes
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

            // 创建 DropdownField
            // 使用 Inspector 中显示的属性名称作为标签，
            // choices 为场景列表，同时设置初始下拉选中项
            int currentIndex = System.Array.IndexOf(sceneOptions, property.stringValue);
            if (currentIndex < 0)
                currentIndex = 0;

            var dropdown = new DropdownField(
                property.displayName,
                new List<string>(sceneOptions),
                currentIndex);

            // 设置下拉框当前值
            dropdown.value = sceneOptions[currentIndex];

            // 当用户选择不同选项时，将值写回到 SerializedProperty
            dropdown.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            container.Add(dropdown);
            return container;
        }
    }

    // [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    // public class SceneNameDrawer : PropertyDrawer
    // {
    //     int sceneIndex = -1;
    //     GUIContent[] sceneNames;

    //     readonly string[] scenePathSplit = { "/", ".unity" };
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         if (EditorBuildSettings.scenes.Length == 0) return;

    //         if (sceneIndex == -1)
    //             GetSceneNameArray(property);

    //         int oldIndex = sceneIndex;

    //         sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);

    //         if (oldIndex != sceneIndex)
    //             property.stringValue = sceneNames[sceneIndex].text;
    //     }

    //     private void GetSceneNameArray(SerializedProperty property)
    //     {
    //         var scenes = EditorBuildSettings.scenes;
    //         //初始化数组
    //         sceneNames = new GUIContent[scenes.Length];

    //         for (int i = 0; i < sceneNames.Length; i++)
    //         {
    //             string path = scenes[i].path;
    //             string[] splitPath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);

    //             string sceneName = "";

    //             if (splitPath.Length > 0)
    //             {
    //                 sceneName = splitPath[splitPath.Length - 1];
    //             }
    //             else
    //             {
    //                 sceneName = "(Deleted Scene)";
    //             }
    //             sceneNames[i] = new GUIContent(sceneName);
    //         }

    //         if (sceneNames.Length == 0)
    //         {
    //             sceneNames = new[] { new GUIContent("Check Your Build Settings") };
    //         }

    //         if (!string.IsNullOrEmpty(property.stringValue))
    //         {
    //             bool nameFound = false;

    //             for (int i = 0; i < sceneNames.Length; i++)
    //             {
    //                 if (sceneNames[i].text == property.stringValue)
    //                 {
    //                     sceneIndex = i;
    //                     nameFound = true;
    //                     break;
    //                 }
    //             }
    //             if (nameFound == false)
    //                 sceneIndex = 0;
    //         }
    //         else
    //         {
    //             sceneIndex = 0;
    //         }

    //         property.stringValue = sceneNames[sceneIndex].text;
    //     }
    // }
}