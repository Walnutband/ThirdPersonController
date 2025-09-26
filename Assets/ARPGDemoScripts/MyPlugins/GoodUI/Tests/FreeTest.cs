using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeTest : MonoBehaviour
{

    [ContextMenu("改变Cull状态")]
    private void ChangeCull()
    {
        transform.GetComponent<CanvasRenderer>().cull = !transform.GetComponent<CanvasRenderer>().cull;
    }

}