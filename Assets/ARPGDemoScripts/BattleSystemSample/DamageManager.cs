using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///负责处理游戏中所有的DamageInfo
    ///</summary>
    public class DamageManager : MonoBehaviour
    {
        private List<DamageInfo> damageInfos = new List<DamageInfo>();

        /*TODO：监测处理伤害，但是调用顺序和调用频率是否会产生不可控的影响呢？*/
        private void FixedUpdate()
        {
            int i = 0; //作为索引的i不变，是Count一直在减小，直到减小到0就是将该次的伤害处理完了。
            while (i < damageInfos.Count)
            {
                DealWithDamage(damageInfos[i]);
                damageInfos.RemoveAt(0);
            }
        }

        ///<summary>
        ///处理DamageInfo的流程，也就是整个游戏的伤害流程
        ///<param name="dInfo">要处理的damageInfo</param>
        ///<retrun>处理完之后返回出一个damageInfo，依照这个，给对应角色扣血处理</return>
        ///</summary>
        private void DealWithDamage(DamageInfo dInfo)
        {
            /*Ques：这里是判断受击者是否为空，恐怕已经死了也只是其中一种情况，围绕这个点应该可以展开一些设计，比如攻击时已经确定好目标，但是在参与计算之前该目标因为其他某些逻辑而移除了（比如什么技能之类的）。
            或者就是作为Buff可以作用于的属性，也就是说可以存在Buff*/
            //如果目标已经挂了，就直接return了
            if (!dInfo.defender) return;
            ChaState defenderChaState = dInfo.defender.GetComponent<ChaState>();
            //TODO：因为对于攻击者和受击者的类型就是设置为GameObject，所以存在变化的可能，但是我在想，基本可以肯定的说，应该把类型直接设置为代表个体的（组件）类类型，而不是过于宽泛的GameObject。
            if (!defenderChaState) return; //因为这里的个体都是通过ChaState组件来控制的，没有的话就说明不是个体。
            ChaState attackerChaState = null;
            if (defenderChaState.dead == true)
                return;
            //判断受击者是个体，且没死
            //先走一遍所有攻击者的onHit
            if (dInfo.attacker)
            {
                /*TODO：我一直很疑惑到底是否应该设置成组件，因为设置成组件就可以被外部获取，会破坏结构性，但是这里要获取需要传入的参数就必须这样获取到实例。。。
                这里的attacker是一个GameObject，这是最让我感觉没有结构性的，结构性带有强制性，但一个GameObject又不代表它必须带有这里的ChaState组件，
                而且我感觉attacker就应该是可以成为攻击者的代表类型。*/
                attackerChaState = dInfo.attacker.GetComponent<ChaState>();
                for (int i = 0; i < attackerChaState.buffs.Count; i++)
                {
                    /*Ques：*/
                    if (attackerChaState.buffs[i].model.onHit != null)
                    {//注意这里传入的引用ref dInfo，将DamageInfo传入到Buff回调中，处理之后再返回出来。
                        attackerChaState.buffs[i].model.onHit(attackerChaState.buffs[i], ref dInfo, dInfo.defender);
                    }
                }
            }
            //然后走一遍挨打者的beHurt
            for (int i = 0; i < defenderChaState.buffs.Count; i++)
            {
                if (defenderChaState.buffs[i].model.onBeHurt != null)
                {
                    defenderChaState.buffs[i].model.onBeHurt(defenderChaState.buffs[i], ref dInfo, dInfo.attacker);
                }
            }
            //如果受击者被这次攻击杀死的话。。。（只是一个提前判断，还没有实际计算伤害影响）
            if (defenderChaState.CanBeKilledByDamageInfo(dInfo) == true)
            {
                //如果角色可能被杀死，就会走OnKill和OnBeKilled，这个游戏里面没有免死金牌之类的技能，所以只要判断一次就好
                if (attackerChaState != null)
                {
                    for (int i = 0; i < attackerChaState.buffs.Count; i++)
                    {
                        if (attackerChaState.buffs[i].model.onKill != null)
                        {
                            attackerChaState.buffs[i].model.onKill(attackerChaState.buffs[i], dInfo, dInfo.defender);
                        }
                    }
                }
                for (int i = 0; i < defenderChaState.buffs.Count; i++)
                {
                    if (defenderChaState.buffs[i].model.onBeKilled != null)
                    {
                        defenderChaState.buffs[i].model.onBeKilled(defenderChaState.buffs[i], dInfo, dInfo.attacker);
                    }
                }
            }
            //最后根据结果处理：如果是治疗或者角色非无敌，才会对血量进行调整。
            bool isHeal = dInfo.isHeal();
            int dVal = dInfo.DamageValue(isHeal); //以上是经过Buff处理，此处是经过最后的处于Buff之外的影响伤害的处理。
            if (isHeal == true || defenderChaState.immuneTime <= 0)
            {
                if (dInfo.requireDoHurt() == true && defenderChaState.CanBeKilledByDamageInfo(dInfo) == false)
                {
                    UnitAnim ua = defenderChaState.GetComponent<UnitAnim>();
                    if (ua) ua.Play("Hurt");
                }
                //这里就是伤害值实际造成作用的代码，非常朴素的加减法。
                defenderChaState.ModResource(new ChaResource(
                    -dVal
                ));
                //按游戏设计的规则跳数字，如果要有暴击，也可以丢在策划脚本函数（lua可以返回多参数）也可以随便怎么滴
                //Tip：这属于UI表现层的内容，实际上应该通过类似于UIManager的类调用UI的相关逻辑，比如触发某个UI元素的某个回调、并且将自己的相关数据传入。
                SceneVariants.PopUpNumberOnCharacter(dInfo.defender, Mathf.Abs(dVal), isHeal);
            }

            //伤害流程走完，添加buff（预定的机制，随具体设计变化此处的逻辑）
            for (int i = 0; i < dInfo.addBuffs.Count; i++)
            {
                GameObject toCha = dInfo.addBuffs[i].target;
                ChaState toChaState = toCha.Equals(dInfo.attacker) ? attackerChaState : defenderChaState;

                if (toChaState != null && toChaState.dead == false)
                {
                    toChaState.AddBuff(dInfo.addBuffs[i]);
                }
            }

        }

        ///<summary>
        ///添加一个damageInfo（也就是注册，将攻击者和被攻击者以及其他相关信息封装到类DamageInfo中，然后交由DamageManager的FixetUpdate统一处理）
        ///<param name="attacker">攻击者，可以为null</param>
        ///<param name="target">挨打对象</param>
        ///<param name="damage">基础伤害值</param>
        ///<param name="damageDegree">伤害的角度</param>
        ///<param name="criticalRate">暴击率，0-1</param>
        ///<param name="tags">伤害信息类型</param>
        ///</summary>
        public void DoDamage(GameObject attacker, GameObject target, Damage damage, float damageDegree, float criticalRate, DamageInfoTag[] tags)
        {
            this.damageInfos.Add(new DamageInfo(
                attacker, target, damage, damageDegree, criticalRate, tags
            ));
        }
    }

}