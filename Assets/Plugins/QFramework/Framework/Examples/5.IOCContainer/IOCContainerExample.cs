using UnityEngine;

namespace QFramework.Example
{
    public class IOCContainerExample : MonoBehaviour
    {
        public class SomeService
        {
            public void Say()
            {
                Debug.Log("SomeService Say Hi");
            }
        }

        public interface INetworkService
        {
            void Connect();
        }

        public class NetworkService : INetworkService
        {
            public void Connect()
            {
                Debug.Log("NetworkService Connect Succeed");
            }
        }

        private void Start()
        {
            var container = new IOCContainer();

            container.Register(new SomeService());
            //泛型为接口，实例为NetworkService，那么如果有其他实例应该就会把之前的覆盖掉
            container.Register<INetworkService>(new NetworkService());

            container.Get<SomeService>().Say();
            container.Get<INetworkService>().Connect();
        }
    }
}
