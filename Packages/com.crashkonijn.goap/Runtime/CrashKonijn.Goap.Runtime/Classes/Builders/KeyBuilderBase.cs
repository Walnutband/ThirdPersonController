using System;
using System.Collections.Generic;

namespace CrashKonijn.Goap.Runtime
{
    public abstract class KeyBuilderBase<TInterface>
    {
        private Dictionary<Type, TInterface> keys = new();

        //传入Key类型，
        public TInterface GetKey<TKey>()
            where TKey : TInterface
        {
            var type = typeof(TKey);

            if (this.keys.TryGetValue(type, out var key))
            {
                return key;
            }
            //TODO：其实创建实例的话可以要求其类型有new()无参构造函数，大概会比这里的CreateInstance更好。
            //因为有泛型约束指明，所以直接从object转换为TInterface。
            key = (TInterface) Activator.CreateInstance(type);
            key = (TKey)Activator.CreateInstance(type);

            //Tip：就是创建好Key实例之后通过InjectData方法传给自己进行处理，也就是将创建实例的逻辑单独放到了这个共享的GetKey方法中，而使用实例的逻辑就可以在派生类实现的InjectData方法中处理了。
            this.InjectData(key); 
            this.keys.Add(type, key); //看来一个类型就是用一个Key实例。

            return key;
        }

        protected abstract void InjectData(TInterface key);
    }
}
