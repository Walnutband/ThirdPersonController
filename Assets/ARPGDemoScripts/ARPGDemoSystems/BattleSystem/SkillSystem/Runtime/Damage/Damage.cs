
namespace ARPGDemo.BattleSystem
{
    public class Damage
    {
        /*Tip：发现想要对外只读不写的时候这样写确实很方便。*/
        public float physics { get; private set; } //物理伤害
        public float magic { get; private set; } //魔法伤害

        public int poison { get; private set; } //中毒异常值
        public int rot { get; private set; } //腐败异常值
    }

    public class DamageInfo
    {

    }
}