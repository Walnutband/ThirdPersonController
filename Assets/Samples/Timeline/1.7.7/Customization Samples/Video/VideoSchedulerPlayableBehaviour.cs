using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Samples
{
    // The runtime instance of the VideoTrack. It is responsible for letting the VideoPlayableBehaviours
    //  they need to start loading the video
    public sealed class VideoSchedulerPlayableBehaviour : PlayableBehaviour
    {
        // Called every frame that the timeline is evaluated. This is called prior to
        // PrepareFrame on any of its input playables.
        //Tip：处理的就是视频内容的预加载，
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            // Searches for clips that are in the 'preload' area and prepares them for playback
            var timelineTime = playable.GetGraph().GetRootPlayable(0).GetTime();
            for (int i = 0; i < playable.GetInputCount(); i++)
            {
                if (playable.GetInput(i).GetPlayableType() != typeof(VideoPlayableBehaviour))
                    continue;

                if (playable.GetInputWeight(i) <= 0.0f)
                {
                    //注意这里连接的实际是ScriptPlayable而不是PlayableBehaviour
                    ScriptPlayable<VideoPlayableBehaviour> scriptPlayable = (ScriptPlayable<VideoPlayableBehaviour>)playable.GetInput(i);
                    VideoPlayableBehaviour videoPlayableBehaviour = scriptPlayable.GetBehaviour();
                    double preloadTime = Math.Max(0.0, videoPlayableBehaviour.preloadTime); //最小为0
                    double clipStart = videoPlayableBehaviour.startTime;
                    //当前时间线所在时刻，如果夹在预加载时刻和片段开始时刻之间的话，也就是说要开始预加载了，也就是调用PrepareVideo。
                    if (timelineTime > clipStart - preloadTime && timelineTime <= clipStart)
                        videoPlayableBehaviour.PrepareVideo();
                }
            }
        }
    }
}
