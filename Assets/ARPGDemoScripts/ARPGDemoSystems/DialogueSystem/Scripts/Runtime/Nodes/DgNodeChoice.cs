
using System;
using System.Collections.Generic;
using Codice.Client.Common.TreeGrouper;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [Serializable]
    public class DgNodeChoice : DgNodeBase
    {
        /*TODO：按理来说，一个Text就必须对应一个Node，而这里的结构性并没有保证满足这个条件，大概需要从编辑器入手实现。*/
        [SerializeField] private List<DgNodeBase> m_NextNodes;
        [SerializeField] private List<string> m_Texts;
        public List<string> choices => m_Texts;
        // [SerializeField] private Choice[] m_Choices;

        public DgNodeChoice(List<DgNodeBase> _nextNodes, List<string> _texts) : base(DgNodeType.Choice)
        {
            m_NextNodes = _nextNodes;
            m_Texts = _texts;
        }

        public override DgNodeBase GetNext(int _index = -1)
        {
            // if (m_Choices == null || m_Choices.Length <= 0)
            if (m_NextNodes == null || m_NextNodes.Count <= 0)
            {
                //这是不符合正常编辑要求的情况
                Debug.LogError("Choice节点没有连接出口节点");
                return null;
            }
            if (_index < 0)
            {
                Debug.LogError("Choice节点的出口节点索引不能为负数！");
                return null;
            }
            return m_NextNodes[_index];
        }

        // [Serializable]
        // public struct Choice
        // {
        //     [SerializeField] private DgNodeBase m_Node;
        //     public DgNodeBase node => m_Node;
        //     [SerializeField] private string m_Text;
        //     public string text => m_Text;
        // }
    }
}