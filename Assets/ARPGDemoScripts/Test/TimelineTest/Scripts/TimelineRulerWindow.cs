using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.Timeline
{
    public class TimelineRulerWindow : EditorWindow
    {
        RulerElement ruler;

        // 时间与像素映射状态（）
        /*TODO：初始就是0，但在RulerElement中的OnPointerMove方法中会对该值进行更新，那么从这一点来看，也可以把该变量放到RulerElement中自行管理，但还要考虑到的是，
        该值可能会用于非RulerElement的其他元素的逻辑，因为一个完整的时间轴UI中的刻度尺只是其中一部分，而刻度尺要使用的这里的visibleStartTime也可能被被其他部分所使用，
        这样的话就应该放在位于这些部分之上的对象了，如此就能实现各部分之间的相互联系。
        应该可以专门搞一个设置文件，比如TimelineEditorSettings_SO，这样就可以非常方便地调整基本参数，让编辑器的体验更加舒适了，其实重点就是时间轴的相关交互。*/
        double visibleStartTime = 0.0;        // seconds at left edge
        /*Tip：单位时间与像素数量的映射关系，这应该算是时间尺的一个核心概念，*/
        float pixelsPerSecond = 100f;         // scale: px / sec

        [MenuItem("Window/Minimal Timeline Ruler")]
        public static void Open()
        {
            var wnd = GetWindow<TimelineRulerWindow>();
            wnd.titleContent = new GUIContent("Timeline Ruler");
            wnd.minSize = new Vector2(600, 120);
        }

        void OnEnable()
        {

            // repaint on update so playhead/animations can move (optional)
            EditorApplication.update += OnEditorUpdate;
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            // container
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.flexGrow = 1;
            rootVisualElement.Add(container);

            // create ruler
            ruler = new RulerElement(() => visibleStartTime, () => pixelsPerSecond,
                                     (newStart) => visibleStartTime = newStart,
                                     (newScale, anchorPx) =>
                                     {
                                         // adjust visibleStartTime to keep anchor time fixed
                                         //anchorTime代表鼠标位置对应的时刻，anchorPx就是鼠标位置对应的像素
                                         double anchorTime = PxToTime(anchorPx, visibleStartTime, pixelsPerSecond);
                                         pixelsPerSecond = newScale;
                                         /*Tip：以鼠标位置为缩放锚点体现在，以鼠标位置时刻不变为前提，计算缩放后的鼠标位置与可视区域左边界的距离所代表的时长，减去即可得到此时
                                         左边界的开始时刻。这就是一个简单的解方程，从逻辑关系来看原本应该是visibleStartTime + anchorPx / pixelsPerSecond = anchorTime，而
                                         根据anchorTime、anchorPx、pixelsPerSecond已知，就可以求出visibleStartTime*/
                                         visibleStartTime = anchorTime - (anchorPx / pixelsPerSecond);
                                         Debug.Log($"锚点时刻：{anchorTime}, 锚点像素: {anchorPx}");
                                     });
            ruler.style.height = 40;
            // ruler.style.height = 50;
            ruler.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            ruler.style.marginLeft = 10;
            // ruler.style.paddingLeft = 10;
            container.Add(ruler);

            // a placeholder content area to show scrolling/panning context
            var content = new Label("Timeline content placeholder (extend me)");
            content.style.height = 200;
            content.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            content.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(content);
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        void OnEditorUpdate()
        {
            // If you need continuous repaint (e.g., playback), call MarkDirtyRepaint
            // ruler.MarkDirtyRepaint();
        }

        //求得鼠标所在位置对应的时刻。
        static double PxToTime(float px, double visibleStartTime, float pixelsPerSecond)
        {
            return px / pixelsPerSecond + visibleStartTime;
        }
    }

}