
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [CreateAssetMenu(fileName = "SO_DgNodeRandom", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeRandom", order = -245)]
    public class SO_DgNodeRandom : SO_DgNodeBase
    {
        public List<SO_DgNodeBase> nextNodes = new List<SO_DgNodeBase>();

        public override DgNodeBase GetNode()
        {
            return new DgNodeRandom(nextNodes.ConvertAll(x => x.GetNode()));
        }
    }
} 