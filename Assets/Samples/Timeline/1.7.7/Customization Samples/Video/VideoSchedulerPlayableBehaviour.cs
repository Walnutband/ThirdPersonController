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
        // PrepareFrame on any of its input playables.这是PlayableGraph正常运行机制。
        //Tip：处理的就是视频内容的预加载，
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            // Searches for clips that are in the 'preload' area and prepares them for playback
            //获取当前Timeline的所在时刻（已经运行的时间）
            var timelineTime = playable.GetGraph().GetRootPlayable(0).GetTime();
            
            /*TODO：很显然，如果更实际来看，同一时刻应该最多只可能有一个片段会需要预加载，因为都是按顺序播放的（其实也可能会有其他极端情况，但至少我从设计合理性的角度来思考，无法想象
            这种情况如何才能合理？），所以这里大概可以改成，只要检查到一个在此时需要预加载的片段，那么就可以直接break退出遍历了。*/
            //就是遍历该轨道上的各个片段的Playable
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
