
using System.Collections;
using ARPGDemo.AbilitySystem;
using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.AI;

namespace ARPGDemo.ControlSystem.Enemy
{
    
    public class SimpleEnemyContorller : MonoBehaviour
    {
        public EnemyASC ASC;
        public EnemyStateMachine m_StateMachine;
        public EnemyDeadState m_DeadState;
        public bool isMoving;
        public Transform targetPos;
        public NavMeshAgent agent;
        public AnimationClip hurtAnim;
        public int animLayer = 1;
        public float fadeDuration;
        public float stopDuration = 0.2f;
        private AnimationClipState m_State;

        public void OnHurt()
        {
            m_State = GetComponentInChildren<AnimatorAgent>().Play(animLayer, hurtAnim, fadeDuration);
            StartCoroutine(ExitHurt(hurtAnim.length));
        }
        private IEnumerator ExitHurt(float _duration)
        {
            yield return new WaitForSeconds(_duration);
            m_State.Stop(stopDuration);
        }

        private void Awake()
        {
            ASC = GetComponent<EnemyASC>();
            agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            ASC.AS.RegisterHPChangedEvent(Dead);
        }
        private void OnDisable()
        {
            ASC.AS.UnregisterHPChangedEvent(Dead);
        }

        private void Update()
        {
            // Debug.Log($"是否在NavMesh上：{agent.isOnNavMesh}");
            m_StateMachine.OnUpdate();
            if (agent == null) return;
            if (isMoving)
            {
                //未在移动。
                if (agent.velocity.magnitude > 0.1f == false)
                {
                    agent.SetDestination(targetPos.position);
                }
                    
            }
            else
            {
                if (agent.isStopped == false) agent.isStopped = true;
            }
        }

        private void Dead(float _value)
        {
            if (_value <= 0)
            {
                m_StateMachine.TrySetState(m_DeadState);
            }
        }
    }
}