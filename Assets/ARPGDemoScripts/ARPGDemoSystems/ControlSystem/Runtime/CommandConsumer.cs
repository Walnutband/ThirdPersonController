using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    /*Tip：从设计逻辑上，控制器的本质就是一个命令接收器（命令消费者），让它成为组件是因为可以直接获取到所在游戏对象身上的其他组件，从而进行驱动。显然也可以不作为组件，只要能获取到所控制对象的相关引用即可。
    不过更加具体的代码暂时还写不出来，现在大概认为作为组件就是继承自这里的CommandConsumer，如果不作为组件，就是直接实现接口ICommandConsumer*/
    public abstract class CommandConsumer : MonoBehaviour, ICommandConsumer
    {
        public abstract void OnStart();
        public abstract void OnEnd();
    }
}