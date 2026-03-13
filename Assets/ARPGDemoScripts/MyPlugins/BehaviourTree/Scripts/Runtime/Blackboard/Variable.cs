using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    /// <summary>
    /// 使用此类来定义一个blackboardvariable
    /// </summary>
    /// <typeparam name="TData">该Variable实际存储了什么类型的值</typeparam>
    public abstract class Variable<TData> : BlackboardVariable //标记为抽象，是为了避免在反射搜索变量类型的派生类时被包括进去。
    {
        // public string typeName;

        //实际值 T可以是值类型和引用类型
        [SerializeField]
        protected TData val = default(TData); //default就是该类型的默认值，编译器自己处理。
        //注意如果是结构体，比如Vector3，这是值类型，return返回的是副本，无法直接设置Vector3的成员值，显然就不合适了。
        //Tip：这里说错了，其实值类型结构体的读写本来就是副本，而不是引用，所以本来就不能直接修改其成员值，而应该分别计算后，再整体赋值。
        public TData Value
        {
            get { return val; }
            set { val = value; }
        }
    }

    /*节点类需要使用VariableReference来访问。这里是作为所有黑板变量的非泛型基类，往往基类不要设置为泛型，因为要通过基类引用派生类*/
    public abstract class BlackboardVariable : ScriptableObject//作为资产（或者游戏对象），才能持久化存储。
    {
        //variable 和 reference之间通过key来一一对应，这里的key是人为规定的变量名称，
        public string key;
    }
}