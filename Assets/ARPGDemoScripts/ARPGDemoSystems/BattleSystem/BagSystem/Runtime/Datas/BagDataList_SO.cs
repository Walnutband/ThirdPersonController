using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [CreateAssetMenu(fileName = "BagDataList_SO", menuName = "ARPGDemo/BattleSystem/BagDataList_SO", order = 1)]
    public class BagDataList_SO : ScriptableObject
    {
        public List<BagData> bagDatas = new List<BagData>();
    }
}