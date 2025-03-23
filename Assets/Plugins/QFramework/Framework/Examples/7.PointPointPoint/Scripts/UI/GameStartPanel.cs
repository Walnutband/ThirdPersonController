using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.PointGame
{
    public class GameStartPanel : MonoBehaviour, IController
    { //作为启动界面的控制器，为按钮的点击事件注册方法
        private IGameModel mGameModel;

        void Start()
        {
            //开始游戏按钮
            transform.Find("BtnStart").GetComponent<Button>()
                .onClick.AddListener(() =>
                {
                    gameObject.SetActive(false);
                    this.SendCommand<StartGameCommand>();
                });
            //购买生命按钮
            transform.Find("BtnBuyLife").GetComponent<Button>()
                .onClick.AddListener(() =>
                {
                    this.SendCommand<BuyLifeCommand>();
                });
            //重置数据按钮
            transform.Find("BtnReset").GetComponent<Button>()
                .onClick.AddListener(() =>
                {
                    this.SendCommand<ResetDataCommand>();
                });

            mGameModel = this.GetModel<IGameModel>();
            //因为在开始界面有个购买生命的按钮，会导致金币数量和血量发生变化，所以需要注册值变化方法
            mGameModel.Gold.Register(OnGoldValueChanged);
            mGameModel.Life.Register(OnLifeValueChanged);

            // 第一次需要调用一下
            OnGoldValueChanged(mGameModel.Gold.Value);
            OnLifeValueChanged(mGameModel.Life.Value);
            //因为最高分只会在面板上显示，且在游戏结束时才会更新，不涉及变化处理，所以不需要注册事件
            transform.Find("BestScoreText").GetComponent<Text>().text = "最高分: " + mGameModel.BestScore.Value;
        }

        private void OnLifeValueChanged(int life)
        {
            transform.Find("LifeText").GetComponent<Text>().text = "生命：" + life;
        }

        private void OnGoldValueChanged(int gold)
        {
            if (gold > 0)
            {
                transform.Find("BtnBuyLife").gameObject.SetActive(true);
            }
            else //没有金币时，不显示购买生命按钮
            {
                transform.Find("BtnBuyLife").gameObject.SetActive(false);
            }

            transform.Find("GoldText").GetComponent<Text>().text = "金币：" + gold;
        }



        private void OnDestroy()
        {
            mGameModel.Gold.UnRegister(OnGoldValueChanged);
            mGameModel.Life.UnRegister(OnLifeValueChanged);
            mGameModel = null;
        }

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}
