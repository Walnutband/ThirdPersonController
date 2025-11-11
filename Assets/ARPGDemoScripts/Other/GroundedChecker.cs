using System;
using UnityEngine;

namespace ARPGDemo
{
    [AddComponentMenu("ARPGDemo/Other/GroundedChecker")]
    public class GroundedChecker : MonoBehaviour, ICanMoveWithPlatform
    {
        public bool isGrounded;
        public Transform m_Root;
        public Transform root
        {
            get => m_Root;
            set
            {
                m_Root = value;
            }       
        }
        public bool onPlatform
        {
            set
            {
                onPlatformEvent?.Invoke(value);
            }
        }

        // private MovablePlatform m_Platform;

        public event Action onLanded; //从滞空到落地的一刻。
        public event Action onAir; //离开地面的一刻
        public event Action<bool> onPlatformEvent; 
        
        private void Awake()
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter");
            if (isGrounded == false)
            {
                isGrounded = true;
                onLanded?.Invoke();
            }
            if (other.TryGetComponent<MovablePlatform>(out var _platform))
            {
                _platform.CaptureTarget(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("OnTriggerExit");
            if (isGrounded == true)
            {
                isGrounded = false;
                onAir?.Invoke();
            }
            if (other.TryGetComponent<MovablePlatform>(out var _platform))
            {
                _platform.ReleaseTarget(this);
            }
        }

        public void Release()
        {
            
        }
    }
}