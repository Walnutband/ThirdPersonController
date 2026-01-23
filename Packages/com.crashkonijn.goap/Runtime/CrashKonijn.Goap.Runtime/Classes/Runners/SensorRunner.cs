using System;
using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Goap.Core;

namespace CrashKonijn.Goap.Runtime
{
    //专门用于运行传感器，运行就是调度、就是按规矩调用传感器的对应方法。
    public class SensorRunner : ISensorRunner
    {
        private SensorSet defaultSet = new();
        private Dictionary<IGoapAction, SensorSet> actionSets = new();
        private Dictionary<IGoal, SensorSet> goalSets = new();
        private Dictionary<string, SensorSet> goalsSets = new();
        private Dictionary<Type, ISensor> sensors = new();

        private IGlobalWorldData worldData;

        /*Ques：WorldSensor、TargetSensor、MultiSensor、LocalSensor、GlobalSensor，如何协调使用的呢？*/

        public SensorRunner(
            IEnumerable<IWorldSensor> worldSensors,
            IEnumerable<ITargetSensor> targetSensors,
            IEnumerable<IMultiSensor> multiSensors,
            IGlobalWorldData globalWorldData
        )
        {
            this.worldData = globalWorldData;
            //在defaultSet中记录该运行器具有的所有传感器。
            foreach (var worldSensor in worldSensors)
            {
                this.defaultSet.AddSensor(worldSensor);

                this.sensors.TryAdd(worldSensor.Key.GetType(), worldSensor);
            }

            foreach (var targetSensor in targetSensors)
            {
                this.defaultSet.AddSensor(targetSensor);

                this.sensors.TryAdd(targetSensor.Key.GetType(), targetSensor);
            }

            foreach (var multiSensor in multiSensors)
            {
                this.defaultSet.AddSensor(multiSensor);

                foreach (var (key, value) in multiSensor.LocalSensors)
                {
                    this.sensors.TryAdd(key, value);
                }

                foreach (var (key, value) in multiSensor.GlobalSensors)
                {
                    this.sensors.TryAdd(key, value);
                }
            }
        }

        public void Update()
        {
            foreach (var localSensor in this.defaultSet.LocalSensors)
            {
                localSensor.Update();
            }
        }

        public void Update(IGoapAction action)
        {
            var set = this.GetSet(action);

            foreach (var localSensor in set.LocalSensors)
            {
                localSensor.Update();
            }
        }

        public void SenseGlobal()
        {
            foreach (var globalSensor in this.defaultSet.GlobalSensors)
            {
                globalSensor.Sense(this.worldData);
            }
        }

        public void SenseGlobal(IGoapAction action)
        {
            var set = this.GetSet(action);

            foreach (var globalSensor in set.GlobalSensors)
            {
                globalSensor.Sense(this.worldData);
            }
        }

        public void SenseLocal(IMonoGoapActionProvider actionProvider)
        {
            foreach (var localSensor in this.defaultSet.LocalSensors)
            {
                localSensor.Sense(actionProvider.WorldData, actionProvider.Receiver, actionProvider.Receiver.Injector);
            }
        }

        public void SenseLocal(IMonoGoapActionProvider actionProvider, IGoapAction action)
        {
            if (actionProvider.IsNull())
                return;

            if (action == null)
                return;

            var set = this.GetSet(action);

            foreach (var localSensor in set.LocalSensors)
            {
                localSensor.Sense(actionProvider.WorldData, actionProvider.Receiver, actionProvider.Receiver.Injector);
            }
        }

        public void SenseLocal(IMonoGoapActionProvider actionProvider, IGoal goal)
        {
            if (actionProvider.IsNull())
                return;

            if (goal == null)
                return;

            var set = this.GetSet(goal);

            foreach (var localSensor in set.LocalSensors)
            {
                localSensor.Sense(actionProvider.WorldData, actionProvider.Receiver, actionProvider.Receiver.Injector);
            }
        }

        public void SenseLocal(IMonoGoapActionProvider actionProvider, IGoalRequest goalRequest)
        {
            if (actionProvider.IsNull())
                return;

            if (goalRequest.Goals.Count == 0)
                return;

            var set = this.GetSet(goalRequest);

            foreach (var localSensor in set.LocalSensors)
            {
                localSensor.Sense(actionProvider.WorldData, actionProvider.Receiver, actionProvider.Receiver.Injector);
            }
        }

        public void InitializeGraph(IGraph graph)
        {   
            //为每个根节点即Goal节点创建一个SensorSet
            foreach (var rootNode in graph.RootNodes)
            {
                if (rootNode.Action is not IGoal goal)
                    continue;

                if (this.goalSets.ContainsKey(goal))
                    continue;

                var set = this.CreateSet(rootNode);
                this.goalSets[goal] = set;
            }
        }

        private SensorSet GetSet(IGoapAction action)
        {
            if (this.actionSets.TryGetValue(action, out var existingSet))
                return existingSet;

            return this.CreateSet(action);
        }

        private SensorSet GetSet(IGoal goal)
        {
            return this.goalSets.GetValueOrDefault(goal);
        }

        private SensorSet GetSet(IGoalRequest goalRequest)
        {
            if (string.IsNullOrEmpty(goalRequest.Key))
                goalRequest.Key = GuidCacheKey.GenerateKey(goalRequest.Goals);

            if (this.goalsSets.TryGetValue(goalRequest.Key, out var existingSet))
                return existingSet;

            return this.CreateSet(goalRequest);
        }

        private SensorSet CreateSet(IGoapAction action)
        {
            var set = new SensorSet();

            foreach (var condition in action.Conditions)
            {
                set.Keys.Add(condition.WorldKey.GetType());
            }

            foreach (var key in set.Keys)
            {
                if (this.sensors.TryGetValue(key, out var sensor))
                {
                    set.AddSensor(sensor);
                }
            }

            this.actionSets[action] = set;

            return set;
        }

        private SensorSet CreateSet(IGoalRequest goalRequest)
        {
            var set = new SensorSet();

            foreach (var goal in goalRequest.Goals)
            {
                var goalSet = this.GetSet(goal);
                set.Merge(goalSet);
            }

            this.goalsSets[goalRequest.Key] = set;

            return set;
        }

        //传入根节点即目标节点。
        private SensorSet CreateSet(INode node)
        {
            var actions = new List<IGoapAction>();
            node.GetActions(actions); //获取根节点下的所有节点（未连接的节点不算，不过正常情况下应该不允许出现未连接的节点，因为这完全取决于预先的设计，无关实际的运行情况。）

            var set = new SensorSet();

            //选择Condition作为遍历元素
            foreach (var condition in node.Conditions.Select(x => x.Condition))
            {
                var key = condition.WorldKey.GetType();

                set.Keys.Add(key); //Condition的Type信息

                if (this.sensors.TryGetValue(key, out var sensor))
                {
                    set.AddSensor(sensor);
                }
            }
            //去掉重复元素再遍历。
            foreach (var action in actions.Distinct())
            {
                var actionSet = this.GetSet(action);
                set.Merge(actionSet);

                //Ques：意思就是指定了Target，
                if (action.Config.Target != null)
                {
                    set.Keys.Add(action.Config.Target.GetType());
                    set.AddSensor(this.sensors[action.Config.Target.GetType()]);
                }
            }

            return set;
        }
    }

    public class SensorSet
    {
        public HashSet<Type> Keys { get; } = new();
        public HashSet<ILocalSensor> LocalSensors { get; } = new();
        public HashSet<IGlobalSensor> GlobalSensors { get; } = new();

        public void AddSensor(ISensor sensor)
        {
            switch (sensor)
            {
                case IMultiSensor multiSensor:
                    this.LocalSensors.Add(multiSensor);
                    this.GlobalSensors.Add(multiSensor);
                    break;
                case ILocalSensor localSensor:
                    this.LocalSensors.Add(localSensor);
                    break;
                case IGlobalSensor globalSensor:
                    this.GlobalSensors.Add(globalSensor);
                    break;
            }
        }

        public void Merge(SensorSet set)
        {
            this.Keys.UnionWith(set.Keys);
            this.LocalSensors.UnionWith(set.LocalSensors);
            this.GlobalSensors.UnionWith(set.GlobalSensors);
        }
    }
}
