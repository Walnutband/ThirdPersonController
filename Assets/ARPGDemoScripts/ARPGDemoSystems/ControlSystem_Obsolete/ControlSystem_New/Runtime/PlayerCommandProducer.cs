
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem_New
{
    /*Ques：兼容性，是否可以直接用ICommandConsumer的子接口来代表？就是说一个CommandProducer知道自己的消费对象是什么，也就是一个具体的ICommandConsumer子接口，
    而在此之上如果实现更多接口或是怎样，都能够使用该生产者，只是可能有些接口无法实现，但是并不会出现运行错误，当然也可以对接口方法空实现，那么表现出来就是有些命令
    无效，这也同样是兼容的。*/
    [AddComponentMenu("ARPGDemo/ControlSystem_New/PlayerCommandProducer")]
    public class PlayerCommandProducer : MonoBehaviour, ICommandProducer
    {
        /*TODO：控制目标？*/
        [SerializeField] private PlayerCommandConsumer m_Consumer;

        #region 输入绑定
        [SerializeField] private InputActionAsset m_InputActionAsset;
        // [SerializeField] private InputActionReference cursorUnlock;
        [Space(10)]
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference walk;
        [SerializeField] private InputActionReference lightAttack;
        [SerializeField] private InputActionReference jump;

        #endregion

        private void Awake()
        {

        }

        private void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable()
        {
            // cursorUnlock.ToInputAction().performed += ctx => Cursor.lockState = CursorLockMode.None;
            // cursorUnlock.ToInputAction().canceled += ctx => Cursor.lockState = CursorLockMode.Locked;

            move.ToInputAction().performed += Move;
            move.ToInputAction().canceled += Move;
            walk.ToInputAction().performed += Walk;
            walk.ToInputAction().canceled += Walk;
            lightAttack.ToInputAction().performed += LightAttack;
            jump.ToInputAction().performed += Jump;
        }



        private void OnDisable()
        {
            move.ToInputAction().performed -= Move;
            move.ToInputAction().canceled -= Move;
            walk.ToInputAction().performed -= Walk;
            walk.ToInputAction().canceled -= Walk;
            lightAttack.ToInputAction().performed -= LightAttack;
            jump.ToInputAction().performed -= Jump;
        }

        public void OnStart()
        {
            m_InputActionAsset.Enable();
        }

        public void OnEnd()
        {
            m_InputActionAsset.Disable();
        }

        private void Move(InputAction.CallbackContext ctx)
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            m_Consumer.HandleCommand(new MoveCommand(moveInput));
        }

        private void Walk(InputAction.CallbackContext ctx)
        {
            switch (ctx.phase)
            {
                case InputActionPhase.Performed:
                    m_Consumer.HandleCommand(new MoveCommand(MoveCommand.MoveType.Walk));
                    break;
                case InputActionPhase.Canceled:
                    m_Consumer.HandleCommand(new MoveCommand(MoveCommand.MoveType.WalkCancel));
                    break;
            }
            // m_Consumer.HandleCommand(new MoveCommand(MoveCommand.MoveType.Walk));
        }

        private void LightAttack(InputAction.CallbackContext ctx)
        {
            m_Consumer.HandleCommand(new LightAttackCommand());
        }

        private void Jump(InputAction.CallbackContext context)
        {
            m_Consumer.HandleCommand(new JumpCommand());
        }
    }
}