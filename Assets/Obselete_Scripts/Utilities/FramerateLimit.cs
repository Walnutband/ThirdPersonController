using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateLimit : MonoBehaviour
{
    public enum LimitType
    {
        NoLimit = -1,
        Limit30 = 30,
        Limit60 = 60,
        Limit120 = 120
    }

    public LimitType framerate = LimitType.NoLimit; //注意该变量和LimitType的访问权限要保持相同

    private void Awake()
    {
        Application.targetFrameRate = (int)framerate;
    }
}
