using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPlugins.BehaviourTree;

public class MoveToPosition : ActionNode
{
    public float speed = 5;
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 40.0f;
    public float tolerance = 1.0f;

    #region 黑板变量
    //通过运行时节点本身的blackboard字段来访问当前的黑板。首先要获取到黑板变量，然后再读取其值来使用。
    //其实可以在渲染检视面板时读取这里定义的黑板变量，将其类型和变量名作为单独一行显示，这样就更加直观清晰了。

    private Vector3Variable moveToPosition;
    // private Vector3 moveToPosition;
    #endregion

    protected override void OnStart() {
        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = speed;
        context.agent.destination = moveToPosition.Value;
        context.agent.updateRotation = updateRotation;
        context.agent.acceleration = acceleration;
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.agent.pathPending) {
            return State.Running;
        }

        if (context.agent.remainingDistance < tolerance) {
            return State.Success;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
            return State.Failure;
        }

        return State.Running;
    }
    protected override void GetBlackboardVariables()
    {
        moveToPosition = blackboard.GetVariable<Vector3Variable>(nameof(moveToPosition));
        // moveToPosition = moveToPositionVar.Value;
    }
}
