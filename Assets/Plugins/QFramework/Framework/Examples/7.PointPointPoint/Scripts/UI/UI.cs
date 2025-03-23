using UnityEngine;

namespace QFramework.PointGame
{
    public class UI : MonoBehaviour, IController
    {
        void Start()
        {//GamePass和GameOver，在这个层面就是控制对应UI面板的显示和隐藏
            this.RegisterEvent<GamePassEvent>(OnGamePass);

            this.RegisterEvent<OnCountDownEndEvent>(e =>
            {
                transform.Find("Canvas/GamePanel").gameObject.SetActive(false);
                transform.Find("Canvas/GameOverPanel").gameObject.SetActive(true);
            }).UnRegisterWhenGameObjectDestroyed(gameObject); //物体销毁时自动取消注册
        }

        /// <summary>
        /// 设置通关界面显示
        /// </summary>
        /// <param name="e"></param>
        private void OnGamePass(GamePassEvent e)
        {
            transform.Find("Canvas/GamePanel").gameObject.SetActive(false);
            transform.Find("Canvas/GamePassPanel").gameObject.SetActive(true);
        }

        void OnDestroy()
        {
            this.UnRegisterEvent<GamePassEvent>(OnGamePass);
        }

        public IArchitecture GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}