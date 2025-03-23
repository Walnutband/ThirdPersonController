namespace QFramework.PointGame
{
    public class BuyLifeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {//此处就是购买生命值的交互逻辑，而同时Gold和Life的值变化事件调用的方法会修改对应的UI属性值，这就是表现逻辑
            var gameModel = this.GetModel<IGameModel>();

            gameModel.Gold.Value--;
            gameModel.Life.Value++;
        }
    }
}