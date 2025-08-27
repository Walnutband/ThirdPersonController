using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public readonly string header;

        // 可以传入一个可视化标题，留空则用字段名
        public FoldoutAttribute(string header = null)
        {
            this.header = header;
        }
    }
}
