// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Samples.Basics
{
    /// <summary>
    /// Starts with an idle animation and performs an action when the user clicks the mouse, then returns to idle.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/basics/action">
    /// Basic Action</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Basics/PlayAnimationOnClick
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Basics - Play Animation On Click")]
    [AnimancerHelpUrl(typeof(PlayAnimationOnClick))]
    public class PlayAnimationOnClick : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;
        [SerializeField] private AnimationClip _Idle;
        [SerializeField] private AnimationClip _Action;

        /************************************************************************************************************************/

        protected virtual void OnEnable()
        {
            _Animancer.Play(_Idle);
        }

        /************************************************************************************************************************/

        protected virtual void Update()
        {
            if (SampleInput.LeftMouseUp)
            {
                // Play the action animation and grab the returned state which we can use to control it.
                AnimancerState state = _Animancer.Play(_Action);

                // Rewind the animation because Play doesn't do that automatically if it was already playing.
                /*Tip：因为这里的Play方法目标状态如果正在播放的话就无影响，所以在此处加上每次输入时就设置到开始位置的代码，就相当于Play方法不会因为正在播放目标状态而无影响，
                不过我疑惑的是，为何没有提供Play相关的额外的方法，或者是多一个参数，之类的，来实现一行调用即可？？？*/
                state.Time = 0;

                // When the animation reaches its end, call OnEnable to go back to idle.
                //对AnimancerEvent的公开字段委托callback注册回调方法
                state.Events(this).OnEnd ??= OnEnable; //该操作符表示，如果OnEnd为空，才会赋值，在文档上说是避免重复分配，提高性能。
                //经测试，确实是在构造时（AnimancerEvent.Sequence的私有字段_EndEvent）的默认值NaN
                Debug.Log($"的_EndEvent的归一化时间是{state.Events(this).EndEvent.normalizedTime}"); 
            }
        }

        /************************************************************************************************************************/
    }
}
