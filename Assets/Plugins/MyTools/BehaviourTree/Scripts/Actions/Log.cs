using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool {
    /// <summary>
    /// 日志节点，打印指定的日志信息
    /// </summary>
    public class Log : ActionNode 
    {
        public string message;

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }
        
        protected override State OnUpdate() {
            Debug.Log($"{message}");
            return State.Success;
        }
    }
}
