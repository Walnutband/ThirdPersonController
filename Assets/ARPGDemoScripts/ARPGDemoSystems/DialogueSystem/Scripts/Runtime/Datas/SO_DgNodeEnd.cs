
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [CreateAssetMenu(fileName = "SO_DgNodeEnd", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeEnd", order = -242)]
    public class SO_DgNodeEnd : SO_DgNodeBase
    {
        public override DgNodeBase GetNode()
        {
            return new DgNodeEnd();
        }
    }
}