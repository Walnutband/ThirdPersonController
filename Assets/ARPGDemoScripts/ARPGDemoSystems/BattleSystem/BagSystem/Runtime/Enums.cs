
using System;

namespace ARPGDemo.BattleSystem
{
    #region ItemEnums物品枚举

    // public enum ItemType
    // {
    //     None = 0,
    //     Currency, //货币，不过可能涉及更多的逻辑
    //     Weapon, //武器
    //     Armor,  //防具
    //     Amulet, //护符
    //     Consumable, //消耗品
    //     Material, //材料
    //     Other
    // }

    public enum ItemType
    {
        All = -1, //All = ~0  感觉取反是最优雅的直接取到全1二进制int值的方式。注意是补码，所以-1就是全1
        None = 0,
        Currency = 1 << 0, //货币，不过可能涉及更多的逻辑
        Weapon = 1 << 1, //武器
        Armor = 1 << 2,  //防具
        Amulet = 1 << 3, //护符
        Consumable = 1 << 4, //消耗品
        Material = 1 << 5, //材料
        
    }


    public enum ItemPropertyType
    {
        None = 0,
        HP, MP, SP,
        PhysicalAttack,
        PhysicalDefense,
        MagicAttack,
        MagicDefense,
    }
    #endregion

    #region BagEnums背包枚举
    /*Tip：如何对背包分类，取决于游戏设计，就是看物品类型，以及物品之间的本质差异，从而导致各自类型的成员差异。
BagType应该是决定该背包能装备哪些类型的物品，而从逻辑上来看，并没有需要分背包的强制性，只是能够更加方便地管理、以及更快地读写，但是也应该有一个*/
    // [Flags]
    // public enum BagType
    // {
    //     None = 0,
    //     Weapon = 1 << 0, //武器，
    //     Armor = 1 << 1,
    //     Amulet = 1 << 2,
    //     Consumable = 1 << 3,
    //     All = 1 << 4
    // }
    #endregion
}