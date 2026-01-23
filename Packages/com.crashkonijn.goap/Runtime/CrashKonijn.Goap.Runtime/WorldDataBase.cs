using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    public abstract class WorldDataBase : IWorldData
    {
        //Tip：这些都是通过属性直接表达字段，也就是隐式的后台字段。
        protected abstract bool IsLocal { get; }
        public Dictionary<Type, IWorldDataState<int>> States { get; } = new();
        public Dictionary<Type, IWorldDataState<ITarget>> Targets { get; } = new();

        public ITarget GetTarget(IGoapAction action)
        {
            if (action == null)
                return null;

            if (action.Config.Target == null)
                return null;

            return this.GetTargetValue(action.Config.Target.GetType());
        }

        public bool IsTrue<TWorldKey>(Comparison comparison, int value)
        {
            return this.IsTrue(typeof(TWorldKey), comparison, value);
        }

        public bool IsTrue(IWorldKey worldKey, Comparison comparison, int value)
        {
            return this.IsTrue(worldKey.GetType(), comparison, value);
        }

        //就是做的一个转接，因为有两个IsTrue版本，所以将各自的预处理放在各自实现的接口方法中，然后统一转接到这里的具体实现版本。
        //Ques：从结构上来看，这里方法并非接口方法，实际上应该设置为private。下面的SetState同理。
        public bool IsTrue(Type worldKey, Comparison comparison, int value)
        {
            var (exists, state) = this.GetWorldValue(worldKey);

            if (!exists)
                return false;

            switch (comparison)
            {
                case Comparison.GreaterThan:
                    return state > value;
                case Comparison.GreaterThanOrEqual:
                    return state >= value;
                case Comparison.SmallerThan:
                    return state < value;
                case Comparison.SmallerThanOrEqual:
                    return state <= value;
            }

            return false;
        }

        public void SetState(IWorldKey key, int state)
        {
            this.SetState(key.GetType(), state);
        }

        public void SetState<TKey>(int state) where TKey : IWorldKey
        {
            this.SetState(typeof(TKey), state);
        }

        /*TODO：state就是世界状态的值，这就是GOAP中的WorldState，而由于Target也是使用WorldDataState类型，而Target并不是WorldState，所以有一些迷惑性，感觉可以改进。
        使用泛型确实可以达到复用的目的，而且由于确实只有Value成员的类型不同，也正好是泛型的应用场景，但毕竟也就只有int和ITarget两种类型，所以使用泛型又感觉多此一举了。
        */
        public void SetState(Type key, int state)
        {
            if (key == null)
                return;

            if (this.States.ContainsKey(key))
            {
                this.States[key].Value = state;
                this.States[key].Timer.Touch();
                return;
            }

            this.States.Add(key, new WorldDataState<int>
            {
                Key = key,
                Value = state,
                IsLocal = this.IsLocal,
            });
        }

        public void SetTarget(ITargetKey key, ITarget target)
        {
            this.SetTarget(key.GetType(), target);
        }

        public void SetTarget<TKey>(ITarget target) where TKey : ITargetKey
        {
            this.SetTarget(typeof(TKey), target);
        }

        private void SetTarget(Type key, ITarget target)
        {
            if (key == null)
                return;

            if (this.Targets.ContainsKey(key))
            {
                this.Targets[key].Value = target;
                this.Targets[key].Timer.Touch();
                return;
            }

            this.Targets.Add(key, new WorldDataState<ITarget>
            {
                Key = key,
                Value = target,
                IsLocal = this.IsLocal,
            });
        }

        public (bool Exists, int Value) GetWorldValue<TKey>(TKey worldKey) where TKey : IWorldKey => this.GetWorldValue(worldKey.GetType());

        public abstract (bool Exists, int Value) GetWorldValue(Type worldKey);
        public abstract ITarget GetTargetValue(Type targetKey);
        public abstract IWorldDataState<ITarget> GetTargetState(Type targetKey);
        public abstract IWorldDataState<int> GetWorldState(Type worldKey);
    }

    //可见接口就是刻意设置的一层抽象。
    public class WorldDataState<T> : IWorldDataState<T>
    {
        public bool IsLocal { get; set; }
        public Type Key { get; set; }
        public T Value { get; set; }
        public ITimer Timer { get; } = new Timer();
    }
}