
using System;

namespace ARPGDemo.BattleSystem
{
    // /*Tip：如何对背包分类，取决于游戏设计，就是看物品类型，以及物品之间的本质差异，从而导致各自类型的成员差异。
    // BagType应该是决定该背包能装备哪些类型的物品，而从逻辑上来看，并没有需要分背包的强制性，只是能够更加方便地管理、以及更快地读写，但是也应该有一个*/
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
}