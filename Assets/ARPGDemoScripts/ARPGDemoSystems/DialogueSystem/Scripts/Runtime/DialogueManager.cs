using ARPGDemo.UISystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.DialogueSystem
{
    [AddComponentMenu("ARPGDemo/DialogueSystem/DialogueManager")]
    public class DialogueManager : SingletonMono<DialogueManager>
    {
        [SerializeField] private InputActionAsset m_InputActions;
        [SerializeField] private DialoguePanel m_Panel;
        private DialogueHandler m_Handler;
        public SO_DialogueTreeGroup group;

        // private DialogueTreeGroup m_CurrentGroup;

        protected override void Awake()
        {
            base.Awake();
            m_Handler = new DialogueHandler(m_Panel);
        }

        private void Start()
        {
            //初始化时，直接绑定
            m_Panel.moveToNext = MoveToNext;
            m_Panel.Close();
        }

        private void Update()
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                StartDialogue(group.GetGroup());
            }
        }

        private void OnEnable()
        {
            /*Ques: 似乎对话系统确实很特殊，所以拥有自己专用的UI和输入。*/
            m_InputActions.FindActionMap("Dialogue").Enable();
        }

        private void OnDisable()
        {
            m_InputActions.FindActionMap("Dialogue").Disable();
        }

        /*TODO：如何实现在战斗状态下无法进入对话状态的功能？*/
        public void StartDialogue(DialogueTreeGroup _group)
        {
            // m_CurrentGroup = _group;
            m_Handler.StartDialogue(_group);
        }

        private void MoveToNext()
        {
            m_Handler.MoveToNext();
        }

        // private void ProcessDialogueNode()
        // {

        // }
    }
    
    /*TODO：只是尝试这样的另外一种结构，如果能将就的话就将就。算了，已经超出经验范围了，还是采用参考结构算了。*/
    public class DialogueHandler
    {
        private DialoguePanel m_Panel;
        private DialogueTreeGroup m_CurrentGroup;
        private DgNodeBase m_CurrentNode;


        public DialogueHandler(DialoguePanel panel)
        {
            m_Panel = panel;
        }

        /*TODO：使用TreeGroup，而不是Tree、也不是Node，考虑到在一次对话流程中可能会在不同Tree之间跳转，当然总范围还是Group。*/
        public void StartDialogue(DialogueTreeGroup _group)
        {
            //说明此时正在进行对话。
            if (m_CurrentGroup != null) return;

            m_Panel.Open();

            m_CurrentGroup = _group;
            m_CurrentNode = m_CurrentGroup.GetStartNode();
            ProcessNode(m_CurrentNode);
        }
        //TODO：其实不应该叫停止，本质应该是结束对话。而我在想的是，还可以引入“打断对话”，我见到的貌似也就魂游可以打断对话，因为它在对话过程中是可以正常控制角色的。
        public void StopDialogue()
        {
            m_CurrentGroup = null;
            m_CurrentNode = null;
            m_Panel.Close();
        }

        private void ProcessNode(DgNodeBase _node)
        {
            DgNodeType type = _node.type;

            switch (type)
            {
                case DgNodeType.Start:
                    ProcessNode(_node as DgNodeStart);
                    Debug.Log("处理Start节点");
                    break;
                case DgNodeType.Sentence:
                    ProcessNode(_node as DgNodeSentence);
                    Debug.Log("处理Sentence节点");
                    break;
                case DgNodeType.End:
                    ProcessNode(_node as DgNodeEnd);
                    Debug.Log("处理End节点");
                    break;
                case DgNodeType.Choice:
                    ProcessNode(_node as DgNodeChoice);
                    Debug.Log("处理Choice节点");
                    break;
                case DgNodeType.Random:
                    ProcessNode(_node as DgNodeRandom);
                    Debug.Log("处理Random节点");
                    break;
                case DgNodeType.Action:
                    ProcessNode(_node as DgNodeAction);
                    Debug.Log("处理Action节点");
                    break;
                
            }
        }

        /**/
        public void MoveToNext(int _index = -1)
        {//移动的同时进行处理。
            // if (_index >= 0)
            // {
            //     m_CurrentNode = m_CurrentNode.GetNext(_index);    
            // }
            // else
            // {
            //     m_CurrentNode = m_CurrentNode.GetNext();
            // }
            Debug.Log($"当前节点类型：{m_CurrentNode.type}, 此时index: {_index}， 将要执行的下一个节点：{m_CurrentNode.GetNext(_index).type}");
            m_CurrentNode = m_CurrentNode.GetNext(_index);
            ProcessNode(m_CurrentNode);
        }

        #region 处理节点逻辑
        //Tip：处理各个具体的节点。
        private void ProcessNode(DgNodeStart _node)
        {
            MoveToNext();
        }

        private void ProcessNode(DgNodeSentence _node)
        {
            DgNodeSentence node = _node as DgNodeSentence;
            // m_Panel.SetContent(node.content);
            m_Panel.SetSpeakerAndContent(node.speaker, node.content);
        }

        private void ProcessNode(DgNodeEnd _node)
        {
            StopDialogue();
        }
        
        private void ProcessNode(DgNodeChoice _node)
        {
            //传入选中后回调，就是移动到Choice节点的下一个指定节点。
            var choices = _node.choices;
            m_Panel.SetChoices(choices, MoveToNext);
        }

        private void ProcessNode(DgNodeRandom _node)
        {
            DgNodeBase node = _node.GetNext();
            ProcessNode(node);
        }
        
        private void ProcessNode(DgNodeAction _node)
        {
            //因为Action节点会自己在GetNext方法中触发Action。
            DgNodeBase node = _node.GetNext();
            ProcessNode(node);
        }
        #endregion
    }
}