using HalfDog.EasyInteractive;
using System;
using UnityEngine;
//从AbstractInteractCase到DragSubjectFocusTargetInteractCase再到这里的DragToScene，就是一个从抽象到具体的过程，

/// <summary>
/// 从UIItem拖拽到场景中的交互情景
/// </summary>
[InteractCase(typeof(UIItem), typeof(Table))]
public class DragToScene : DragSubjectFocusTargetInteractCase
{
    private UIItem uiItem;
    private Table table;
    public DragToScene(Type subject, Type target) : base(subject, target)
    {
    }

    protected override void OnEnter(IDragable subject, IFocusable target)
    {
        uiItem = (subject as UIItem);
        table = (target as Table);
        table.tempItem.gameObject.SetActive(true); //显示（比较透明的）临时物体，就相当于预览效果
    }

    protected override void OnExecute(IDragable subject, IFocusable target)
    {
        if (Input.GetMouseButtonUp(0))
        { //这里是鼠标左键抬起时，找到场景中的SceneItem对象并激活，这里有点局限了，因为在这个示例场景里面只有一个SceneItem对象就这样方便写了，但要真的实用的话肯定是需要编写数据类的，然后从中读取数据，再根据数据来激活对应的物体
            GameObject.FindObjectOfType<SceneItem>(true).gameObject.SetActive(true);
        }
    }

    protected override void OnExit()
    {
        table.tempItem.gameObject.SetActive(false);
        uiItem.icon.gameObject.SetActive(false);
    }
}
