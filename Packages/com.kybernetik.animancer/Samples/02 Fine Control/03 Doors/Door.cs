// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Samples.FineControl
{
    /// <summary>
    /// An <see cref="IInteractable"/> door which toggles between open and closed when something interacts with it.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/fine-control/doors">
    /// Doors</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.FineControl/Door
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Fine Control - Door")]
    [AnimancerHelpUrl(typeof(Door))]
    [SelectionBase]
    public class Door : MonoBehaviour, IInteractable
    {
        /************************************************************************************************************************/

        [SerializeField] private SoloAnimation _SoloAnimation;

        /************************************************************************************************************************/

        /// <summary>[<see cref="IInteractable"/>] Toggles this door between open and closed.</summary>
        public void Interact() 
        {//通过一整段动画的正放和倒放来实现开门和关门的效果，NormalizedTime记录的就是当前动画播放进度，由于是开门动画（默认条件），所以轻易从播放进度判断当前是开关还是关门。
            if (_SoloAnimation.Speed == 0)
            {
                //此时，肯定处于关门状态，设置Speed为1就会正向播放动画，也就是开门动画。
                bool playForwards = _SoloAnimation.NormalizedTime < 0.5f;
                _SoloAnimation.Speed = playForwards ? 1 : -1;
            }
            else
            {
                _SoloAnimation.Speed = -_SoloAnimation.Speed;
            }

            _SoloAnimation.IsPlaying = true;
        }

        /************************************************************************************************************************/
    }
}
