
using System;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    [Serializable]
    public class ActorAttributeSet : AttributeSetBase<ActorAttributeSet.AttributeType>
    {

        /*Tip：Enum就是逻辑性的标签，GameplayTag就是数据性的标签。似乎在这种属性集中，由于属于底层固定规则，或许使用Enum才是更好的选择。*/
        //Ques：似乎Enum就已经完全确定了属性集中的所有属性信息，并且明确了先后顺序，可以直接作为下标在数组中访问？！因为各个属性本来就是同一类型，只是被用于的逻辑不同而造就了不同属性。
        public enum AttributeType
        {
            HPMax, //生命值上限
            HP, //当前生命值
            ATK, //攻击力
            DEF, //防御力
            SPMax, //体力值上限
            SP, //当前体力值
            CritRate, //暴击率
            CritDamage //暴击伤害
        }
        // public enum AttributeType
        // {
            
        //     生命值上限,
        //     当前生命值,
        //     攻击力,
        //     防御力,
        //     体力值上限,
        //     当前体力值,
        //     暴击率,
        //     暴击伤害
        // }
        protected override GameplayAttribute GetAttribute(AttributeType _attr)
        {//因为在派生类中就知道具体的枚举信息了。利用虚函数的性质就可以让基类间接利用到派生类的信息。
            return m_Attributes[(int)_attr];
        }

        //Tip：每个具体属性集都有自己的属性变化事件，

        public ActorAttributeSet(float[] _attr) : base(_attr) {}

        // protected override void OnBeforeAttributeValueChanged(AttributeType _attr, ModifierType _type, float _value)
        // {
        //     base.OnBeforeAttributeValueChanged(_attr, _type, _value);

        //     // m_Events.OnBeforeAttributeValueChanged?.Invoke(this, _attr);
        // }
        protected override void OnAfterAttributeValueChanged(AttributeType _attr)
        {
            base.OnAfterAttributeValueChanged(_attr); 

            // m_Events.OnAfterAttributeValueChanged?.Invoke(this, _attr);
            //触发生命值变化事件。
            if (_attr == AttributeType.HP)
            {
                GameplayAttribute ga = GetAttribute(_attr);
                // ga.currentValue = Mathf.Max(0f, ga.currentValue); 
                ga.currentValue = Mathf.Clamp(ga.currentValue, 0f, GetAttribute(AttributeType.HPMax).currentValue);
                OnHPChanged?.Invoke(GetAttributeCurrentValue(_attr));
                Debug.Log("触发HP变化事件");
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

        public float GetHPMax()
        {
            return GetAttributeCurrentValue(AttributeType.HPMax);
        }
        public float GetHPCurrent()
        {
            return GetAttributeCurrentValue(AttributeType.HP);
        }

        // public class Events
        // {
        //     //属性集就是事件所处环境的信息，属性类型就是事件本身的信息。
        //     // public Action<ActorAttributeSet, AttributeType> OnBeforeAttributeValueChanged;
        //     // public Action<ActorAttributeSet, AttributeType> OnAfterAttributeValueChanged;
        // }

        // private Events m_Events = new Events();
    }
}