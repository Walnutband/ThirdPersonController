using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    [CustomPropertyDrawer(typeof(Temperature))]
    public class TemperatureDrawer : PropertyDrawer //自定义绘制属性的检视面板
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Debug.Log("CreatePropertyGUI");
            var asset = Resources.Load<VisualTreeAsset>("temperature_drawer");
            var drawer = asset.Instantiate(property.propertyPath);
            //Nice display name友好显示名称，就是Inspector处理之后的显示名称，因为Inspector显示属性名往往不会完全按照变量原本的名字显示
            drawer.Q<Label>().text = property.displayName;

            // Do not allow conversion when having multiple objects selected in the Inspector
            if (!property.serializedObject.isEditingMultipleObjects)
            { //需要用到SerializedProperty类型的值，所以作为第二个泛型参数
                drawer.Q<Button>().RegisterCallback<ClickEvent, SerializedProperty>(Convert, property);
            }

            return drawer;
        }

        static void Convert(ClickEvent evt, SerializedProperty property)
        {
            var valueProperty = property.FindPropertyRelative("value");
            var unitProperty = property.FindPropertyRelative("unit");

            // F -> C华氏温度到摄氏温度
            if (unitProperty.enumValueIndex == (int)TemperatureUnit.Farenheit)
            {
                valueProperty.doubleValue -= 32;
                valueProperty.doubleValue *= 5.0d / 9.0d;
                unitProperty.enumValueIndex = (int)TemperatureUnit.Celsius;
            }
            else // C -> F摄氏温度到华氏温度
            {
                valueProperty.doubleValue *= 9.0d / 5.0d;
                valueProperty.doubleValue += 32;
                unitProperty.enumValueIndex = (int)TemperatureUnit.Farenheit;
            }

            // Important: because we are bypassing the binding system here, we must save the modified SerializedObject
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}