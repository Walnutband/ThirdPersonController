using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{

    public class PlayerMoveState : PlayerState
    {
        public override string StateID => "PlayerMoveState";
        bool isWalkLast;
        float rotateSpeed = 1000;
        float fallSpeed = 0; //低下落，不进入Fall状态，但要有竖直向下的速度

        protected override void OnEnterState()
        {
            base.OnEnterState();
            isWalkLast = player.isWalkPressed; //是否按下慢走键（Ctrl）
            player.RotatePlayer(rotateSpeed, PlayerSimpleController.RotateType.Click);
            player.SetMoveAnim(); //使用RootMotion通过动画驱动角色运动，具体代码实现在PlayerController的OnAnimatorMove方法中
        }

        protected override void OnUpdateState()
        {//像以下这样一堆if语句块，看到来就很低级的感觉，也不太直观。至于是否转换之后立刻return，有待思考
            base.OnUpdateState();
            //转入Idle
            if (player.playerInputVec.magnitude < 0.1f) //没有输入或输入死区。
            {
                StateMachine.Trigger("MoveToIdle");
                return;
            }
            //小踩空或转入Fall
            if (!player.isGrounded)
            {
                if (player.canFall) //高于了踩空高度
                {
                    StateMachine.Trigger("MoveToFall");
                    return;
                }
                else
                {
                    fallSpeed = player.CalcDownGravity(fallSpeed);
                    player.verticalSpeed = fallSpeed; //计算重力时才对竖直速度进行赋值
                }

            }
            //转入跳跃
            if (player.isJumpPressed)
            {
                StateMachine.Trigger("MoveToJump");
                return;
            }
            //转入闪避
            if (player.isDodgePressed)
            {
                StateMachine.Trigger("MoveToDodge");
                return;
            }

            player.RotatePlayer(rotateSpeed, PlayerSimpleController.RotateType.Click);
            player.SetMoveAnim();

            ////由于将walk和run放在同一个状态中，所以每帧需要检测，以便及时调整动画，不过只需要在输入发生改变即isWalkPressed改变时才调用方法
            //if (player.isWalkPressed != isWalkLast)
            //{
            //    player.SetMoveAnim();
            //}
        }

        protected override void OnExitState()
        {
            base.OnExitState();

        }
    }
}
