using UnityEngine;
using MyPlugins.AnimationPlayer;

namespace ARPGDemo.ControlSystem_Old
{
    //这样的一个基类，算是区分了不同个体的状态（实际上从游戏设计层面应该是通用的），而且定义一些会被所有派生状态拥有的成员。
    public abstract class PlayerStateBehaviour : StateBehaviour
    {
        // protected PlayerStateMachine stateMachine;
        [SerializeField] protected AnimatorAgent m_AnimPlayer; //注意
        public AnimatorAgent animPlayer { get => m_AnimPlayer; set => m_AnimPlayer = value; }

        // [SerializeField] protected int m_TempPriority;
        public override int tempPriority { get => -10; } //默认为-10

    }
}