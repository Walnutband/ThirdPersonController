using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{

    public class PlayerDodgeState : PlayerState
    {
        public override string StateID => "PlayerDodgeState";
        float defaultDashSpeed = 12f;


        protected override void OnEnterState()
        {
            base.OnEnterState();
            //注意枚举类型是静态的，所以通过类名访问，而不是类的实例
            player.RotatePlayer(0, PlayerSimpleController.RotateType.Immediate); //立刻转向，然后闪避
            player.dodgeing = true; //注意下面方法依赖于dodgeing变量的值。
            player.isDodgePressed = false; //消耗。
            player.SetDodgeAnim();
        }

        protected override void OnUpdateState()
        {
            base.OnUpdateState();
            //下落中断
            if (!player.isGrounded && player.canFall)
            {
                player.dodgeing = false;
                StateMachine.Trigger("DodgeToFall");
                return;
            }
            //跳跃中断
            if (player.isJumpPressed)
            {
                player.dodgeing = false;
                StateMachine.Trigger("DodgeToJump");
                return;
            }
            //未中断,动画结束。在动画的结束帧设置了一个动画事件，将dodgeing设置为false，所以此处不用处理。（有待优化）
            if (!player.dodgeing)
            {
                StateMachine.Trigger("DodgeToIdle");
            }
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            player.SetDodgeAnim(); //此时dodgeing已经设置为false（一定要保证如此），其实可以把该方法拆分，但是有没有必要写个方法也有待后续优化
        }
    }
}
