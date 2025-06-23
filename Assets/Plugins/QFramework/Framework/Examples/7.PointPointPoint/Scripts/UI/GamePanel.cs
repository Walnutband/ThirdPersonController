using UnityEngine;
using UnityEngine.UI;

namespace QFramework.PointGame
{
    public class GamePanel : MonoBehaviour, IController
    {//注册值变化时的方法，更新UI数据，也就是处理表现逻辑
        private ICountDownSystem mCountDownSystem;
        private IGameModel mGameModel;

        private void Awake()
        {
            mCountDownSystem = this.GetSystem<ICountDownSystem>();

            mGameModel = this.GetModel<IGameModel>(); //传入具体层接口作为泛型参数
            //游戏画面中的UI，注册了金币、生命、分数的变化事件
            mGameModel.Gold.Register(OnGoldValueChanged);
            mGameModel.Life.Register(OnLifeValueChanged);
            mGameModel.Score.Register(OnScoreValueChanged);

            // 第一次需要调用一下，初始化各文本值
            OnGoldValueChanged(mGameModel.Gold.Value);
            OnLifeValueChanged(mGameModel.Life.Value);
            OnScoreValueChanged(mGameModel.Score.Value);
        }

        private void OnLifeValueChanged(int life)
        {
            transform.Find("LifeText").GetComponent<Text>().text = "生命：" + life;
        }

        private void OnGoldValueChanged(int gold)
        {
            transform.Find("GoldText").GetComponent<Text>().text = "金币：" + gold;
        }

        private void OnScoreValueChanged(int score)
        {
            transform.Find("ScoreText").GetComponent<Text>().text = "分数:" + score;
        }

        private void Update()
        {
            // 每 20 帧 更新一次。这是个优化技巧，因为可以确定在 至少20 帧内不会有变化，就是利用这一个特殊的已知条件来减少 Update 的调用次数
            if (Time.frameCount % 20 == 0)
            {
                //更新UI文本
                transform.Find("CountDownText").GetComponent<Text>().text =
                    mCountDownSystem.CurrentRemainSeconds + "s";

                mCountDownSystem.Update();
            }
        }

        private void OnDestroy()
        {
            mGameModel.Gold.UnRegister(OnGoldValueChanged);
            mGameModel.Life.UnRegister(OnLifeValueChanged);
            mGameModel.Score.UnRegister(OnScoreValueChanged);
            mGameModel = null;
            mCountDownSystem = null;
        }

        public IArchitecture GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}