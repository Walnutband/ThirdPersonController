using DG.Tweening;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Old
{
    public class UISettingsView : UIView
    {

        #region 控件绑定变量声明，自动生成请勿手改
    #pragma warning disable 0649
        [ControlBinding]
        private Button button_Return;
        [ControlBinding]
        private RectTransform rect_Top;
        [ControlBinding]
        private RectTransform rect_Bottom;

    #pragma warning restore 0649
        #endregion



        private float topHeight, bottomHeight;

        public override void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            base.OnInit(uIControlData, controller);
            topHeight = rect_Top.rect.height;
            bottomHeight = rect_Bottom.rect.height;
            canvasGroup.alpha = 0f;
            rect_Top.anchoredPosition = new Vector2(0, topHeight);
            rect_Bottom.anchoredPosition = new Vector2(0, 0 - bottomHeight);
        }

        protected override void OnAddListener()
        {
            base.OnAddListener();
            button_Return.onClick.AddListener(OnCancel);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();
            button_Return.onClick.RemoveListener(OnCancel);
        }

        public override void OnCloseAnim(TweenCallback complete)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect_Top.DOAnchorPosY(topHeight, 0.3f))
            .Join(rect_Bottom.DOAnchorPosY(-bottomHeight, 0.3f))
            .Append(canvasGroup.DOFade(0f, 0.3f))
            .onComplete += complete;
            // rect_Top.DOAnchorPosY(topHeight, 0.3f);
            // rect_Bottom.DOAnchorPosY(-bottomHeight, 0.3f);
            // canvasGroup.DOFade(0f, 0.3f).onComplete += complete;
        }

        public override void OnOpenAnim()
        {
            // Sequence sequence = DOTween.Sequence();
            // sequence.
            //如果都是同时进行的话，那也不需要使用Sequence
            rect_Top.DOAnchorPosY(0f, 0.3f);
            rect_Bottom.DOAnchorPosY(0f, 0.3f);
            canvasGroup.DOFade(1f, 0.2f);
        }
    }
}