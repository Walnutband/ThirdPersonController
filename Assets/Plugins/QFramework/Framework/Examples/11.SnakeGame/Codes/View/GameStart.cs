using UnityEngine;
using QFramework;

namespace SnakeGame
{
    //暂时表现为点击开始按钮的效果
    public class GameStart : MonoBehaviour, IController
    {
        //游戏就从这里发送的InitGameCommand初始化命令开始
        public void Start() => this.SendCommand(new InitGameCommand(20, 20)); //地图宽为20个格子，高为20个格子
        IArchitecture IBelongToArchitecture.GetArchitecture() => SnakeGame.Interface;
    }
}