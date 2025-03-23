using Ilumisoft.VisualStateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    public class PlayerState : StateBehaviour
    {
        public override string StateID => ""; //这只是Player状态的基类，就在各自派生类的状态中设置相应的状态ID
        protected PlayerSimpleController player;
        protected Animator animator;

        protected float stateTimer;

        protected override void Awake()
        {
            base.Awake();
            player = GetComponentInParent<PlayerSimpleController>();
            animator = player.animator;
        }

        protected override void OnEnterState() //对抽象类和虚拟类都必须用override关键字
        {

        }

        protected override void OnExitState()
        {

        }

        protected override void OnUpdateState()
        {

        }
    }
}
