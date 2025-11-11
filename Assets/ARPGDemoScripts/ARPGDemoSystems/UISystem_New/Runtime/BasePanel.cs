using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem
{
    
    public abstract class BasePanel : MonoBehaviour
    {
        protected Image m_MaskImage;
        protected SimpleButton m_MaskButton;

        protected virtual void Awake()
        {
            //这里要保证与UI面板预制体的基础结构保持一致
            m_MaskImage = transform.Find("UIMask").GetComponent<Image>();
            m_MaskButton = transform.Find("UIMask").GetComponent<SimpleButton>();
        }

        public virtual void Open()
        {
            Show();
            RegisterCallbacks();
        }
        public virtual void Close()
        {
            Hide();
            UnregisterCallbacks();
        }

        protected abstract void Show();
        protected abstract void Hide();


        protected abstract void RegisterCallbacks();

        protected abstract void UnregisterCallbacks();
    }
}