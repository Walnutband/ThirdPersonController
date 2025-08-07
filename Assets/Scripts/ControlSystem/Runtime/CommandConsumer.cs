using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public abstract class CommandConsumer : MonoBehaviour, ICommandConsumer
    {
        public abstract void OnStart();
        public abstract void OnEnd();
    }
}