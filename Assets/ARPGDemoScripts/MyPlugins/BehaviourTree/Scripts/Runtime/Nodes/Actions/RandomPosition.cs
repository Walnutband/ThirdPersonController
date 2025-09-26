using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPlugins.BehaviourTree;

public class RandomPosition : ActionNode
{
    public Vector2 min = Vector2.one * -10;
    public Vector2 max = Vector2.one * 10;

#region 黑板变量
    private Vector3Variable moveToPosition;
    // private Vector3 moveToPosition;
#endregion

    protected override void OnStart() {
        GetBlackboardVariables();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        // moveToPosition.x = Random.Range(min.x, max.x);
        // moveToPosition.z = Random.Range(min.y, max.y);
        float x = Random.Range(min.x, max.x);
        float z = Random.Range(min.y, max.y);
        moveToPosition.Value = new Vector3(x, moveToPosition.Value.y, z);
        return State.Success;
    }

    protected override void GetBlackboardVariables()
    {
        moveToPosition = blackboard.GetVariable<Vector3Variable>(nameof(moveToPosition));
        // moveToPosition = moveToPositionVar.Value;
    }
}
