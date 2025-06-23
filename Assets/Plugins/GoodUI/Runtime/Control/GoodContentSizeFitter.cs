using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    [AddComponentMenu("GoodUI/Layout/Good Content Size Fitter")] //这里设置的组件名会决定组件检视面板的标题名，与类名其实没有绑定关系
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// Resizes a RectTransform to fit the size of its content.
    /// </summary>
    /// <remarks>
    /// The ContentSizeFitter can be used on GameObjects that have one or more ILayoutElement components, such as Text, Image, HorizontalLayoutGroup, VerticalLayoutGroup, and GridLayoutGroup.
    /// </remarks>
    public class GoodContentSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutGroup //加入一个ILayoutGroup就可以将自己添加到布局重建中了，不得不说C#的接口机制确实很妙
    {
        /// <summary>
        /// The size fit modes avaliable to use.
        /// </summary> 
        public enum FitMode
        {
            /// <summary>
            /// Don't perform any resizing.
            /// </summary>
            Unconstrained,
            /// <summary>
            /// Resize to the minimum size of the content.
            /// </summary>
            MinSize,
            /// <summary>
            /// Resize to the preferred size of the content.
            /// </summary>
            PreferredSize
        }

        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the width.
        /// </summary>
        public FitMode horizontalFit { get { return m_HorizontalFit; } set { if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty(); } }

        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the height.
        /// </summary>
        public FitMode verticalFit { get { return m_VerticalFit; } set { if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty(); } }

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        [SerializeField]
        private RectTransform m_ContentRect;
        public RectTransform contentRect { get { if (m_ContentRect == null) m_ContentRect = transform.GetChild(0) as RectTransform; return m_ContentRect; }}

        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

        protected GoodContentSizeFitter()
        { }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (fitting == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            // Set size to min or preferred size
            if (fitting == FitMode.MinSize)
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_ContentRect, axis));
            else
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_ContentRect, axis));
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
    }
}
