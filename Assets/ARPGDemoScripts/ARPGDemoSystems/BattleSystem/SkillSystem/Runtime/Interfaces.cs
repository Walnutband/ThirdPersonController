using System;
using System.Collections.Generic;
using ARPGDemo.BattleSystem;

namespace ARPGDemo.SkillSystemtest
{
    public interface IActor : IAttacker, IDefender
    {
        ActorProperty property { get; }
        ActorResource resource { get; }

        void ModResource(ActorResource value);
    }

    /*Tip：攻击者，本质上属于交互行为的一部分，但是具有高度的特殊性，专用于战斗系统中的伤害计算，由此独立处理。
    由此就是实现该接口就可以产生攻击，而攻击目标就是IDefender，但实现IAttacker不一定要实现IDefender，也就是说可以攻击对方但是不一定能够被攻击，比如环境中的某些固定机关，
    它就很可能只是一个攻击者，但是并非一个攻击目标，如果要让它成为攻击目标比如被击中几次就摧毁的话，显然可以特殊化处理，但是也可以实现IDefender统一处理、只要调整好数值就行了。
    */
    public interface IAttacker : IBuffCarrier
    {
        Damage damage { get; } 
        // bool CanAttack();
        // bool Kill(); //似乎从逻辑上看，只能通过判断受击者是否死亡来判断攻击者是否杀死了对方。所以这个方法设置似乎没啥意义
        /*TODO：除了Buff的回调以外，也可以添加一些自身的接口方法，因为可能有一些不同的物体它在这些回调点的逻辑比较孤立，不会复用到其他物体上，如果都使用Buff的话，这样
        就可能出现很多“定制”Buff，那就违背Buff的原意了。*/
        // void OnHit(IDefender defender);
        /*Tip：我想到在DamageHandler中是依赖于IAttacker和IDefender接口的，而计算ActorResource*/
        void OnHit(IDefender defender, DamageInfo damageInfo);
    }

    public interface IDefender : IBuffCarrier
    {
        Defense defense { get; }
        bool CanBeAttack(); //将各种因素比如死亡、无敌之类的考虑在内，最终返回一个布尔值表明此时是否可以被攻击

        void OnBeHurt(IAttacker attacker, DamageInfo damageInfo);

        bool BeKilled();
        // void OnBeHurt(IAttacker attacker);

    }

    /*TODO：Buff的作用范围远不止于伤害计算。
    这里接口就可以让装备类物品实现，因为装备效果往往是通过Buff实现的，在装备时就调用个体的AddBuff方法将装备携带的Buff传入添加到个体中即可，卸下时就移除。
    该接口的含义在于，可以携带Buff的对象，而且List<BuffObj>的重点不在于要求实现对象必须设置一个List<BuffObj>的字段，而是在其他位置访问该对象的Buff时能够
    直接通过buffs属性获取到Buff列表，然后遍历，其实就是在DamageHandler中能够直接访问到个体的Buff列表，然后遍历触发回调，不用知道它们的具体信息。
    */
    public interface IBuffCarrier
    {
        List<BuffObj> buffs { get; }
        void AddBuff(BuffInfo buffInfo);
        void RemoveBuff(BuffInfo buffInfo);
    }
}