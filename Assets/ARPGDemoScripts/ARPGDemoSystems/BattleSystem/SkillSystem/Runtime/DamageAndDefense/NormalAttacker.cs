
using System.Collections.Generic;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [AddComponentMenu("ARPGDemo/BattleSystem/NormalAttacker")]
    [RequireComponent(typeof(Collider))]
    public class NormalAttacker : MonoBehaviour, IAttacker
    {
        [SerializeField] private Damage m_Damage;
        public Damage damage
        {
            get => m_Damage;
            set
            {
                m_Damage = value;
            }
        }

        public void OnHit(IDefender defender, DamageInfo damageInfo)
        {
            
        }

        public List<BuffObj> buffs => null;

        public void AddBuff(BuffInfo buffInfo)
        {
            
        }

        public void RemoveBuff(BuffInfo buffInfo)
        {
            
        }
    }
}