using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPlugins.BehaviourTree;

public class Breakpoint : ActionNode, ISerializationCallbackReceiver
{
    protected override void OnStart() {
        Debug.Log("Trigging Breakpoint");
        Debug.Break();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        return State.Success;
    }

    public void OnAfterDeserialize()
    {
        Debug.Log("BreakPoint  OnAfterDeserialize");
    }

    public void OnBeforeSerialize()
    {

    }
}
