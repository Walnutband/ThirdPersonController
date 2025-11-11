using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    [AddComponentMenu("ARPGDemo/MyPlugins/GoodUI/Layout/GoodLayoutElement")]
    public class GoodLayoutElement : UIBehaviour, ILayoutElement
    {
        /*Tip: 引用的价值就在于能够从程序层面实现自动同步，而不用手动同步，这两者的效果是截然不同的。
        其实源于LayoutGroup的作用机制，它只管理直接子对象，但有时候由于不得不的层级关系，无法实现跨层级管理，
        所以才想到用这一个组件来传递另外层级的对象的布局值。
        */
        [SerializeField] private RectTransform m_Element; 
        public float minWidth => 0f;

        public float preferredWidth => m_Element.rect.width;

        public float flexibleWidth => 0f;

        public float minHeight => 0f;

        public float preferredHeight => m_Element.rect.height;

        public float flexibleHeight => 0f;

        [SerializeField] private int m_LayoutPriority;
        public int layoutPriority => m_LayoutPriority;

        public void CalculateLayoutInputHorizontal()
        {
            
        }

        public void CalculateLayoutInputVertical()
        {
            
        }
    }
}
