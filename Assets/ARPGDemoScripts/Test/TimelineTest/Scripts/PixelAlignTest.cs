using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{
    static class PixelAlignUtil
    {
        // 在 Editor 中可用 EditorGUIUtility.pixelsPerPoint; 在运行时，可从 panel 获取或默认使用 1
        public static float GetPixelsPerPoint(IPanel panel = null)
        {
    #if UNITY_EDITOR
            return UnityEditor.EditorGUIUtility.pixelsPerPoint;
    #else
            // if (panel != null) return panel.devicePixelRatio; // runtime panel API may vary
            return 1f;
    #endif
        }

        public static float RoundToPixel(float value, float ppp)
        {
            return Mathf.Round(value * ppp) / ppp;
        }

        public static void SnapElementToPixels(VisualElement ve, IPanel panel = null)
        {
            float ppp = GetPixelsPerPoint(panel);
            var rect = ve.worldBound; //最后计算得到的Rect。
            // 先计算对齐后的 rect，然后设置 style.left/top/width/height（使用 resolved values）
            float x = RoundToPixel(rect.x, ppp);
            float y = RoundToPixel(rect.y, ppp);
            float w = RoundToPixel(rect.width, ppp);
            float h = RoundToPixel(rect.height, ppp);

            ve.style.left = x - (ve.parent?.worldBound.x ?? 0f);
            ve.style.top = y - (ve.parent?.worldBound.y ?? 0f);
            ve.style.width = w;
            ve.style.height = h;
        }
    }

    public class PixelAlignedElement : VisualElement
    {
        public PixelAlignedElement()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Panel 可以从 panel property 获取（根据 Unity 版本）
            IPanel panel = this.panel;
            PixelAlignUtil.SnapElementToPixels(this, panel);
        }
    }

}

