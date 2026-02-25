
using System;
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

        //Tip：意味着每一个属性集都会自带一个Acquirer（获取器）专门用于从属性集中的特定属性获取一定倍率的数值，作为自己的初始数值，这些都属于基础属性的一些作用映射。
        //虽然使用的数组，但这本来就是针对于单一数值需求的比如一段伤害的原始数值，所以绝大多数情况下就是一个攻击力
        [Serializable]
        //Tip：似乎应该是“操纵器”，因为不只是从属性集获取值，还有修改属性值也是通过该类型，因为确实掌握了属性集的信息，只要传入属性集实例就可以实现指定的（属性）操作。
        //Tip：并非，这种结构体是用于直接编辑的，编辑好相关数据，写好的内部方法就会直接使用这些编辑的数据进行操作，所以就应该专注于单一的特定操作，否则的话多个操作的数据会
        // public struct AttributeValueManipulator
        public class AttributeValueAcquirer //似乎应该使用class更好。
        {
            [Serializable]
            private struct Acquirer
            {
                public TAttribute attr; //作为嵌套类，本来就掌握从外部类传入的泛型类型，所以这里直接写成泛型即可。
                public float magnification; //倍率
                //是否要获取基础值，因为默认就是当前值（经过各种Buff作用之后的值），而且由于是二分的，所以直接用bool即可，如果更多的话就需要使用Enum了。
                public bool getBaseValue; 
            }

            [SerializeField] private Acquirer[] acquirers;
            // private AttributeSetBase<TAttribute> m_AS;

            //享有访问权，传入即可获取，而其他外部类却没有这样的访问权。类型是类型，实例是实例，由此理解逻辑。
            /*Tip：这里是从属性集中获取指定属性值并且经过倍率计算返回最终值，其实就是各种技能从属性集中获取的初始值，所以似乎用“InitialValue”更合适。
            但是转念一想，这里并不应该感知到调用者的意图，应该只从逻辑来描述该方法，所以就是“从属性获取数值”。
            */
            public float GetValueFromAttributes(AttributeSetBase<TAttribute> _as)
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
                    // result += _as.m_Attributes[Convert.ToInt32(value.attr)].currentValue * value.magnification;
                    //指定BaseValue或CurrentValue
                    result += (acquirer.getBaseValue == true ? _as.m_Attributes[Convert.ToInt32(acquirer.attr)].baseValue : 
                    _as.m_Attributes[Convert.ToInt32(acquirer.attr)].currentValue) * acquirer.magnification;
                }
                return result;
            }
        }

        //属性值修改器
        [Serializable]
        public struct AttributeValueModifier
        {
            [Serializable]
            private struct Modifier
            {
                public TAttribute attr; //指定属性
                public ModifierType type;
                public float value;

            }

            private enum ModifierType : int //其实默认就是int（32位）
            {
                AddByValue,
                AddByPercentage,
                SubByValue,
                SubByPercentage,
                MultiplayByValue, //直接乘以值。不过这个应该通常不会用，而是使用百分比乘法。
                MultiplyByPercentage, //按照百分比相乘，也就是会将指定value进行百分数处理。
            }

            // private Action<AttributeSetBase<TAttribute>, Modifier>[] modifierFunc = ;
            [SerializeField] private Modifier[] m_Modifiers;

            //应用修改器。
            public void ApplyModifier(AttributeSetBase<TAttribute> _as)
            {
                //就和属性集中使用数组存储属性值是相同原理，类型相同、有枚举提供标识信息，排好序就可以使用标识信息直接获取实际对象，这里的对象就是该签名的函数。
                // Action<AttributeSetBase<TAttribute>, Modifier>[] modifierFunc = new Action<AttributeSetBase<TAttribute>, Modifier>[]
                Action<GameplayAttribute, float>[] modifierFunc = new Action<GameplayAttribute, float>[]
                { AddByValue, AddByPercentage, SubByValue, SubByPercentage, MultiplyByValue, MultiplyByPercentage };
                foreach (var modifier in m_Modifiers)
                {
                    //因为本来在外部就可以获取指定属性以及value，操作函数只需要属性和value即可执行自己的操作逻辑，就不应该把获取属性的逻辑塞进去了。
                    GameplayAttribute ga = _as.m_Attributes[Convert.ToInt32(modifier.attr)];
                    //通过ModifierType知道调用哪个操作函数。
                    // modifierFunc[(int)modifier.type](_as, modifier);
                    modifierFunc[(int)modifier.type](ga, modifier.value);
                }
            }

            private void AddByValue(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue += _value;
            }
            private void AddByPercentage(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue += _ga.currentValue * _value / 100f;
            }
            private void SubByValue(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue -= _value;
            }
            private void SubByPercentage(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue -= _ga.currentValue * _value / 100f;
            }
            private void MultiplyByValue(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue *= _value;
            }
            private void MultiplyByPercentage(GameplayAttribute _ga, float _value)
            {
                _ga.currentValue *= _value / 100f;
            }
        }


        protected GameplayAttribute[] m_Attributes;
        public float GetAttributeCurrentValue(TAttribute _attr)
        {
            int attr = Convert.ToInt32(_attr);
            if (m_Attributes.Count() <= attr)
            {
                Debug.LogError("在获取属性集时");
                return 0f;
            }
            return m_Attributes[attr].currentValue;
        }
        public float GetAttributeBaseValue(TAttribute _attr)
        {
            int attr = Convert.ToInt32(_attr);
            if (m_Attributes.Count() <= attr)
            {
                Debug.LogError("在获取属性集时");
                return 0f;
            }
            return m_Attributes[attr].baseValue;
        }
        // //TODO：似乎每次都需要走这里获取，而这是很频繁的操作，是否要添加一些缓存措施呢？尽管我暂时感觉这点计算量毫无影响。
        // public float GetAttributeValue(AttributeValueAcquirer _acquirer)
        // {
        //     //TODO：不一定是currentValue，比如基础攻击力，就应该访问originalValue。
        //     float result = 0f;
        //     foreach (var value in _acquirer.values)
        //     {//Ques：Convert.ToInt32是因为泛型约束只能到Enum，不能再进一步确定其值类型，而这个函数可以处理所有类型的Enum。
        //         result += m_Attributes[Convert.ToInt32(value.attr)].currentValue * value.magnification;
        //     }
        //     return result;
        // }

    }



    public class ActorAttributeSet : AttributeSetBase<ActorAttributeSet.AttributeType>
    {

        /*Tip：Enum就是逻辑性的标签，GameplayTag就是数据性的标签。似乎在这种属性集中，由于属于底层固定规则，或许使用Enum才是更好的选择。*/
        //Ques：似乎Enum就已经完全确定了属性集中的所有属性信息，并且明确了先后顺序，可以直接作为下标在数组中访问？！因为各个属性本来就是同一类型，只是被用于的逻辑不同而造就了不同属性。
        public enum AttributeType
        {
            HPMax,
            HP,
            ATK,
            DEF,
            SPMax,
            SP,
            CritRate,
            CritDamage
        }
        // private GameplayAttribute[] m_Attributes;

        // private GameplayAttribute m_HPMax;
        // private GameplayAttribute m_HP;
        // private GameplayAttribute m_ATK; //攻击力
        // private GameplayAttribute m_DEF; //防御力
        // private GameplayAttribute m_SPMax; //体力
        // private GameplayAttribute m_SP;
        // private GameplayAttribute m_CritRate; //暴击率
        // private GameplayAttribute m_CritDamage; //暴击伤害

        // public float GetAttributeValue(AttributeValueAcquirer _acquire)
        // {
            
        // }
    }

    public class GameplayAttribute
    {//TODO：如果使用一个Enum表达BaseValue和CurrentValue，那么这里也可以改为数组而不是一个个的变量。
        // public float originalValue;
        public float baseValue; //似乎base更贴切，因为想到了“基础生命值”、“基础攻击力”
        public float currentValue;
    }
}