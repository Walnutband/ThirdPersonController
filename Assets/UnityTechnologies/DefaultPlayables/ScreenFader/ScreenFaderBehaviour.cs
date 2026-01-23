using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

/*Tip：显然可以控制Image的更多属性，*/
[Serializable]
public class ScreenFaderBehaviour : PlayableBehaviour
{
    public Color color = Color.black;
}
