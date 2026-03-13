using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Editor = UnityEditor.Editor;

namespace ARPGDemo.AbilitySystem
{
    //TODO：由于属性集的职责都是统一的，只是各自拥有的属性不同而已，所以在这个用于编辑的资产类其实可以适当调整来用于编辑所有属性集，
    [CreateAssetMenu(fileName = "ActorAttributeSet", menuName = "ARPGDemo/AbilitySystem/ActorAttributeSet")]
    public class ActorAttributeSet_SO : ScriptableObject
    {
        // [SerializeField] private ActorAttributeSet m_AS;
        //只需要编辑属性集中含有的各个属性。
        //TODO：因为属性都是统一按照float来计数，只是在使用时会进行一些区分，暂时就直接使用float[]了。
        [SerializeField] private float[] m_Attributes;

        public ActorAttributeSet GetAttributeSet()
        {
            return new ActorAttributeSet(m_Attributes);
        }
    }

#if UNITY_EDITOR
    public class ActorASEditor : UnityEditor.Editor    
    {
        
    }
#endif
}