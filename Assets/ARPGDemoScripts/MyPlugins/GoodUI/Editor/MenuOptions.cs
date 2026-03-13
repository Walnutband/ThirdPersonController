
using UnityEditor;
using UnityEngine;

namespace MyPlugins.GoodUI.EditorSection
{
    public static class MenuOptions
    {
        
        [MenuItem("GameObject/GoodUI/ScrollBar", false, 0)]
        private static void CreateScrollBar()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null) return; //其实应该直接创建Canvas。

            GameObject go = new GameObject("ScrollBar");
            go.transform.SetParent(canvas.transform, false);
            go.AddComponent<ScrollBar>();

            RectTransform slidingArea = new GameObject("SlidingArea").AddComponent<RectTransform>();
            slidingArea.SetParent(go.transform, false);
            slidingArea.anchorMin = Vector2.zero;
            slidingArea.anchorMax = Vector2.one;

            RectTransform handle = new GameObject("Handle").AddComponent<RectTransform>();
            handle.SetParent(slidingArea.transform, false);
            
            
        }
    }
}