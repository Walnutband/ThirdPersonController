using System;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    // public enum SentenceSpeaker
    // {
    //     未知,
    //     Player,
    //     NPC
    // }

    [Serializable]
    public class DgNodeSentence : DgNodeBase
    {
        /*TODO：似乎使用一个专门记录speaker名字的登记册（Registry）的资产文件，比起使用枚举类型更加合适，再使用一个专门的Attribute标记string，
        在检视器中直接下拉框选择登记册中的名字。
        其实这种会随着游戏内容量变化的变化的枚举型数据，基本都应该放在一个专门的数据文件中，从而转化为图形化编辑，不需要每次修改都得
        修改代码、重新编译。
        */
        // private SentenceSpeaker m_Speaker;
        [SerializeField] private string m_Speaker;
        public string speaker => m_Speaker;
        [SerializeField] private string m_Content = "";
        public string content => m_Content;
        [SerializeField] private DgNodeBase m_NextNode;

        public DgNodeSentence(DgNodeBase _node, string _speaker, string _content): base(DgNodeType.Sentence)
        {
            m_NextNode = _node;
            m_Speaker = _speaker;
            m_Content = _content;
        }

        public override DgNodeBase GetNext(int _index = -1)
        {
            if (m_NextNode == null)
            {
                Debug.LogError("<b>对话系统：</b>Sentence节点没有连接出口节点");
                return null;
            }
            return m_NextNode;
            // if (m_NextNodes == null || m_NextNodes.Count <= 0)
            // {
            //     //这是不符合正常编辑要求的情况
            //     Debug.LogError("Sentence节点没有连接出口节点");
            //     return null;
            // }

            /*TODO: 按理来说Sentence节点必须且只能连接一个出口节点，因为分支选项是交给Choice节点来实现的，所以也是由Choice节点来连接多个分支节点。不过为了在DgNodeBase中兼容
            连接不同数量出口节点的情况，就直接使用一个List来存储了。*/
            // return m_NextNodes[0];
        }
        
    }
}