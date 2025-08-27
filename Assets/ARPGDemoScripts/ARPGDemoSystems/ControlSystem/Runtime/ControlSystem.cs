using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    [AddComponentMenu("ARPGDemo/ControlSystem/ControlSystem")]
    public class ControlSystem : SingletonMono<ControlSystem> //让控制系统在DontDestroy场景中参与生命周期以及维持生命期。
    {
        private struct CommandPair
        {
            public CommandProducer producer;
            public List<CommandConsumer> consumers;
        }
        [SerializeField] private CommandProducer producer;
        [SerializeField] private CommandConsumer consumer;

        // [SerializeField] private List<CommandPair> pairs;
        // [SerializeField] private List<CommandConsumer> consumers;

        private ICommand cachedCommand;

        protected override void Awake()
        {
            //从所有加载的场景中查找带有标签“System”的游戏对象，取出其中第一个名为“ControlSystem”的（注意有可能为空），获取其组件ControlSystem。
            //TODO：这里要求严格的游戏对象Tag和name，到底合不合适还值得思考，不过具体来说，对于这种单例类，本来就是少数，所以用肯定是可以用的，稍微注意一下就行。
            m_Instance = GameObject.FindGameObjectsWithTag("System").FirstOrDefault(go => go.name == "ControlSystem")?.GetComponent<ControlSystem>();
            base.Awake();
        }

        private void Start()
        {
            producer.OnStart();
            consumer.OnStart();
        }

        /*TODO: 这里是肯定要进行优化的，DOTS？可能主要是ECS和Job System，一个是恰当排列数据高效处理，一个是多线程同时处理互不影响的多个任务。、
        还有这里单独实现了FixedUpate方法，是否应该考虑与其他系统一起实现一个OnFixedUpdate方法，由一个专门的管理器统一调用，以便控制调用顺序，因为可能会影响到具体逻辑*/
        private void Update()
        // private void FixedUpdate()
        {
            List<ICommand> commands = producer.Produce();
            SendCommand(consumer, commands);
            // commands = null;

            //手动控制所有受控对象的每帧刷新。
            /*TODO：关于此处，比如对于那些角色个体，肯定会有自己的监测逻辑，但是应该如何调用、如何执行，还需要进一步测试。
            以及此处定义为IController，或许应该更准确一些定义为INeedUpdate？管它是不是什么控制器，总之需要每帧刷新的话就实现INeedUpdate接口，在控制系统这里就会统一调用。*/
            if (consumer is IController controller)
            {
                controller.OnUpdate();
            }

            // foreach (CommandPair pair in pairs)
            // {
            //     List<ICommand> commands = pair.producer.Produce();
            //     if (commands == null) continue;
            //     else
            //     {
            //         List<CommandConsumer> consumers = pair.consumers;
            //         foreach (CommandConsumer consumer in consumers)
            //         {
            //             consumer.Consume(commands);
            //         }
            //     }
            // }
        }

        public void SendCommand(CommandConsumer consumer, ICommand command)
        {

        }

        public void SendCommand(CommandConsumer consumer, List<ICommand> commands)
        {
            if (consumer == null || commands == null) return;

            int count = commands.Count;
            ICommand command;
            for (int i = 0; i < count; i++)
            {
                command = commands[i];
                if (command == null) continue;
                else
                {
                    // //如果成功执行，那就清除掉，也就是这里的不再引用。
                    // if (command.Execute(consumer))
                    // {
                    //     commands[i] = null;
                    // }
                    //TODO:其实再具体点，应该是需要缓存的（通常是作为预输入）才会因为没有成功执行而保留，否则的话都应该清楚，否则会造成命令滞留。
                    command.Execute(consumer);
                    commands[i] = null;
                }
            }
            commands.RemoveAll(n => n == null);
        }

        public void SendCommand(List<CommandConsumer> consumers, ICommand command)
        {

        }

        public void SendCommand(List<CommandConsumer> consumers, List<ICommand> commands)
        {

        }

        public void ChangeConsumer(CommandProducer producer, CommandConsumer targetConsumer)
        {
            consumer.OnEnd();
            consumer = targetConsumer;
            consumer.OnStart();
        }
        
    }

    #region 其他
    
    #endregion
}