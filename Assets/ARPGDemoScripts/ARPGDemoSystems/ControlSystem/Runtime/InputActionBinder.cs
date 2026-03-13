using System;
using ARPGDemo.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace ARPGDemo.ControlSystem.InputActionBindings
{

    /*Tip：假设不用泛型，那么bindedEvent就是直接定义为IInputActionBinderEvent，这样就需要从外部传入实例或者继承。不过确实都能够实现，泛型的重点在于，可以不用定义新类型，
    因为只需要改变bindedEvent的类型，所以作为泛型参数传入，可以直接将具体类型信息传入编译器。*/

    /// <summary>
    /// 可序列化的输入动作绑定器，允许在检视面板中选择事件类型和交互条件。
    /// 通过 Enable/Disable 控制绑定的生命周期，Execute 方法会在满足条件时调用。
    /// </summary>
    [Serializable] //TODO：似乎可以把这个设置为结构体，因为外部会访问的就是这里的TEvent，而TEvent如果是引用类型的话，就不会被结构体副本影响了。
    public class InputActionBinder<TEvent> where TEvent : IInputActionBinderEvent //确定这个类型，编译器从这个类型知道有Execute这个方法。
    {
        // [DisplayName("<b>能力类型</b>")]
        // [SerializeField] private AbilityType abilityType;

        //Tip：注意这里TEvent可能为空。将其序列化，就可以保证在反序列化时会分配一个来自于序列化文件的实例。另外也可以规定必须有new()无参构造函数。
        [ExpandInlineProperties("<b>触发事件数据</b>")]
        [SerializeField] private TEvent m_BindedEvent; //Binder只负责调用，逻辑和数据都由该实例自己决定。
        public TEvent bindedEvent => m_BindedEvent;

        [DisplayName("<b>绑定输入</b>")]
        [SerializeField] private InputActionReference actionReference;

        [DisplayName("<b>绑定输入事件</b>")]
        [SerializeField] private BindingEventType bindingEvent = BindingEventType.Performed;
        [DisplayName("<b>交互条件</b>")]
        [SerializeField] private InputInteraction requiredInteraction;

        // private Action<AbilityType> onExecute;
        // private Action<AbilityType> onExecute;

        // 内部保存当前是否已启用
        private bool isEnabled = false;

        /// <summary>
        /// 启用绑定：将 Execute 方法注册到指定事件上（如果尚未注册）
        /// </summary>
        public void Enable()
        {
            if (isEnabled) return;
            if (actionReference == null || actionReference.action == null)
            {
                Debug.LogWarning("ConditionalInputActionBinder.Enable: 无效的 InputActionReference");
                return;
            }

            var action = actionReference.action;

            //如果没有
            if (action == null)
            {
                Debug.Log("注意，该Binder没有指定InputAction，无法响应输入");
                return;
            }

            // 根据选择的事件类型注册 Execute
            switch (bindingEvent)
            {
                case BindingEventType.Started:
                    action.started += OnActionTriggered;
                    break;
                case BindingEventType.Performed:
                    action.performed += OnActionTriggered;
                    break;
                case BindingEventType.Canceled:
                    action.canceled += OnActionTriggered;
                    break;
                default:
                    Debug.LogError($"未处理的事件类型: {bindingEvent}");
                    return;
            }

            isEnabled = true;
        }

        /// <summary>
        /// 禁用绑定：从对应事件上移除 Execute 注册
        /// </summary>
        public void Disable()
        {
            if (!isEnabled) return;
            if (actionReference == null || actionReference.action == null)
            {
                // 资源可能已经被销毁，忽略警告
                isEnabled = false;
                return;
            }

            var action = actionReference.action;

            switch (bindingEvent)
            {
                case BindingEventType.Started:
                    action.started -= OnActionTriggered;
                    break;
                case BindingEventType.Performed:
                    action.performed -= OnActionTriggered;
                    break;
                case BindingEventType.Canceled:
                    action.canceled -= OnActionTriggered;
                    break;
            }

            isEnabled = false;
        }

        /// <summary>
        /// 内部回调：检查交互条件后触发事件
        /// </summary>
        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            // Debug.Log("OnActionTriggered");
            // 如果设置了交互条件，验证是否匹配
            if (requiredInteraction != InputInteraction.None)
            {
                var interaction = context.interaction;
                //只能进行类型转换，因为并没有接口方法提供有关名称的信息
                switch (requiredInteraction)
                {//经过测试，如果此时的interaction并非所要求的类型，就直接return，不会触发onExecute。
                    case InputInteraction.Tap:
                        if (!(interaction is TapInteraction)) return;
                        break;
                    case InputInteraction.SlowTap:
                        if (!(interaction is SlowTapInteraction)) return;
                        break;
                    case InputInteraction.MultiTap:
                        if (!(interaction is MultiTapInteraction)) return;
                        break;
                    case InputInteraction.Hold:
                        if (!(interaction is HoldInteraction)) return;
                        break;
                    case InputInteraction.Press:
                        if (!(interaction is PressInteraction)) return;
                        break;
                }
            }

            // 条件满足，触发事件
            // onExecute?.Invoke(abilityType);
            m_BindedEvent?.Execute();
            // Debug.Log("输入触发绑定事件");

        }

    }

    /// <summary>
    /// 事件类型枚举
    /// </summary>
    public enum BindingEventType
    {
        Started,
        Performed,
        Canceled
    }

    public enum InputInteraction
    {
        None,
        Tap,
        SlowTap,
        MultiTap,
        Hold,
        Press
    }

    //可以由InputActionBinder绑定，触发这里的事件，而方法要用到的
    public interface IInputActionBinderEvent
    {
        void Execute();
    }
}