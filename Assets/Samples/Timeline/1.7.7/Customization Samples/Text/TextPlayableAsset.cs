#if TEXT_TRACK_REQUIRES_TEXTMESH_PRO

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Represents the serialized data for a clip on the TextTrack
    [Serializable]
    public class TextPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        //作为资产的序列化字段，直接在检视器中编辑TextClip的相关数据，然后运行时以供TextTrack节点使用来混合显示当下的
        [NoFoldOut]
        [NotKeyable] // NotKeyable used to prevent Timeline from making fields available for animation.
        public TextPlayableBehaviour template = new TextPlayableBehaviour();

        // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        // Creates the playable that represents the instance of this clip.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // Using a template will clone the serialized values
            return ScriptPlayable<TextPlayableBehaviour>.Create(graph, template);
        }
    }
}

#endif
