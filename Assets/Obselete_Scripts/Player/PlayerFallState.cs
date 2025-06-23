using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    public class PlayerFallState : PlayerState
    {
        public override string StateID => "PlayerFallState";

        float rotateSpeed = 100f; //要看水平运行即Move所使用的旋转速度数值如何
        //float defaultFallSpeed = 0; 
        float maxFallSpeed = -6; //最大下落速度，现实由于空气阻力也是会存在下落的最大速度，而且实际游戏中这样也比较合适
        float fallSpeed; //当前下落速度
        //计算进入和退出该状态中间经历的竖直落差，如果是以掉落时检测到的下落高度的话，一方面是检测范围有限，更重要的是下落过程中可能发生各种情况，导致实际掉落到更低或更高的位置
        float fallStartPos; //掉落开始的位置（实际上是y坐标值）
        //float fallEndPos; //掉落结束即着地的位置（同样是y坐标值）。其实用不上，因为可以直接用当时的player位置

        protected override void OnEnterState()
        {
            base.OnEnterState();
            fallSpeed = 0f; //实时下落速度
            player.verticalSpeed = fallSpeed; //进入时先归零
            fallStartPos = player.transform.position.y; //记录起始高度
            player.SetFallAnim();
        }

        protected override void OnUpdateState()
        {
            base.OnUpdateState();
            if (player.isGrounded)
            {
                StateMachine.Trigger("FallToIdle");
                return;
            }

            player.RotatePlayer(rotateSpeed, PlayerSimpleController.RotateType.Hold);
            //计算，应用
            fallSpeed = Mathf.Max(player.CalcDownGravity(fallSpeed), maxFallSpeed); //注意是负数，在此就是限制最大下落速度为maxFallSpeed
            player.verticalSpeed = fallSpeed; //计算完别忘了赋值。
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            player.justLand = true; //标记刚刚落地
            player.fallHeight = fallStartPos - player.transform.position.y;
        }
    }
}
