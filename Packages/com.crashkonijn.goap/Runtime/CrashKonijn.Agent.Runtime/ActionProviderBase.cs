using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using UnityEngine;

namespace CrashKonijn.Agent.Runtime
{
    public abstract class ActionProviderBase : MonoBehaviour, IActionProvider
    {
        private Dictionary<IAction, IActionDisabler> disablers = new();

        public abstract IActionReceiver Receiver { get; set; }
        public abstract void ResolveAction();

        public bool IsDisabled(IAction action)
        {
            if (!this.disablers.TryGetValue(action, out var disabler))
                return false;
            //Ques：什么意思？IMonoAgent有什么特殊性？
            if (this.Receiver is not IMonoAgent agent)
                return false;
            //检查到没到时间。
            if (disabler.IsDisabled(agent))
                return true;

            this.Enable(action);
            return false;
        }
        //因为互补关系，所以默认启用，而禁用需要指定，所以启用就是直接移除禁用即可，而禁用实际上是禁用指定时长。
        public void Enable(IAction action)
        {
            this.disablers.Remove(action);
        }

        public void Disable(IAction action, IActionDisabler disabler)
        {
            this.disablers[action] = disabler;
        }
    }
}
