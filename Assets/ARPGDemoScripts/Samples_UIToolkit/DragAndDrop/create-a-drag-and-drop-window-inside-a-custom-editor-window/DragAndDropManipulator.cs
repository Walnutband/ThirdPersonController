using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator
{
    public DragAndDropManipulator(VisualElement target)
    {
        this.target = target;
        root = target.parent; //获取父对象，注意创建时传入的VE是谁，在这里就是UXML的根元素。
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    private Vector2 targetStartPosition { get; set; }

    private Vector3 pointerStartPosition { get; set; }
    //标记是否已经按下，并且处于该状态。类似于Clickable的保护属性active
    private bool enabled { get; set; }

    private VisualElement root { get; }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        targetStartPosition = target.transform.position;
        pointerStartPosition = evt.position; //鼠标按下时的指针所在位置（全局坐标系）
        //捕获指针，传入指针事件的pointerId，比CaptureMouse捕获鼠标指针更通用，不仅限于鼠标指针。接收所有随后而来的指针事件。
        target.CapturePointer(evt.pointerId); 
        enabled = true;
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        //已经按下
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;

            target.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId); //释放指针
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            VisualElement slotsContainer = root.Q<VisualElement>("slots");
            UQueryBuilder<VisualElement> allSlots = //找到所有槽（Query找所有，Q找首个。）
                slotsContainer.Query<VisualElement>(className: "slot"); //name和className参数，这里是按名传入的写法，一般是按序传入。
            //只包含与拖拽的元素有重叠的槽
            UQueryBuilder<VisualElement> overlappingSlots =
                allSlots.Where(OverlapsTarget); //传入Where的是Action方法，返回类型为bool，参数类型为泛型类型
            VisualElement closestOverlappingSlot =
                FindClosestSlot(overlappingSlots);
            Vector3 closestPos = Vector3.zero;
            //找到了最近槽（前提是有重叠）
            if (closestOverlappingSlot != null)
            {
                closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
            }
            //要么移动到最近的槽的（pivot）位置，要么回到刚开始拖拽时所处的位置（就是回归原位）
            target.transform.position =
                closestOverlappingSlot != null ?
                closestPos :
                targetStartPosition;

            enabled = false;
        }
    }

    //目标元素的位置区域与相应槽的位置区域是否有交集（即是否出现重叠部分）
    private bool OverlapsTarget(VisualElement slot)
    {
        return target.worldBound.Overlaps(slot.worldBound);
    }

    /// <summary>
    /// 寻找最近的槽
    /// </summary>
    /// <param name="slots"></param>
    /// <returns></returns>
    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
    {
        List<VisualElement> slotsList = slots.ToList();
        float bestDistanceSq = float.MaxValue; //从最大值开始，逐渐缩小
        VisualElement closest = null;
        foreach (VisualElement slot in slotsList)
        {
            //
            Vector3 displacement =
                RootSpaceOfSlot(slot) - target.transform.position;
            //因为只需要比较大小，所以直接用平方值即可，不需要开平方，这样可以节约性能
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    /// <summary>
    /// 获取槽在根元素坐标系下的坐标
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private Vector3 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return root.WorldToLocal(slotWorldSpace);
    }
}