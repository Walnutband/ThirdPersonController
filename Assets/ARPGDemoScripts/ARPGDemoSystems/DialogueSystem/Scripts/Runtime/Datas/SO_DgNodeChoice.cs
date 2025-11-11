using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [CreateAssetMenu(fileName = "SO_DgNodeChoice", menuName = "ARPGDemo/DialogueSystem/SO_DgNodeChoice", order = -246)]
    public class SO_DgNodeChoice : SO_DgNodeBase
    {
        public List<Choice> choices = new List<Choice>();

        public override DgNodeBase GetNode()
        {
            List<DgNodeBase> nextNodes = new List<DgNodeBase>(choices.Count);
            List<string> texts = new List<string>(choices.Count);
            for (int i = 0; i < choices.Count; i++)
            {
                nextNodes.Add(choices[i].node.GetNode());
                texts.Add(choices[i].text);
            }
            return new DgNodeChoice(nextNodes, texts);
        }

        [Serializable]
        public struct Choice
        {
            [SerializeField] private SO_DgNodeBase m_Node;
            public SO_DgNodeBase node => m_Node;
            [SerializeField] private string m_Text;
            public string text => m_Text;
        }
    }
}