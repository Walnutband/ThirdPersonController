using UnityEngine;

namespace QFramework.Example
{
    public class TypeEventSystemInheritEventExample : MonoBehaviour
    {
        public interface IEventA
        {

        }
        //结构体不能自其他结构体或类。结构体是值类型，而继承是引用类型的特性，因此结构体不支持继承机制。但是可以实现接口，比如此处。
        public struct EventB : IEventA
        {

        }
        //每个类就是一个事件类型
        private void Start()
        {
            //这里的事件是EasyEvent<T>类型，即只要是相同参数类型的方法都可以注册到该类型的事件中，甚至在传入参数是还可以是参数的子类型
            TypeEventSystem.Global.Register<IEventA>(e => //泛型类型就是注册的回调方法所需的参数类型
            {
                Debug.Log(e.GetType().Name); //类型名
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TypeEventSystem.Global.Send<IEventA>(new EventB());

                // 无效。注册类型需要一一对应，只是函数参数可以是继承。
                TypeEventSystem.Global.Send<EventB>();
            }
        }
    }
}
