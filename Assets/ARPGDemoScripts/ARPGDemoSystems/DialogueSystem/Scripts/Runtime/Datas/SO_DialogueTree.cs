using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{

    [CreateAssetMenu(fileName = "SO_DialogueTree", menuName = "ARPGDemo/DialogueSystem/SO_DialogueTree", order = -290)]
    public class SO_DialogueTree : ScriptableObject 
    {
        public List<SO_DgNodeBase> nodes = new List<SO_DgNodeBase>();
        public SO_DgNodeStart startNode;

        public DialogueTree GetTree()
        {
            return new DialogueTree(nodes.ConvertAll(x => x.GetNode()), startNode.GetNode()); 
        }
    }
}