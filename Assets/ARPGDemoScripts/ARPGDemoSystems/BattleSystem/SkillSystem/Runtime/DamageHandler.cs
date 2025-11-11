using System.Collections.Generic;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*Tip：由于这只是一个处理器，职责完全是处理运行时的数据，并不需要编辑时对它单独编辑，所以用Singleton可能比SingletonMono更恰当。
    但是这个处理器又需要参与生命周期，如果没有统一管理的话，就只能自己作为组件直接参与生命周期了。
    */
    [AddComponentMenu("ARPGDemo/BattleSystem/DamageHandler")]
    public class DamageHandler : SingletonMono<DamageHandler>
    // public class DamageHandler : Singleton<DamageHandler>
    {

        protected override void RetrieveExistingInstance()
        {
            m_Instance = GameObject.Find("DamageHandler").GetComponent<DamageHandler>();
        }

        //每一轮处理完就清空了，
        private List<DamageInfo> damages = new List<DamageInfo>();

        private void FixedUpdate()
        {
            if (damages == null) return;

            foreach (var damage in damages)
            {
                DamageCaculation(damage);
            }
            //全部计算完之后就清空。
            damages.Clear();
        }

        private void DamageCaculation(DamageInfo _damage)
        {
            IDefender defender = _damage.defender;
            IAttacker attacker = _damage.attacker;
            if (defender != null &&!defender.CanBeAttack()) return;

            List<BuffObj> attackerBuffs;
            List<BuffObj> defenderBuffs;
            //Tip：应用攻击者和受击者的Buff在Hit时的效果。
            if (attacker != null)
            {
                /*Tip：偶然发现，在if分支内赋值，在退出分支后这个值就还回去了*/
                attackerBuffs = attacker.buffs;
                foreach (var buff in attackerBuffs)
                {
                    buff.OnHit(buff, ref _damage, defender);
                }
                attacker.OnHit(defender, _damage);
            }
            defenderBuffs = defender.buffs;
            foreach (var buff in defenderBuffs)
            {
                // buff.OnHit(buff, ref _damage, defender);
                buff.OnBeHurt(buff, ref _damage, defender);
            }
            defender.OnBeHurt(attacker, _damage);

            /*Ques：从这里，就是与案例项目存在较大差异的开始了。*/
            // ActorResource result = _damage.FinalDamage();

            // if (defender is IActor defenderActor)
            // {
            //     defenderActor.ModResource(result);
            // }

            //受击者被杀，就意味着攻击者进行了击杀，这就是纠缠态罢。
            if (defender != null && defender.BeKilled())
            {
                if (attacker != null)
                {
                    attackerBuffs = attacker.buffs;
                    foreach (var buff in attackerBuffs)
                    {
                        buff.OnKill(buff, _damage, defender);
                    }
                }
                defenderBuffs = defender.buffs;
                foreach (var buff in defenderBuffs)
                {
                    buff.OnBeKilled(buff, _damage, defender);
                }
            }
        }

        public void DoDamage(IAttacker _attacker, IDefender _defender, Damage _damage, Defense _defense)
        {
            damages.Add(new DamageInfo(_attacker, _defender, _damage, _defense));
        }
    }
}