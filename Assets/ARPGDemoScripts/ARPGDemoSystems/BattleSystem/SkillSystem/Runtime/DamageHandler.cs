
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [AddComponentMenu("ARPGDemo/BattleSystem/DamageHandler")]
    public class DamageHandler : SingletonMono<DamageHandler>
    {
        //每一轮处理完就清空了，
        private List<DamageInfo> damages = new List<DamageInfo>();

        private void FixedUpdate()
        {
            foreach (var damage in damages)
            {
                DamageCaculation(damage);
            }
            //全部计算完之后就清空。
            damages.Clear();
        }

        private void DamageCaculation(DamageInfo _damage)
        {
            
        }

        public void DoDamage()
        {

        }
    }
}