using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.SkillSystemtest
{
    [CreateAssetMenu(fileName = "TimelineEditorSettings_SO", menuName = "ARPGDemo/TimelineEditorSettings_SO", order = 0)]
    public class TimelineEditorSettings : ScriptableObject
    {
        public VisualTreeAsset timelineEditorUXML;

        static TimelineEditorSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:TimelineEditorSettings"); //按类型查找
            if (guids.Length > 1)
            { //存在多个
                Debug.LogWarning($"Found multiple settings files, using the first.存在多个设置文件，默认使用第一个");
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]); //获取到的是文件路径
                    return AssetDatabase.LoadAssetAtPath<TimelineEditorSettings>(path);
            }
        }

        internal static TimelineEditorSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TimelineEditorSettings>();
                AssetDatabase.CreateAsset(settings, "Assets"); //创建资产文件，到路径Asset下。
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }
}