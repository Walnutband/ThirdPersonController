using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TransformTweenClip : PlayableAsset, ITimelineClipAsset
{
    //利用模版对象，就可以序列化在检视器中直接编辑，运行时作为参数传入Create方法即可。
    public TransformTweenBehaviour template = new TransformTweenBehaviour ();
    public ExposedReference<Transform> startLocation;
    public ExposedReference<Transform> endLocation;
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; } //支持混合
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TransformTweenBehaviour>.Create (graph, template);
        TransformTweenBehaviour clone = playable.GetBehaviour ();
        //查表，获取对应的Transform实例
        clone.startLocation = startLocation.Resolve (graph.GetResolver ());
        clone.endLocation = endLocation.Resolve (graph.GetResolver ());
        return playable;
    }
}