
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*TODO：角色职业数据，其实就是角色的初始数据，也就是各个职业各自具有独特数值的数据，本质上还是角色属性值的子集，不过也是随游戏设计变化的*/
    [CreateAssetMenu(fileName = "ActorCareerData", menuName = "ARPGDemo/BattleSystem/ActorCareerData", order = 0)]
    public class ActorCareerData : ScriptableObject
    {
        [Header("角色等级")]
        [SerializeField] private int m_Level;
        public int level => m_Level;

        [Header("角色能力值")]
        [SerializeField] private ActorAbility m_Ability;
        public ActorAbility ability => m_Ability;

        //TODO：还有职业自带的一些装备、物品等。

    }
}