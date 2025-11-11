using UnityEngine;
using MyPlugins.AnimationPlayer;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace ARPGDemo.Test
{
    public class PlayerControllerTest : MonoBehaviour
    {
        public MixerAnimation m_MoveAnimations;
        public AnimatorAgent m_AnimatorAgent;
        // public InputAction m_AttackOneAction;
        // public AnimationClip m_AttackOneClip;
        // public InputAction m_AttackTwoAction;
        // public AnimationClip m_AttackTwoClip;
        // public FadeAnimation m_AttackTwo;

        // public AnimationClip idle;
        // public InputAction m_AttackAction;
        public InputActionAsset m_InputActions;
        public InputActionReference m_MoveAction;
        public InputActionReference m_WalkAction;
        public InputActionReference m_SprintAction;
        public InputActionReference m_AttackAction;
        public FadeAnimation m_Attack1;
        public FadeAnimation m_Attack2;
        public FadeAnimation m_Attack3;

        private AnimationClipState m_AttackState;
        // public List<AnimationClip> clips = new List<AnimationClip>();
        private AnimationMixerState m_MoveState;

        public bool isEnded;
        public bool isMoving;
        public bool isWalk;
        public bool isSprint;

        [Header("移动速度")]
        public float walkSpeed = 2f;
        public float runSpeed = 5f;
        public float sprintSpeed = 8f;

        public float targetSpeed;
        public float? specialSpeed;
        public float currentSpeed;
        public float duration;

        // [Header("混合动画"),]

        // [ContextMenu("测试")]
        // public void aa()
        // {
        //     clips = new List<AnimationClip>(3);
        //     clips.Add(null);
        //     clips[0] = m_Attack1.clip;
        //     clips[2] = m_Attack1.clip;
        // }

        private void Start()
        {
            Debug.Log("播放Idle动画");
            // m_AnimatorAgent.Play(idle);
            // m_AnimatorAgent.Play(m_MoveAnimations);
        }


        private void OnEnable()
        {
            m_InputActions.Enable();
            // m_AttackOneAction.Enable();
            // m_AttackTwoAction.Enable();
            // m_AttackOneAction.started += ctx => m_AnimatorAgent.Play(m_AttackOneClip);
            // m_AttackTwoAction.started += ctx => m_AnimatorAgent.Play(m_AttackTwoClip);
            // m_AttackTwoAction.started += ctx => m_AnimatorAgent.Play(m_AttackTwo);
            // m_AttackAction.Enable();
            m_AttackAction.action.started += Attack;
            m_MoveAction.action.performed += ctx =>
            {
                Debug.Log("Move");
                isMoving = true;
                m_MoveState = m_AnimatorAgent.Play(0, m_MoveAnimations);
                // targetSpeed = runSpeed;
                // m_MoveState.SetParameter(runSpeed);
            };
            m_MoveAction.action.canceled += ctx =>
            {
                isMoving = false;
                // targetSpeed = 0f; 
                // currentSpeed = 0f;
                // m_MoveState.SetParameter(0f);
            };
            m_WalkAction.action.performed += ctx =>
            {
                // if (!isMoving) return;

                // currentSpeed = (float)(m_MoveState?.GetParameter());
                // targetSpeed = walkSpeed;
                // specialSpeed = walkSpeed;
                
                isWalk = true;
                isSprint = false;
                // targetSpeed = 
                // m_MoveState.SetParameter(walkSpeed);
                // currentSpeed = walkSpeed;
            };
            m_WalkAction.action.canceled += ctx =>
            {
                // if (!isMoving) return;
                isWalk = false;
                // targetSpeed = runSpeed;
                // specialSpeed = null;
            };
            m_SprintAction.action.performed += ctx =>
            {
                if (!isMoving) return;

                // currentSpeed = (float)(m_MoveState?.GetParameter());
                // targetSpeed = sprintSpeed;
                // specialSpeed = sprintSpeed;
                isWalk = false;
                isSprint = true;
                // m_MoveState.SetParameter(sprintSpeed);
            };
            m_SprintAction.action.canceled += ctx =>
            {
                // if (!isMoving) return;
                isSprint = false;
                // targetSpeed = runSpeed;
                // specialSpeed = null;
            };
        }
        private void OnDisable()
        {
            m_InputActions.Disable();
            // m_AttackOneAction.Disable();
            // m_AttackTwoAction.Disable();
            // m_AttackAction.Disable();
            m_AttackAction.action.started -= Attack;
        }

        private void Update()
        {
            /*Tip：targetSpeed可以完全确定，但是currentSpeed由插值决定、存在不确定性
            插值完全可以直接用currentSpeed作为插值的起始值，效果可以认为是等价的。
            */

            /*Tip:之前就感觉移动动画的控制很别扭，因为InputSystem自身的触发机制。但是现在感觉，压根就不应该直接依赖于回调，就是应该把一部分逻辑放在监测部分来。*/

            if (isMoving)
            {
                targetSpeed = runSpeed;
                if (isWalk) targetSpeed = walkSpeed;
                else if (isSprint) targetSpeed = sprintSpeed;
            }
            else
            {
                targetSpeed = 0f;
            }

            // if (isWalk) targetSpeed = walkSpeed;
            // if (isSprint) targetSpeed = sprintSpeed;
            // m_MoveState?.SetParameter(currentSpeed + Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / duration));
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / duration);
            // currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, duration * Time.deltaTime);
            if (Mathf.Abs(currentSpeed - targetSpeed) < 0.01f) currentSpeed = targetSpeed;
            m_MoveState?.SetParameter(currentSpeed);
            // Debug.Log($"实际速度：{currentSpeed}");
        }

        private void Attack(InputAction.CallbackContext _ctx)
        {
            if (m_AttackState == null)
            {
                // Debug.Log("攻击1");
                // m_AttackState = m_AnimatorAgent.Play(1, m_Attack1);
                m_AttackState = m_AnimatorAgent.Play(m_Attack1);
                m_AttackState.EndedEvent += () =>
                {
                    m_AttackState.Stop(); 
                };
            }
            else
            {
                if (m_AttackState.clip == m_Attack1.clip)
                {
                    // Debug.Log("攻击2");
                    // m_AttackState = m_AnimatorAgent.Play(1, m_Attack2);
                    m_AttackState = m_AnimatorAgent.Play(m_Attack2);
                    m_AttackState.EndedEvent += () =>
                    {
                        m_AttackState.Stop(); 
                    };
                }
                else if (m_AttackState.clip == m_Attack2.clip)
                {
                    // Debug.Log("攻击3");
                    // m_AttackState = m_AnimatorAgent.Play(1, m_Attack3);
                    m_AttackState = m_AnimatorAgent.Play(m_Attack3);
                    m_AttackState.EndedEvent += () =>
                    {
                        m_AttackState.Stop(); 
                        // isEnded = true;
                        // Debug.Log("第三段结束");
                    };
                }
                else if (m_AttackState.clip == m_Attack3.clip)
                {
                    // Debug.Log("从攻击3到攻击1");
                    // m_AttackState = m_AnimatorAgent.Play(1, m_Attack1);
                    m_AttackState = m_AnimatorAgent.Play(m_Attack1);
                    m_AttackState.EndedEvent += () =>
                    {
                        m_AttackState.Stop();
                    };
                    // Debug.Log("尝试播放从第三段到播放第一段");
                    // if (isEnded == true)
                    // {
                    //     isEnded = false;
                    //     m_AttackState = m_AnimatorAgent.Play(1, m_Attack1);
                    //     m_AttackState.EndedEvent += () =>
                    //     {
                    //         m_AttackState.Stop(); 
                    //     };
                    // }
                }
            }
        }

    }   
}