using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
//存储行为树所需的引用资源，以及使用UIElements注册自己的SettingsProvider到Project Settings中

// Create a new type of Settings Asset.
namespace MyPlugins.BehaviourTree.EditorSection
{
    public class BehaviourTreeSettings : ScriptableObject {
        public VisualTreeAsset behaviourTreeEditorUxml;
        public StyleSheet behaviourTreeStyle;
        public VisualTreeAsset nodeViewUxml; //视图节点的uxml文件的实例
        public VisualTreeAsset variableUxml; //变量
        [Header("脚本模板文本文件")]
        public TextAsset scriptTemplateActionNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;
        [Space(10)]
        public string newNodeBasePath = "Assets/";

        static BehaviourTreeSettings FindSettings() {
            var guids = AssetDatabase.FindAssets("t:BehaviourTreeSettings"); //按类型查找
            if (guids.Length > 1) { //存在多个
                Debug.LogWarning($"Found multiple settings files, using the first.存在多个设置文件，默认使用第一个");
            }

            switch (guids.Length) {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]); //获取到的是文件路径
                    return AssetDatabase.LoadAssetAtPath<BehaviourTreeSettings>(path);
            }
        }

        /// <summary>
        /// 获取或创建行为树设置文件
        /// </summary>
        /// <returns></returns>
        internal static BehaviourTreeSettings GetOrCreateSettings() {
            var settings = FindSettings();
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<BehaviourTreeSettings>();
                AssetDatabase.CreateAsset(settings, "Assets"); //创建资产文件，到路径Asset下。
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        /// <summary>
        /// 获取设置文件的序列化对象
        /// </summary>
        /// <returns></returns>
        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using UIElements for the drawing framework:
    static class MyCustomSettingsUIElementsRegister {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            // First parameter is the path in the Settings window.第一个参数是在窗口中的路径
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            //第二个参数是一个枚举类型SettingsScope，Project就是在Projec Settings，User就是在Preferences
            var provider = new SettingsProvider("Project/MyCustomUIElementsSettings", SettingsScope.Project) {
                label = "BehaviourTree", //指定页面显示名，相当于替换掉指定路径的最后一段。
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                //UIElements用这个，而IMGUI用guiHandler
                activateHandler = (searchContext, rootElement) => {
                    var settings = BehaviourTreeSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label() {
                        text = "Behaviour Tree Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);
                    /*注意这里用到的C#语法，叫做Object Initialization，其实和C++的初始化列表差不多。可以在创建时直接为成员赋值，但要注意
                    并不能直接设置嵌套属性，比如style.flexDirection = FlexDirection.Column这是错误的，只能直接对其整体赋值，
                    所以就用到了下面的初始化块，相当于直接对style进行了整体赋值。当然也可以创建实例后再访问赋值，但是像下面这样写确实很简洁优雅*/
                    var properties = new VisualElement() {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);
                    //InspectorElement继承自BindableElement，用于从SerializedObject创建一个VisualElement检视器，可以根据序列化对象中存在的序列化属性自动生成Property Field
                    properties.Add(new InspectorElement(settings)); //可以在调试窗口中看到InspectorElement子元素的层级结构，其实有点像IMGUIContainer
                    //BUG：在Project Settings中修改字段时可以立刻反映到检视面板中，但是在设置窗口中却不会立刻变化，需要再打开该页面才行。
                    rootElement.Bind(settings);
                },
            };

            return provider;
        }
    }
}