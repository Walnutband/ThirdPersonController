
using DG.Tweening;
// using EnjoyGameClub.TextLifeFramework.Processes;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Old
{
    
    public class UIAttributeView : UIView
    {
        #region 控件绑定变量声明，自动生成请勿手改
    #pragma warning disable 0649
        [ControlBinding]
        [SerializeField] private RectTransform rect_Content;
        [ControlBinding]
        [SerializeField] private Button button_Close;
        [ControlBinding]
        [SerializeField] private SimpleButton sbutton_Return;
        [ControlBinding]
        [SerializeField] private AccordionGroup accordionGroup;

    #pragma warning restore 0649
        #endregion



        private Vector2 position;
        private float offset;

        public override void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            base.OnInit(uIControlData, controller);
            position = rect_Content.anchoredPosition;
            offset = rect_Content.rect.width / 6; //取比例。
        }

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            accordionGroup.ResetElementState();
            // transform.localScale = Vector3.one;
        }

        public override void OnOpenAnim()
        {
            // rect_Content.anchoredPosition = position + new Vector2(200f, 0f);
            rect_Content.anchoredPosition = position + new Vector2(offset, 0f); //取比例，而不是绝对数值，可能更合适。
            rect_Content.DOAnchorPos(position, 0.2f).SetEase(Ease.OutSine);
            canvasGroup.DOFade(1f, 0.2f);
        }

        public override void OnCloseAnim(TweenCallback complete)
        {
            rect_Content.DOAnchorPos(position - new Vector2(offset, 0f), 0.2f).SetEase(Ease.OutSine);
            canvasGroup.DOFade(0f, 0.1f).onComplete += complete;
        }

        protected override void OnAddListener()
        {
            base.OnAddListener();
            button_Close.onClick.AddListener(OnCancel);
            sbutton_Return.AddListener(OnCancel);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();
            button_Close.onClick.RemoveListener(OnCancel);
            sbutton_Return.AddListener(OnCancel);
        }

    }
}