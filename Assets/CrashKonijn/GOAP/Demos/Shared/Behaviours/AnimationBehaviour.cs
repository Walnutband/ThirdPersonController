using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace Demos.Shared.Behaviours
{
    public class AnimationBehaviour : MonoBehaviour
    {
        private Animator animator;
        private AgentBehaviour agent;
        private AgentMoveBehaviour moveBehaviour;
        private static readonly int Walking = Animator.StringToHash("Walking");

        private bool isWalking;
        private bool isMovingLeft;

        private void Awake()
        {
            this.animator = this.GetComponentInChildren<Animator>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.moveBehaviour = this.GetComponent<AgentMoveBehaviour>();
            
            // Random y offset to prevent clipping
            this.animator.transform.localPosition = new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
        }

        private void Update()
        {
            this.UpdateAnimation();
            this.UpdateScale();
        }

        private void UpdateAnimation()
        {
            var shouldWalk = this.moveBehaviour.IsMoving;
            //状态不变，即逻辑不变。
            if (this.isWalking == shouldWalk)
                return;

            this.isWalking = shouldWalk;
            
            this.animator.SetBool(Walking, shouldWalk);
        }

        //就是改变运动方向，往左或往右。
        private void UpdateScale()
        {
            if (!this.isWalking)
                return;
            
            var shouldMoveLeft = this.IsMovingLeft();

            if (this.isMovingLeft == shouldMoveLeft)
                return;

            this.isMovingLeft = shouldMoveLeft;
            
            this.animator.transform.localScale = new Vector3(shouldMoveLeft ? -1 : 1, 1, 1);
        }

        //直接看Agent与目标的位置横坐标大小关系即可。
        private bool IsMovingLeft()
        {
            if (this.agent.ActionState.Data == null)
                return false;
            
            if (this.agent.ActionState.Data.Target == null)
                return false;
            
            var target = this.agent.ActionState.Data.Target.Position;
            
            return this.transform.position.x > target.x;
        }
    }
}