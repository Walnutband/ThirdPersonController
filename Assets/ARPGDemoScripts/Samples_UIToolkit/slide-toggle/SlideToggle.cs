using UnityEngine;
using UnityEngine.UIElements;

namespace MyUILibrary
{
    // Derives from BaseField<bool> base class. Represents a container for its input part.
    //BaseField继承自BindableElement，而后者就继承自VisualElement
    public class SlideToggle : BaseField<bool> //代表输入值类型，并且字段类型，默认水平布局，左边标签，右边输入框
    {
        public new class UxmlFactory : UxmlFactory<SlideToggle, UxmlTraits> { } //这里的UxmlTraits指的是下面定义的。似乎先后顺序是无关的，
        //BaseFieldTraits是一个基类，专门用来初始化BaseField的Traits。一个是基本数据类型bool，一个是UXML的为布尔值的属性类型UxmlBoolAttributeDescription
        //使用该Traits只需要传入泛型参数即可，通过查看其源码可以看到代替做了什么工作
        public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription> { } 

        // In the spirit of the BEM standard, the SlideToggle has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly new string ussClassName = "slide-toggle"; //static readonly其实和const作用一样，不知道有何区别
        public static readonly new string inputUssClassName = "slide-toggle__input"; //toggle框
        public static readonly string inputKnobUssClassName = "slide-toggle__input-knob"; //把手，就是toggle框中的小圆钮
        public static readonly string inputCheckedUssClassName = "slide-toggle__input--checked"; //代表开关打开时

        VisualElement m_Input;
        VisualElement m_Knob;

        /* Custom controls need a default constructor. This default constructor calls the other constructor in this
         class.注意这里的this(null)指的是调用该类中定义的只有一个参数的构造方法，也就是下面的SlideToggle(string label)，
         因为是使用的UXML文件来创建实例，而不是通过程序，所以会默认调用无参数的构造函数，所以在此提供SlideToggle()，然后
         在其初始化列表中调用自定义的构造方法，也就是this(null)的作用，而且是先执行this(null)，再执行其内部的程序。*/
        public SlideToggle() : this(null) { }

        // This constructor allows users to set the contents of the label.
        public SlideToggle(string label) : base(label, new())
        {
            // Style the control overall.
            AddToClassList(ussClassName);

            // Get the BaseField's visual input element and use it as the background of the slide.
            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);

            // Create a "knob" child element for the background to represent the actual slide of the toggle.
            m_Knob = new();
            m_Knob.AddToClassList(inputKnobUssClassName);
            m_Input.Add(m_Knob);

            // There are three main ways to activate or deactivate the SlideToggle. All three event handlers use the
            // static function pattern described in the Custom control best practices.
            //只有被聚焦的元素才能接收键盘事件
            // ClickEvent fires when a sequence of pointer down and pointer up actions occurs.
            RegisterCallback<ClickEvent>(evt => OnClick(evt));
            // KeydownEvent fires when the field has focus and a user presses a key.
            RegisterCallback<KeyDownEvent>(evt => OnKeydownEvent(evt));
            // NavigationSubmitEvent detects input from keyboards, gamepads, or other devices at runtime.
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt)); //Enter键就是submit button
        }

        static void OnClick(ClickEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation(); //停止事件传播
        }

        static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;

            // NavigationSubmitEvent event already covers keydown events at runtime, so this method shouldn't handle
            // them.
            if (slideToggle.panel?.contextType == ContextType.Player) //运行时
                return;

            // Toggle the value only when the user presses Enter, Return, or Space.
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
            }
        }

        // All three callbacks call this method.
        void ToggleValue()
        {
            value = !value; //开关，就是取反即可
        }

        // Because ToggleValue() sets the value property, the BaseField class fires a ChangeEvent. This results in a
        // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // enabled.
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);

            //This line of code styles the input element to look enabled or disabled.
            m_Input.EnableInClassList(inputCheckedUssClassName, newValue); //true就添加，false就移除
        }
    }
}
