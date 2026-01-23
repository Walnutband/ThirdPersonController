using UnityEngine;

namespace MyPlugins.AnimationPlayer.Utility
{
    [RequireComponent(typeof(Animator))]
    public class RedirectRootMotion : MonoBehaviour  //“Redirect”重定向，感觉这个词最贴切。
    {
        private Animator m_Animator;
        public Transform m_Target; //TODO：这是直接驱动Transform，实际如果是驱动角色移动的话，大概会使用一个专门的控制器组件比如内置的CharacterController。

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            if (m_Target == null) m_Target = transform.parent; //如果没有指定重定向目标，那么就是默认重定向给自己的父对象。
        }

        //周期方法，脚本处理RootMotion
        private void OnAnimatorMove()
        {
            Debug.Log("onAnimatorMove");
            Vector3 deltaPos = m_Animator.deltaPosition;
            m_Target.position += deltaPos;
        }
    }
    
}