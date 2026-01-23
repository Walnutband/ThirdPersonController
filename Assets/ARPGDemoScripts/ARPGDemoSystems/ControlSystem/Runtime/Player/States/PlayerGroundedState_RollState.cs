
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.ControlSystem.Player
{
    [AddComponentMenu("ARPGDemo/ControlSystem/Player/PlayerGroundedState_RollState")]
    public class PlayerGroundedState_RollState : PlayerGroundedState
    {
        [SerializeField] private FadeAnimation m_Anim;
        [SerializeField] private float m_MoveSpeed;
        private bool m_CanExitState;
        public override bool canExitState => m_CanExitState;
        public override int tempPriority => 10;

        private Vector3 m_RollDir;
        private float m_ActualMoveSpeed;
        private AnimationClipState m_State;

        public override void OnEnterState()
        {
            base.OnEnterState();
            // transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up); //直接转向翻滚方向
            SetOrientation();
            m_CanExitState = false;
            m_ActualMoveSpeed = 0f;
            m_State = m_AnimPlayer.Play(m_Anim);
            m_State.EndedEvent += () =>
            {
                m_IsEnd = true;
                m_CanExitState = true;
            };
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Move();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            
        }

        private void SetOrientation()
        {
            // Debug.Log($"SetOrientation时的MoveInput为：{m_MoveInput}");
            //有输入才转向，否则就按照本来的朝向。
            if (m_MoveInput != Vector2.zero)
            {
                transform.rotation = Quaternion.LookRotation(m_MoveDir, Vector3.up); //直接转向翻滚方向
            }
            m_RollDir = transform.forward;
        }

        private void Move()
        {
            Ease(m_State.normalizedTime);
            // transform.position += m_ActualMoveDir * m_ActualMoveSpeed * Time.deltaTime;
            m_CC.Move(m_RollDir * m_ActualMoveSpeed * Time.deltaTime);
        }

        private void Ease(float progress)
        {
            // Debug.Log("progress:" + progress);
            if (progress <= 0f || progress >= 1f) return;
            if (progress <= 0.5f)
            {
                // m_ActualMoveSpeed = Mathf.Lerp(0f, m_MoveSpeed, progress * 2f);
                // m_ActualMoveSpeed = -Mathf.Pow((progress - 0.5f), 2) + m_MoveSpeed;
                //TODO：使用二次函数抛物线的曲线来控制移速，当然这里只是很低级的运用，不过也暂时足够了。
                m_ActualMoveSpeed = Mathf.Max(-Mathf.Pow(7 * (progress), 2) + m_MoveSpeed, 0f);
            }
            else
            {
                // m_ActualMoveSpeed = Mathf.Lerp(m_MoveSpeed, 0f, (progress - 0.5f) * 2f);
                m_ActualMoveSpeed = Mathf.Max(-Mathf.Pow(7 * (progress), 2) + m_MoveSpeed, 0f);
            }
        }
    }
}