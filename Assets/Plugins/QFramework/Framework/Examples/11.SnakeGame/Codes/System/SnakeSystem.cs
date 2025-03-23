using QFramework;

namespace SnakeGame
{
    internal interface ISnakeSystem : ISystem
    {
        void CreateSnake(int w, int h);
    }
    public class SnakeSystem : AbstractSystem, ISnakeSystem
    {
        private ISnake mCurSnake;
        private int mBodyCount; //身体数量（这里是由多个相同的对象连接起来组成的蛇身体），初始为零
        public int mSecondIndex;

        private DelayTask mAutoMoveTask;
        private SnakeMoveEvent mMoveEvent;

        protected override void OnInit()
        {
            this.RegisterEvent<GameOverEvent>(OnGameOver);
            this.RegisterEvent<GameInitEndEvent>(OnGameInitEnd); //游戏初始化结束
            this.RegisterEvent<DirInputEvent>(OnInputDir); //获取方向输入后立刻发送
            this.RegisterEvent<EatFoodEvent>(OnFoodEat);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ISnakeSystem.CreateSnake(int x, int y)
        {
            mCurSnake = new Snake();
            Bigger(x, y); //第一次调用，就是将本体加入数组中
        }
        private void OnFoodEat(EatFoodEvent e) =>
            Bigger(e.x, e.y);
        private void OnGameInitEnd(GameInitEndEvent e) =>
            mAutoMoveTask = this.GetSystem<ITimeSystem>().AddDelayTask(0.3f, AutoMove, true);
        private void OnInputDir(DirInputEvent e) => //获取下一次的移动方向
            mCurSnake.GetMoveDir(e.hor, e.ver);
        private void OnGameOver(GameOverEvent e) =>
            mAutoMoveTask.StopTask();
        private void Bigger(int x, int y)
        {
            mCurSnake.Bigger(mBodyCount++); //这是传入原mBodyCount值，mBodyCount再加一，因为这里参数代表的是索引，所以差一
            this.SendEvent(new SnakeBiggerEvent() { x = x, y = y, dir = mCurSnake.NextMoveDir });
        }
        private void AutoMove()
        {
            mMoveEvent.lastIndex = mCurSnake.TailIndex;
            mMoveEvent.headIndex = mCurSnake.HeadIndex;
            mMoveEvent.nextMove = mCurSnake.NextMoveDir;
            mCurSnake.Move();
            this.SendEvent(mMoveEvent);
        }
    }
}