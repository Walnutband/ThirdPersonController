
using System;
using UnityEngine;

namespace ARPGDemo.UISystem_New
{
    public class UIPanelController
    {
        public UILayerType layerType; //面板所在层级
        public UIPanelType panelType;
        // public string prefabPath; // 预制体路径

        //记录自己所控制的UI面板组件。
        public UIPanelBase panelBase;  
        public Canvas canvas;

        public UIPanelController()
        {
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public void Open()
        {
            // if (panelBase == null)
            // {
            //     var go = UIResourceManager.Instance.InstantiatePrefab(panelType);

            // }

            //Tip：保证在创建时就已经加载好了面板的预制体。
            if (panelBase == null)
            {
                Debug.LogError("在尝试打开面板时，面板控制器中的面板对象为空，请检查");
                return;
            }

            panelBase.Open();

                
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void Close()
        {
            if (panelBase == null)
            {
                Debug.LogError("在尝试关闭面板时，面板控制器中的面板对象为空，请检查");
                return;
            }

            panelBase.Close();
        }

        /// <summary>
        /// 暂停面板
        /// </summary>
        public void Pause()
        {
            if (panelBase == null)
            {
                Debug.LogError("在尝试暂停面板时，面板控制器中的面板对象为空，请检查");
                return;
            }
            panelBase.Pause();
        }

        /// <summary>
        /// 恢复面板
        /// </summary>
        public void Resume()
        {
            if (panelBase == null)
            {
                Debug.LogError("在尝试恢复面板时，面板控制器中的面板对象为空，请检查");
                return;
            }
            panelBase.Resume();
        }

        /// <summary>
        /// 销毁面板
        /// </summary>
        public void Destroy()
        {
            if (panelBase != null)
            {
                UnityEngine.Object.Destroy(panelBase.gameObject);
                panelBase = null;
            }
        }
    }
}