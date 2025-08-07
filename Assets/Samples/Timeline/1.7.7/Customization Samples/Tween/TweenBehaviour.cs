using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Samples
{
    // Runtime representation of a Tween clip.运行时表示
    //添加到Graph中，用于存储用来实现transform Tween动画的数据。
    public class TweenBehaviour : PlayableBehaviour
    {
        public Transform startLocation;
        public Transform endLocation;

        public bool shouldTweenPosition;
        public bool shouldTweenRotation;

        public AnimationCurve curve;
    }
}
