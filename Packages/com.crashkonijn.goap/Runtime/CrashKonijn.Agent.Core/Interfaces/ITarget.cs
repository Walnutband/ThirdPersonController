using UnityEngine;

namespace CrashKonijn.Agent.Core
{
    public interface ITarget
    {
        public Vector3 Position { get; }
        //这个有效性非常重要，实时判断Target是否发生了变故，比如作为拾取目标，但是在前往的过程中被其他对象拾取了。
        public bool IsValid();
    }
}