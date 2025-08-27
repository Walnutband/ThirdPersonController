
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public class Player_Commands : ICommand
    {
        public enum PlayerCommands
        {
            Zoom,
            ResetCamera,
            ChangePlayer
        }

        private PlayerCommands command = default;
        private Vector2 zoomInput = default;
        private CommandProducer producer = default;

        public Player_Commands(Vector2 _zoomInput)
        {
            zoomInput = _zoomInput;
            command = PlayerCommands.Zoom;
        }

        public Player_Commands()
        {
            command = PlayerCommands.ResetCamera;
        }

        public Player_Commands(PlayerCommands _command)
        {
            command = _command;
        }

        public Player_Commands(CommandProducer _producer)
        {
            producer = _producer;
            command = PlayerCommands.ChangePlayer;
        }

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is IPlayerConsumer pc)
            {
                switch (command)
                {
                    case PlayerCommands.Zoom:
                        return pc.Zoom(zoomInput);
                    case PlayerCommands.ResetCamera:
                        return pc.ResetCamera();
                    case PlayerCommands.ChangePlayer:
                        return pc.ChangePlayer(producer);
                    default:
                        return false;
                }
            }
            else return false;
        }
    }
}