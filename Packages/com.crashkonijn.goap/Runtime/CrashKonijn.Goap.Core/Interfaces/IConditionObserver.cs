namespace CrashKonijn.Goap.Core
{
    public interface IConditionObserver
    {
        //是否满足指定条件
        bool IsMet(ICondition condition);
        //设置世界数据。“修改对于世界的认知”
        void SetWorldData(IWorldData worldData);
    }
}
