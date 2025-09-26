using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [CreateAssetMenu(fileName = "ItemDataList_SO", menuName = "ARPGDemo/BattleSystem/ItemDataList_SO", order = 0)]
    public class ItemDataList_SO : ScriptableObject
    {
        public List<ItemData> itemDataList = new List<ItemData>();
    }
}