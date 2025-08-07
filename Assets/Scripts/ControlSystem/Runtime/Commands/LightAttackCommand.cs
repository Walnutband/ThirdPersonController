
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public class LightAttackCommand : ICommand
    {
        public LightAttackCommand() { }

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is ILightAttack_Consumer cons)
            {
                return cons.LightAttack();
            }
            return false;
        }
    }
}