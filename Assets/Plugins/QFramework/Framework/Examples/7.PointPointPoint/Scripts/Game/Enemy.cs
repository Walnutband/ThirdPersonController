using UnityEngine;

namespace QFramework.PointGame
{
    public class Enemy : MonoBehaviour, IController
    {
        //MonoBehaviour的消息方法，当鼠标在碰撞器上按下时调用
        private void OnMouseDown()
        {
            gameObject.SetActive(false);

            this.SendCommand<KillEnemyCommand>();
        }

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}
