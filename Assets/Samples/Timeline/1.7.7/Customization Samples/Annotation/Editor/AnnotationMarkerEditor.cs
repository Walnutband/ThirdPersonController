using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Editor used by the Timeline window to customize the appearance of an AnnotationMarker
    [CustomTimelineEditor(typeof(AnnotationMarker))]   
    public class AnnotationMarkerEditor : MarkerEditor //三类编辑器基类：ClipEditor、TrackEditor、MarkerEditor
    {
        const float k_LineOverlayWidth = 6.0f;

        const string k_OverlayPath = "timeline_annotation_overlay";
        const string k_OverlaySelectedPath = "timeline_annotation_overlay_selected";
        const string k_OverlayCollapsedPath = "timeline_annotation_overlay_collapsed";

        static Texture2D s_OverlayTexture;
        static Texture2D s_OverlaySelectedTexture;
        static Texture2D s_OverlayCollapsedTexture;

        static AnnotationMarkerEditor()
        {
            s_OverlayTexture = Resources.Load<Texture2D>(k_OverlayPath);
            s_OverlaySelectedTexture = Resources.Load<Texture2D>(k_OverlaySelectedPath);
            s_OverlayCollapsedTexture = Resources.Load<Texture2D>(k_OverlayCollapsedPath);
        }

        // Draws a vertical line on top of the Timeline window's contents.
        //在USS样式首先被绘制之后，就会调用DrawOverlay，利用这里参数region提供的markerRegion和timelineRegion可以在相应区域上绘制一些额外的GUI
        public override void DrawOverlay(IMarker marker, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            // The `marker argument needs to be cast as the appropriate type, usually the one specified in the `CustomTimelineEditor` attribute
            AnnotationMarker annotation = marker as AnnotationMarker;
            if (annotation == null)
            {
                return;
            }

            if (annotation.showLineOverlay)
            {
                DrawLineOverlay(annotation.color, region);
            }

            DrawColorOverlay(region, annotation.color, uiState);
        }

        // Sets the marker's tooltip based on its title.
        public override MarkerDrawOptions GetMarkerOptions(IMarker marker)
        {
            // The `marker argument needs to be cast as the appropriate type, usually the one specified in the `CustomTimelineEditor` attribute
            AnnotationMarker annotation = marker as AnnotationMarker;
            //这里由于基类实现是构建相关属性为空的对象，而派生类则是构建相关属性为给定值的对象，所以在逻辑上属于互斥的条件分支，而不是递进的关系。
            if (annotation == null)
            {
                return base.GetMarkerOptions(marker);
            }

            return new MarkerDrawOptions { tooltip = annotation.title }; //这里是提供好的有关tip的接口。
        }

        static void DrawLineOverlay(Color color, MarkerOverlayRegion region)
        {
            //重点就是理解这里如何通过region宽度和预想的线段宽度来求得
            // Calculate markerRegion's center on the x axis
            float markerRegionCenterX = region.markerRegion.xMin + (region.markerRegion.width - k_LineOverlayWidth) / 2.0f;
            
            // Calculate a rectangle that uses the full timeline region's height
            Rect overlayLineRect = new Rect(markerRegionCenterX,
                region.timelineRegion.y,
                k_LineOverlayWidth,
                region.timelineRegion.height);

            Color overlayLineColor = new Color(color.r, color.g, color.b, color.a * 0.5f);
            EditorGUI.DrawRect(overlayLineRect, overlayLineColor);
        }

        static void DrawColorOverlay(MarkerOverlayRegion region, Color color, MarkerUIStates state)
        {
            // Save the Editor's overlay color before changing it
            Color oldColor = GUI.color;
            GUI.color = color;

            //Tip：这些都是与当前帧的输入相关的，逻辑顺序是先读取输入，触发输入事件，改变某些属性值（改变UI状态），而后续逻辑就会用到这些更新之后的属性值（也就是读取当前状态），比如赋值给内部变量、作为参数传入方法，等等。
            if (state.HasFlag(MarkerUIStates.Selected))
            {
                GUI.DrawTexture(region.markerRegion, s_OverlaySelectedTexture);
            }
            else if (state.HasFlag(MarkerUIStates.Collapsed))
            {
                GUI.DrawTexture(region.markerRegion, s_OverlayCollapsedTexture);
            }
            else if (state.HasFlag(MarkerUIStates.None))
            {
                GUI.DrawTexture(region.markerRegion, s_OverlayTexture);
            }

            // Restore the previous Editor's overlay color
            /*Tip：恢复颜色，与UI Toolkit相区别，IMGUI使用的样式值都是全局共享的的，所以需要根据当前绘制的内容对这些全局属性进行实时修改，而UI Toolkit中各个元素的样式都是独有的，单独设置即可。
            不过看来，还是各有*/
            GUI.color = oldColor;
        }
    }
}
