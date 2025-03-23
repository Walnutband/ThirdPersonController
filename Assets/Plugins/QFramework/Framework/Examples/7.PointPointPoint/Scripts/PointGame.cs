namespace QFramework.PointGame
{
    public class PointGame : Architecture<PointGame> //整个游戏的架构类
    {
        protected override void Init() //（初次）访问的时候就会进行初始化，同时对注册的System和Model进行初始化
        {
            //注意细节，传入的泛型类型是具体系统的抽象接口，而不是具体的系统类，也不是ISystem。存储的是类型和实例的映射
            RegisterSystem<IScoreSystem>(new ScoreSystem()); //分数系统
            RegisterSystem<ICountDownSystem>(new CountDownSystem()); //倒计时系统
            RegisterSystem<IAchievementSystem>(new AchievementSystem()); //成就系统

            RegisterModel<IGameModel>(new GameModel()); //游戏数据模型

            //注意！当我新建了一个IPrefsStorage接口后，必须要保证这里注册时传入的类型是IPrefsStorage而不是IStorage，
            //因为容器是存储的类型和实例的映射，类型就是键，如果存储的是IStorage，那么用IPrefsStorage获取时当然就会返回null，因为键不匹配
            //RegisterUtility<IStorage>(new PlayerPrefsStorage()); //存储系统
            RegisterUtility<IPrefsStorage>(new PlayerPrefsStorage());
        }
    }
}
