
using System;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [AddComponentMenu("ARPGDemo/BattleSystem/CollisionDetector")]
    public class CollisionDetector : MonoBehaviour
    {
        private Collider m_Collider;
        public Action<Collider> triggerEnter;
        public Action<Collider> triggerStay;
        public Action<Collider> triggerExit;

        private void Awake()
        {
            m_Collider = GetComponent<Collider>();
        }

        private void Start()
        {
            m_Collider.isTrigger = true; //默认就应该是触发器
        }

        private void OnTriggerEnter(Collider other)
        {
            triggerEnter?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            triggerStay?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            triggerExit?.Invoke(other);
        }

        public void EnableDetector()
        {
            m_Collider.enabled = true;
        }

        public void DisableDetector()
        {
            m_Collider.enabled = false;
        }

    }
}