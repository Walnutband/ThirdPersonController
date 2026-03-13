
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    [AddComponentMenu("GoodUI/Controls/Scroll Bar")]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollBar : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler, 
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {

        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop
        }

        [SerializeField] private Direction m_Direction = Direction.LeftToRight;

        [SerializeField] private RectTransform m_HandleRect;
        [SerializeField] private RectTransform m_ContainerRect;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoveredColor;
        private bool m_IsHovered;
        [SerializeField] private Color pressedColor;
        private bool m_IsPressed;

        [Range(0f, 1f)]
        [SerializeField] private float m_Size = 0.2f; 
        [Range(0f, 1f)]
        [SerializeField] private float m_Value = 0f; //代表进度。
        [SerializeField] private float m_Sensitivity = 1f;

        private bool IsHorizontal => m_Direction == Direction.LeftToRight || m_Direction == Direction.RightToLeft;
        private bool IsReverse => m_Direction == Direction.RightToLeft || m_Direction == Direction.TopToBottom;

        public event Action<float> ValueChangedEvent;
        // public void RegisterValueChangedCallback(Action<float> callback)
        // {
        //     m_OnValueChanged += callback;
        // }


        private CanvasRenderer m_CanvasRenderer;
        public CanvasRenderer canvasRenderer
        {
            get
            {
                if (m_CanvasRenderer == null)
                {
                    // m_CanvasRenderer = GetComponent<CanvasRenderer>();
                    m_CanvasRenderer = m_HandleRect.GetComponent<CanvasRenderer>();
                }
                return m_CanvasRenderer;
            }
        }

        private bool m_IsDraggingHandle;
        private Vector2 m_HandleCenterToMouse;

        protected override void Reset()
        {
            // Debug.Log("Reset");
            // base.Reset();
            ResetHandle();
        }

        protected override void OnValidate()
        {
            // Debug.Log("OnValidate");
            // ResetHandle();
        }

        [ContextMenu("重置滑块")]
        private void ResetHandle()
        {
            if (m_HandleRect == null || m_ContainerRect == null)
            {
                // Debug.LogError($"ScrollBar：{name} 没有设置HandleRect，请检查");
                // return;
                m_HandleRect = transform.FindRecursively("Handle") as RectTransform;
                m_ContainerRect = m_HandleRect.parent as RectTransform;
            }

            InitializeHandle();
            //直接与锚点矩形对齐。
            m_HandleRect.offsetMin = Vector2.zero;
            m_HandleRect.offsetMax = Vector2.zero;
        }



        protected override void OnEnable()
        {
            InitializeHandle();
        }

        protected override void Start()
        {
            InitializeHandle();
        }

        private void InitializeHandle()
        {
            float baseValue = Mathf.Clamp01(IsReverse ? (1 - m_Value) * (1 - m_Size) : m_Value * (1 - m_Size));
            Vector2 anchorMin;
            Vector2 anchorMax;
            if (IsHorizontal)
            {
                anchorMin = new Vector2(baseValue * (1 - m_Size), 0f);
                anchorMax = new Vector2(baseValue * (1 - m_Size) + m_Size, 1f);
            }
            else
            {
                anchorMin = new Vector2(0f, baseValue * (1 - m_Size));
                anchorMax = new Vector2(1f, baseValue * (1 - m_Size) + m_Size);
            }

            m_HandleRect.anchorMin = anchorMin;
            m_HandleRect.anchorMax = anchorMax;
        }

        private void Update()
        {
            // SetAnchors();
            // m_HandleRect.anchorMin = new Vector2(m_Value * (1 - m_Size), 0f);
            // m_HandleRect.anchorMax = new Vector2(m_Value * (1 - m_Size) + m_Size, 1f);
        }

        private void SetAnchors(float _value, bool _isHorizontal)
        {
            Debug.Log($"value: {_value}");
            if (_isHorizontal)
            {
                // m_HandleRect.anchorMin = new Vector2(_value, 0f);
                // m_HandleRect.anchorMax = new Vector2(_value + m_Size, 1f);
                m_HandleRect.anchorMin = new Vector2(_value * (1 - m_Size), 0f);
                m_HandleRect.anchorMax = new Vector2(_value * (1 - m_Size) + m_Size, 1f);
            }
            else
            {
                m_HandleRect.anchorMin = new Vector2(0f, _value * (1 - m_Size));
                m_HandleRect.anchorMax = new Vector2(1f, _value * (1 - m_Size) + m_Size);
            }

            ValueChangedEvent?.Invoke(m_Value);
        }

        

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.pressEventCamera))
            {
                m_IsDraggingHandle = true;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition))
                {
                    // Debug.Log("OnBeginDrag");
                    m_HandleCenterToMouse = localMousePosition - m_HandleRect.rect.center;
                    // Debug.Log("m_HandleCenterToMouse: " + m_HandleCenterToMouse);
                }
                
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_IsDraggingHandle)
            {
                Vector2 localMousePos = Vector2.zero;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ContainerRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                {
                    // Debug.Log("OnDrag");
                    //Tip：应该用绝对而非相对。
                    // Vector2 currentOffset = localMousePos - m_ContainerRect.rect.position;
                    // m_Value = Mathf.Clamp01(m_Value + (currentOffset - m_DragStartOffset).x / (m_ContainerRect.rect.width * (1 - m_Size)));
                    // SetAnchors();
                    //Tip：一般说Corner，就是左下角。
                    Vector2 containerCornerToMouse = localMousePos - m_ContainerRect.rect.position; //
                    // Vector2 containerCornerToHandleCorner = containerCornerToMouse - m_HandleCenterToMouse - (m_HandleRect.rect.size - m_HandleRect.sizeDelta) * 0.5f;
                    //Tip：实际上这样还更合理，直接求出Handle左下角指向中心的向量，而不是把sizeDelta掺杂进来。
                    Vector2 containerCornerToHandleCorner = containerCornerToMouse - m_HandleCenterToMouse - (m_HandleRect.rect.center - m_HandleRect.rect.position);

                    // m_Value = Mathf.Clamp01(containerCornerToHandleCorner.x / (m_ContainerRect.rect.width * (1 - m_Size)));
                    UpdateValueAndHandle(containerCornerToHandleCorner, m_ContainerRect.rect.size * (1 - m_Size));
                    // Debug.Log($"containerCornerToHandleCorner: {containerCornerToHandleCorner}");


                }

            }
        }

        private void UpdateValueAndHandle(Vector2 _cornerToCorner, Vector2 _remainSize) //剩余尺寸，就是可滑动的尺寸。
        {
            switch (m_Direction)
            {
                case Direction.LeftToRight:
                    m_Value = Mathf.Clamp01(_cornerToCorner.x / _remainSize.x);
                    SetAnchors(m_Value, true);
                    break;
                case Direction.RightToLeft:
                    m_Value = Mathf.Clamp01(1 - _cornerToCorner.x / _remainSize.x);
                    SetAnchors(1 - m_Value, true);
                    break;
                case Direction.BottomToTop:
                    m_Value = Mathf.Clamp01(_cornerToCorner.y / _remainSize.y);
                    SetAnchors(m_Value, false);
                    break;
                case Direction.TopToBottom:
                    m_Value = Mathf.Clamp01(1 - _cornerToCorner.y / _remainSize.y);
                    SetAnchors(1 - m_Value, false);
                    break;
            }
            // Debug.Log("containerCornerToHandleCorner: " + _cornerToCorner + "value: " + m_Value);
            // ValueChangedEvent?.Invoke(m_Value);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_IsDraggingHandle = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

            //是否在Handle内部
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition)
                && m_HandleRect.rect.Contains(localMousePosition))
            {
                m_IsPressed = true;
            }
            UpdateHandleAppearance();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_IsPressed = false;
            UpdateHandleAppearance();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.enterEventCamera, out Vector2 localMousePosition)
                && m_HandleRect.rect.Contains(localMousePosition))
            {
                m_IsHovered = true;
            }
            UpdateHandleAppearance();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.enterEventCamera, out Vector2 localMousePosition)
                && m_HandleRect.rect.Contains(localMousePosition))
            {
                m_IsHovered = true;
            }
            else
            {
                m_IsHovered = false;
            }
            UpdateHandleAppearance();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_IsHovered = false;
            UpdateHandleAppearance();
        }

        private void UpdateHandleAppearance()
        {
            Image image = m_HandleRect.GetComponent<Image>();
            // canvasRenderer.SetColor(normalColor);
            image.color = normalColor;
            if (m_IsHovered == true)
                // canvasRenderer.SetColor(hoveredColor);
                image.color = hoveredColor;
            //越靠后，优先级越高
            if (m_IsPressed == true)
                // canvasRenderer.SetColor(pressedColor);
                image.color = pressedColor;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float delta = eventData.scrollDelta.y * m_Sensitivity;
            m_Value = Mathf.Clamp01(m_Value + delta);
            SetAnchors(m_Value, IsHorizontal);
        }
    }
}