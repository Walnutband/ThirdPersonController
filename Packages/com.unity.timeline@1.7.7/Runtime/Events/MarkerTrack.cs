using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Use this track to add Markers bound to a GameObject.
    /// </summary>
    [Serializable]
    [TrackBindingType(typeof(GameObject))]
    [HideInMenu] //Tip：在菜单中隐藏，是因为该Track是作为事件轨道的基类，而内置的是SignalTrack直接继承自MarkerTrack，开发者也可以自行扩展其他事件轨道。
    [ExcludeFromPreset]
    [TimelineHelpURL(typeof(MarkerTrack))]
    public class MarkerTrack : TrackAsset
    {
        /// <inheritdoc/>
        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                return this == timelineAsset?.markerTrack ?
                    new List<PlayableBinding> { ScriptPlayableBinding.Create(name, null, typeof(GameObject)) } :
                    base.outputs;
            }
        }
    }
}
