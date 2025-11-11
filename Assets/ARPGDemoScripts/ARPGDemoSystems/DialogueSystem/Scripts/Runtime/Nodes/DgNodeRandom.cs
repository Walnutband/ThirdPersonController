using System;
using System.Collections.Generic;
using UnityEngine;
//Tip：使用别名，指定Random必然是UnityEngine.Random
using Random = UnityEngine.Random;

namespace ARPGDemo.DialogueSystem
{
    [Serializable]
    public class DgNodeRandom : DgNodeBase
    {
        [SerializeField] private List<DgNodeBase> m_NextNodes;

        public DgNodeRandom(List<DgNodeBase> _nextNodes) : base(DgNodeType.Random)
        {
            m_NextNodes = _nextNodes;
        }

        public override DgNodeBase GetNext(int _index = -1)
        {
            if (m_NextNodes == null || m_NextNodes.Count <= 0)
            {
                //这是不符合正常编辑要求的情况
                Debug.LogError("Random节点没有连接出口节点");
                return null;
            }
            //Tip：正好左闭右开，可以直接使用Count作为右边界。
            _index = Random.Range(0, m_NextNodes.Count);
            return m_NextNodes[_index];
        }
    }
}