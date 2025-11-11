
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [CreateAssetMenu(fileName = "SO_DgNodeAction_Logger", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeAction_Logger", order = -239)]
    public class SO_DgNodeAction_Logger : SO_DgNodeBase
    {
        public SO_DgNodeBase nextNode;
        public string content;

        public override DgNodeBase GetNode()
        {
            return new DgNodeAction_Logger(nextNode.GetNode(), content);
        }
    }
}