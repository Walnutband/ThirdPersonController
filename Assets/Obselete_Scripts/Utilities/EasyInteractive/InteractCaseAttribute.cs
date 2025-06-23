using System;

namespace HalfDog.EasyInteractive
{
    /// <summary>
    /// 交互情景标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)] //表示该特性只能应用于类（Class）
    public class InteractCaseAttribute : Attribute
    {
        public Type interactSubject; //交互主体
        public Type interactTarget; //交互目标
        public bool enableExecuteOnLoad;

        /// <summary>
        /// 交互情景标识
        /// </summary>
        /// <param name="subject">交互主体类型</param>
        /// <param name="target">交互目标类型</param>
        /// <param name="enableExecuteOnLoad">默认开启执行</param>
        public InteractCaseAttribute(Type subject, Type target, bool enableExecuteOnLoad = true)
        {
            interactSubject = subject;
            interactTarget = target;
            this.enableExecuteOnLoad = enableExecuteOnLoad;
        }
    }
}

/*自定义特性
继承Attribute类： 所有自定义特性都必须继承自 System.Attribute。
构造函数： 可通过构造函数向特性传递参数。

AttributeUsage：比如[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
AttributeTargets： 指定特性可以应用的目标（类、方法、属性等）。
Inherited： 指定特性是否可以被子类继承。
AllowMultiple： 指定是否可以多次应用于同一目标。
*/
