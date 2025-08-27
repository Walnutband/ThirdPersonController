using System.Collections.Generic;
using ARPGDemo.ControlSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerCommandProducer : CommandProducer
{

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference dodge;
    [SerializeField] private InputActionReference lightAttack;
    [SerializeField] private InputActionReference heavyAttack;
    [SerializeField] private InputActionReference zoom;
    [SerializeField] private InputActionReference resetCamera;
    [SerializeField] private InputActionReference changePlayer;

    private void OnEnable()
    {
        Debug.Log("ProducerOnEnable");
        // move.ToInputAction().performed += CommandFactory.Create_MoveCommand;
        move.ToInputAction().performed += Create_MoveCommand;
        move.ToInputAction().canceled += Create_MoveCommand;
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
    }

    private void OnDisable()
    {
        move.ToInputAction().performed -= Create_MoveCommand;
        move.ToInputAction().canceled -= Create_MoveCommand;
        // jump.ToInputAction().started -= Create_JumpCommand;
        dodge.ToInputAction().started -= Create_DodgeCommand;
        lightAttack.ToInputAction().started -= Create_LightAttackCommand;
        // heavyAttack.ToInputAction().started += Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().performed -= Create_HeavyAttackCommand;
        // heavyAttack.ToInputAction().canceled -= Create_HeavyAttackCommand;
        zoom.ToInputAction().performed -= Create_Zoom;
        resetCamera.ToInputAction().started -= Create_ResetCamera;
        changePlayer.ToInputAction().started -= Create_ChangePlayer;
    }

    public override void OnStart()
    {
        inputActions.Enable();
    }

    public override List<ICommand> Produce()
    {
        return commands;
    }

    public override void OnEnd()
    {
        inputActions.Disable();
    }

    #region 生成命令
    private void Create_MoveCommand(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateMove");
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        MoveCommand command = new MoveCommand(moveInput);
        commands.Add(command);
    }

    private void Create_JumpCommand(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateJump");
        JumpCommand command = new JumpCommand();
        commands.Add(command);
    }

    private void Create_DodgeCommand(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateDodge");
        DodgeCommand command = new DodgeCommand();
        commands.Add(command);
    }

    private void Create_LightAttackCommand(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateLightAttack");
        LightAttackCommand command = new LightAttackCommand();
        commands.Add(command);
    }

    //蓄力重攻击，稍微多一点逻辑，而这就要求在编辑器中对于相关InputAction的interaction编辑内容要同步
    /*TODO：常见的蓄力重攻击应该是短蓄和长蓄（满蓄），也就是一个Tap如果在Max时间内松开就是短蓄，如果超过了，就进入了SlowTap，并且特意设置Min小于Max，也就是
    超过Max时直接在SlowTap中Performed。*/
    private void Create_HeavyAttackCommand(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateHeavyAttack");
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
        Debug.Log("CreateZoom");
        Vector2 zoomInput = ctx.ReadValue<Vector2>();
        Player_Commands command = new Player_Commands(zoomInput);
        commands.Add(command);
    }

    private void Create_ResetCamera(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateReset");
        Player_Commands command = new Player_Commands();
        commands.Add(command);
    }

    private void Create_ChangePlayer(InputAction.CallbackContext ctx)
    {
        Debug.Log("CreateChangePlayer");
        Player_Commands command = new Player_Commands(this);
        commands.Add(command);
    }
    #endregion
}