
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old
{
    public class DodgeCommand : ICommand
    {
        public DodgeCommand() {}

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is IDodge_Consumer cons)
            {
                return cons.Dodge();
            }
            return false;
        }
    }
}