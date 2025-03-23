using System;
using UnityEngine;
using HalfDog.EasyInteractive;

/// <summary>
/// 放置物体的台面
/// </summary>
public class Table : MonoBehaviour, IFocusable //只能聚焦，也就是鼠标悬停其上时的描边提示
{
    public GameObject tempItem;
    public Type interactTag => typeof(Table);
    public bool enableFocus => true;
    private Outline _outline;
    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }
    public void OnFocus()
    {
        _outline.enabled = true;
    }
    public void EndFocus()
    {
        _outline.enabled = false;
    }

}
