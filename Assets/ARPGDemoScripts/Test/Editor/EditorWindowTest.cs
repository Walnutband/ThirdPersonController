using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.EditorSection
{
    
    public class EditorWindowTest : EditorWindow 
    {
        [MenuItem("ThirdPersonController/WindowTest")]
        private static void ShowWindow() {
            var window = GetWindow<EditorWindowTest>();
            window.titleContent = new GUIContent("WindowTest");
            window.Show();
        }
    
        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>("Test");
            root.Add(asset.CloneTree());
        }
    }
}