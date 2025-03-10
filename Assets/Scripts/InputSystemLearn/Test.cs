using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    private InputAction action;

    void Start()
    {
        // Set up an action that triggers when the A button is
        // held for 1 second.
        //设置了一个操作，当玩家按住手柄的下按钮（通常是A键）达到 1 秒时触发。
        action = new InputAction(
            type: InputActionType.Button,
            binding: "<Gamepad>/buttonSouth",
            interactions: "hold(duration=1)");

        action.Enable();
    }

    void Update()
    {
        if (action.WasPerformedThisFrame())
            Debug.Log("A button on gamepad was held for one second");
    }
}
