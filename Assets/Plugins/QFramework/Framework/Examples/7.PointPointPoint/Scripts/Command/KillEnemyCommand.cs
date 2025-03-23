namespace QFramework.PointGame
{
    public class KillEnemyCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<IGameModel>();

            gameModel.KillCount.Value++; //击杀数量

            if (UnityEngine.Random.Range(0, 10) < 3) //30%的概率掉落金币（1~3个）
            {
                gameModel.Gold.Value += UnityEngine.Random.Range(1, 3);
            }

            this.SendEvent<OnEnemyKillEvent>();
            //可见在这个命令中会决定游戏是否通关
            if (gameModel.KillCount.Value == 10) //因为设置的总量就只有10个。其实应当是可变的
            {
                this.SendEvent<GamePassEvent>(); //点完方块就通过。
            }
        }
    }
}