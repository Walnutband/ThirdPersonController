using System;

namespace QFramework.PointGame
{
    //倒计时接口，其实倒计时大概也就一个，所以完全可以不用定义接口
    public interface ICountDownSystem : ISystem
    {

        int CurrentRemainSeconds { get; } //当前剩余秒数（只读）

        void Update();
    }

    public class CountDownSystem : AbstractSystem, ICountDownSystem
    {
        protected override void OnInit()
        {
            this.RegisterEvent<GameStartEvent>(e =>
            {
                mStarted = true;
                mGameStartTime = DateTime.Now;

            });

            this.RegisterEvent<GamePassEvent>(e =>
            {
                mStarted = false;
            });
        }

        private DateTime mGameStartTime { get; set; }

        private bool mStarted = false;

        //倒计时，总的10秒（应当可变，这里就可以认为是magic number）减去已经过去的时间即可
        //每次访问会自动计算当前倒计时
        public int CurrentRemainSeconds => 10 - (int)(DateTime.Now - mGameStartTime).TotalSeconds;
        //在这个系统中会决定游戏是否结束（倒计时结束，与通关区别）
        public void Update() //由作为组件的GamePanel中的Update方法调用
        {
            if (mStarted) //已经开始。
            {
                if (DateTime.Now - mGameStartTime > TimeSpan.FromSeconds(10))
                {
                    /**/
                    this.SendEvent<OnCountDownEndEvent>();
                    mStarted = false;
                }
            }
        }
    }
}