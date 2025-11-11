
using MyPlugins.GoodUI;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem.EditorSection
{
    public static class MenuOptions
    {
        [MenuItem("GameObject/ARPGDemo/UISystem/BasePanel", false, -500)]
        private static void CreateBasePanel()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            GameObject go = new GameObject("BasePanel");
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetAnchor(AnchorPresets.StretchAll);
            if (canvas == null)
            {
                canvas = new GameObject("Canvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                go.transform.SetParent(canvas.transform);
            }
            else
            {
                go.transform.SetParent(canvas.transform);
            }

            GameObject mask = new GameObject("UIMask");
            rect = mask.AddComponent<RectTransform>();
            rect.SetAnchor(AnchorPresets.StretchAll);
            mask.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            mask.AddComponent<SimpleButton>();
            mask.transform.SetParent(go.transform);

            GameObject root = new GameObject("UIRoot");
            rect = root.AddComponent<RectTransform>();
            rect.SetAnchor(AnchorPresets.StretchAll);
            root.transform.SetParent(go.transform);
            /*似乎在之前设置的锚点位置和sizeDelta会失效？*/
            go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            go.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }
    }
}