using System;
using Codice.CM.Common;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_New
{
    //挂载到一个UI面板的根对象上
    public abstract class UIPanelBase : MonoBehaviour
    {

        //Tip：遮罩图片和按钮响应，属于一个UI面板的必备成员。
        protected Image m_MaskImage;
        protected SimpleButton m_MaskButton;

        protected virtual void Awake()
        {
            //这里要保证与UI面板预制体的基础结构保持一致，也就是对UI面板预制体的制作提出要求。不过这种基础内容，应该直接写在菜单项中保证一致性。
            m_MaskImage = transform.Find("UIMask").GetComponent<Image>();
            m_MaskButton = transform.Find("UIMask").GetComponent<SimpleButton>();

            Initialize();
        }

        public virtual void Initialize()
        {
            
        }

        public virtual void Open()
        {
            RegisterCallbacks(); //打开的同时注册回调。
            Show();
        }
        public virtual void Close()
        {
            UnregisterCallbacks(); //关闭的同时注销回调。
            Hide();
        }

        public virtual void Pause()
        {
            
        }
        public virtual void Resume()
        {
            
        }

        protected virtual void OnOpenAnim()
        {
            
        }
        protected virtual void OnCloseAnim(Action _completed)
        {
            
        }

        //显示之后执行动效，隐藏之前执行动效。
        protected abstract void Show();
        protected abstract void Hide();


        protected abstract void RegisterCallbacks();
        protected abstract void UnregisterCallbacks();
    }
}