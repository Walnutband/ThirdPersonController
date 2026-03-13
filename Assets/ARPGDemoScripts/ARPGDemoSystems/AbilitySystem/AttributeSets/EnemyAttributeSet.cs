
using System;

namespace ARPGDemo.AbilitySystem
{
    public class EnemyAttributeSet : AttributeSetBase<EnemyAttributeSet.AttributeType>
    {
        public EnemyAttributeSet(float[] _attrs) : base(_attrs)
        {
        }

        public enum AttributeType
        {
            HPMax,
            HP,
            ATK,

        }

        protected override void OnAfterAttributeValueChanged(AttributeType _attr)
        {
            base.OnAfterAttributeValueChanged(_attr);

            // m_Events.OnAfterAttributeValueChanged?.Invoke(this, _attr);
            //触发生命值变化事件。
            if (_attr == AttributeType.HP)
            {
                OnHPChanged?.Invoke(GetAttributeCurrentValue(_attr));
            }
        }

        private event Action<float> OnHPChanged;
        public void RegisterHPChangedEvent(Action<float> _action)
        {
            OnHPChanged += _action;
        }
        public void UnregisterHPChangedEvent(Action<float> _action)
        {
            OnHPChanged -= _action;
        }
    }
}