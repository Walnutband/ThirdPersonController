

using System;
using UnityEngine;
using UnityEngine.Playables;

namespace MyPlugins.SkillEditor
{
    [Serializable]
    public class ActionClipBehaviour : PlayableBehaviour
    {
        // public enum ActionLayer
        // {
        //     FullBody,
        //     UpperBody,
        //     LowerBody
        // }

        public AnimationClip clip; //所播放的动画片段
        public float fadeIn; //过渡进入的时间，因为时间轴上通常是单向的，就固定一个过渡时间就行了。

    }
}