using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public class TargetKeyBuilder : KeyBuilderBase<ITargetKey>
    {
        /*Ques：使用ITargetKey作为类型，这样调用时只需要传入各种派生类型的实例即可，无需关注具体类型。但这只是理想情况，实际上对于特定类型来说，比如该类型，就是使用TargetKeyBase类型
        的Key，而且必须要有，并不是那种可有可无的逻辑，所以是否应该在这个注入方法上直接指定Key的具体类型呢？或者如果使用接口类型的话，是否应该在类型判定不对的时候打印调试信息呢？*/
        protected override void InjectData(ITargetKey key)
        {
            if (key is TargetKeyBase targetKey)
                targetKey.Name = key.GetType().GetGenericTypeName();
        }
    }
}