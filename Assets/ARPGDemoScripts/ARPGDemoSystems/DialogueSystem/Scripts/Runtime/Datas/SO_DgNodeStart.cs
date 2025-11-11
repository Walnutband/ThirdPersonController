using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    
    [CreateAssetMenu(fileName = "SO_DgNodeStart", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeStart", order = -249)]
    public class SO_DgNodeStart : SO_DgNodeBase
    {
        public SO_DgNodeBase nextNode;

        public override DgNodeBase GetNode()
        {
            return new DgNodeStart(nextNode.GetNode());
        }
    }
}