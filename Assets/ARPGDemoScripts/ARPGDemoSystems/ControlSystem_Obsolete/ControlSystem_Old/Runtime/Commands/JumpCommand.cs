
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old
{
    public class JumpCommand : ICommand
    {
        public JumpCommand() {}

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is IJump_Consumer cons)
            {
                return cons.Jump();
            }
            return false;
        }
    }
}