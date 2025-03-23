using UnityEngine;

namespace QFramework.PointGame
{
    public interface IScoreSystem : ISystem //逻辑含义，分数系统的抽象接口
    {

    }

    public class ScoreSystem : AbstractSystem, IScoreSystem
    {
        /*初始化，注册了三个方法：游戏通关、击杀敌人（点击方块）、点到空白（Miss）。
        */
        protected override void OnInit()
        {
            var gameModel = this.GetModel<IGameModel>(); //编译时确定类型，运行时确定对象
            //通关时计算最终得分，以及更新最高分
            this.RegisterEvent<GamePassEvent>(e =>
            {
                var countDownSystem = this.GetSystem<ICountDownSystem>(); //因为需要获取剩余时间来计算得分

                var timeScore = countDownSystem.CurrentRemainSeconds * 10; //剩余时间的得分

                gameModel.Score.Value += timeScore;
                //更新最高分
                if (gameModel.Score.Value > gameModel.BestScore.Value)
                {
                    gameModel.BestScore.Value = gameModel.Score.Value;

                    Debug.Log("新纪录");
                }
            });

            this.RegisterEvent<OnEnemyKillEvent>(e =>
            {
                gameModel.Score.Value += 10;
                Debug.Log("得分:10");
                Debug.Log("当前分数:" + gameModel.Score.Value);
            });

            this.RegisterEvent<OnMissEvent>(e =>
            {
                gameModel.Score.Value -= 5;
                Debug.Log("得分:-5");
                Debug.Log("当前分数:" + gameModel.Score.Value);
            });
        }
    }
}