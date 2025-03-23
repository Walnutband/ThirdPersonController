using System;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.PointGame
{
    public class GamePassPanel : MonoBehaviour, IController
    {
        private void Start()
        {//直接读取数据更新UI数据，不用注册方法，因为不涉及值变化。
            transform.Find("RemainSecondsText").GetComponent<Text>().text =
                "剩余时间: " + this.GetSystem<ICountDownSystem>().CurrentRemainSeconds + "s";

            var gameModel = this.GetModel<IGameModel>();

            transform.Find("ScoreText").GetComponent<Text>().text =
                "分数: " + gameModel.Score.Value;

            transform.Find("BestScoreText").GetComponent<Text>().text =
                "最高分数: " + gameModel.BestScore.Value;

        }


        public IArchitecture GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}