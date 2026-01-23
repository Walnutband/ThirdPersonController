using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityHFSM.Samples.GuardAI {

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float speed = 5;

    void Start()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        // Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 input = new Vector2(Keyboard.current.aKey.ReadValue() * -1 + Keyboard.current.dKey.ReadValue(),
                                    Keyboard.current.sKey.ReadValue() * -1 + Keyboard.current.wKey.ReadValue()
        );
        input = Vector2.ClampMagnitude(input, 1);
        input *= speed;

        transform.position += (Vector3) (input * Time.deltaTime);
    }
}

}