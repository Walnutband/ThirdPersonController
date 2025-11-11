
using DG.Tweening;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Old
{
    
    public class UIMainView : UIView
    {

        #region 控件绑定变量声明，自动生成请勿手改
    #pragma warning disable 0649
        [ControlBinding]
        private Button button_Exit;
        [ControlBinding]
        private Button button_Settings;
        [ControlBinding]
        private Button button_Character;
        [ControlBinding]
        private Button button_Activities;

    #pragma warning restore 0649
        #endregion



        protected override void OnAddListener()
        {
            base.OnAddListener();
            button_Exit.onClick.AddListener(OpenExitView);
            button_Settings.onClick.AddListener(OpenSettingsView);
            button_Character.onClick.AddListener(OpenCharacterView);
        }

        protected override void OnRemoveListener()
        {
            base.OnRemoveListener();
            button_Exit.onClick.RemoveListener(OpenExitView);
            button_Settings.onClick.RemoveListener(OpenSettingsView);
            button_Character.onClick.RemoveListener(OpenCharacterView);

        }

        public override void OnCloseAnim(TweenCallback complete)
        {
            complete?.Invoke();
        }

        public override void OnOpenAnim()
        {

        }

        //UI主视图的特殊性就在于控制鼠标的显隐。
        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Cursor.lockState = CursorLockMode.Locked;
            // UIManager.Instance.GetPlayerInput().SwitchCurrentActionMap("Player");
        }

        public override void OnClose()
        {
            base.OnClose();
            Cursor.lockState = CursorLockMode.None;
            // UIManager.Instance.GetPlayerInput().SwitchCurrentActionMap("UI");
        }

        //OnResume，卧槽，直到需要控制鼠标的显隐时，才体会到这个框架为UIView设置的OnResume和OnPause方法的用处。
        public override void OnResume()
        {
            base.OnResume();
            // UIManager.Instance.GetPlayerInput().SwitchCurrentActionMap("Player");
            Cursor.lockState = CursorLockMode.Locked;
            UIManager.Instance.InMainView = true;
        }

        public override void OnPause()
        {
            base.OnPause();
            // UIManager.Instance.GetPlayerInput().SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            UIManager.Instance.InMainView = false;
        }

        private void OpenExitView()
        {
            UIManager.Instance.Open(UIViewType.UIExitView, "是否退回到开始界面？");
        }

        private void OpenCharacterView()
        {
            UIManager.Instance.FadeOutIn(0.5f, () => UIManager.Instance.Open(UIViewType.UICharacterView));
        }

        private void OpenSettingsView()
        {
            UIManager.Instance.Open(UIViewType.UISettingsView);
        }
    }
}