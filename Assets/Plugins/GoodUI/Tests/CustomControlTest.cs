
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomControl : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private RectTransform m_AreaRect;
    [SerializeField]
    private RectTransform m_HandleRect;

    public Image handleImage;

    public Color hoverColor;
    public Color pressColor;
    [SerializeField]
    private float sensitivity;
    private bool m_IsHovered;
    public bool isHovered { get => m_IsHovered; set { if (m_IsHovered != value) { m_IsHovered = value; ChangeState(); } } }
    private bool m_IsPressed;
    public bool isPressed { get => m_IsPressed;  set {if (m_IsPressed != value) { m_IsPressed = value;  ChangeState(); }}}

    private RectTransform m_RectTransform;

    private Vector2 lastPosition;
    private float heightRange;

    [Range(0, 1)]
    [SerializeField]
    private float m_Value;
    //夹在0~1,并且保留四位精度，注意这里的简单算式。
    public float value { get => m_Value; set { if (m_Value != value) { m_Value = Mathf.Clamp01(value); m_Value = Mathf.Round(m_Value * 10000f) / 10000f; UpdateHandlePosition(); } } }

    [Range(0, 1)]
    [SerializeField]
    private float m_Length;
    public float length { get => m_Length;  set {if (m_Length != value) { m_Length = Mathf.Clamp01(value); m_Value = Mathf.Round(m_Value * 10000f) / 10000f; UpdateHandleSize(); }}}

    [ContextMenu("改变方向")]
    private void ChangeDirection()
    {
        m_RectTransform.anchorMin = m_RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        m_RectTransform.pivot = new Vector2(0.5f, 0.5f);
        m_RectTransform.sizeDelta = new Vector2(m_RectTransform.sizeDelta.y, m_RectTransform.sizeDelta.x);
    }

    protected override void Awake()
    {
        m_AreaRect = transform.GetChild(0) as RectTransform;
        m_HandleRect = m_AreaRect.GetChild(0) as RectTransform;
        m_RectTransform = transform as RectTransform;
        handleImage = m_HandleRect.GetComponent<Image>();
        heightRange = m_AreaRect.rect.height;
    }

    protected override void OnEnable()
    {
        m_AreaRect.anchorMin = Vector2.zero;
        m_AreaRect.anchorMax = Vector2.one;
        m_HandleRect.anchorMin = Vector2.zero;
        m_HandleRect.anchorMax = Vector2.one;

        m_AreaRect.sizeDelta = Vector2.zero; //填满滚动条父对象
        m_HandleRect.pivot = new Vector2(0.5f, 0f);
        // m_HandleRect.sizeDelta = new Vector2(0, m_AreaRect.rect.height * (length - 1));
        UpdateHandleSize();
        UpdateHandlePosition();
    }

    //假设为竖直滚动条
    protected override void Start()
    {


    }

    protected override void OnRectTransformDimensionsChange()
    {
        // heightRange = m_RectTransform.rect.height;
    }

    private void ChangeState()
    {
        handleImage.color = Color.white;

        if (isHovered == true)
            handleImage.color = hoverColor;
        //越靠后，优先级越高
        if (isPressed == true)
            handleImage.color = pressColor;
    }

    public void UpdateHandleSize()
    {
        Debug.Log("UpdateHandleSize");
        m_HandleRect.sizeDelta = new Vector2(0, m_AreaRect.rect.height * (length - 1));
    }

    public void UpdateHandlePosition()
    {
        Debug.Log("UpdateHandlePosition");
        m_HandleRect.anchorMin = m_HandleRect.anchorMax = new Vector2(0.5f, 0f);//锚点定到下边中点再设置锚点位置
        float areaHeight = m_AreaRect.rect.height;
        float offsetByCenter = areaHeight * (1 - length) * value;
        m_HandleRect.anchoredPosition = new Vector2(0, offsetByCenter);
        m_HandleRect.anchorMin = Vector2.zero;
        m_HandleRect.anchorMax = Vector2.one;
    }   

    public void OnPointerEnter(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition);
        if (m_HandleRect.rect.Contains(localMousePosition))
        {
            isHovered = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition);
        // if (m_HandleRect.rect.Contains(eventData.position))
        // {
        //     isHovered = false;
        //     ChangeState();
        // }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_HandleRect.rect.Contains(eventData.position))
        {
            isPressed = true;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition);
            float deltaValue = (localMousePosition.y - m_HandleRect.rect.center.y) / (m_AreaRect.rect.height * (1 - length));
            // if (value + deltaValue <= 0f)
            // {
            //     value = 0f;
            //     return;
            // }
            // else if (value + deltaValue >= 1f)
            // {
            //     value = 1f;
            //     return;
            // }

            value += deltaValue;
            isPressed = true;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastPosition = eventData.position;
        isPressed = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 moveVector = eventData.position - lastPosition;
        lastPosition = eventData.position;

        value += moveVector.y / (heightRange * (1 - length));
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition);
        if (m_HandleRect.rect.Contains(localMousePosition))
        {
            isHovered = true;
        }
        else isHovered = false;
    }
}