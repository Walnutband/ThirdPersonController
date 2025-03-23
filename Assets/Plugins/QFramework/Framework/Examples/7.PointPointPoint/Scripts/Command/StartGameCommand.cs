namespace QFramework.PointGame
{
    public class StartGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // 重置数据（准确来说是重置游戏实时数据即当前分数和击杀数量，而不是其他存储数据即最高分、金币数量）
            var gameModel = this.GetModel<IGameModel>();
            //击杀数量，当前分数
            gameModel.KillCount.Value = 0;
            gameModel.Score.Value = 0;

            this.SendEvent<GameStartEvent>();
        }
    }
}