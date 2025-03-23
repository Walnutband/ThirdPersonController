using HalfDog.EasyInteractive;
using System;
using UnityEngine;

/// <summary>
/// 场景对象
/// </summary>
public class SceneItem : MonoBehaviour, IDragable
{
    public Sprite iconSprite; //该场景物体的UI图标
    public Type interactTag => typeof(SceneItem);
    public bool enableDrag => true;
    public bool enableFocus => true;
    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    //聚焦与不聚焦，以有无外轮廓来区分
    public void OnFocus()
    {
        _outline.enabled = true;
    }
    public void EndFocus()
    {
        _outline.enabled = false;
    }
    public void OnDrag()
    {
        GhostIcon.Instance.ShowGhostIcon(iconSprite);
        gameObject.SetActive(false); //拖拽时隐藏自身，显示鼠标上的图标
    }
    public void ProcessDrag()
    {
    }
    public void EndDrag(IFocusable target)
    {
        GhostIcon.Instance.HideGhostIcon();
        //判断是否拖拽到了目标 且当前对象和目标对象是否属于同一个交互对象
        if (target == null || !this.IsBelongToSameCase(target))
            gameObject.SetActive(true);
    }
}
