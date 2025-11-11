using System;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [Serializable]
    public struct Damage
    {//Ques：一直很纠结到底要设置为float还是int类型呢？
        /*Tip：发现想要对外只读不写的时候这样写确实很方便。但是C#属性不能被序列化（至少Unity默认的序列化机制确实做不到。）*/
        // public float physics { get; set; } //物理伤害
        public float physics; //物理伤害
        // public float magic { get; private set; } //魔法伤害

        public float poison;//中毒异常值
        public float bleed; //出血异常值
        // public int rot { get; private set; } //腐败异常值

        public Damage(float _physics, float _poison, float _bleed)//构造函数
        {
            physics = _physics;
            poison = _poison;
            bleed = _bleed;
        }
        
    }

    public struct Defense
    {
        public float physicsResist { get; set; } //物理抗性
        public float poisonResist { get; set; } //中毒抗性
        public float bleedResist { get; set; } //出血抗性
    }

    /*Tip：DamageInfo包含这次攻击的全部信息，包括攻击和防御的相关信息。其实Damage本来就是一个泛指，包括伤害和治疗和防御等等。*/
    public class DamageInfo
    {
        public IAttacker attacker;
        public IDefender defender;
        public Damage damage;
        public Defense defense;

        /*Ques：这里在构造时将攻击者和受击者的攻击和防御数值提取了出来，之后会经过Buff作用得到最终的数值。而在同一帧有可能damage或defense的值会发生变化，
        会不会产生什么bug呢？*/
        public DamageInfo(IAttacker _attacker, IDefender _defender)
        {
            this.attacker = _attacker;
            this.defender = _defender;
            this.damage = _attacker.damage;
            this.defense = _defender.defense;
        }
        public DamageInfo(IAttacker _attacker, IDefender _defender, Damage _damage, Defense _defense)
        {
            this.attacker = _attacker;
            this.defender = _defender;
            this.damage = _damage;
            this.defense = _defense;
        }

        /*Tip：该方法决定了最终的伤害的计算过程，通常来说就是计算一个负的hp值，但是统一为ActorResource来计算Actor的资源变化。但是加入了异常值之后，其实就应该这样
        显然该方法的逻辑可以是多样的，取决于游戏设计中是如何决定的，而且通常整个游戏中都是使用的这套规则。
        */
        public ActorResource FinalDamage()
        {
            Damage result = damage;
            //计算物理伤害，防御力就是以百分比减伤起作用的。
            // result.physics *= 1 - defense.physicsResist / 100f;
            result.physics = Mathf.Max(0, result.physics * (1 - defense.physicsResist / 100f));
            //中毒异常，直接绝对数值。
            result.poison = Mathf.Max(0, result.poison - defense.poisonResist);
            //流血异常
            result.bleed = Mathf.Max(0, result.bleed - defense.bleedResist);
            return ActorResource.DamageResource(result);
            
        }

    }
}