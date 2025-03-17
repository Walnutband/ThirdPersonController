using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public override string StateID => "PlayerJumpState";
    [SerializeField] float defaultJumpSpeed = 10f; //默认值，不应该在状态进行中被改变所以通过额外的一个变量jumpSpeed来计算进行时的竖直速度
    float jumpSpeed; //"字段初始值设定项无法引用非静态字段"，所以不能直接用defaultJumpSpeed赋值
    float rotateSpeed = 100;

    protected override void OnEnterState()
    {
        base.OnEnterState();
        player.SetJumpAnim(); //改变动画
        jumpSpeed = defaultJumpSpeed;
        //竖直速度
        player.verticalSpeed = jumpSpeed; //应用速度
        player.isJumpPressed = false;
    }

    protected override void OnUpdateState()
    {
        base.OnUpdateState();
        //头上发生碰撞，直接进入下落状态。
        if (player.controller.collisionFlags == CollisionFlags.Above)
        {
            StateMachine.Trigger("JumpToFall");
            return;
        }

        jumpSpeed = player.CalcUpGravity(jumpSpeed);
        //正常来说就是这一帧计算发现下一帧竖直速度会小于0，即开始下落了，那么就立刻转换到Fall状态，并且的Fall的Enter方法中就会将竖直速度初始化为0，非常合理
        if (jumpSpeed <= 0f)
        {
            StateMachine.Trigger("JumpToFall");
            return;
        }
        //在Jump和Fall状态都是只能稍微转身，从而改变速度方向，但不能改变速度大小
        player.RotatePlayer(rotateSpeed, PlayerController.RotateType.Hold);

        player.verticalSpeed = jumpSpeed;
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        //jumpSpeed = 0f; //浮点值还是加上f，就不用隐式转换了
        //player.verticalSpeed = 0f; //在Fall状态的进入方法中会设置初始下落速度为0

    }
}
