using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem_Old
{
    public class InputSystemTest : MonoBehaviour
    {
        // public InputActionAsset inputActions;
        // public InputActionReference inputActionRef;
        public InputActionReference input;
        public bool changeAssetState;
        public bool changeActionState;

        /*BugFix:之前就算调用了Enable都还是一直无响应，然后发现是我把顺序搞错了，因为Enable方法是在Start之前被调用，而我的inputAction是在Start方法中获取的，但回调方法却是在
        OnEnable方法中注册的，所以注册时压根就不是我想要的那个inputAction，所以放到Awake方法中获取InputAction就可以了。
        这其实也体现出了Awake的作用，Awake是在OnEnable启用之前的初始化，Start是在OnEnable启用之后的初始化，或者说Awake是与启用状态无关的初始化，Start是与启用状态相关的初始化，
        所以Awake只会调用一次，而Start在每次启用时都会调用。*/
        private void Awake()
        {
            // inputAction = inputActionRef.action;
        }

        private void OnEnable()
        {
            input.action.Enable();
            input.action.started += Started;
            input.action.performed += Performed;
            input.action.canceled += Canceled;
        }

        public void Started(InputAction.CallbackContext context)
        {
            Debug.Log($"在{Time.frameCount}，Started，交互：{context.interaction}");
        }

        public void Performed(InputAction.CallbackContext context)
        {
            Debug.Log($"在{Time.frameCount}，Performed，交互：{context.interaction}");
        }

        public void Canceled(InputAction.CallbackContext context)
        {
            Debug.Log($"在{Time.frameCount}，Canceled，交互：{context.interaction}");
        }

        private void Start()
        {
            // inputAction = inputActionRef.action;
            //一定要启用，才会开始响应。
            //Tip：这样看来，应该是Unity底层的生命周期中对InputSystem进行了监测（其实手册上也说了是轮询），但是在C#层确实没有暴露相关细节，只能学会用法，无法了解原理，不过对于输入系统这个模块来说，也就足够了
            // inputActionRef.asset.Enable();
            // inputAction.actionMap.Enable();



        }

        private void FixedUpdate()
        {
            // Debug.Log($"在{Time.frameCount}帧触发了FixedUpdate");
        }

        private void Update()
        {
            // Debug.Log($"在{Time.frameCount}帧触发了Update");
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("触发");
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("碰撞Enter");
        }

        private void OnCollisionStay(Collision collision)
        {
            Debug.Log("碰撞Stay");
        }

        private void OnCollisionExit(Collision collision)
        {
            Debug.Log("碰撞Exit");
        }

        // private void OnEnable()
        // {
        //     inputAction.Enable();
        //     inputAction.started += DebugIfInput;
        // }

        // private void DebugIfInput(InputAction.CallbackContext ctx)
        // {
        //     Debug.Log($"第{Time.frameCount}帧按下了空格键");
        // }

        // private void OnEnable()
        // {
        //     inputActions.Enable();
        //     inputAction.started += DebugStarted;
        //     inputAction.performed += DebugPerformed;
        //     inputAction.canceled += DebugCanceled;

        // }

        // private void OnDisable()
        // {
        //     inputActions.Disable();
        //     inputAction.started -= DebugStarted;
        //     inputAction.performed -= DebugPerformed;
        //     inputAction.canceled -= DebugCanceled;

        // }

        // private void DebugStarted(InputAction.CallbackContext ctx)
        // {
        //     Debug.Log($"第{Time.frameCount}帧触发了Started");
        // }

        // private void DebugPerformed(InputAction.CallbackContext ctx)
        // {
        //     Debug.Log($"第{Time.frameCount}帧触发了Performed");
        // }

        // private void DebugCanceled(InputAction.CallbackContext ctx)
        // {
        //     Debug.Log($"第{Time.frameCount}帧触发了Canceled");
        // }
    } 
}