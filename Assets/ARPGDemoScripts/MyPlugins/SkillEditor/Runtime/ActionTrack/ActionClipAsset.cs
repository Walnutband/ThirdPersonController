
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MyPlugins.SkillEditor
{
    [Serializable]
    public class ActionClipAsset : PlayableAsset, ITimelineClipAsset
    {
        public ActionClipBehaviour template;
        // public AnimationClip clip;

        //TODO：先占个位
        // public ClipCaps clipCaps => ClipCaps.None;
        public ClipCaps clipCaps 
        {
            get
            {
                return ClipCaps.Extrapolation | ClipCaps.Blending | ClipCaps.SpeedMultiplier;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // var playable = ScriptPlayable<ActionClipBehaviour>.Create(graph, template);
            // ActionClipBehaviour clone = playable.GetBehaviour();
            // // clone.clip = clip;
            // return playable;

            return AnimationClipPlayable.Create(graph, template.clip);
        }
    }
}