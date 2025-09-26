
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    /*TODO：其实这个命令类的设计值得进一步思考，可能代表了命令模式的一个新机制，就是特殊化，将对于某些个体特定的一些命令打包在一起，因为基本不会复用，放在一起还更加具有逻辑性，
    */
    public class Player_Commands : ICommand
    {
        public enum Commands
        {
            Zoom,
            ResetCamera,
            ChangePlayer,
            UseItem, //使用物品，就是消耗品。
            //Tip：交互键，就是将捡物品、与人对话等功能打包在一起，具体交互对象是通过碰撞检测获取的，而实际采取哪个交互行为、应该取决于当前检测到的可交互对象及其优先级。
            Interact, 
        }

        private Commands command = default;
        private Vector2 zoomInput = default;
        private CommandProducer producer = default;

        public Player_Commands(Vector2 _zoomInput)
        {
            zoomInput = _zoomInput;
            command = Commands.Zoom;
        }

        //Ques：是否要设置默认命令呢？
        // public Player_Commands()
        // {
        //     command = PlayerCommands.ResetCamera;
        // }

        public Player_Commands(Commands _command)
        {
            command = _command;
        }

        public Player_Commands(CommandProducer _producer)
        {
            producer = _producer;
            command = Commands.ChangePlayer;
        }

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is IPlayerConsumer pc)
            {
                switch (command)
                {
                    case Commands.Zoom:
                        return pc.Zoom(zoomInput);
                    case Commands.ResetCamera:
                        return pc.ResetCamera();
                    case Commands.ChangePlayer:
                        return pc.ChangePlayer(producer);
                    case Commands.UseItem:
                        return pc.UseItem();
                    case Commands.Interact:
                        return pc.Interact();
                    default:
                        return false;
                }
            }
            else return false;
        }
    }
}