
using ARPGDemo.ControlSystem.InputActionBindings;
using JetBrains.Annotations;
using UnityEngine;

namespace ARPGDemo.UISystem_Test
{
    /*Tip：简单用于UI面板功能测试，主要是方便调度面板，各个面板实现调度接口，以及自己的交互逻辑。*/

    public class UIManager : SingletonMono<UIManager>
    {
        public InputActionBinder<OpenSpecifiedUIPanel> openAttributePanel;
        private UIPanelBase currentPanel;

        private void OnEnable()
        {
            openAttributePanel.Enable();
        }
        private void OnDisable()
        {
            openAttributePanel.Disable();
        }

        //Tip：对于Panel面板自己来说，自己就可以执行关闭自己的逻辑，而仍然应该走UIManager的流程，就是在从上到下通知各级、自己要关闭了，最后转到面板自己的Close方法。

        public void OpenPanel(GameObject _panel)
        {
            UIPanelBase panel = _panel.GetComponent<UIPanelBase>();
            if (panel == null)
            {
                Debug.LogWarning($"{_panel.name}没有UIPanelBase组件。");
                return;
            }

            //TODO：打开的是当前面板，就将其关闭，这是一个基本机制了，但是不一定这么写的。
            if (panel == currentPanel)
            {
                ClosePanel();
                return;
            }

            panel.Open();
            currentPanel = panel;
        }

        public void ClosePanel()
        {
            currentPanel.Close();
            currentPanel = null;
        }

        public void OpenPanel(UIPanelType _type)
        {
            
        }

        
    }
}