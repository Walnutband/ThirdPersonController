
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    public class MoveCommand : CommandBase
    {
        public Vector2 moveInput { get; private set; } //Vector2
        public MoveType moveType { get; private set; }
        public float moveSpeed;


        public MoveCommand(Vector2 _moveInput)
        {
            moveInput = _moveInput;
            moveType = MoveType.Run;
        }

        public MoveCommand(Vector2 _moveInput, MoveType _moveType)
        {
            moveInput = _moveInput;
            moveType = _moveType;
        }
        
        public MoveCommand(MoveType _moveType)
        {
            moveInput = Vector2.zero;
            moveType = _moveType;
        }

        /*Ques：都在考虑加一个修改命令数据的方法了。*/
        // public void SetMoveDir(Vector2 moveDir)
        // {
        // }

        public enum MoveType
        {
            Run, //正常就是跑，
            Walk,
            WalkCancel, //Tip：尬住了，之前以为不需要这个，但是发现还真得有，因为通常是用一个布尔变量来表示，而布尔变量的true和false就代表了Walk和WalkCancel
            Sprint,
            SprintCancel
        }
    }

}