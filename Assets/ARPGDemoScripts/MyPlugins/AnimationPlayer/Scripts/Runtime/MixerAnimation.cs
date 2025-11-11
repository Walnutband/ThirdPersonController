using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyPlugins.AnimationPlayer
{
    [Serializable]
    public class MixerAnimation
    {
        [Serializable]
        public class Motion
        {
            [SerializeField] private AnimationClip m_Clip;
            public AnimationClip clip => m_Clip;
            /*TODO：或许会支持更多数据类型的阈值罢。*/
            [SerializeField] private float m_Threshold;
            public float threshold => m_Threshold;
        }

        public int key
        {
            get
            {//将片段的实例ID加起来除以片段个数作为Key
                int key = 0;
                foreach (var item in m_Motions)
                {
                    key += item.clip.GetInstanceID() / m_Motions.Count;
                }
                // Debug.Log($"MixerAnimation的Key：{key}");
                return key;
            }    
        }

        [SerializeField] private float m_FadeDuration;
        public float fadeDuration => m_FadeDuration;

        /*Tip：要求只要分配了列表元素就必须设置好片段和阈值，并且在编辑时就保证按照从小到大的顺序排列。*/
        [SerializeField] private List<Motion> m_Motions;
        public List<Motion> motions => m_Motions;
    }

}