// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Samples.Basics
{
    /// <summary>
    /// Combines <see cref="BasicMovementAnimations"/> and <see cref="PlayTransitionOnClick"/> into one script.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/basics/character">
    /// Basic Character</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Basics/BasicCharacterAnimations
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Basics - Basic Character Animations")]
    [AnimancerHelpUrl(typeof(BasicCharacterAnimations))]
    public class BasicCharacterAnimations : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private ClipTransition _Idle;
        [SerializeField] private ClipTransition _Move;
        [SerializeField] private ClipTransition _Action;

        private State _CurrentState; //默认值就是State的第一个常量成员

        private enum State
        {
            NotActing,// Idle and Move can be interrupted.
            Acting,// Action can only be interrupted by itself.
        }

        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            _Action.Events.OnEnd = UpdateMovement;
            Debug.Log($"Awake: CurrentState : {_CurrentState}");

        }

        /************************************************************************************************************************/

        protected virtual void Update()
        {
            Debug.Log($"Update: CurrentState : {_CurrentState}");
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
            /*Tip：在_Action结束时检测方向输入，决定过渡到Move或者Idle，这样大概可以实现状态帧的连续（暂不明确具体术语），因为以前写状态机的时候，往往就是要求必须
            经由某些状态才能转换到某些状态，比如跳跃落地后必须先变成Idle再变成Move，那么中间的一帧就是已经落地同时有方向输入，但处于Idle状态，下一帧才会进入Move状态，
            虽然实际情况下可能这不会产生任何影响，但从理论上来看，又确实有可能在具有相关设计（复杂设计）的游戏中因此而出现非常意外的bug。*/
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
                _Animancer.Play(_Action);
            }
        }

        /************************************************************************************************************************/
    }
}
