using System;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEngine.InputSystem.Editor;
using UnityEngine.UIElements;
#endif

////REVIEW: rename to RadialDeadzone

////TODO: add different deadzone shapes and/or option to min/max X and Y separately

namespace UnityEngine.InputSystem.Processors
{
    /// <summary>
    /// Processes a Vector2 to apply deadzoning according to the magnitude of the vector (rather
    /// than just clamping individual axes). Normalizes to the min/max range.
    /// </summary>
    /// <seealso cref="AxisDeadzoneProcessor"/>
    public class StickDeadzoneProcessor : InputProcessor<Vector2>
    {
        /// <summary>
        /// Value at which the lower bound deadzone starts.
        /// </summary>
        /// <remarks>
        /// Values in the input at or below min will get dropped and values
        /// will be scaled to the range between min and max.
        /// </remarks>
        public float min;
        public float max;

        private float minOrDefault => min == default ? InputSystem.settings.defaultDeadzoneMin : min;
        private float maxOrDefault => max == default ? InputSystem.settings.defaultDeadzoneMax : max;

        public override Vector2 Process(Vector2 value, InputControl control = null)
        {
            var magnitude = value.magnitude;
            var newMagnitude = GetDeadZoneAdjustedValue(magnitude);
            if (newMagnitude == 0)
                value = Vector2.zero;
            else
                value *= newMagnitude / magnitude;
            //在超过max时newMagnitude就是1，可以理解为从圆心到所在位置的连线段，延伸出去与摇杆圆周的交点，这个位置的值就是调整后的value。说白了此时就是以(0,0)为起点
            //在min到max之间时，newMagnitude就是magnitude在min和max之内的部分，所以在min和max之内时就是以min位置作为起点而不是以(0,0)作为起点。
            //Tip：其实想象一下摇杆的圆周范围就很容易理解这里的逻辑了。如果是测试的话，我发现还很难推测出这里准确的逻辑，因为数值范围本来就很小。
            return value;
        }

        private float GetDeadZoneAdjustedValue(float value)
        {
            var min = minOrDefault;
            var max = maxOrDefault;

            var absValue = Mathf.Abs(value);
            if (absValue < min)
                return 0;
            if (absValue > max)
                return Mathf.Sign(value);

            return Mathf.Sign(value) * ((absValue - min) / (max - min));
        }

        public override string ToString()
        {
            return $"StickDeadzone(min={minOrDefault},max={maxOrDefault})";
        }
    }

    #if UNITY_EDITOR
    internal class StickDeadzoneProcessorEditor : InputParameterEditor<StickDeadzoneProcessor>
    {
        protected override void OnEnable()
        {
            m_MinSetting.Initialize("Min",
                "Vector length  below which input values will be clamped. After clamping, vector lengths will be renormalized to [0..1] between min and max.",
                "Default Deadzone Min",
                () => target.min, v => target.min = v,
                () => InputSystem.settings.defaultDeadzoneMin);
            m_MaxSetting.Initialize("Max",
                "Vector length above which input values will be clamped. After clamping, vector lengths will be renormalized to [0..1] between min and max.",
                "Default Deadzone Max",
                () => target.max, v => target.max = v,
                () => InputSystem.settings.defaultDeadzoneMax);
        }

        public override void OnGUI()
        {
#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
            if (!InputSystem.settings.IsFeatureEnabled(InputFeatureNames.kUseIMGUIEditorForAssets)) return;
#endif
            m_MinSetting.OnGUI();
            m_MaxSetting.OnGUI();
        }

#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
        public override void OnDrawVisualElements(VisualElement root, Action onChangedCallback)
        {
            m_MinSetting.OnDrawVisualElements(root, onChangedCallback);
            m_MaxSetting.OnDrawVisualElements(root, onChangedCallback);
        }

#endif

        private CustomOrDefaultSetting m_MinSetting;
        private CustomOrDefaultSetting m_MaxSetting;
    }
    #endif
}
