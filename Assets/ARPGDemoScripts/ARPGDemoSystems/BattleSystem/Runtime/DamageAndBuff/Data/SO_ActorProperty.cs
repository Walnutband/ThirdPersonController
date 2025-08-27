using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [CreateAssetMenu(fileName = "ActorProperty", menuName = "ARPGDemo/BattleSystem/ActorPropertyData", order = 0)]
    public class SO_ActorProperty : ScriptableObject
    {
        [Tooltip("个体血量")]
        public int hp;
        [Tooltip("个体蓝量")]
        public int mp;
        [Tooltip("个体体力")]
        public int sp;
    }
}