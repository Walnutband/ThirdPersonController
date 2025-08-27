using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    /*TODO：这个应该是动画系统自身自带的功能才对。*/
    public class RootMotionController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController target;
        public bool applyRootMotion;

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
            target ??= transform.parent.GetComponent<CharacterController>();
        }

        private void Reset()
        {
            animator ??= GetComponent<Animator>();
            target ??= transform.parent.GetComponent<CharacterController>();
        }

        private void OnAnimatorMove()
        {
            if (applyRootMotion)
            {
                // target.Move(animator.deltaPosition);
                target.Move(target.transform.TransformDirection(new Vector3(0f, 0f, 1f)) * animator.deltaPosition.magnitude);
                // animator.ApplyBuiltinRootMotion();
                // target.rotation *= animator.deltaRotation;
            }

        }

        public void ApplyRootMotion(bool apply)
        {
            Debug.Log("ApplyRootMotion");
            applyRootMotion = apply;
        }

        // private void Update()
        // {
        //     Debug.Log("RootMotion Update: " + Time.frameCount);
        // }

        // private void LateUpdate()
        // {
        //     Debug.Log("RootMotion LateUpdate" + Time.frameCount);
        // }

        // private void OnAnimatorMove()
        // {
        //     Debug.Log("RootMotion OnAnimatorMove" + Time.frameCount);
        // }
    }
}