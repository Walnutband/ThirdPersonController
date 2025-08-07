using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Timeline track to play videos.
    // This sample demonstrates the following
    //  * Using built in blending, speed and clip-in capabilities in custom clips.
    //  * Using ClipEditors to customize clip drawing.
    //  * Using a mixer PlayableBehaviour to perform look-ahead operations.
    //  * Managing UnityEngine.Object lifetime (VideoPlayer) with a PlayableBehaviour.
    //  * Using ExposedReferences to reference Components in the scene from a PlayableAsset.
    [Serializable]
    [TrackClipType(typeof(VideoPlayableAsset))] //指定该轨道可以放入的Clip类型
    [TrackColor(0.008f, 0.698f, 0.655f)]
    public class VideoTrack : TrackAsset
    {
        // Called to create a PlayableBehaviour instance to represent the instance of the track, commonly referred
        // to as a Mixer playable.
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // This is called immediately before CreatePlayable on VideoPlayableAsset.
            // Each playable asset needs to be updated to the last clip values.
            foreach (var clip in GetClips()) //获取该轨道上当前所有的Clip
            {
                var asset = clip.asset as VideoPlayableAsset;
                if (asset != null)
                {
                    asset.clipInTime = clip.clipIn;
                    asset.startTime = clip.start;
                }
            }

            return ScriptPlayable<VideoSchedulerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}
