

using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem.Player
{
    /*作为作为分层状态的上层状态，主要是处理下层状态所共同需要处理的一些输入，总之本质上就是处理下层状态共同需要处理的逻辑，放在这里以便复用。*/
    public abstract class PlayerGroundedState : PlayerStateBase
    {
        [SerializeField] protected InputActionReference m_MoveAction;
        [SerializeField] protected Transform m_CamTransform;
        [SerializeField] protected CharacterController m_CC;
        protected Vector2 m_MoveInput;
        protected Vector3 m_MoveDir;

        public override void OnEnterState()
        {
            base.OnEnterState();
            GetMoveDir();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            GetMoveDir();

        }

        private void GetMoveDir()
        {
            m_MoveInput = m_MoveAction.action.ReadValue<Vector2>();
            // Debug.Log($"moveInput: {m_MoveInput}");
            /*移动方向同时受到相机和方向输入的影响。*/
            Vector3 camFoward = new Vector3(m_CamTransform.forward.x, 0, m_CamTransform.forward.z).normalized;
            m_MoveDir = camFoward * m_MoveInput.y + m_CamTransform.right * m_MoveInput.x;
        }
    }

}