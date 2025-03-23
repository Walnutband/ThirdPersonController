using Ilumisoft.VisualStateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{

    public class PlayerIdleState : PlayerState
    {
        public override string StateID => "PlayerIdleState";

        float rotateSpeed = 1000;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            player.SetIdleAnim();
            player.horizontalSpeed = Vector2.zero;
            if (player.transform.rotation == player.RotatePlayer(rotateSpeed, PlayerSimpleController.RotateType.Click))
            {
                if (!player.playerInputVec.Equals(Vector2.zero))
                {
                    StateMachine.Trigger("IdleToMove");
                    return;
                }
            }
        }

        protected override void OnUpdateState()
        {
            base.OnUpdateState();
            player.SetIdleAnim(); //由于使用了SetFloat的过渡版本，所以需要每帧调用，否则参数变化会被中断
            //转入Dodge
            if (player.isDodgePressed)
            {
                StateMachine.Trigger("IdleToDodge");
                return;
            }
            //转入Jump
            if (player.isJumpPressed)
            {
                StateMachine.Trigger("IdleToJump");
                return;
            }

            //转入Move
            //if (player.playerInputVec != Vector2.zero) //有方向输入即有水平移动。错误的，应该是输入方向与面向方向相同时才开始移动
            if (player.transform.rotation == player.RotatePlayer(rotateSpeed, PlayerSimpleController.RotateType.Click))
            {//调用并且比较
                //目标角度与当前角度相同，且是在有输入的情况下，才转换到Move，因为RotatePlayer方法中在没输入且为Click的情况下会返回自己即player的rotation
                if (!player.playerInputVec.Equals(Vector2.zero)) //注意浮点值比较存在不精确性，最好还是确定一个近似范围
                {
                    StateMachine.Trigger("IdleToMove");
                    return;
                }
            }
        }

        protected override void OnExitState()
        {
            base.OnExitState();
        }
    }
}
