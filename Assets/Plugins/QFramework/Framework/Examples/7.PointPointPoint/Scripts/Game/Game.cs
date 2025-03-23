using UnityEngine;

namespace QFramework.PointGame
{
    public class Game : MonoBehaviour, IController
    { //控制器类，Controller模块，注册方法（游戏开始，倒计时结束，游戏通关）
        private void Awake()
        {
            this.RegisterEvent<GameStartEvent>(OnGameStart);
            //计时结束事件
            //因为过于简单，所以直接用一个匿名方法。
            //使用GameObject的SetActive方法可以直接统一设置其本身和子对象的enabled属性即启用或禁用。
            this.RegisterEvent<OnCountDownEndEvent>(e => { transform.Find("Enemies").gameObject.SetActive(false); })
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            //游戏通过事件（在限定时间内点掉所有方块）
            this.RegisterEvent<GamePassEvent>(e => { transform.Find("Enemies").gameObject.SetActive(false); })
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        //OnEnable or OnDisable are called as the GameObject received SetActive(true) or SetActive(false).
        //使用SetActive方法时会对应调用OnEnable或OnDisable方法
        private void OnGameStart(GameStartEvent e) //属于表现逻辑的方法，就是显示敌人即那些方块
        {
            var enemyRoot = transform.Find("Enemies"); //Enemies在该类所挂载的对象下作为子对象

            enemyRoot.gameObject.SetActive(true);

            foreach (Transform childTrans in enemyRoot)
            {
                childTrans.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            this.UnRegisterEvent<GameStartEvent>(OnGameStart);
        }

        public IArchitecture GetArchitecture()
        {
            return PointGame.Interface;
        }
    }
}