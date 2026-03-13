using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：该类主要是用于初始化配置AnimatorAgent，因为有一些经常的设置比如有多少层级、层级遮罩、层级混合等等，如果没有这样专门的设置文件的话，就只有在类中逐个编写代码，
    但这其实完全可以改为在检视器中编辑，当然随后在代码中也可以修改，这是动画系统的灵活性，不过实际上基本就不会再改动了。*/
    [CreateAssetMenu(fileName = "AnimatorAgentSettings_SO", menuName = "ARPGDemo/MyPlugins/AnimationPlayer/AnimatorAgentSettings_SO", order = 0)]
    public class AnimatorAgentSettings : ScriptableObject
    {
        //层级数量
        public int layerCount;
        //层级遮罩
        public List<LayerInfo> layerInfos = new List<LayerInfo>();

        [Serializable]
        public struct LayerInfo
        {
            public AvatarMask mask;
            public bool additive;
        }

        public void AddLayer()
        {
            layerCount++;
            layerInfos.Add(default);
        }
        public void RemoveLayer()
        {
            layerCount--;
            layerInfos.RemoveAt(layerCount);
        }
    }
}