using System;
using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public class ClassResolver
    {
        /*TODO：这里的解析过程，就是根据传入的配置对象中记录的字符串，获取其对应的Type元数据信息，然后创建实例。
        当然我也感觉这里很没必要，猜测开发者是想要将这些配置文件统一用这一个解析器来处理，但是我感觉有点设计过度了，导致逻辑很迷惑。
        */
        public List<TType> Load<TType, TConfig>(IEnumerable<TConfig> list)
            where TType : class, IHasConfig<TConfig>
            where TConfig : IClassConfig
        {
            TType action;

            if (list == null)
                return new List<TType>();

            return list.Where(x => !string.IsNullOrEmpty(x?.ClassType) && x.ClassType != "UNDEFINED").Select(x =>
            {
                //根据字符串获取Type元数据，创建实例，然后类型转换。
                action = Activator.CreateInstance(Type.GetType(x.ClassType)) as TType;
                action?.SetConfig(x);
                return action;
            }).ToList();
        }

        public TType Load<TType>(string type)
            where TType : class
        {
            if (string.IsNullOrEmpty(type))
                return null;

            return Activator.CreateInstance(Type.GetType(type)) as TType;
        }

        public HashSet<T> LoadTypes<T>(IEnumerable<string> list)
        {
            var types = list.Select(Type.GetType);
            var classes = types.Select(Activator.CreateInstance);

            return classes.Cast<T>().ToHashSet();
        }
    }
}
