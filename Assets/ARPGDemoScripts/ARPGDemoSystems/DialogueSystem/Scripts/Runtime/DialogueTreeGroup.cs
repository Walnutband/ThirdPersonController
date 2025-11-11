using System.Collections.Generic;

namespace ARPGDemo.DialogueSystem
{
    public class DialogueTreeGroup
    {
        private List<DialogueTree> m_DialogueTrees = new List<DialogueTree>();
        /*TODO: 关于ActiveTree，还需要结合对话系统、甚至是任务系统来处理。*/
        private DialogueTree m_ActiveTree;
        public DialogueTree activeTree
        {
            get
            {
                if (m_ActiveTree == null)
                {
                    m_ActiveTree = m_DialogueTrees[0];
                }
                return m_ActiveTree;
            }
        }

        public DialogueTreeGroup(List<DialogueTree> _trees)
        {
            m_DialogueTrees = _trees;
        }

        public DgNodeBase GetStartNode()
        {
            // return m_ActiveTree.startNode;
            return activeTree.startNode;
        }
    }
}