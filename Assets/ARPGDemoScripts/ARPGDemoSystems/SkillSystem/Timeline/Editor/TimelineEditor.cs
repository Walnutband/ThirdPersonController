using ARPGDemo.SkillSystemtest;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace APRGDemo.SkillSystemtest
{
    public class TimelineEditor : EditorWindow
    {
        TimelineEditorSettings settings;

        [MenuItem("MyPlugins/TimelineEditor")]
        public static void ShowExample()
        {
            TimelineEditor wnd = GetWindow<TimelineEditor>();
            wnd.titleContent = new GUIContent("TimelineEditor");
        }

        private void CreateGUI()
        {
            settings = TimelineEditorSettings.GetOrCreateSettings();
            VisualElement root = rootVisualElement;

            var tree = settings.timelineEditorUXML;
            tree.CloneTree(root);

        }
    }
}
