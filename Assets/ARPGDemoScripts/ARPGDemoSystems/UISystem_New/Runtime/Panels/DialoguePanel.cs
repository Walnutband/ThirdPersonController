using System;
using System.Collections.Generic;
using ARPGDemo.UISystem_New;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ARPGDemo.UISystem
{
    [AddComponentMenu("ARPGDemo/UISystem_New/Panels/DialoguePanel")]
    public class DialoguePanel : UIPanelBase
    {
        /*TODO：从实际来看，都不需要使用InputAction，直接静态访问输入即可，不过这样的话就需要管理器在对话面板打开的时候保持监测，但这也不会产生多少的消耗。
        而且这种UI操作可能会让同一个操作可以由多种输入来触发，要是单个定义InputActionReference变量的话，就有些冗余了感觉。
        */

        //一般就是空格
        [SerializeField] private InputActionReference m_ContinueAction;
        //一般就是鼠标点击，不过这是对于Button的默认输入，而更方便的是加上按键，比如常用的交互键F，而且键盘操作几乎总是会比鼠标操作更加快捷。
        [SerializeField] private InputActionReference m_ChooseAction; 

        [SerializeField] protected TextMeshProUGUI m_SpeakerName; //说话人名字
        [SerializeField] protected TextMeshProUGUI m_Sentence; //说话内容
        [SerializeField] protected RectTransform m_Arrow; //用于表示继续对话的箭头
        [SerializeField] protected GameObject m_ChoiceContainer;
        //选项的模板对象，只要设置文本内容和注册按钮回调即可。
        [SerializeField] protected GameObject m_ChoiceTemplate;
        // [SerializeField] protected TextMeshProUGUI m_ChoiceText;
        // [SerializeField] protected Button m_ChoiceButton;

        protected List<GameObject> m_Choices = new List<GameObject>();
        private Tweener m_ContentTweener;
        private bool isChoosing;

        public Action moveToNext;

        protected override void Awake()
        {
            base.Awake();
        }
        
        /*Tip：Show与Hide最终都是落实到所在游戏对象的SetActive，但是在此之前，该面板会根据当前情况决定是否要真正执行，以及向调用者返回是否真正执行的信息。
        因为这部分逻辑本来就应该是由对话面板本身来执行，而不是由调用者DialogueManager来执行，管理器只管调度、以及根据执行结果决定后续的调度策略、并不负责受调度对象的具体执行逻辑。
        */
        protected override void Show()
        {
            gameObject.SetActive(true);
            // return true;
        }
        protected override void Hide()
        {
            if (m_ContentTweener != null && m_ContentTweener.IsPlaying())
            {
                m_ContentTweener.Complete();
                // return false;
            }
            else
            {
                gameObject.SetActive(false);
                HideChoices();
                // return true;
            }
        }

        protected override void RegisterCallbacks()
        {
            m_ContinueAction.action.started += Continue;
        }

        protected override void UnregisterCallbacks()
        {
            m_ContinueAction.action.started -= Continue;
        }
        
        private void Continue(InputAction.CallbackContext _ctx)
        {
            if (m_ContentTweener != null && m_ContentTweener.IsPlaying())
            {
                m_ContentTweener.Complete();
            }
            else if (isChoosing == false) 
            {//没有在选择的时候，就是继续下一个节点。
                moveToNext?.Invoke();
            }
        }

        public bool SetSpeakerAndContent(string _speaker, string _content)
        {
            return SetSpeaker(_speaker) && SetContent(_content);
        }

        public bool SetSpeaker(string _name)
        {
            m_SpeakerName.text = _name;
            return true;
        }

        public bool SetContent(string _content)
        {
            // m_Sentence.text = _content;
            if (m_ContentTweener != null && m_ContentTweener.IsPlaying())
            {
                m_ContentTweener.Complete();
                // m_ContentTweener = null;
                return false;
            }
            // m_ContentTweener = m_Sentence.DOText(_content, 0.5f);
            //如果是新的对话，那么就要首先清空上一个，因为使用了Tween动画，并不是一次性赋值给text。
            m_Sentence.text = "";
            m_ContentTweener = m_Sentence.DOText(_content, _content.Length * 0.1f);
            return true;
            // m_ContentTweener.OnComplete(() => { m_ContentTweener = null; });
        }

        public void SetChoices(IList<string> _choices, Action<int> _callback)
        {
            // isChoosing = true;

            int count = _choices.Count;

            for (int i = 0; i < count; i++)
            {
                //实例化的对象激活状态与原对象一致。所以需要手动SetActive(true)
                GameObject choice = Instantiate(m_ChoiceTemplate, m_ChoiceContainer.transform, false);
                choice.SetActive(true);
                choice.GetComponentInChildren<TextMeshProUGUI>().text = _choices[i];
                int index = i;
                //Tip：利用选项的次序信息。
                m_Choices.Add(choice);
                choice.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {//点击选项，移动到下一个节点，销毁选项对象（当然也可以缓存起来，以便下次复用），隐藏选项面板。
                    _callback(index);
                    // Destroy(choice);
                    HideChoices();
                });
                // choice.GetComponentInChildren<Button>().onClick.AddListener(() =>
                // {
                //     /*TODO：出现了一个问题，由于匿名函数会捕获局部变量，所以如果在回调方法中使用变量i的话，会发现到时候打印的值都是循环退出时i的值，而不是所期望的在注册时i的值，
                //     所以改为使用一个循环内的局部变量index，就可以了。
                //     但是我想知道有没有更好的做法呢？*/
                //     // Debug.Log($"点击分支选项：{i}");
                //     Debug.Log($"点击分支选项：{index}");
                // });
            }

            ShowChoices();
        }

        private void ShowChoices()
        {
            isChoosing = true;

            CanvasGroup cg = m_ChoiceContainer.GetComponent<CanvasGroup>();
            cg.DOFade(1, 0.2f).onComplete += () => { cg.interactable = true; cg.blocksRaycasts = true; };
        }
        
        private void HideChoices()
        {
            isChoosing = false;

            CanvasGroup cg = m_ChoiceContainer.GetComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;
            m_Choices.ForEach(x => Destroy(x));
            m_Choices.Clear();
            // cg.DOFade(0, 0.2f);
        }

    }
}