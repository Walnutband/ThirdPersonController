using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CanvasRebuildTest : MonoBehaviour
{
    IList<ICanvasElement> layoutRebuildQueue;
    IList<ICanvasElement> graphicRebuildQueue;

    private void Start()
    {
        //利用反射获取私有变量
        Type type = typeof(CanvasUpdateRegistry);
        FieldInfo fieldInfo = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        layoutRebuildQueue = (IList<ICanvasElement>)fieldInfo.GetValue(CanvasUpdateRegistry.instance); //从实例获取

        fieldInfo = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        graphicRebuildQueue = (IList<ICanvasElement>)fieldInfo.GetValue(CanvasUpdateRegistry.instance);
    }

    private void Update()
    {
        for (int i = 0; i < layoutRebuildQueue.Count; i++)
        {
            var rebuild = layoutRebuildQueue[i];
            if (ObjectValidForUpdate(rebuild))
            {
                Debug.LogFormat("{0}引起{1}网格重建", rebuild.transform.name, rebuild.transform.GetComponent<Graphic>().canvas.name);
            }
        }

        for (int i = 0; i < graphicRebuildQueue.Count; i++)
        {
            var rebuild = graphicRebuildQueue[i];
            if (ObjectValidForUpdate(rebuild))
            {
                Debug.LogFormat("{0}引起{1}网格重建", rebuild.transform.name, rebuild.transform.GetComponent<Graphic>().canvas.name);
            }
        }
    }

    private bool ObjectValidForUpdate(ICanvasElement element)
    {
        var valid = element != null;
        var isUnityObject = element is UnityEngine.Object;
        if (isUnityObject)
        {
            valid = (element as object) != null;
        }
        return valid;
    }
}