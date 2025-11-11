
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    [Serializable]
    public class DgNodeBase
    {

        [SerializeField] protected DgNodeType m_Type;
        public DgNodeType type => m_Type;

        /*TODO：似乎不需要在基类中定义，只需要提供GetNext方法返回下一个节点即可。因为考虑到多数类型的节点其实都只有一个出口节点，似乎就是Choice节点会连接多个出口节点。*/
        // [SerializeField] protected List<DgNodeBase> m_NextNodes = new List<DgNodeBase>();

        public virtual DgNodeBase GetNext(int _index = -1) => null;

        public DgNodeBase(DgNodeType _type)
        {
            m_Type = _type;
        }
    }
}