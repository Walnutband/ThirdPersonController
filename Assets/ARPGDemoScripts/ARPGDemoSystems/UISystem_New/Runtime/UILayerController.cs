
using System.Collections.Generic;
using Physalia.Flexi;
using UnityEngine;

namespace ARPGDemo.UISystem_New
{
    public class UILayerController
    {
        public UILayerType layerType;
        public int sortingOrder = 0;

        private Canvas m_Canvas;
        private int m_LayerOrder; //固定的层级顺序

        // 当前打开的UI面板（按打开顺序）
        private List<UIPanelController> openedPanels = new List<UIPanelController>();

        //Tip：逻辑上，只有打开的面板才会记录在层级中，而虽然每个面板都是固定位于特定层级，但是没有打开那么就与层级无关。
        //TODO：严格按照栈来开关。暂时不考虑直接指定关闭某个面板的功能。
        private Stack<UIPanelController> m_Panels = new Stack<UIPanelController>();

        //Tip：缓存面板，其实相当于对象池，总之先从这里取，如果没有再创建，而关闭一个面板之后就将其缓存下来，以便下次直接取用。
        private Dictionary<UIPanelType, List<UIPanelController>> m_CachedPanels = new Dictionary<UIPanelType, List<UIPanelController>>();

        public UILayerController(UILayerType _layer, Canvas _canvas)
        {
            layerType = _layer;
            m_Canvas = _canvas;
            m_LayerOrder = (int)_layer;
        }

        //传进来了，就说明该面板是在该层级中显示。
        public void OpenPanel(UIPanelType _panelType)
        {
            UIPanelController panel;
            if (m_CachedPanels.TryGetValue(_panelType, out var panels) && panels != null && panels.Count > 0)
            {
                panel = panels[0];
            }
            else
            {
                panel = new UIPanelController()
                {
                    layerType = layerType,
                    panelType = _panelType
                };
            }

            //获取到Panel之后
            OpenPanel(panel);
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public void OpenPanel(UIPanelController _panel)
        {
            if (_panel == null) return;

            var lastPanel = m_Panels.Peek();
            if (lastPanel != null)
            {
                lastPanel.Pause();
            }

            // 添加到列表
            m_Panels.Push(_panel);

            // 设置层级顺序
            SetPanelSortingOrder(_panel, openedPanels.Count);

            // 打开新面板
            _panel.Open();
        }

        //打开时只需要指定面板类型，而关闭时应该直接给出面板控制器。

        //默认弹出最顶部的Panel。
        public void ClosePanel()
        {
            UIPanelController _panel = m_Panels.Pop();
            if (_panel == null) return; //没有打开的面板，啥都不干。

            //_panel.Pause(); //Ques：需要调用暂停吗？
            _panel.Close();
            if (m_CachedPanels.TryGetValue(_panel.panelType, out var panels))
            {
                panels.Add(_panel);
            }
            else //添加缓存容器。
            {
                panels = new List<UIPanelController>
                {
                    _panel
                };
                m_CachedPanels.Add(_panel.panelType, panels);
            }

            var lastPanel = m_Panels.Peek();
            if (lastPanel != null)
            {
                lastPanel.Resume(); //恢复上一个面板，也就是现在最顶部的面板。
            }
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel(UIPanelController panel)
        {
            if (panel == null || !openedPanels.Contains(panel)) return;

            int index = openedPanels.IndexOf(panel);

            // 关闭面板
            panel.Close();

            // 从列表中移除
            openedPanels.RemoveAt(index);

            // 恢复上一个面板
            if (openedPanels.Count > 0 && index > 0)
            {
                UIPanelController lastPanel = openedPanels[openedPanels.Count - 1];
                lastPanel.Resume();
            }
        }

        /// <summary>
        /// 关闭最顶层的面板
        /// </summary>
        public void CloseTopPanel()
        {
            if (openedPanels.Count > 0)
            {
                UIPanelController topPanel = openedPanels[openedPanels.Count - 1];
                ClosePanel(topPanel);
            }
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            while (openedPanels.Count > 0)
            {
                ClosePanel(openedPanels[openedPanels.Count - 1]);
            }
        }

        /// <summary>
        /// 获取当前显示的面板数量
        /// </summary>
        public int GetPanelCount()
        {
            return openedPanels.Count;
        }

        /// <summary>
        /// 检查是否包含某个面板
        /// </summary>
        public bool ContainsPanel(UIPanelController panel)
        {
            return openedPanels.Contains(panel);
        }

        /// <summary>
        /// 获取最顶层的面板
        /// </summary>
        public UIPanelController GetTopPanel()
        {
            return openedPanels.Count > 0 ? openedPanels[openedPanels.Count - 1] : null;
        }

        /// <summary>
        /// 设置面板的渲染顺序
        /// </summary>
        private void SetPanelSortingOrder(UIPanelController panel, int depth)
        {
            // Canvas canvas = panel.GetComponent<Canvas>();
            // if (canvas == null)
            // {
            //     canvas = panel.gameObject.AddComponent<Canvas>();
            // }

            // canvas.overrideSorting = true;
            // canvas.sortingOrder = sortingOrder + depth * 10; // 每层间隔10

            // // 确保有GraphicRaycaster
            // if (panel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            // {
            //     panel.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            // }
        }
    }
}