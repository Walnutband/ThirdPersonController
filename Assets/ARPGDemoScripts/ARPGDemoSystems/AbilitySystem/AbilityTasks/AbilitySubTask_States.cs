
using System;
using ARPGDemo.ControlSystem.Player;
using MyPlugins.AnimationPlayer;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    [Serializable]
    // public class AbilitySubTask_RollState : IAbilitySubTask
    public class AbilitySubTask_State : IAbilitySubTask
    {
        // public PlayerStateBase State;
        public AbilitySubTask_AnimationInfo Animation;
        [SerializeField] private float m_MoveSpeed;

        public void OnBegin()
        {
            // State.OnEnterState();
        }

        public void OnTick(float _curTime)
        {
            // State.OnUpdate();
        }

        public void OnEnd()
        {
            // State.OnExitState();
        }

        public bool IsValid()
        {
            return true;
            // if (State != null) return true;
            // else return false;
        }
    }
}