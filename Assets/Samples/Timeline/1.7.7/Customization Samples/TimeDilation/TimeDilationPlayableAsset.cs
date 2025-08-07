using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // A clip for the timeline dilation track.
    [Serializable]
    public class TimeDilationPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        // Using a template for the playable behaviour will allow any serializable fields on the behaviour
        // to be animated.
        //自己构建PlayableBehaviour实例，可以对其进行额外的操作。
        [NoFoldOut] //依赖于该特性的PropertyDrawer，在检视器中显示标记的字段时，会将类中各个成员逐个显示出来。
        public TimeDilationBehaviour template = new TimeDilationBehaviour();

        // Implementation of ITimelineClipAsset, that tells the timeline editor which
        // features this clip supports.
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Extrapolation | ClipCaps.Blending; }
        }

        // Called to creates a runtime instance of the clip.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // Note that template is passed as a parameter - this
            // creates a clone of the template PlayableBehaviour.
            return ScriptPlayable<TimeDilationBehaviour>.Create(graph, template);
        }
    }
}
