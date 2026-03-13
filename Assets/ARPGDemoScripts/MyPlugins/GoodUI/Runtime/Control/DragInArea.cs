using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class ScriptTest : UIBehaviour, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler
{
    private RectTransform areaRect;
    private RectTransform selfRect;
    private Vector2 beginMousePosition;
    private Vector2 beginSelfPosition;
    public float sensitivity = 1f;
    public bool horizontal = true;
    public bool vertical = true;
    // private Transform beginTransform;
    public bool useDragThreshold;
    private DrivenRectTransformTracker tracker;

    protected override void Awake()
    {
        areaRect = transform.parent as RectTransform;
        selfRect = transform as RectTransform;
        tracker = new DrivenRectTransformTracker();
    }

    protected override void OnEnable()
    {
        //固定在左上角，即左上角为原点，这样左边界和上边界的最值都是0，虽然似乎也没啥区别。。
        tracker.Add(this, selfRect, DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax | DrivenTransformProperties.Pivot);
        selfRect.anchorMin = selfRect.anchorMax = new Vector2(0f, 1f);
        selfRect.pivot = new Vector2(0f, 1f);
    }

    protected override void OnDisable()
    {
        tracker.Clear();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {//这些数据就是为了基于开始拖拽时的状态，来计算拖拽时应该处于的状态。
        beginMousePosition = eventData.position;
        beginSelfPosition = selfRect.anchoredPosition;
    }

    /*Tip:就是两种思路(相对与绝对，对象与关系)，要么计算相邻差值，要么计算当前与起始的差值，总之要把相应的数值确定好，如果弄串了就会出bug了
    不过实际测试了一下，如果要用绝对方式的话，就需要记录开始拖拽时的Transform（关键是坐标映射），但它是引用类型，而且是组件（不可直接实例化，必须依附于游戏对象），
    那么基本就只能创建一个游戏对象，显然搞得太麻烦了，而用相对方式的话，甚至都不需要定义这些存储开始数据的变量，不过稍微不便的就是拖拽时如果鼠标超出范围的话也照样
    会响应移动，虽然不算bug，但是有点影响手感，可以加上条件判断*/

    // public void OnDrag(PointerEventData eventData)
    // {
    //     float delta = eventData.position.x - beginMousePosition.x;
    //     if (selfXMax + delta >= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMax))
    //         selfRect.anchoredPosition = new Vector2(areaRect.rect.width - selfRect.rect.width, 0f);
    //     else if (selfXMin + delta <= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMin))
    //         selfRect.anchoredPosition = Vector2.zero;
    //     else
    //     {//由于需要确定坐标，所以要固定pivot和锚点位置，怎么方便就怎么固定。
    //         selfRect.anchoredPosition = beginSelfPosition + new Vector2(delta, 0f) * sensitivity;
    //     }
    // }

    /*Tip:我是究极无敌小丑。其实只用一个夹子，就可以实现边界修正，以及限制在边界之内了，完全不需要那些坐标转换、最值比较之类的。所以其实下面十来行代码就可以搞定了。
    也可以反思一手，这似乎只能以绝对计算的方式实现，如果是相对计算的话那确实需要边界修正。似乎也不是，相对计算的话也可以对计算出来的目标值使用夹子，总结为，我是纯小丑。*/
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 targetPos = beginSelfPosition; //直接计算绝对位置，由于无法对anchoredPosition的x和y单独赋值，所以用该变量来记录，计算之后直接赋值给anchoredPosition。
        float deltaX = (eventData.position - beginMousePosition).x, deltaY = (eventData.position - beginMousePosition).y;
        if (horizontal)
        {//Tip:我是sb，这一点很重要。本来xMax和xMin这种点的局部坐标就是不变的（因为矩形本身尺寸没变，pivot即原点也没有变），那么直接比较就行了，只要将areaRect的点转换到该局部坐标系来就行了
            targetPos.x = Mathf.Clamp(targetPos.x + deltaX * sensitivity, 0f, areaRect.rect.width - selfRect.rect.width);
        }
        if (vertical)
        {
            targetPos.y = Mathf.Clamp(targetPos.y + deltaY * sensitivity, 0f - (areaRect.rect.height - selfRect.rect.height), 0f);
        }

        selfRect.anchoredPosition = targetPos;
    }

    /*
        public void OnDrag(PointerEventData eventData)
        {
            // Vector2 deltaPos = Vector2.zero;
            Vector2 targetPos = Vector2.zero; //直接计算绝对位置，由于无法对anchoredPosition的x和y单独赋值，所以用该变量来记录，计算之后直接赋值给anchoredPosition。
            float deltaX = (eventData.position - beginMousePosition).x, deltaY = (eventData.position - beginMousePosition).y;
            if (horizontal)
            {//Tip:我是sb，这一点很重要。本来xMax和xMin这种点的局部坐标就是不变的（因为矩形本身尺寸没变，pivot即原点也没有变），那么直接比较就行了，只要将areaRect的点转换到该局部坐标系来就行了
             //基于向量（基本概念，平移等价性质），基于参考系，来思考。
             // if (selfRect.rect.xMax >= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMax) - eventData.delta.x)
             // {
             //     targetPos.x = areaRect.rect.width - selfRect.rect.width;
             //     // Debug.Log("xMax");
             // }
             // else if (selfRect.rect.xMin <= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMin) - eventData.delta.x)
             // {
             //     targetPos.x = 0f;
             //     // Debug.Log("xMin");
             // }
             // else
             // {//由于需要确定坐标，所以要固定pivot和锚点位置，怎么方便就怎么固定。
             //BugFix:两个关键问题：绝对还是相对，通常表现为是“+=”还是“==”；累积差值还是比较初始值，通常表现为使用的数据，在开始时记录的begin还是在过程中每次的delta，总之往往会因为错误的累加而造成异常的飞速移动
             // targetPos.x = beginSelfPosition.x + deltaX * sensitivity;
                targetPos.x = Mathf.Clamp(beginSelfPosition.x + deltaX * sensitivity, 0f, areaRect.rect.width - selfRect.rect.width);
                // Debug.Log("no");
                // }
            }

            if (vertical)
            {
                //转换为同一坐标系，直接比较边界点坐标即可。
                //BugFix：我草了，这里错用的TransformLocalPointX一直没发现，浪费好多时间，但确实只要足够仔细地检查一遍，通常发现不了任何逻辑错误的情况下往往是实际逻辑与理想逻辑产生了偏差，不先入为主地检查就能发现。
                // if (selfRect.rect.yMax >= areaRect.TransformLocalPointY(selfRect, areaRect.rect.yMax) - eventData.delta.y)
                // {
                //     targetPos.y = 0f;
                // }
                // else if (selfRect.rect.yMin <= areaRect.TransformLocalPointY(selfRect, areaRect.rect.yMin) - eventData.delta.y)
                // {
                //     targetPos.y = 0f - (areaRect.rect.height - selfRect.rect.height);
                // }
                // else
                // {//由于需要确定坐标，所以要固定pivot和锚点位置，怎么方便就怎么固定。
                // targetPos.y = beginSelfPosition.y + deltaY * sensitivity;
                targetPos.y = Mathf.Clamp(beginSelfPosition.y + deltaY * sensitivity, 0f - (areaRect.rect.height - selfRect.rect.height), 0f);
                // }
            }

            selfRect.anchoredPosition = targetPos;
        }
    */

    // public void OnDrag(PointerEventData eventData)
    // {
    //     // Vector2 deltaPos = Vector2.zero;
    //     Vector2 targetPos = selfRect.anchoredPosition; //直接计算绝对位置，由于无法对anchoredPosition的x和y单独赋值，所以用该变量来记录，计算之后直接赋值给anchoredPosition。
    //     float deltaX = eventData.delta.x, deltaY = eventData.delta.y;
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(areaRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
    //     //如果已经到达边界，才会在鼠标超出区域之外时不再响应，否则的话还是需要响应，否则当由于某些原因导致鼠标位置超出按钮范围时，可能鼠标移动到了边界之外，按钮就卡在中间某个位置不动了。
    //     if (!IsMouseOutAreaX(localPoint) && horizontal)
    //     {//BugFix:注意在边界修正时需要直接设置锚点位置，也就是绝对位置，而在非边界时才是加上相对位置，如果都按照相对位置来计算的话，会发现矩形直接飞出去了，因为帧率高，而每帧都要加上这个距离，所以极短时间内会移动很长一段距离
    //         if (selfRect.rect.xMax + deltaX >= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMax))
    //             targetPos.x = areaRect.rect.width - selfRect.rect.width;
    //         else if (selfRect.rect.xMin + deltaX <= areaRect.TransformLocalPointX(selfRect, areaRect.rect.xMin))
    //             targetPos.x = 0f;
    //         else
    //         {//由于需要确定坐标，所以要固定pivot和锚点位置，怎么方便就怎么固定。
    //             targetPos.x += deltaX * sensitivity;
    //         }
    //     }

    //     if (!IsMouseOutAreaY(localPoint) && vertical)
    //     {
    //         //转换为同一坐标系，直接比较边界点坐标即可。
    //         //BugFix：我草了，这里错用的TransformLocalPointX一直没发现，浪费好多时间，但确实只要足够仔细地检查一遍，通常发现不了任何逻辑错误的情况下往往是实际逻辑与理想逻辑产生了偏差，不先入为主地检查就能发现。
    //         if (selfRect.rect.yMax + deltaY >= areaRect.TransformLocalPointY(selfRect, areaRect.rect.yMax))
    //         {
    //             targetPos.y = 0f;
    //             Debug.Log("Y上边界");
    //         }
    //         else if (selfRect.rect.yMin + deltaY <= areaRect.TransformLocalPointY(selfRect, areaRect.rect.yMin))
    //         {
    //             targetPos.y = 0f - (areaRect.rect.height - selfRect.rect.height);
    //             Debug.Log("Y下边界");
    //         }
    //         else
    //         {//由于需要确定坐标，所以要固定pivot和锚点位置，怎么方便就怎么固定。
    //             targetPos.y += deltaY * sensitivity;
    //         }
    //     }
    //     // Debug.Log($@"deltaPos：{targetPos.x}, {targetPos.y}
    //     // delta: {deltaX}, {deltaY}");
    //     selfRect.anchoredPosition = targetPos;
    // }

    //移动矩形已经到达边界，并且鼠标位置在areaRect范围之外

    private bool IsMouseOutAreaX(Vector2 localPoint) => (Mathf.Approximately(selfRect.anchoredPosition.x, areaRect.rect.width - selfRect.rect.width) || Mathf.Approximately(selfRect.anchoredPosition.x, 0f))
        && !areaRect.rect.Contains(localPoint);

    private bool IsMouseOutAreaY(Vector2 localPoint) => (Mathf.Approximately(selfRect.anchoredPosition.y, 0f - (areaRect.rect.height - selfRect.rect.height)) || Mathf.Approximately(selfRect.anchoredPosition.y, 0f))
        && !areaRect.rect.Contains(localPoint);

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = useDragThreshold;
    }
}