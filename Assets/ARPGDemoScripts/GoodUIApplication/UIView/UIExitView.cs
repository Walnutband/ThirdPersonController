
using System;
using DG.Tweening;
using MyPlugins.GoodUI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{

    public class UIExitView : UIView
    {
        #region 控件绑定变量声明，自动生成请勿手改
#pragma warning disable 0649
        [ControlBinding]
        private Button button_Cancel;
        [ControlBinding]
        private Button button_Confirm;
        [ControlBinding]
        private SimpleButton sbutton_Return;
        [ControlBinding]
        private RectTransform rect_Content;
        [ControlBinding]
        private TextMeshProUGUI text_Tip;

#pragma warning restore 0649
        #endregion

        private string returnStartScene = "StartScene";

        private Vector2 sizeDelta;

        public override void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            base.OnInit(uIControlData, controller);
            sizeDelta = rect_Content.sizeDelta;
            rect_Content.sizeDelta = Vector2.zero;
            canvasGroup.alpha = 0f;
        }

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            string tip = userData as string;
            text_Tip.text = tip; 
        }

        protected override void OnAddListener()
        {
            base.OnAddListener();

            button_Cancel.onClick.AddListener(OnCancel);
            button_Confirm.onClick.AddListener(OnConfirm);
            sbutton_Return.AddListener(OnCancel);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();

            button_Cancel.onClick.RemoveListener(OnCancel);
            button_Confirm.onClick.RemoveListener(OnConfirm);
            sbutton_Return.RemoveListener(OnCancel);
        }

        public override void OnOpenAnim()
        {
            // rect_Content.DOSizeDelta(Vector2.zero, 0.2f).From();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            rect_Content.DOSizeDelta(sizeDelta, 0.2f);
            canvasGroup.DOFade(1f, 0.1f);
        }

        public override void OnCloseAnim(TweenCallback complete) //这里的complete就是TrueClose，在播放完关闭动画之后再结束。
        {
            Debug.Log("OnCloseAnim");
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            rect_Content.DOSizeDelta(Vector2.zero, 0.2f).onComplete += complete;
            canvasGroup.DOFade(0f, 0.1f);
        }

        // public override void OnCancel()
        // {
        //     base.OnCancel();
        //     UIManager.Instance.Close(UIViewType.UIExitView);
        // }

        private void OnConfirm()
        {
            //TODO:硬编码注意。就是判断此时如果时处于启动场景的话才会退出，否则就是退回到启动场景
            if (SceneManager.GetActiveScene().name == "StartScene")
            {
                Application.Quit();
            }
            else
            {
                //极其短暂的淡出淡入
                UIManager.Instance.FadeOutIn(0.3f, () =>
                {//在黑屏时，打开加载界面，同时关闭启动界面。
                    UIManager.Instance.Open(UIViewType.UILoadingView, returnStartScene);
                    UIManager.Instance.Close(UIViewType.UIExitView);
                });
            }

        }
    }
}