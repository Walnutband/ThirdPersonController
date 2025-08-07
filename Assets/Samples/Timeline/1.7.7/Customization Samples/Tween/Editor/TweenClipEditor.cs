using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Editor used by the TimelineEditor to customize the view of TweenClip.
    [CustomTimelineEditor(typeof(TweenClip))]
    public class TweenClipEditor : ClipEditor
    {
        //Ques:突然想到，这是Timeline自定义的Editor类，其基类并不是Unity的Editor，而Unity的Editor是可以在检视器中直接拖拽场景物体的，那么更准确的说法应该是那些基类非Editor的编辑器类才需要使用ExposedReference
        public Transform start;
        public Transform end;

        static GUIStyle s_StartTextStyle;
        static GUIStyle s_EndTextStyle;

        static TweenClipEditor()
        {
            s_StartTextStyle = GUI.skin.label;
            s_EndTextStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
        }

        // Called by the Timeline editor to draw the background of a TweenClip.
        //自定义绘制Clip的背景GUI
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            TweenClip asset = clip.asset as TweenClip;

            if (asset == null)
                return;

            PlayableDirector director = TimelineEditor.inspectedDirector; //正在检视的PlayableDirector

            if (director == null)
                return;

            Transform startLocation = director.GetReferenceValue(asset.startLocation.exposedName, out bool startFound) as Transform;
            Transform endLocation = director.GetReferenceValue(asset.endLocation.exposedName, out bool endFound) as Transform;
            //在Clip背景的左右两边分别绘制Transform所在对象的名称。
            if (startFound && startLocation != null)
                EditorGUI.LabelField(region.position, startLocation.gameObject.name, s_StartTextStyle);

            if (endFound && endLocation != null)
                EditorGUI.LabelField(region.position, endLocation.gameObject.name, s_EndTextStyle);
        }
    }
}
