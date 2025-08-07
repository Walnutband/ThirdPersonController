
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{

    public class UIStartView : UIView
    {

        #region 控件绑定变量声明，自动生成请勿手改
    #pragma warning disable 0649
        [ControlBinding]
        private GameObject go_ProgressBar;
        [ControlBinding]
        private TextMeshProUGUI text_Progress;
        [ControlBinding]
        private Slider slider_Progress;
        [ControlBinding]
        private GameObject go_Enter;
        [ControlBinding]
        private Button button_Enter;
        [ControlBinding]
        private Button button_Exit;

#pragma warning restore 0649
        #endregion

        //TODO:
        [SceneName]
        public string enterMainScene = "MainScene";

        public override void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            base.OnInit(uIControlData, controller);
            Reset();
            // go_ProgressBar.SetActive(true);
            // go_Enter.SetActive(false);
            // slider_Progress.value = 0f;
        }

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            // Reset(); //首先初始化各个对象的状态。
            StartCoroutine(Loading()); //开启协程，也就是开始模拟加载进度条。
        }

        protected override void OnAddListener()
        {
            base.OnAddListener();
            slider_Progress.onValueChanged.AddListener(UpdateText);
            button_Enter.onClick.AddListener(EnterGame);
            button_Exit.onClick.AddListener(OpenExitView);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();
            slider_Progress.onValueChanged.RemoveListener(UpdateText);
            button_Enter.onClick.RemoveListener(EnterGame);
            button_Exit.onClick.RemoveListener(OpenExitView);
        }

        public override void OnOpenAnim()
        {
            
        }
        public override void OnCloseAnim(TweenCallback complete)
        {
            complete?.Invoke();
        }

        public override void OnClose()
        {
            base.OnClose();
            Reset();
        }


        private void Reset()
        {
            go_ProgressBar.SetActive(true);
            go_Enter.SetActive(false);
            slider_Progress.value = 0f;
            button_Exit.interactable = false;
        }

        //TODO:现在只是对进度条的一个模拟，这里的逻辑原本应该是异步加载资源（比如绝区零就是加载配置数据），随着加载进度而更新进度条。
        private IEnumerator Loading()
        {
            yield return new WaitForSeconds(0.8f); //这里是等待Splash黑布渐变为透明

            Sequence sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => slider_Progress.value, (value) => slider_Progress.value = value, 0.3f, 0.3f));
            sequence.AppendInterval(0.5f);
            sequence.Append(DOTween.To(() => slider_Progress.value, (value) => slider_Progress.value = value, 0.9f, 1f));
            sequence.AppendInterval(0.4f);
            sequence.Append(DOTween.To(() => slider_Progress.value, (value) => slider_Progress.value = value, 1f, 1f));
            sequence.onComplete += LoadComplete;

        }

        //Tip：将加载新场景的逻辑都交给UILoadingView处理，这里只负责打开UILoadingView。
        private void EnterGame()
        {
            // AsyncOperation asyncOperation = SceneController.Instance.LoadSceneAsync(enterMainScene);
            // asyncOperation.allowSceneActivation = false;
            // UIManager.Instance.FadeOut();

            //极其短暂的淡出淡入
            UIManager.Instance.FadeOutIn(0.3f, () =>
            {//在黑屏时，打开加载界面，同时关闭启动界面。
                UIManager.Instance.Open(UIViewType.UILoadingView, enterMainScene);
                UIManager.Instance.Close(UIViewType.UIStartView);
            }); 

            // SceneController.Instance.TransitionToScene(enterMainScene,
            // loadStart: () =>
            // {//因为观察绝区零，发现很多时候都会有个短暂的渐出渐入的效果。
            //     UIManager.Instance.FadeOutIn(0.3f, () => UIManager.Instance.Open(UIViewType.UILoadingView));
            // },
            // loadComplete: (load) =>
            // {//加载完成场景之后，此时LoadingView还覆盖在其上，淡出，然后关闭UILoadingView
            //     /*Tip：其实从这个流程就可以想通，为何在很多游戏的场景加载中，在加载界面快要结束时，就已经出现了新场景的声音，以及其他一些内容，就是因为此时已经加载完成，
            //     只不过还可能要进行一个过渡，或者是等待卸载旧场景
            //     */
            //     //确保此时已经处于UILoadingView界面，在加载完成后就准备激活，同时淡出，否则如果加载过快的话，可能在loadStart中还没打开UILoadingView时就已经看到新场景了，虽然不会出错，但显然不合适
            //     load.allowSceneActivation = true; 
            //     UIManager.Instance.FadeOut(0.2f, () => UIManager.Instance.Close(UIViewType.UILoadingView));
            // },
            // unloadComplete: () => 
            // {
            //     UIManager.Instance.FadeIn(0.3f);
            // });

        }

        private void LoadComplete()
        {
            go_ProgressBar.SetActive(false);
            go_Enter.SetActive(true);
            button_Exit.interactable = true;
            //TODO:应该加上渐变切换的动画，使用CanvasGroup组件的alpha来控制。
            // Sequence sequence = DOTween.Sequence();
            // sequence.Append()
        }

        private void OpenExitView()
        {
            UIManager.Instance.Open(UIViewType.UIExitView, "是否退出游戏？");
        }

        private void UpdateText(float value)
        {
            //两位小数的百分数。value是0~1的浮点值
            // Debug.Log($"{value}");
            text_Progress.text = $"{Mathf.Round(value * 10000f) / 100}%";
        }
    }
}