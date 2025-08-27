
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public class MoveCommand : ICommand
    {
        private Vector2 moveInput; //移动方向由发令者提供，而移动速度之类的数值就是由听令者自主决定的。
        public MoveCommand(Vector2 _moveInput)
        {
            moveInput = _moveInput;
        }

        public bool Execute(ICommandConsumer consumer)
        {//Tip：如果为空，就代表接收者不是该命令的目标对象，但是对发送命令没有强行限制，能执行就执行，不能执行就算了。
            if (consumer is IMove_Consumer cons)
            {
                return cons.Move(moveInput);
            }
            return false;
        }
    }
}