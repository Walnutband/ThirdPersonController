using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;

namespace CrashKonijn.Goap.Core
{
    public interface IWorldData
    {
        //因为每个类型就是一种状态，而且每个状态只需要唯一实例。State与Key是一一对应的，就是State封装了Key及其数据。
        Dictionary<Type, IWorldDataState<int>> States { get; }
        Dictionary<Type, IWorldDataState<ITarget>> Targets { get; }
        ITarget GetTarget(IGoapAction action);
        void SetState(IWorldKey key, int state);
        void SetState<TKey>(int state) where TKey : IWorldKey;
        //Target就是要到达的目标位置，这就是将移动嵌入到了
        void SetTarget(ITargetKey key, ITarget target);
        void SetTarget<TKey>(ITarget target) where TKey : ITargetKey;
        //这个判断的是该WorldData的指定Key是否满足所传入的条件。
        bool IsTrue<TWorldKey>(Comparison comparison, int value);
        bool IsTrue(IWorldKey worldKey, Comparison comparison, int value);
        (bool Exists, int Value) GetWorldValue<TKey>(TKey worldKey) where TKey : IWorldKey;
        (bool Exists, int Value) GetWorldValue(Type worldKey);
        //获取位置目标。
        ITarget GetTargetValue(Type targetKey);
        //
        IWorldDataState<ITarget> GetTargetState(Type targetKey);
        IWorldDataState<int> GetWorldState(Type worldKey);
    }

    public interface IWorldDataState<T>
    {
        public bool IsLocal { get; } //说明默认是Global即共享的。
        public Type Key { get; } //TODO：这里Key竟然使用元数据，没有任何类型约束，我真觉得不太合适罢，虽然在引用途径上确实保证了类型约束，但总感觉不合适（缺失了本该有的信息）。
        public T Value { get; set; } //这个泛型应该是很合理的，因为数据类型是各不相同的，不过也只是理论上，实际就只有int和ITarget。。。
        public ITimer Timer { get; }
    }
}