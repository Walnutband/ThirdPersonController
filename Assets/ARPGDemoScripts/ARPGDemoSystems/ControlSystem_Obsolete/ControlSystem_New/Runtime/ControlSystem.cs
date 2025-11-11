
using System.Linq;
using UnityEngine;

namespace ARPGDemo.ControlSystem_New
{
    [AddComponentMenu("ARPGDemo/ControlSystem_New/ControlSystem")]
    public class ControlSystem : SingletonMono<ControlSystem>
    {
        private class Pair
        {
            public ICommandProducer producer;
            public ICommandConsumer consumer;
        }

        public PlayerCommandProducer producer;

        protected override void Awake()
        {
            //从所有加载的场景中查找带有标签“System”的游戏对象，取出其中第一个名为“ControlSystem”的（注意有可能为空），获取其组件ControlSystem。
            //TODO：这里要求严格的游戏对象Tag和name，到底合不合适还值得思考，不过具体来说，对于这种单例类，本来就是少数，所以用肯定是可以用的，稍微注意一下就行。
            m_Instance = GameObject.FindGameObjectsWithTag("System").FirstOrDefault(go => go.name == "ControlSystem")?.GetComponent<ControlSystem>();
            base.Awake();
        }

        private void OnEnable()
        {
            producer.OnStart();
        }

        private void OnDisable()
        {
            producer.OnEnd();
        }

    }
}