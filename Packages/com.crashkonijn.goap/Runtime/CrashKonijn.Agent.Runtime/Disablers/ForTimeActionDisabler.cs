using CrashKonijn.Agent.Core;
using UnityEngine;

namespace CrashKonijn.Agent.Runtime
{
    public class ForTimeActionDisabler : IActionDisabler
    {
        private readonly float enableAt;

        public ForTimeActionDisabler(float time)
        {
            this.enableAt = Time.time + time;
        }
        //enableAt是绝对时间，所以这里直接比较，而不需要另外使用计时器，因为有统一的Time.time在持续计时。
        public bool IsDisabled(IAgent agent)
        {
            return Time.time < this.enableAt;
        }
    }
}