
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    //传入代表当前属性集有哪些属性的Enum类型。
    public abstract class AttributeSetBase<TAttribute> where TAttribute : Enum
    {

        //Ques：可以分配一个ID或Tag，这样可以将属性集也作为可配置，现在只是具体属性集中的属性可配置，不过这个作用似乎完全可以被“更恰当地划分属性集”而取代。
        /*
        //通过Tag查找到对应的属性集
        //Ques：派生属性集的类名信息是否应该利用呢？
        // [SerializeField] private GameplayTag m_Tag;
        // public GameplayTag tag
        // {
        //     get => m_Tag;
        // }
        */

        //Ques：似乎Acquirer与Modifier都应该定义在主体类，而不是那两个嵌套类的内部。
        [Serializable]
        private struct Acquirer
        {
            public TAttribute attr; //作为嵌套类，本来就掌握从外部类传入的泛型类型，所以这里直接写成泛型即可。
            public float magnification; //倍率
            //是否要获取基础值，因为默认就是当前值（经过各种Buff作用之后的值），而且由于是二分的，所以直接用bool即可，如果更多的话就需要使用Enum了。
            public bool getBaseValue; //获取基础值，也就是没有经过GE作用的原始值。
        }
        //Tip：意味着每一个属性集都会自带一个Acquirer（获取器）专门用于从属性集中的特定属性获取一定倍率的数值，作为自己的初始数值，这些都属于基础属性的一些作用映射。
        //虽然使用的数组，但这本来就是针对于单一数值需求的比如一段伤害的原始数值，所以绝大多数情况下就是一个攻击力
        [Serializable]
        //Tip：似乎应该是“操纵器”，因为不只是从属性集获取值，还有修改属性值也是通过该类型，因为确实掌握了属性集的信息，只要传入属性集实例就可以实现指定的（属性）操作。
        //Tip：并非，这种结构体是用于直接编辑的，编辑好相关数据，写好的内部方法就会直接使用这些编辑的数据进行操作，所以就应该专注于单一的特定操作，否则的话多个操作的数据会
        // public struct AttributeValueManipulator
        public class AttributeValueAcquirer //似乎应该使用class更好。
        {


            [SerializeField] private Acquirer[] acquirers;
            // private AttributeSetBase<TAttribute> m_AS;

            //享有访问权，传入即可获取，而其他外部类却没有这样的访问权。类型是类型，实例是实例，由此理解逻辑。
            /*Tip：这里是从属性集中获取指定属性值并且经过倍率计算返回最终值，其实就是各种技能从属性集中获取的初始值，所以似乎用“InitialValue”更合适。
            但是转念一想，这里并不应该感知到调用者的意图，应该只从逻辑来描述该方法，所以就是“从属性获取数值”。
            */
            public float GetValue(AttributeSetBase<TAttribute> _as)
            {
                if (_as == null)
                {
                    Debug.LogError("未分配属性集，无法获取属性值。");
                    return 0f;
                }
                //TODO：不一定是currentValue，比如基础攻击力，就应该访问originalValue。
                float result = 0f;
                foreach (var acquirer in this.acquirers)
                {//Ques：Convert.ToInt32是因为泛型约束只能到Enum，不能再进一步确定其值类型，而这个函数可以处理所有类型的Enum。
                    // result += (acquirer.getBaseValue == true ? _as.m_Attributes[Convert.ToInt32(acquirer.attr)].baseValue : 
                    // _as.m_Attributes[Convert.ToInt32(acquirer.attr)].currentValue) * acquirer.magnification;
                    //指定BaseValue或CurrentValue
                    // result += (acquirer.getBaseValue == true ? _as.GetAttributeBaseValue(acquirer.attr) : _as.GetAttributeCurrentValue(acquirer.attr)) * acquirer.magnification;
                    result += GetValueFromAttributes(_as, acquirer);
                }
                return result;
            }


        }

        private static float GetValueFromAttributes(AttributeSetBase<TAttribute> _as, Acquirer _acquirer)
        {
            return (_acquirer.getBaseValue == true ? _as.GetAttributeBaseValue(_acquirer.attr) : _as.GetAttributeCurrentValue(_acquirer.attr)) * _acquirer.magnification;
        }


        [Serializable]
        private struct Modifier
        {
            public TAttribute attr; //指定属性
            public ModifierType type;
            public float value;

        }
        //属性值修改器
        [Serializable]
        public struct AttributeValueModifier
        {
            //在结构体中的字段无法设置初始化，这是语法决定并非技术原因，不过这里确实也不需要，直接在方法中定义局部变量即可、哪里用到哪里就定义。
            // private Action<AttributeSetBase<TAttribute>, Modifier>[] modifierFunc = ;
            //每个修改器针对一个属性，一系列修改器针对
            [SerializeField] private Modifier[] m_Modifiers;

            public AttributeValueModifier(TAttribute _attr, ModifierType _type, float _value)
            {
                m_Modifiers = new Modifier[] { new Modifier { attr = _attr, type = _type, value = _value } };
            }

            //应用修改器。
            public void Apply(AttributeSetBase<TAttribute> _as)
            {
                //就和属性集中使用数组存储属性值是相同原理，类型相同、有枚举提供标识信息，排好序就可以使用标识信息直接获取实际对象，这里的对象就是该签名的函数。
                // Action<AttributeSetBase<TAttribute>, Modifier>[] modifierFunc = new Action<AttributeSetBase<TAttribute>, Modifier>[]
                // Action<TAttribute, float>[] modifierFunc = new Action<TAttribute, float>[]
                // { _as.AddByValue, _as.AddByPercentage, _as.SubByValue, _as.SubByPercentage, _as.MultiplyByValue, _as.MultiplyByPercentage };
                foreach (var modifier in m_Modifiers)
                {
                    //因为本来在外部就可以获取指定属性以及value，操作函数只需要属性和value即可执行自己的操作逻辑，就不应该把获取属性的逻辑塞进去了。
                    //Tip：后续发现，在操作属性值时至少需要触发值改变事件，还有其他回调方法，不应该在这里即属性集之外获取Attribute。
                    //通过ModifierType知道调用哪个操作函数。
                    // modifierFunc[(int)modifier.type](modifier.attr, modifier.value);
                    // _as.ChangeAttributeValue(modifier.type, modifier.attr, modifier.value);
                    ApplyModifier(_as, modifier); 
                }
            }

        }

        /*Tip：这算是一个“原子效果”了，从一个属性值获取数值、应用到另一个属性值。*/
        //属性值变化规则，其实这才是GE通常会使用的。
        [Serializable]
        public struct AttributeValueChangedRule
        {
            //值获取器和值修改器。值的来源以及应用到哪里。
            [SerializeField] private Acquirer m_ValueGetter;
            // [SerializeField] private Modifier valueSetter;
            [SerializeField] private TAttribute m_Attribute;
            [SerializeField] private ModifierType m_Operator; //通常都是AddByValue


            //传入要作用的属性集，这里分了来源和去处两个属性集，但其实通常就是同一个，本质上还是取决于设计需求。
            public void Apply(AttributeSetBase<TAttribute> _FromAS, AttributeSetBase<TAttribute> _ToAS) 
            {
                float value = GetValueFromAttributes(_FromAS, m_ValueGetter);
                ApplyModifier(_ToAS, m_Attribute, m_Operator, value);
            }
            public void Apply(AttributeSetBase<TAttribute> _as)
            {
                Apply(_as, _as);
            }
        }

        private static void ApplyModifier(AttributeSetBase<TAttribute> _as, Modifier _modifier)
        {
            _as.ChangeAttributeValue(_modifier.attr, _modifier.type, _modifier.value);
        }

        private static void ApplyModifier(AttributeSetBase<TAttribute> _as, TAttribute _attr, ModifierType _type, float _value)
        {
            _as.ChangeAttributeValue(_attr, _type, _value); 
        }

        //字段初始值无法引用非静态成员。
        // private Action<TAttribute, float>[] modifierFunc = new Action<TAttribute, float>[]
        // { AddByValue, AddByPercentage, SubByValue, SubByPercentage, MultiplyByValue, MultiplyByPercentage };

        //Tip：存储属性值，根据具体类型，容量就等于属性值类型的数量。注意这些属性集会被封装、然后经过编辑器处理再编辑，运行时加载。
        [SerializeField] protected GameplayAttribute[] m_Attributes;
        // protected GameplayAttribute[] m_Attributes = new GameplayAttribute[m_AttributeCount];
        // protected virtual int m_AttributeCount => 0;

        //传入原始数据，然后由属性集自己处理。
        public AttributeSetBase(float[] _attrs) 
        {
            m_Attributes = new GameplayAttribute[_attrs.Length];
            for (int i = 0; i < _attrs.Length; i++)
            {
                m_Attributes[i] = new GameplayAttribute(_attrs[i]);
            }
        }

        public float GetAttributeCurrentValue(TAttribute _attr)
        {
            return GetAttribute(_attr).currentValue;
        }
        public float GetAttributeBaseValue(TAttribute _attr)
        {
            return GetAttribute(_attr).baseValue;
        }
        protected virtual GameplayAttribute GetAttribute(TAttribute _attr)
        {
            return m_Attributes[Convert.ToInt32(_attr)];
        }

        // protected enum ModifierType : int //其实默认就是int（32位）
        public enum ModifierType : int //其实默认就是int（32位）
        {
            AddByValue,
            AddByPercentage,
            SubByValue,
            SubByPercentage,
            MultiplyByValue, //直接乘以值。不过这个应该通常不会用，而是使用百分比乘法。
            MultiplyByPercentage, //按照百分比相乘，也就是会将指定value进行百分数处理。
        }

        private void ChangeAttributeValue(TAttribute _attr, ModifierType _type, float _value)
        {
            Action<TAttribute, float>[] modifierFunc = new Action<TAttribute, float>[]
            {
                AddByValue, AddByPercentage, SubByValue, SubByPercentage, MultiplyByValue, MultiplyByPercentage
            };

            //变化前
            // OnBeforeAttributeValueChanged(_attr, _type, _value);
            //应用变化
            modifierFunc[(int)_type](_attr, _value);
            OnAfterAttributeValueChanged(_attr);
        }

        // protected virtual void OnBeforeAttributeValueChanged(TAttribute _attr, ModifierType _type, float _value)
        // {
            
        // }
        protected virtual void OnAfterAttributeValueChanged(TAttribute _attr)
        {

        }

        //Tip：操作属性值的几种计算方式（常用，其他的大概要另外写逻辑了）
        private void AddByValue(TAttribute _attr, float _value)
        {
            // GameplayAttribute ga = m_Attributes[Convert.ToInt32(_attr)];
            GameplayAttribute ga = GetAttribute(_attr);
            ga.currentValue += _value;
        }
        private void AddByPercentage(TAttribute _attr, float _value)
        {
            GameplayAttribute ga = GetAttribute(_attr);
            //Ques：存在疑惑，因为原神里面说“生命值提高20%”是基于基础生命值的20%，加到当前生命值，而不是把当前生命值与百分比相乘。其实这也是从基础生命值获取值，然后加到当前生命值。
            // ga.currentValue += ga.currentValue * _value / 100f;
            ga.currentValue += ga.baseValue * _value / 100f;
        }
        private void SubByValue(TAttribute _attr, float _value)
        {
            GameplayAttribute ga = GetAttribute(_attr);
            ga.currentValue -= _value;
        }
        private void SubByPercentage(TAttribute _attr, float _value)
        {
            GameplayAttribute ga = GetAttribute(_attr);
            // ga.currentValue -= ga.currentValue * _value / 100f;
            ga.currentValue -= ga.baseValue * _value / 100f;
        }
        private void MultiplyByValue(TAttribute _attr, float _value)
        {
            GameplayAttribute ga = GetAttribute(_attr);
            ga.currentValue *= _value;
        }
        private void MultiplyByPercentage(TAttribute _attr, float _value)
        {
            GameplayAttribute ga = GetAttribute(_attr);
            ga.currentValue *= _value / 100f;
        }

    }

    [Serializable]
    public class GameplayAttribute
    {//TODO：如果使用一个Enum表达BaseValue和CurrentValue，那么这里也可以改为数组而不是一个个的变量。
        // public float originalValue;
        public float baseValue; //似乎base更贴切，因为想到了“基础生命值”、“基础攻击力”
        public float currentValue;
        public GameplayAttribute(float _originalValue)
        {
            baseValue = _originalValue;
            currentValue = _originalValue;
        }
    }



}