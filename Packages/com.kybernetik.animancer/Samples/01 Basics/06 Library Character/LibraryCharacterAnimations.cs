// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Samples.Basics
{
    /// <summary>
    /// Implements the same behaviour as <see cref="BasicCharacterAnimations"/>
    /// using <see cref="TransitionAsset"/>s.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/basics/library">
    /// Library Basics</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Basics/LibraryCharacterAnimations
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Basics - Library Character Animations")]
    [AnimancerHelpUrl(typeof(LibraryCharacterAnimations))]
    public class LibraryCharacterAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/
        // This script is almost identical to BasicCharacterAnimations, with a few differences:
        // - It uses TransitionAssets instead of ClipTransitions.
        // - It assigns the Action state's End Event after playing it instead of on startup.
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private TransitionAsset _Idle;
        [SerializeField] private TransitionAsset _Move;
        [SerializeField] private TransitionAsset _Action;

        private State _CurrentState;

        private enum State
        {
            NotActing,// Idle and Move can be interrupted.
            Acting,// Action can only be interrupted by itself.
        }

        /************************************************************************************************************************/

        protected virtual void Update()
        {
            switch (_CurrentState)
            {
                case State.NotActing:
                    UpdateMovement();
                    UpdateAction();
                    break;

                case State.Acting:
                    UpdateAction();
                    break;
            }
        }

        /************************************************************************************************************************/

        private void UpdateMovement()
        {
            _CurrentState = State.NotActing;

            float forward = SampleInput.WASD.y;
            if (forward > 0)
            {
                _Animancer.Play(_Move);
            }
            else
            {
                _Animancer.Play(_Idle);
            }
        }

        /************************************************************************************************************************/

        private void UpdateAction()
        {
            if (SampleInput.LeftMouseUp)
            {
                _CurrentState = State.Acting;

                // _Action is an asset that could be shared by multiple different characters
                // as well as instances of the same character so we can't set up the events
                // in the transition because the characters would conflict with each other.
                // Instead, we add events to the state so each character's events are separate.
                /*Tip：由于TransitionState是共享的，所以不能直接在其本身的End事件上注册回调方法（要知道是具备该功能的，自由选择即可），
                而在Play时会创建一个临时实例AnimancerState，可以捕获其引用，然后向End事件注册回调方法，那么就是分别独立、互不影响的了。
                主要是由此了解到相关机制，实际运行时都是以AnimancerState作为对象的，它具有临时性，似乎这里的注释有错，因为TransitionAsset只是对于持久化资产的运行时实例，并没有
                Events成员，有Events成员、可以直接注册回调的是ClipTransition。*/
                AnimancerState state = _Animancer.Play(_Action);
                state.Events(this).OnEnd ??= UpdateMovement;
            }
        }

        /************************************************************************************************************************/
    }
}
