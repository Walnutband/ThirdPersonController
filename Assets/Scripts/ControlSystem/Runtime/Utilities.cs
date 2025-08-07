using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    [Serializable]
    public class AnimationList
    {
        public List<AnimationClip> animationList;
    }
    
    

    //TODO: 也不一定就是放在角色控制器中，可能会放在管理器之类的位置，这其实由具体的游戏玩法和机制决定，不存在一定的情况。
    /// <summary>
    /// 用于CinemachineFreeLook相机的缩放功能。
    /// </summary>
    /// <remarks>
    /// 在设计上，就是使用FreeLook相机的角色控制器自己会拥有一个该结构体实例，而不是作为通用的工具类型，这样一来就可以根据各个控制器进行某些<b>数值上的定制化</b>，
    /// 而不需要共用，当然理论上来说设置为工具类型也可以实现定制化，但是在程序上肯定没那么方便，而且本来就没必要那样强行。
    /// <para/>由于基本原理是记录最开始的数值，然后进行<b>等比例缩放</b>，如果是在测试的话，肯定会想要改变作为基准的“最开始的数值”，那么只要重新生成一个FreeLookZoomer实例、
    /// 再次传入FreeLook相机即可实现更换
    /// </remarks>
    public struct FreeLookZoomer
    {
        private CinemachineFreeLook freelook; //缩放的目标。
        private Vector2 topRig;
        private Vector2 middleRig;
        private Vector2 bottomRig;
        private float minZoom, maxZoom;
        private float zoomCounter;

        public FreeLookZoomer(CinemachineFreeLook _freelook) : this(_freelook, 0.5f, 2.0f)
        {
            // FreeLookZoomer(_freelook, 0.5f, 2.0f);
        }
        //Tip：突然想到重载相对于设置参数默认值的好处，就是要求必须传入这些参数，而不是像默认值那样可以选择性传入，显然是否要使用这种强制性就要看具体的逻辑了。
        public FreeLookZoomer(CinemachineFreeLook _freelook, float _minZoom, float _maxZoom)
        {
            freelook = _freelook;
            topRig = new Vector2(_freelook.m_Orbits[0].m_Height, _freelook.m_Orbits[0].m_Radius);
            middleRig = new Vector2(_freelook.m_Orbits[1].m_Height, _freelook.m_Orbits[1].m_Radius);
            bottomRig = new Vector2(_freelook.m_Orbits[2].m_Height, _freelook.m_Orbits[2].m_Radius);
            // minZoom = _minZoom; maxZoom = _maxZoom;
            minZoom = Mathf.Clamp(_minZoom, 0f, 1f); //限制在0~1合情合理。
            maxZoom = Mathf.Max(_maxZoom, 1f); //因为逻辑上来说最大应该是没有限制的，只要比初始数值大就行了。
            zoomCounter = 1f;
        }

        public void Zoom(float delta)
        {
            //zoomCounter记录了当前相对于初始值的倍数，并且由于记录了初始值，所以能够直接使用初始值乘以zoomCounter来得到当前应该的数值，这样就可以直接复用以下的SetRig来设置数值。。
            zoomCounter += delta;
            if (zoomCounter <= minZoom)
            {//其实也可以选择超出边界时就不设置数值，可以减少一些逻辑，而且实际效果也感觉不到区别。
                zoomCounter = minZoom;
                SetRig();
            }
            else if (zoomCounter >= maxZoom)
            {
                zoomCounter = maxZoom;
                SetRig();
            }
            else
            { //Ques：等比例缩放，效果很好。但是有一说一，我真没想通为何效果这么好，因为商业游戏就是这样的效果。
                SetRig();
            }
        }

        public void Reset()
        {
            zoomCounter = 1f;
            SetRig();
        }

        private void SetRig()
        {//这就是以绝对数值的方式来计算。
            freelook.m_Orbits[0].m_Height = topRig[0] * zoomCounter;
            freelook.m_Orbits[0].m_Radius = topRig[1] * zoomCounter;
            freelook.m_Orbits[1].m_Height = middleRig[0] * zoomCounter;
            freelook.m_Orbits[1].m_Radius = middleRig[1] * zoomCounter;
            freelook.m_Orbits[2].m_Height = bottomRig[0] * zoomCounter;
            freelook.m_Orbits[2].m_Radius = bottomRig[1] * zoomCounter;
        }

    }
}