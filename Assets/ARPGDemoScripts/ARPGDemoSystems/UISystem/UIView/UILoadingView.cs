
using System.Collections;
using DG.Tweening;
using MyPlugins.GoodUI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Old
{

    /*Tip：加载界面，通常是在切换场景时才会出现，而一般的打开或切换全屏的UI界面只需要一个固定时间的淡出淡入的效果即可*/
    public class UILoadingView : UIView
    {

        #region 控件绑定变量声明，自动生成请勿手改
#pragma warning disable 0649
        [ControlBinding]
        private TextMeshProUGUI text_Progress;
        [ControlBinding]
        private Slider slider_Progress;
        [ControlBinding]
        private TextMeshProUGUI text_Concept;
        [ControlBinding]
        private TextMeshProUGUI text_Description;
        [ControlBinding]
        private SimpleButton sbutton_Conception;

#pragma warning restore 0649
        #endregion

        //记录动态加载滚动条
        private Tweener _tweener;

        //TODO:问题在于，
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
            string toScene = userData as string;
            SceneController.Instance.TransitionToSceneSingle(toScene,
            loadStart: null,
            loading: (progress) =>
            {
                //TODO：这里判定必须差别大于0.3时才会移动进度条，因为我发现不做这样一个判断的话，会出现NaN的情况，不过具体原因我也说不清楚。
                if (progress - slider_Progress.value <= 0.3f) return;

                // slider_Progress.value = progress;
                if (_tweener != null && _tweener.IsPlaying())
                    _tweener.Complete();
                //这里是以滑动条值与进度值的距离占进度值的比值作为过渡时间的，也就是说，越接近就越慢，越远就越快。
                _tweener = DOTween.To(() => slider_Progress.value, (prog) => slider_Progress.value = prog, progress, Mathf.Clamp((progress - slider_Progress.value) / progress, 0.1f, 1f));
                
            },
            loadComplete: () =>
            {//Tip：（Single模式下）在加载新场景和卸载旧场景都完成之后，淡出，在黑屏下悄悄关闭加载界面，然后淡入。
                UIManager.Instance.FadeOutIn(1f, () =>
                {
                    UIManager.Instance.Close(UIViewType.UILoadingView);
                    // if (toScene == "MainScene")
                    if (toScene == "TestScene")
                    {//TODO：硬编码，耦合等问题严重。
                        UIManager.Instance.Open(UIViewType.UIMainView);
                    }
                });
            });
        }

        protected override void OnAddListener()
        {
            base.OnAddListener();
            slider_Progress.onValueChanged.AddListener(UpdateText);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();
            slider_Progress.onValueChanged.RemoveListener(UpdateText);
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
            slider_Progress.value = 0f;
        }


        private void UpdateText(float value)
        {
            //两位小数的百分数。value是0~1的浮点值
            // Debug.Log($"{value}");
            //Tip：所谓的卡在99%之类的问题，大概就是跟舍去小数部分，以及未激活已经加载好的场景，有关。
            text_Progress.text = $"{Mathf.Round(value * 100f)}%";
        }
    }
}