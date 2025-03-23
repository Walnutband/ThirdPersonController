using HalfDog.EasyInteractive;
using System;

/// <summary>
/// 拖拽物体到UI上的交互情景
/// </summary>
[InteractCase(typeof(SceneItem), typeof(UIItem))]  //拖拽场景物体到UI物体
public class DragToUI : DragSubjectFocusTargetInteractCase
{
    private SceneItem sceneItem; //场景物体
    private UIItem uiItem; //UI物体
    public DragToUI(Type subject, Type target) : base(subject, target)
    {
    }
    protected override void OnEnter(IDragable subject, IFocusable target)
    {//进入该交互情景，获取主体和目标的引用
        sceneItem = (subject as SceneItem);
        uiItem = (target as UIItem);
    }
    protected override void OnExecute(IDragable subject, IFocusable target)
    {
        if (EndDrag) //结束拖拽
        {
            uiItem.icon.gameObject.SetActive(true);
            uiItem.icon.sprite = sceneItem.iconSprite;
        }
    }
    protected override void OnExit()
    {
        sceneItem.gameObject.SetActive(false);
    }
}
