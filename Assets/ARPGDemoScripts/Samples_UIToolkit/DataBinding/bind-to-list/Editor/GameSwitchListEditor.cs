using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    [CustomEditor(typeof(GameSwitchListAsset))]
    public class GameSwitchListEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset m_ItemAsset;

        [SerializeField]
        VisualTreeAsset m_EditorAsset;

        public override VisualElement CreateInspectorGUI()
        {
            var root = m_EditorAsset.CloneTree();
            var listView = root.Q<ListView>();
            //注意这里是把VisualTreeAsset.CloneTree的方法作为回调传给makeItem了，这样ListView构造列表元素就会通过调用该方法。
            listView.makeItem = m_ItemAsset.CloneTree;
            return root;
        }
    }
}