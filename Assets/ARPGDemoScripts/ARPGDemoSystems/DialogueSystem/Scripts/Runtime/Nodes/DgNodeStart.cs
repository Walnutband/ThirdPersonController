using System;

namespace ARPGDemo.DialogueSystem
{
    [Serializable]
    public class DgNodeStart : DgNodeBase
    {
        private DgNodeBase m_NextNode;
        public DgNodeStart(DgNodeBase _node) : base(DgNodeType.Start)
        {
            m_NextNode = _node;
        }

        public override DgNodeBase GetNext(int _index = -1)
        {
            return m_NextNode;
        }
    }
}