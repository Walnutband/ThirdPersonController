using System.Collections.Generic;
using CrashKonijn.Goap.Core;
using UnityEngine;

namespace CrashKonijn.Goap.Runtime
{
    /*Tip：注意区别AgentTypeFactory和AgentTypeFactoryBase之间的区别，这里是AgentType的工厂，而后者应该是AgentTypeConfig的工厂*/
    public class AgentTypeFactory
    {
        private readonly IGoapConfig goapConfig;
        private readonly ClassResolver classResolver = new();
        private IAgentTypeConfigValidatorRunner agentTypeConfigValidatorRunner = new AgentTypeConfigValidatorRunner();

        public AgentTypeFactory(IGoapConfig goapConfig)
        {
            this.goapConfig = goapConfig;
        }

        /*Tip：该工厂的必要性就在于，AgentType的构造函数要求多个参数，而这些参数只有在这里工厂中才能集齐，然后调用AgentType的构造函数，而工厂之外的地方就没法调用AgentType的构造函数。*/
        public AgentType Create(IAgentTypeConfig config, bool validate = true) //根据配置生成对象。
        {
            if (validate)
                this.Validate(config);

            var worldData = new GlobalWorldData();

            var sensorRunner = this.CreateSensorRunner(config, worldData);

            return new AgentType(
                id: config.Name, //在IAgentTypeConfig的构造函数中传入其名称。
                config: this.goapConfig, //其实就是IGoapConfig的那几个工具对象。
                actions: this.GetActions(config), //该类型的所有行动
                goals: this.GetGoals(config), //该类型的所有目标
                sensorRunner: sensorRunner, //传感器运行器。
                worldData: worldData //对于世界全局的认知。
            );
        }

        private void Validate(IAgentTypeConfig config)
        {
            var results = this.agentTypeConfigValidatorRunner.Validate(config);

            foreach (var error in results.GetErrors())
            {
                Debug.LogError(error);
            }

            foreach (var warning in results.GetWarnings())
            {
                Debug.LogWarning(warning);
            }

            if (results.HasErrors())
                throw new GoapException($"AgentTypeConfig has errors: {config.Name}");
        }

        /*Tip：在编辑器中直接编辑AgentType的配置文件，创建并引用所需要的传感器。*/
        private SensorRunner CreateSensorRunner(IAgentTypeConfig config, GlobalWorldData globalWorldData)
        {
            //传入所有传感器，以及GlobalWorldData
            return new SensorRunner(this.GetWorldSensors(config), this.GetTargetSensors(config), this.GetMultiSensors(config), globalWorldData);
        }

        private List<IGoapAction> GetActions(IAgentTypeConfig config)
        {
            //根据配置数据，生成实际对象。
            var actions = this.classResolver.Load<IGoapAction, IActionConfig>(config.Actions);
            var injector = this.goapConfig.GoapInjector;

            actions.ForEach(x =>
            {
                if (x.Config is IClassCallbackConfig classCallbackConfig)
                    classCallbackConfig.Callback?.Invoke(x);

                injector.Inject(x);
                x.Created(); //刚创建。
            });

            return actions;
        }

        private List<IGoal> GetGoals(IAgentTypeConfig config)
        {
            var goals = this.classResolver.Load<IGoal, IGoalConfig>(config.Goals.DistinctBy(x => x.ClassType));
            var injector = this.goapConfig.GoapInjector;
            var index = 0;

            goals.ForEach(x =>
            {
                if (x.Config is IClassCallbackConfig classCallbackConfig)
                    classCallbackConfig.Callback?.Invoke(x);
                
                x.Index = index;
                index++;

                injector.Inject(x);
            });

            return goals;
        }

        private List<IWorldSensor> GetWorldSensors(IAgentTypeConfig config)
        {
            //根据配置文件中的字符串获取其Type元数据，然后根据Type来创建实例并转换为指定的类型。
            var worldSensors = this.classResolver.Load<IWorldSensor, IWorldSensorConfig>(config.WorldSensors);
            var injector = this.goapConfig.GoapInjector;

            worldSensors.ForEach(x =>
            {
                if (x.Config is IClassCallbackConfig classCallbackConfig)
                    classCallbackConfig.Callback?.Invoke(x);
                
                injector.Inject(x);
                x.Created();
            });

            return worldSensors;
        }

        private List<ITargetSensor> GetTargetSensors(IAgentTypeConfig config)
        {
            var targetSensors = this.classResolver.Load<ITargetSensor, ITargetSensorConfig>(config.TargetSensors);
            var injector = this.goapConfig.GoapInjector;

            targetSensors.ForEach(x =>
            {
                if (x.Config is IClassCallbackConfig classCallbackConfig)
                    classCallbackConfig.Callback?.Invoke(x);
                
                injector.Inject(x);
                x.Created();
            });

            return targetSensors;
        }

        private List<IMultiSensor> GetMultiSensors(IAgentTypeConfig config)
        {
            var multiSensor = this.classResolver.Load<IMultiSensor, IMultiSensorConfig>(config.MultiSensors);
            var injector = this.goapConfig.GoapInjector;

            multiSensor.ForEach(x =>
            {
                if (x.Config is IClassCallbackConfig classCallbackConfig)
                    classCallbackConfig.Callback?.Invoke(x);
                
                injector.Inject(x);
                x.Created();
            });

            return multiSensor;
        }
    }
}
