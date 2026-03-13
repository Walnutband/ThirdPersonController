using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    /*Tip：与ScrollRect的作用定位类似。*/
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollingContainer : UIBehaviour
    {
        //可视区域与内容区域
        [SerializeField] private RectTransform m_Viewport;
        [SerializeField] private RectTransform m_Content;

        private Bounds m_ViewBounds;
        private Bounds m_ContentBounds;
    }
}