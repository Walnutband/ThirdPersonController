using System;
using UnityEngine;

namespace ARPGDemo.ControlSystem_Old.Player
{
    [Serializable]
    public class PlayerStateMachine : StateMachine<PlayerStateBehaviour>
    {

        // public override void Initialize()
        // {
        //     base.Initialize();

        // }

    }


    // [Serializable]
    // public class PlayerStateMachine
    // {
    //     public enum PlayerStates
    //     {
    //         MoveState,
    //         AttackState,
    //         SpecialMoveState, //Jump, Roll, StepBack
    //     }
    //     public PlayerStates currentState;

    //     public bool isAttack;
    //     public bool notMove;

    //     public bool CanIdle()
    //     {
    //         // if (currentState == PlayerStates.MoveState) return true;
    //         // else return false;
    //         // return !isAttack && !notMove;
    //         currentState = PlayerStates.MoveState;
    //         return true;
    //     }

    //     public bool CanMove(Vector2 _moveInput)
    //     {
    //         //这样表明只有在Idle时才能转换到Move
    //         if (currentState == PlayerStates.MoveState) return true;
    //         else return false;
    //     }

    //     public bool CanAttack()
    //     {
    //         if (currentState == PlayerStates.MoveState)
    //         {
    //             currentState = PlayerStates.AttackState;
    //             return true;
    //         }
    //         else return false;
    //     }
    // }
}