
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old
{
    public class HeavyAttackCommand : ICommand
    {
        private bool charging;
        private bool? full;
        //Tip：C#中并不支持像C++那样的构造函数初始化列表，最多可以调用类中的其他构造函数如this(参数列表)，或基类base(参数列表)。
        public HeavyAttackCommand(bool _charging, bool? _full)
        {
            charging = _charging;
            full = _full;
        }

        public bool Execute(ICommandConsumer consumer)
        {
            if (consumer is IHeavyAttack_Consumer cons)
            {
                return cons.HeavyAttack(charging, full);
            }
            return false;
        }
    }
}