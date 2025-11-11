
using System;

namespace ARPGDemo.DialogueSystem
{
    public class DgNodeAction : DgNodeBase
    {
        protected DgNodeBase m_NextNode;
        protected Action m_Action;

        protected DgNodeAction(DgNodeBase _node) : base(DgNodeType.Action)
        {
            m_NextNode = _node;
            // m_Action = _action;
        }

        public override DgNodeBase GetNext(int _index = -1)
        {
            m_Action?.Invoke();
            return m_NextNode;
        }

        // public void DoAction()
        // {
        //     m_Action?.Invoke();
        // }
    }
}