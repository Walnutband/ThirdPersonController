using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public abstract class CommandProducer :  MonoBehaviour, ICommandProducer{
        /*TODO：是否应该使用链表而不是列表？*/
        protected List<ICommand> commands = new List<ICommand>();

        //OnStart和OnEnd代表开始生产和结束生产，注意与Mono组件的周期方法区别。
        public virtual void OnStart() { }
        public abstract List<ICommand> Produce();
        public virtual void OnEnd() { }
    }

}