using System.Collections.Generic;
using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MyPlugins.SkillEditor
{
    [TrackColor(0f, 0f, 1.0f)]
    [TrackClipType(typeof(ActionClipAsset))]
    [TrackBindingType(typeof(Animator))]
    public class ActionTrackAsset :  TrackAsset
    {
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AnimationPlayableBinding.Create(name, this); }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, inputCount);
            // ActionTrackBehaviour behaviour =  
            return ScriptPlayable<ActionTrackBehaviour>.Create(graph, inputCount);  
        }
    }
}