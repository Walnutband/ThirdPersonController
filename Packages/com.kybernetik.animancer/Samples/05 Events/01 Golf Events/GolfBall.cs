// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2024 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Samples.Events
{
    /// <summary>Manages the physics of a golf ball.</summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/events/golf">
    /// Golf Events</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Samples.Events/GolfBall
    /// 
    [AddComponentMenu(Strings.SamplesMenuPrefix + "Golf Events - Golf Ball")]
    [AnimancerHelpUrl(typeof(GolfBall))]
    public class GolfBall : MonoBehaviour
    {
        /************************************************************************************************************************/
#if UNITY_PHYSICS_3D
        /************************************************************************************************************************/

        [SerializeField] private Rigidbody _Rigidbody;
        [SerializeField] private Vector3 _HitVelocity = new(0, 10, 10);
        [SerializeField, Meters] private float _ReturnHeight = -10;

        private Vector3 _StartPosition;

        /************************************************************************************************************************/

        public bool ReadyToHit
            => _Rigidbody.isKinematic;

        /************************************************************************************************************************/

        protected virtual void Awake()
        {
            _Rigidbody.isKinematic = true;
            _StartPosition = _Rigidbody.position;
        }

        /************************************************************************************************************************/

        protected virtual void FixedUpdate()
        {
            //下落到一定高度时回归到开始位置
            if (_Rigidbody.position.y < _ReturnHeight)
            {
                _Rigidbody.isKinematic = true;
                _Rigidbody.position = _StartPosition;
            }
        }

        /************************************************************************************************************************/

        //从逻辑上看，应该是“被击打”，调用该方法的就是击打者，而该脚本所在刚体永远是被击打者。
        public void Hit()
        {//Kinematic不会受到velocity作用，因为这属于物理引擎的作用。
            _Rigidbody.isKinematic = false;

#if UNITY_6000_0_OR_NEWER
            _Rigidbody.linearVelocity = _HitVelocity;
#else
            _Rigidbody.velocity = _HitVelocity;
#endif
        }

        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/
        
        protected virtual void Awake()
        {
            SampleReadMe.LogMissingPhysics3DModuleError(this);
        }

        public bool ReadyToHit
            => false;

        public void Hit() { }
        
        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}
