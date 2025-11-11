using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [CreateAssetMenu(fileName = "SO_DgNodeSentence", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeSentence", order = -248)]
    public class SO_DgNodeSentence : SO_DgNodeBase
    {
        public SO_DgNodeBase nextNode;
        public string speaker;
        [TextArea(2, 5)]
        public string content;

        public override DgNodeBase GetNode()
        {
            return new DgNodeSentence(nextNode.GetNode(), speaker, content);
        }
    }
}