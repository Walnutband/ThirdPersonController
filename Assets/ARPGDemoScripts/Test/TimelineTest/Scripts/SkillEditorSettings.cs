using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{
    [CreateAssetMenu(fileName = "SkillEditorSettings", menuName = "ARPGDemo/SkillEditorSettings_SO", order = 0)]
    public class SkillEditorSettings : ScriptableObject
    {
        // public float trackHeight;

        public VisualTreeAsset skillEditorUxml;
        public VisualTreeAsset trackTemplateUxml;

        //StyleClass样式类名
        public string playUssClassName = "playing";











        static SkillEditorSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:SkillEditorSettings"); //按类型查找
            if (guids.Length > 1)
            { //存在多个
                Debug.Log($"存在多个设置文件，默认使用第一个");
            }
            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]); //获取到的是文件路径
                    return AssetDatabase.LoadAssetAtPath<SkillEditorSettings>(path);
            }
        }


        internal static SkillEditorSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SkillEditorSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/SkillEditorSettings.asset"); //创建资产文件，到路径Asset下。
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }
}