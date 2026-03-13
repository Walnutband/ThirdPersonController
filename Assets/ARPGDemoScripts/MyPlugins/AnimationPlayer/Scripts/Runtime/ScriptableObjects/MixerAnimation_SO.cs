
using ARPGDemo.CustomAttributes;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    [CreateAssetMenu(fileName = "MixerAnimation_SO", menuName = "MyPlugins/AnimationPlayer/MixerAnimation_SO")]
    public class MixerAnimation_SO : ScriptableObject
    {
        [DisplayName("混合动画")]
        public MixerAnimation mixerAnimation;
    }
}