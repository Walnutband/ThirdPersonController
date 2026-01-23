using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace Timeline.Samples
{
    // Editor representation of a Clip to play video in Timeline.
    [Serializable]
    public class VideoPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        //将视频渲染在相机画面中所有物体的前方或者后方，也就是NearPlane和FarPlane
        public enum RenderMode
        {
            CameraFarPlane,
            CameraNearPlane
        };

        [Tooltip("The video clip to play.")]
        public VideoClip videoClip;

        [Tooltip("Mutes the audio from the video")]
        public bool mute;

        [Tooltip("Loops the video.")]
        public bool loop = true;

        [Tooltip("The amount of time before the video begins to start preloading the video stream.")]
        public double preloadTime = 0.3; //预加载视频数据流

        [Tooltip("The aspect ratio of the video to playback.")]
        public VideoAspectRatio aspectRatio = VideoAspectRatio.FitHorizontally;

        [Tooltip("Where the video content will be drawn.")]
        public RenderMode renderMode = RenderMode.CameraFarPlane;

        [Tooltip("Specifies which camera to render to. If unassigned, the main camera will be used.")]
        public ExposedReference<Camera> targetCamera;

        [Tooltip("Specifies an optional audio source to output to.")]
        public ExposedReference<AudioSource> audioSource;

        // These are set by the track prior to CreatePlayable being called and are used by the VideoSchedulePlayableBehaviour
        // to schedule preloading of the video clip
        public double clipInTime { get; set; } //开始播放时的时间（相对于Clip开始时间的局部偏移时间，说白了就是可以从视频的中间位置开始播放，而不是默认从头开始）
        public double startTime { get; set; } //在轨道上的开始时间。

        // Creates the playable that represents the instance that plays this clip.
        // Here a hidden VideoPlayer is being created for the PlayableBehaviour to use
        // to control playback. The PlayableBehaviour is responsible for deleting the player.
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            //从ExposedReference获取真正的对象引用。没有指定就默认使用主相机
            Camera camera = targetCamera.Resolve(graph.GetResolver());
            if (camera == null)
                camera = Camera.main;

            // If we are unable to create a player, return a playable with no behaviour attached.
            VideoPlayer player = CreateVideoPlayer(camera, audioSource.Resolve(graph.GetResolver()));
            if (player == null)
                return Playable.Create(graph);

            ScriptPlayable<VideoPlayableBehaviour> playable =
                ScriptPlayable<VideoPlayableBehaviour>.Create(graph);

            VideoPlayableBehaviour playableBehaviour = playable.GetBehaviour();
            playableBehaviour.videoPlayer = player;
            playableBehaviour.preloadTime = preloadTime;
            playableBehaviour.clipInTime = clipInTime;
            playableBehaviour.startTime = startTime;

            return playable;
        }

        // The playable assets duration is used to specify the initial or default duration of the clip in Timeline.
        public override double duration
        {
            get
            {
                if (videoClip == null)
                    return base.duration;
                return videoClip.length;
            }
        }

        // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
        // For video clips, we are using built-in support for clip-in, speed, blending and looping.
        public ClipCaps clipCaps
        {
            get
            {
                var caps = ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;
                if (loop)
                    caps |= ClipCaps.Looping;
                return caps;
            }
        }


        VideoPlayer CreateVideoPlayer(Camera camera, AudioSource targetAudioSource)
        {
            if (videoClip == null)
                return null;

            //因为视频播放器作为一个组件，必须依附于游戏对象，但这个游戏对象确实也只是播放视频，没有其他任何作用，所以就直接HideAndDontSave。
            GameObject gameObject = new GameObject(videoClip.name) { hideFlags = HideFlags.HideAndDontSave };
            VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false; //等待指定播放。
            videoPlayer.source = VideoSource.VideoClip; //可以是本地视频VideoClip，也可以网页视频URL
            videoPlayer.clip = videoClip;
            videoPlayer.waitForFirstFrame = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.targetCamera = camera;
            videoPlayer.renderMode = renderMode == RenderMode.CameraFarPlane ? VideoRenderMode.CameraFarPlane : VideoRenderMode.CameraNearPlane;
            videoPlayer.aspectRatio = aspectRatio;
            videoPlayer.isLooping = loop;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            if (mute)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            }
            else if (targetAudioSource != null)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                for (ushort i = 0; i < videoPlayer.clip.audioTrackCount; ++i)
                    videoPlayer.SetTargetAudioSource(i, targetAudioSource);
            }

            return videoPlayer;
        }
    }
}
