
using System;
using DG.Tweening;
using UnityEngine;

namespace ARPGDemo.UISystem_Test
{
    public interface IUIPanel
    {
        /*Tip：打开面板和关闭面板，一般认为这很直观，打开就是显示、注册回调，关闭就是隐藏、注销回调，由于动效的存在，实际上打开之后还有一段动效，而关闭也应该在关闭之间有一段动效，
        而且回调的注册和注销可能是在通知打开或关闭时，也可能是在动效开始或结束时，所以对于这里Open和Close的理解就应该是UIManager通知面板要打开或关闭，而具体逻辑由面板自己决定，
        向UIManager返回执行情况
        */

        /*Tip：打开动效应该需要一些初始化逻辑，比如渐入的逻辑，就应该先处于渐入前的情况，再进行一个渐入的变化过程，而在编辑器中编辑时，应该是按照动效完成之后的样子来编辑，
        不过DOTween也有过渡到当前值的方法，所以似乎可以由此替代初始化逻辑。
        UI面板就是游戏对象，打开和关闭就是GameObject的SetActive为true或false，动效就是位置、尺寸、alpha等基础属性值的变化。
        */
        void Open(); 
        void Close();
    }

    public abstract class UIPanelBase : MonoBehaviour, IUIPanel
    {
        protected virtual void Awake()
        {
            Hide(); //起手先隐藏
        }

        public void Open()
        {
            Show();
            OnOpen();
            OpenAnim();
            RegisterCallbacks();
        }

        protected virtual void OnOpen()
        {
            
        }
        protected virtual void OnClose()
        {

        }

        //打开时动效
        protected abstract void OpenAnim();

        //关闭时动效
        protected abstract void CloseAnim(TweenCallback _hide);

        public void Close()
        {
            UnregisterCallbacks();
            OnClose();
            CloseAnim(Hide); //让面板自己处理好动效，然后在合适的时候执行Hide隐藏面板。
        }

        protected virtual void RegisterCallbacks()
        {
            
        }

        protected virtual void UnregisterCallbacks()
        {
            
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }
        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}