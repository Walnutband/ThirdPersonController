// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>Executes <see cref="Invoker.InvokeAllAndClear"/> after animations in the Dynamic Update cycle.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/InvokerDynamic
        [AnimancerHelpUrl(typeof(InvokerDynamic))]
        [AddComponentMenu("")]// Singleton creates itself.这里实际作用是不会在Add Component搜索窗口中找到该MonoBehaviour组件。
        public class InvokerDynamic : Invoker
        {
            /************************************************************************************************************************/

            private static InvokerDynamic _Instance; //单例

            /// <summary>Creates the singleton instance.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static InvokerDynamic Initialize()
            //Ques：有一说一，像这样使用ref关键字传参，在Animancer的源码中已经看到过很多次了，说实话我感觉很不直观，经常无法立刻了解到传入的是哪个，以及方法内赋值给哪个。
                => AnimancerUtilities.InitializeSingleton(ref _Instance);

            /************************************************************************************************************************/

            /// <summary>Should this system execute events?</summary>
            /// <remarks>If disabled, this system will not be re-enabled automatically.</remarks>
            public static bool Enabled
            {
                get => _Instance != null && _Instance.enabled;
                set
                {
                    if (value)
                    {
                        Initialize();
                        _Instance.enabled = true;
                    }
                    else if (_Instance != null)
                    {
                        _Instance.enabled = false;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>After animation update with dynamic timestep.</summary>
            protected virtual void LateUpdate()
            {
                // Debug.Log("LateUpdate");
                InvokeAllAndClear();
            }

            /************************************************************************************************************************/
        }
    }
}

