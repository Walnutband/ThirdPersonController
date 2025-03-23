namespace QFramework.PointGame
{
    public interface IGameModel : IModel
    {
        BindableProperty<int> KillCount { get; } //击杀数量（只用于记录，不用于显示）

        BindableProperty<int> Gold { get; } //金币数量

        BindableProperty<int> Score { get; } //当前分数

        BindableProperty<int> BestScore { get; } //最高分

        BindableProperty<int> Life { get; } //生命值


    }

    public static class IGameModelExtension
    {
        public static void LoadData(this IGameModel self, IPrefsStorage storage)
        {
            self.BestScore.Value = storage.LoadInt(nameof(self.BestScore), 0);
            self.Life.Value = storage.LoadInt(nameof(self.Life), 3);
            self.Gold.Value = storage.LoadInt(nameof(self.Gold), 0);
        }
    }

    public class GameModel : AbstractModel, IGameModel
    {
        public BindableProperty<int> KillCount { get; } = new BindableProperty<int>()
        {
            Value = 0
        };

        public BindableProperty<int> Gold { get; } = new BindableProperty<int>()
        {
            Value = 0
        };

        public BindableProperty<int> Score { get; } = new BindableProperty<int>()
        {
            Value = 0
        };

        public BindableProperty<int> BestScore { get; } = new BindableProperty<int>()
        {
            Value = 0
        };

        public BindableProperty<int> Life { get; } = new BindableProperty<int>();
        //读取数据，注册存储方法。开始面板，初始数据：最高分0、生命3、金币0
        protected override void OnInit()
        {//加载初始值，注册存储方法
            var storage = this.GetUtility<IPrefsStorage>();
            //加载初始值，并且注册值变化时的回调方法，即存储数据
            BestScore.Value = storage.LoadInt(nameof(BestScore), 0);
            BestScore.Register(v => storage.SaveInt(nameof(BestScore), v));
            storage.Keys.Add(nameof(BestScore));

            Life.Value = storage.LoadInt(nameof(Life), 3);
            Life.Register(v => storage.SaveInt(nameof(Life), v));
            storage.Keys.Add(nameof(Life));

            Gold.Value = storage.LoadInt(nameof(Gold), 0);
            Gold.Register((v) => storage.SaveInt(nameof(Gold), v));
            storage.Keys.Add(nameof(Gold));
        }
        //加载是统一加载，存储是分开存储

    }
}