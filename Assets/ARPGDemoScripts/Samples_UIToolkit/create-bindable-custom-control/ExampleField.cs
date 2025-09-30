using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    // ExampleField inherits from BaseField with the double type. ExampleField's underlying value, then, is a double.
    //ExampleField继承自具有double类型的BaseField。因此，ExampleField的底层值是一个double类型的值。
    public class ExampleField : BaseField<double> //继承自泛型类BaseField，左侧标签，右侧输入
    {
        // We can provide the existing BaseFieldTraits class as a type parameter for UxmlFactory, and this means we
        // don't need to define our own traits class or override its Init() method. We do, however, need to provide it
        // with the type of the underlying value (double) and the corresponding attribute description type
        // (UxmlDoubleAttributeDescription).
        public new class UxmlFactory :
            UxmlFactory<ExampleField, BaseFieldTraits<double, UxmlDoubleAttributeDescription>> { }

        Label m_Input;

        // Default constructor is required for compatibility with UXML factory
        public ExampleField() : this(null)
        {

        }

        // Main constructor accepts label parameter to mimic BaseField constructor.
        // Second argument to base constructor is the input element, the one that displays the value this field is
        // bound to.
        public ExampleField(string label) : base(label, new Label() { })
        {
            // This is the input element instantiated for the base constructor.
            m_Input = this.Q<Label>(className: inputUssClassName);
        }

        // SetValueWithoutNotify needs to be overridden by calling the base version and then making a change to the
        // underlying value be reflected in the input element.
        public override void SetValueWithoutNotify(double newValue)
        {
            base.SetValueWithoutNotify(newValue); //首先调用基类方法，更新底层值

            m_Input.text = value.ToString("N");
        }
    }
}