
using UnityEditor;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{
    public class TimelineEditor : EditorWindow
    {

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            VisualTreeAsset timelineEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ARPGDemoScripts/Test/TimelineTest/TimelineEditor.uxml");
            var timelineEditorTree = timelineEditorAsset.Instantiate();
        }
    }
}