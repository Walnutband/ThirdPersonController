
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    public class Weapon : EquipmentBase
    {
        //武器等级（区别于角色等级），会直接对武器的属性值产生影响，
        [SerializeField] protected int m_Level;
        public int level { get { return m_Level; } }

        protected int m_PhysicsAttack; //物理攻击力
        public int physicsAttack { get { return m_PhysicsAttack; } }

        public override void Equip()
        {
            throw new System.NotImplementedException();
        }

        public override void UnEquip()
        {
            throw new System.NotImplementedException();
        }
    }
}