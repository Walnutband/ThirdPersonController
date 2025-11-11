using System;
using System.Collections.Generic;
using ARPGDemo.ControlSystem_Old;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerCommandProducer : CommandProducer
{

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference walk;
    [SerializeField] private InputActionReference sprint;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference dodge;
    [SerializeField] private InputActionReference lightAttack;
    [SerializeField] private InputActionReference heavyAttack;
    [SerializeField] private InputActionReference zoom;
    [SerializeField] private InputActionReference resetCamera;
    [SerializeField] private InputActionReference changePlayer;
    [SerializeField] private InputActionReference useItem;
    [SerializeField] private InputActionReference interact;

    /*TODO：这里是依赖于检视器中将对应的InputAction拖拽到对应字段上，是否应该改成在Awake方法中通过名称查找呢？这就是很典型的开发者应该遵守的规范，
    不应该添加额外的代码在这方面纠错。*/

    private void OnEnable()
    {
        Debug.Log("ProducerOnEnable");
        // move.ToInputAction().performed += CommandFactory.Create_MoveCommand;
        move.ToInputAction().performed += Create_MoveCommand;
        move.ToInputAction().canceled += Create_MoveCommand;
        walk.ToInputAction().performed += Create_WalkCommand;
        walk.ToInputAction().canceled += Create_WalkCommand;
        sprint.ToInputAction().performed += Create_SprintCommand;
        sprint.ToInputAction().canceled += Create_SprintCommand;
        // jump.ToInputAction().started += Create_JumpCommand;
        dodge.ToInputAction().started += Create_DodgeCommand;
        lightAttack.ToInputAction().started += Create_LightAttackCommand;
        /*TODO：重攻击以后再说了*/
        // heavyAttack.ToInputAction().started += Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().performed += Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().canceled += Create_HeavyAttackCommand;
        zoom.ToInputAction().performed += Create_Zoom;
        resetCamera.ToInputAction().started += Create_ResetCamera;
        changePlayer.ToInputAction().started += Create_ChangePlayer;
        useItem.ToInputAction().started += Create_UseItem;
        interact.ToInputAction().started += Create_Interact;
    }

    private void OnDisable()
    {
        move.ToInputAction().performed -= Create_MoveCommand;
        move.ToInputAction().canceled -= Create_MoveCommand;
        walk.ToInputAction().performed -= Create_WalkCommand;
        walk.ToInputAction().canceled -= Create_WalkCommand;
        sprint.ToInputAction().performed -= Create_SprintCommand;
        sprint.ToInputAction().canceled -= Create_SprintCommand;
        // jump.ToInputAction().started -= Create_JumpCommand;
        dodge.ToInputAction().started -= Create_DodgeCommand;
        lightAttack.ToInputAction().started -= Create_LightAttackCommand;
        // heavyAttack.ToInputAction().started += Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().performed -= Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().canceled -= Create_HeavyAttackCommand;
        zoom.ToInputAction().performed -= Create_Zoom;
        resetCamera.ToInputAction().started -= Create_ResetCamera;
        changePlayer.ToInputAction().started -= Create_ChangePlayer;
        useItem.ToInputAction().started -= Create_UseItem;
        interact.ToInputAction().started -= Create_Interact;
    }



    public override void OnStart()
    {//注意细节，在OnStart开始生产和OnEnd结束生产时才控制InputActionAsset的启用和禁用，而不是在周期方法OnEnable和OnDisable中。
        inputActions.Enable();
    }

    /*Tip：因为InputSystem是触发式的而且在Update之前执行，所以在此只是返回。*/
    public override List<ICommand> Produce()
    {
        return commands;
    }

    public override void OnEnd()
    {
        inputActions.Disable();
    }

    #region 生成命令

    /*这里的Move、Walk和Sprint其实有点别扭，因为WASD是复合键，而修饰键只能和一个非复合键一起使用，所以无法使用Shift+WASD或Ctrl+WASD这种组合。*/
    //说Move默认就是Run奔跑，而Walk是慢走，Sprint是冲刺，这都是极其常见极其基本的游戏机制，没什么好改动的。
    private void Create_MoveCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateMove");
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        MoveCommand command = new MoveCommand(moveInput, MoveCommand.MoveType.Run);
        commands.Add(command);
    }

    private void Create_WalkCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateWalk");
        MoveCommand command;
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                //在命令接收者实现的接口方法中会根据与第二个参数MoveType决定如何处理，所以此处第一个参数传入default或者其他都无所谓，只是为了填充参数而已，不过default更具有逻辑性
                command = new MoveCommand(default, MoveCommand.MoveType.Walk);
                commands.Add(command);
                break;
            case InputActionPhase.Canceled:
                command = new MoveCommand(default, MoveCommand.MoveType.WalkCancel);
                commands.Add(command);
                break;
        }
    }

    private void Create_SprintCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateSprint");
        MoveCommand command;
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                //在命令接收者实现的接口方法中会根据与第二个参数MoveType决定如何处理，所以此处第一个参数传入default或者其他都无所谓，只是为了填充参数而已，不过default更具有逻辑性
                command = new MoveCommand(default, MoveCommand.MoveType.Sprint);
                commands.Add(command);
                break;
            case InputActionPhase.Canceled:
                command = new MoveCommand(default, MoveCommand.MoveType.SprintCancel);
                commands.Add(command);
                break;
        }
    }

    private void Create_JumpCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateJump");
        JumpCommand command = new JumpCommand();
        commands.Add(command);
    }

    private void Create_DodgeCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateDodge");
        DodgeCommand command = new DodgeCommand();
        commands.Add(command);
    }

    private void Create_LightAttackCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateLightAttack");
        LightAttackCommand command = new LightAttackCommand();
        commands.Add(command);
    }

    //蓄力重攻击，稍微多一点逻辑，而这就要求在编辑器中对于相关InputAction的interaction编辑内容要同步
    /*TODO：常见的蓄力重攻击应该是短蓄和长蓄（满蓄），也就是一个Tap如果在Max时间内松开就是短蓄，如果超过了，就进入了SlowTap，并且特意设置Min小于Max，也就是
    超过Max时直接在SlowTap中Performed。*/
    private void Create_HeavyAttackCommand(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateHeavyAttack");
        bool charging = true;
        HeavyAttackCommand command = null;

        switch (ctx.phase)
        {
            case InputActionPhase.Started:
                command = new HeavyAttackCommand(charging, null);
                break;
            case InputActionPhase.Performed:
                charging = false;
                command = new HeavyAttackCommand(charging, false);
                break;
            case InputActionPhase.Canceled:
                charging = false;
                command = new HeavyAttackCommand(charging, true);
                break;
        }

        commands.Add(command);
        // if (ctx.phase == InputActionPhase.Performed)
        // {
        //     bool? full;
        //     charging = false;

        //     if (ctx.interaction is TapInteraction)
        //     {
        //         full = false;
        //         command = new HeavyAttackCommand(charging, full);
        //     }
        //     else if (ctx.interaction is SlowTapInteraction)
        //     {
        //         full = true;
        //         command = new HeavyAttackCommand(charging, full);
        //     }
        // }
        // else
        // {
        //     command = new HeavyAttackCommand(charging, null);
        // }

    }

    private void Create_Zoom(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateZoom");
        Vector2 zoomInput = ctx.ReadValue<Vector2>();
        Player_Commands command = new Player_Commands(zoomInput);
        commands.Add(command);
    }

    private void Create_ResetCamera(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateReset");
        Player_Commands command = new Player_Commands(Player_Commands.Commands.ResetCamera);
        commands.Add(command);
    }

    private void Create_ChangePlayer(InputAction.CallbackContext ctx)
    {
        // Debug.Log("CreateChangePlayer");
        Player_Commands command = new Player_Commands(this);
        commands.Add(command);
    }

    private void Create_UseItem(InputAction.CallbackContext ctx)
    {
        Player_Commands command = new Player_Commands(Player_Commands.Commands.UseItem);
        commands.Add(command);
    }

    private void Create_Interact(InputAction.CallbackContext context)
    {
        Player_Commands command = new Player_Commands(Player_Commands.Commands.Interact);
        commands.Add(command);
    }
    #endregion
}