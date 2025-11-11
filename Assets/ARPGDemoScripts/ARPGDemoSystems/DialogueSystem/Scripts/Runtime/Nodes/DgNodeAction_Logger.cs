
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    public class DgNodeAction_Logger : DgNodeAction
    {
        public DgNodeAction_Logger(DgNodeBase _node, string _log) : base(_node)
        {//Ques：打印一段字符串内容即可。似乎不太方便打印变量值之类的？
            m_Action = () => { Debug.Log(_log); };
        }

        // public override DgNodeBase GetNext(int _index = -1)
        // {
        //     return m_NextNode;
        // }
    }
}