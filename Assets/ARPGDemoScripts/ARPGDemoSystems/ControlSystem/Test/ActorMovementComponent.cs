using ARPGDemo.CustomAttributes;
using MyPlugins.AnimationPlayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem.Test
{
    public class ActorMovementComponent : MonoBehaviour
    {

        public CharacterController cc;
        public AnimatorAgent animPlayer;
        public InputActionReference m_MoveAction;
        public Transform m_CamTransform;
        public FadeAnimation idle;
        public MixerAnimation animations;
        [DisplayName("动画播放层级")]
        public int animationLayerIndex;
        [DisplayName("动画结束过渡时间")]
        public float stopDuration = 0.2f;
        private Vector2 m_MoveInput;
        private Vector3 m_MoveDir;
        private bool m_IsMoving;
        private AnimationMixerState m_MixerState;
        private AnimationClipState m_IdleState;
        private AnimationStateBase m_CurrentState => m_MixerState == null ? m_IdleState : m_MixerState;

        public float angle;
        public float Speed;

        public float moveSpeed;

        private bool isActive;

        private void Update()
        {
            // if (isActive == true)
            // {
                GetMoveDir();

                Move();

                SetAnimation();
                
            // }
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
            m_CurrentState?.Stop(stopDuration);
        }

        private void GetMoveDir()
        {
            m_MoveInput = m_MoveAction.action.ReadValue<Vector2>();
            // m_MoveInput = _input;
            // Debug.Log($"moveInput: {m_MoveInput}");
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + m_CamTransform.right * m_MoveInput.x;

            if (m_MoveInput.Equals(Vector2.zero))
            {
                m_IsMoving = false;
            }
            else
            {
                m_IsMoving = true;
                // m_IsSprinting = false;
            }
        }

        private void Move()
        {
            if (m_IsMoving == true) cc.Move(moveSpeed * m_MoveDir * Time.deltaTime);
        }
        
        private void SetAnimation()
        {
            if (m_MoveInput.Equals(Vector2.zero) && (m_IdleState == null || m_IdleState.isPlaying == false))
            {
                m_IdleState = animPlayer.Play(idle);
            }
            else if ((m_MoveInput != Vector2.zero) && (m_MixerState == null || m_MixerState.isPlaying == false))
            {
                m_MixerState = animPlayer.Play(animations);
                // m_MixerState.SetParameter(() => angle);
                m_MixerState.SetParameter(GetAngle);
                this.angle = Vector3.SignedAngle(m_CamTransform.right, m_MoveDir, Vector3.down);
            }
            
            Vector3 right = m_CamTransform.right; //相机右方向。
            float angle = Vector3.SignedAngle(right, m_MoveDir, Vector3.down);
            // this.angle = angle;
            // this.angle = Mathf.Lerp(this.angle, angle, Speed * Time.deltaTime);
            this.angle = MoveAngleTowards(this.angle, angle, Speed * Time.deltaTime);
        }

        private float GetAngle()
        {
            Debug.Log("获取角度");
            return angle;
        }

        public float MoveAngleTowards(float currentAngle, float targetAngle, float maxDelta)
        {
            // 1. 计算两个角度的差值（自动处理环绕，结果在 -180 到 180 之间）
            float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);

            // 2. 确定旋转方向（根据差值的符号）
            float step = Mathf.Sign(deltaAngle) * Mathf.Min(maxDelta, Mathf.Abs(deltaAngle));

            // 3. 应用旋转并保证结果仍在 -180 到 180 范围内
            float result = currentAngle + step;

            // 4. 将结果规范化到 -180 到 180 范围（防止边界溢出）
            return Mathf.Repeat(result + 180f, 360f) - 180f;
        }
    }
}