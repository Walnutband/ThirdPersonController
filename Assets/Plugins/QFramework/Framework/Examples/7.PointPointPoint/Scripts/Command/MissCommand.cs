namespace QFramework.PointGame
{
    public class MissCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<IGameModel>();

            if (gameModel.Life.Value > 0)
            {
                gameModel.Life.Value--;
            }
            else
            {
                //这里发送的事件从框架来看是应该继承自IEasyevent接口
                this.SendEvent<OnMissEvent>();
            }
        }
    }
}