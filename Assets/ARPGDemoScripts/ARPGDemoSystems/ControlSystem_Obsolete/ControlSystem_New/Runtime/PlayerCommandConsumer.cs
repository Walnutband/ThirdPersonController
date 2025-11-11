
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    public abstract class PlayerCommandConsumer : MonoBehaviour, ICommandConsumer,
    ICommandHandler<MoveCommand>, ICommandHandler<LightAttackCommand>, ICommandHandler<JumpCommand>, ICommandHandler<FallCommand>
    // ICommandHandler,
    {

        // public abstract void HandleCommand(CommandBase _command);
        public abstract void HandleCommand(MoveCommand _command);
        public abstract void HandleCommand(LightAttackCommand _command);
        public abstract void HandleCommand(JumpCommand _command);
        public abstract void HandleCommand(FallCommand _command);
    }
}