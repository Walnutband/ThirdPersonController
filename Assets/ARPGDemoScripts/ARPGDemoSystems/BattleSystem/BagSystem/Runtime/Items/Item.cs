
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*Tip：对于作为静态数据的ItemData的封装，以便提供一些动态性。而ItemData的数据在运行时是不可变的，但是作为封装类的Item可以将ItemData中的某些字段在Item中进行一些额外处理，
    使其在运行时可以发生一定的变化，这就看游戏设计在此方面的自由度了。*/
    /*Ques: 在编写PickableItem组件时发现应该封装Item而不是ItemData，而之前只是把Item当做纯粹的运行时类型，所以没有任何序列化标记，那么到底是否应该对Item使用序列化呢？
    暂时能够想到的替代方法就是把必要信息拆分出来、在编辑时设置好，然后运行时再根据这些数据生成Item然后返回给发出交互行为（拾取）的对象*/
    // [Serializable] 
    public class Item
    {
        /*TODO：暂定不参与序列化，因为考虑到只是在运行时动态创建实例、作为ItemData的封装外壳、并且提供一些动态性。*/
        // private uint m_DataID;
        // public uint dataID => m_DataID;
        // public uint dataID 
        // {
        //     get => ItemManager.Instance.GetItemData(m_DataID);
        //     set => m_DataID = value;
        // }

        /*又考虑到既然是封装，就不会让外界访问ItemData，而且似乎没必要用ID、那样每次访问ItemData的成员都要经过ID跳转的过程、显然是多余的，ID的作用大概就是为了解耦而用于建立映射关系的，
        这种时候既然明确是Item封装ItemData的话，就没必要搞这种映射了。*/
        private ItemData m_Data;

        public uint id => m_Data.id;
        public string name => m_Data.name;
        public ItemType type => m_Data.type; //这种用属性返回数据对象的成员，其实更接近于“封装外壳”的含义。

        private int m_Amount;
        public int amount => m_Amount;
        public int maxAmount => m_Data.maxAmount;

        public ActorResource resource => m_Data.resource;
        private List<BuffObj> m_Buffs; //到底用Buff还是BuffData呢？感觉用Buff更加体现层次性，Item与Buff是同层的，ItemData与BuffData是同层的、由于是在编辑时确定所以采用ID标识。
        public List<BuffObj> buffs => m_Buffs;


        public Item(ItemData _itemData, int _amount = 1)
        {
            m_Data = _itemData;
            m_Amount = _amount;
        }

        public Item(uint _dataID)
        {
            // m_ItemData = ItemManager.Instance.GetItemData(_dataID);
            m_Data = InventoryManager.Instance.GetItemData(_dataID);
        }

        public bool ChangeItemData(ItemData _itemData)
        {
            //传入为空，或者与当前是同一个ItemData（注意用ID比较，而非直接实例比较）
            if (_itemData == null) return false;
            else if (m_Data != null && m_Data.id == _itemData.id) return false; //相同就不换了

            m_Data = _itemData;
            // m_BuffDatas = BuffManager.Instance.GetBuffDatas(_itemData.buffIDs);
            m_Buffs = BuffManager.Instance.GetBuffs(_itemData.buffIDs);

            return true;
        }

        //换成枚举更好。
        public int ModifyAmount(int _amount)
        {//TODO：暂时还没有处理越界的情况，不过越界的处理有可能会放到UI层。
         // m_Amount += _amount;
         // m_Amount = Mathf.Clamp(m_Amount + _amount, 0, m_Data.maxAmount);
            m_Amount += _amount;
            if (m_Amount > maxAmount)
            {
                m_Amount = maxAmount;
                return 1;
            }
            //其实此时就应该销毁释放内存了，不过C#里面是通过GC的
            if (m_Amount <= 0)
            {
                m_Amount = 0;
                return -1;
            }

            return 0;
        }

        //方便外部直接调用，就是用于增加或减少数量的简单逻辑。
        public bool AddSameItem(Item _item) //返回值含义是true则表示添加成功并且溢出
        {
            if (_item == null || _item.m_Data.id != m_Data.id) return false;//不是同一个类型的物品。
            // return ModifyAmount(_item.amount);
            if (ModifyAmount(_item.amount) > 0)
            {
                return true;
            }
            return false;

        }
        public bool RemoveSameItem(Item _item)
        {
            if (_item == null || _item.m_Data.id != m_Data.id) return false;//不是同一个类型的物品。
            if (ModifyAmount(-1 * _item.amount) < 0)
            {
                return true;
            }
            return false;
        }
    }


    /*Tip：该数据类会作为运行时的所有物品类共用的数据类（的基类？），而该类的字段值会在编辑器中编辑好，序列化保存在一个ScriptableObject资产文件中，那么运行游戏时就可以直接反序列化
生成带有编辑时数据的实例，也就是说通过序列化机制就取代了在运行时赋值的逻辑，所以将字段设置为私有是不会影响的，但是如果像这样约束对字段只能读不能写的话，是否会有与此相关的游戏设计
比如将某个Buff偷换为另一个Buff，那么是否会不支持呢？不过我感觉也不会，因为比如Item、Buff，这些引用对应的数据实例都应该是引用的ID，而非实例本身，这样的话，只需要更换记录的ID即可
实现切换物品或切换Buff这样的效果，而ItemData、BuffData这样的数据类的数据就是应该保持静态性，就是从硬盘反序列化到内存之后就不再改变，只是被各处引用、读取数据而已。*/
    /*TODO：如果只是表示装备类型的物品的话，这样确实完全够用，但是对于材料类的物品以及更多样的类型，可能含有的成员类型区别就比较大了，只是暂时不考虑那么多。*/
    [Serializable]
    public class ItemData
    {
        /*TODO：其实可以直接全部公开，但是为了更好的结构性（这是完全的静态数据，意思是在编辑时完全确定好，运行时不再改动、只是读取）感觉还是这样更好*/

        //唯一ID与具体名称当然是所有物品的基本共同数据。(uint也可以正常序列化，而且能够确保非负)
        [SerializeField] private uint m_ID; //uint还是ulong呢？注意uint（32位）范围是0 到 4294967295
        public uint id => m_ID;
        [SerializeField] private string m_Name;
        public string name => m_Name;

        /*TODO：如果尝试用这一个类表示所有类型物品的话，必然要穷举出所有类型物品需要用到的字段，当然大多都是相同的，但也存在许多差异，除了巧妙使用容器、枚举类型等等来尽量精简需要定义的字段以外，
        估计需要专门定义编辑器面板、在选择对应的ItemType时，只渲染出需要的字段，其他就保持默认值，比如引用类型就是null，这应该是比较好的做法。*/
        [SerializeField] private ItemType m_Type;
        public ItemType type => m_Type; //虽然ItemType设置成了二进制，但应该是Bag使用二进制，而Item还是使用单成员。
        //物品描述 //TODO: 可能还有更多不同的描述，比如获得途径、道具效果、道具故事等等，这种就应该划分到不同的string变量中，然后在UI中的不同文本框中分别显示对应那内容。
        [SerializeField] private string m_Description;
        public string description => m_Description;
        //通常只要是物品就必然会有图标，因为要在UI中显示查看。
        [SerializeField] private Sprite m_Icon; //Sprite可以直接用Image显示，便于模版式的UI图标显示。
        public Sprite icon => m_Icon;

        //最大数量 //TODO：可能还要分能够随身携带的最大数量，以及能够存放的最大数量。
        [SerializeField] private int m_MaxAmount;
        public int maxAmount => m_MaxAmount;

        //用作直接数值的属性值。
        // [SerializeField] private List<ItemProperty<int>> m_Properties_Plus;
        // public List<ItemProperty<int>> properties_Plus => m_Properties_Plus;
        // //用作倍率的属性值
        // [SerializeField] private List<ItemProperty<int>> m_Properties_Times;
        // public List<ItemProperty<int>> properties_Times => m_Properties_Times;

        //TODO：测试
        [SerializeField] private ActorResource m_Resource;
        public ActorResource resource => m_Resource;

        [SerializeField] private List<uint> m_Buffs;
        public List<uint> buffIDs => m_Buffs;
    }


    /*Tip：基础属性的数据类型大多相同（int或float），所以应当放入容器，而由于属性之间通过标识符区别、处理逻辑不同，所以不能放入列表或数组中、应该放入字典，但是又因为字典无法直接序列化，
    所以在此使用一个结构体封装一下作为标识符的ItemPropertyType及其值，其实就是代替序列化字典的作用。*/
    [Serializable]
    public struct ItemProperty<TValue>
    {
        [SerializeField] private ItemPropertyType m_Type;
        public ItemPropertyType type => m_Type;
        [SerializeField] private TValue m_Value;
        public TValue value => m_Value;
    }


}