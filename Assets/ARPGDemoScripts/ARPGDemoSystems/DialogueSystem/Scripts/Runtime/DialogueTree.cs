using System;
using System.Collections.Generic;

namespace ARPGDemo.DialogueSystem
{
    [Serializable]
    public class DialogueTree
    {
        private List<DgNodeBase> m_Nodes = new List<DgNodeBase>();
        private DgNodeBase m_StartNode;
        public DgNodeBase startNode => m_StartNode;

        public DialogueTree(List<DgNodeBase> _nodes, DgNodeBase _startNode)
        {
            m_Nodes = _nodes;
            m_StartNode = _startNode;
        }
    }
}