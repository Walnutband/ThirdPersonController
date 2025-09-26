
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*TODO：搞清楚InventoryManager要做什么事————存储背包信息的用处是什么？可以想到交易功能，*/
    [AddComponentMenu("ARPGDemo/系统与管理器/InventoryManager")]
    public class InventoryManager : SingletonMono<InventoryManager>
    {
        //ID与ItemData的映射
        private Dictionary<uint, ItemData> m_ItemDataDict = new Dictionary<uint, ItemData>();
        public string Path_ItemDataList_SO;

        protected override void Awake()
        {
            m_Instance = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
            base.Awake();
        }

        private void Start()
        {
            LoadItemDataList(Path_ItemDataList_SO);
        }

        private void LoadItemDataList(string _path)
        {
            ItemDataList_SO itemDataList_SO = Resources.Load<ItemDataList_SO>(_path);
            foreach (ItemData itemData in itemDataList_SO.itemDataList)
            {
                m_ItemDataDict.Add(itemData.id, itemData);
            }
        }

        public ItemData GetItemData(uint itemId)
        {
            ItemData itemData = null;
            m_ItemDataDict.TryGetValue(itemId, out itemData);
            return itemData;
        }

        public List<ItemData> GetItemDatas(IList<uint> IDs)
        {
            List<ItemData> itemDatas = new List<ItemData>();
            foreach (uint id in IDs)
            {
                itemDatas.Add(GetItemData(id));
            }
            return itemDatas;
        }
    }
}