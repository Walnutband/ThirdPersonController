
using System.Collections.Generic;

namespace ARPGDemo.BattleSystem
{
    public class BuffManager : SingletonMono<BuffManager>
    {
        private Dictionary<uint, BuffData> m_BuffDataDict = new Dictionary<uint, BuffData>();


        public BuffData GetBuffData(uint itemId)
        {
            BuffData buffData = null;
            m_BuffDataDict.TryGetValue(itemId, out buffData);
            return buffData;
        }

        //Tip：传入用接口、兼容性更好，返回用具体类、因为获取返回值的类型通常是具体类型而非接口（也不一定，主要是对于这种容器类型来说）。
        public List<BuffData> GetBuffDatas(IList<uint> _IDs)
        {
            List<BuffData> buffDatas = new List<BuffData>();
            foreach (uint id in _IDs)
            {
                buffDatas.Add(GetBuffData(id));
            }
            return buffDatas;
        }

        public List<Buff> GetBuffs(IList<uint> _IDs)
        {
            List<Buff> buffs = new List<Buff>();
            foreach (uint id in _IDs)
            {
                buffs.Add(new Buff(GetBuffData(id)));
            }
            return buffs;
        }
    }
}