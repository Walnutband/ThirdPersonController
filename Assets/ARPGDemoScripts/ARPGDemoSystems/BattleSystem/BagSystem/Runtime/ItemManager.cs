using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{

    public class ItemManager : SingletonMono<ItemManager>
    {
        /*Tip：似乎ItemManager不是用来提供查询的，这属于数据库的任务，也就是InventoryManager的任务。ItemManager应该是管理场景中的物品的，*/
        // private Dictionary<uint, ItemData> m_ItemDataDict = new Dictionary<uint, ItemData>();
        // public ItemData GetItemData(uint itemId)
        // {
        //     ItemData itemData = null;
        //     m_ItemDataDict.TryGetValue(itemId, out itemData);
        //     return itemData;
        // }
    }
}