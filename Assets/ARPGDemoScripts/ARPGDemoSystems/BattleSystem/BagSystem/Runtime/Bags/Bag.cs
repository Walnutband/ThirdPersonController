
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    public class Bag
    {
        /*Tip：Bag与Item看起来好像是一样的结构，但细想完全不是一回事————Bag作为物品的容器，BagData的数据其实就是物品的数据，而ItemData的数据才是物品自己的数据，
        所以BagData完全就是为了方便传递物品数据，而不是作为一个独立对象而存在。*/
        // private BagData m_Data;


        // Dictionary<uint, Item> m_Items = new Dictionary<uint, Item>();
        protected List<Item> m_Items; //列表下标兼任作槽位。
        /*字典的键类型是个重点考虑问题，这里不用uint作为键表示ID，而是使用ItemType，就是将Bag存储的元素按照类型划分存储，到时候在Pick元素时就可以快速处理。*/
        // protected Dictionary<ItemType, List<Item>> m_TypeItems = new Dictionary<ItemType, List<Item>>();
        //发现还是需要以Id为键的字典，所以这个嵌套字典就是分层，从Bag到各个类型的字典、再到各类型中各个ID对应的物品Item，最终还是要落实到各个物品。
        //Tip：一定要注意Item必须是引用、才能在字典中快捷访问到并且进行操作、从而影响到列表m_Items中的Item，就不需要遍历列表查找了。
        protected Dictionary<ItemType, Dictionary<uint, Item>> m_TypeItems = new Dictionary<ItemType, Dictionary<uint, Item>>();

        /*Tip: 注意Bag与Item区别，背包要分门别类，但是BagData可能往往会有多个类型物品，分给Bag时可能就要判断类型、选择性划分给符合类型的Bag（不过实际上应该是用Bag封装好，也就是
        Bag与Bag之间的互动，这也是符合应该的代码逻辑的，BagData在运行时就应该只充当编辑时确定好的数据模版，不应该对其数据进行任何修改）*/
        protected ItemType m_ItemType;
        // public ItemType itemType => m_Data.itemType;

        // public Bag(BagData _data)
        // {
        //     // m_Items = new List<Item>(data.capacity);
        //     m_Items = new List<Item>();
        //     //对二进制的枚举类型、通过移位运算就可以实现遍历的效果，就可以方便将其拆分。
        //     m_TypeItems.Add(ItemType.)
        //     _data.items.ForEach(pair => m_Items[pair.slot] = new Item(InventoryManager.Instance.GetItemData(pair.id)));

        // }

        //背包完全可以在构造时就是个空背包（甚至大部分时候都应该是），BagData只是填充其内容，而不像Item必须要绑定ItemData（当然也可以设计空物品，还是看游戏设计）。
        public Bag(ItemType _itemType) //按理来说，构造之后就不应该再改变ItemType了、否则会引起一系列连锁反应、除非需要实现相关的游戏设计。
        {
            m_ItemType = _itemType;
            m_Items = new List<Item>();
            m_TypeItems = new Dictionary<ItemType, Dictionary<uint, Item>>();
            ItemType types = m_ItemType;
            //使用一个专门的二进制数左移，这样的话进行与运算得到的结果就表示m_ItemType是否包含当前位的枚举成员，如果让types右移的话，就会改变原本位对应的枚举成员、从而得出错误结果
            int bit = 1;
            // while (types != 0)
            while (bit <= (int)types) //填充好外层字典，就是在背包内部划分不同类型的物品。
            {
                // if (((int)types & 1) != 0)
                // {
                //     m_TypeItems.Add(types, new Dictionary<uint, Item>());
                // }
                // if (((int)types & bit) != 0)
                ItemType type = (ItemType)((int)types & bit); //
                if ((int)type != 0)
                {
                    m_TypeItems.Add(type, new Dictionary<uint, Item>());
                }
                bit <<= 1;
            }
        }

        #region 背包操作

        //关于命名，Get、Pick、Take等等，感觉Pick最贴切，就是从另一个背包中拿取物品。
        public void PickItemsFromBag(Bag _bag, bool rob = false)
        {
            //背包有更多信息可以利用，比BagData方便多了
            ItemType commonTypes = m_ItemType & _bag.m_ItemType; //只看公共的物品类型即可
            if (commonTypes == 0) return; //无共同类型，直接退出。

            int bit = 1;
            while (bit <= (int)commonTypes)
            {
                ItemType type = (ItemType)((int)commonTypes & bit);
                if ((int)type != 0)
                {
                    foreach (Item item in _bag.m_TypeItems[type].Values)
                    {
                        if (m_TypeItems[type].TryGetValue(item.id, out Item myItem))
                        {
                            myItem.AddSameItem(item);
                        }
                        else
                        {//新增。
                            m_TypeItems[type].Add(item.id, item);
                            m_Items.Add(item);
                        }
                    }
                }
            }

            /*Ques：有点没看懂，这里访问私有字段m_Data竟然是合法的？？？*/
            // PickItemsFromBagData(_bag.m_Data);
        }

        //从BagData中拿取自己能够放入的物品。
        public void PickItemsFromBagData(BagData _bagData)
        {
            ItemData itemData = null;
            foreach (var pair in _bagData.items)
            {
                itemData = InventoryManager.Instance.GetItemData(pair.id);
                if ((itemData.type & m_ItemType) != 0) //可以放入背包。
                {
                    if (m_TypeItems.ContainsKey(itemData.type))
                    {
                        // if (m_TypeItems[itemData.type].ContainsKey(itemData.id))
                        if (m_TypeItems[itemData.type].TryGetValue(itemData.id, out Item item))
                        {
                            item.ModifyAmount(pair.amount); //增加数量
                        }
                        else
                        {
                            m_TypeItems[itemData.type].Add(itemData.id, new Item(itemData));
                            m_Items.Add(new Item(itemData, pair.amount));
                        }
                    }
                }
            }
        }

        public virtual bool AddItem(Item _item)
        {
            ItemType itemType = _item.type & m_ItemType;
            if (itemType != 0)
            {
                if (m_TypeItems[itemType].TryGetValue(_item.id, out Item item))
                {
                    /*Tip：其实一般来说是需要处理溢出时的逻辑的，即将多余的再占一个新的槽位或者怎样，但是对于一个ARPG来说，尤其以法环为例，根本就不需要这种逻辑，毫无意义，
                    既是给代码增加复杂度，又是给游戏增加负担，这是早就该淘汰的设计了。*/
                    item.AddSameItem(_item);
                }
                else
                {
                    //注册新元素，其实向字典添加元素就带有“注册”的含义，而向列表中添加元素则是证明其存在性、代表其存在的表现。
                    m_TypeItems[itemType].Add(_item.id, _item);
                    m_Items.Add(_item);
                }
                return true;
            }

            return false;
        }

        public bool AddItems(ItemData _itemData)
        {
            return AddItem(new Item(_itemData));
        }

        public virtual void RemoveItem(Item _item)
        {
            ItemType itemType = _item.type & m_ItemType;
            if (itemType != 0)
            {
                if (m_TypeItems[itemType].TryGetValue(_item.id, out Item item))
                {
                    if (item.RemoveSameItem(_item) == true)
                    {//直接从容器中移除掉引用，随后GC就回收垃圾了。
                        m_TypeItems[itemType].Remove(_item.id);
                        m_Items.Remove(item);
                    }
                }
            }
        }
        
        public void RemoveItems(ItemData _itemData)
        {
            RemoveItem(new Item(_itemData));
        }

        #endregion
    }


    [Serializable]
    public class BagData
    {
        [SerializeField] private uint m_ID;
        public uint ID => m_ID;

        /*TODO：BagData的使用方式和编辑方式是我非常不确定的地方，而且我也因此多次想到、我花了主要时间在这些系统开发和探究工作流上面、求职的时候真的能够被看到吗？？
        不过从这里来看，可能又回到了之前为UI系统开发相关的编辑器遇到的问题，就是要实现一些强制性功能防止开发者出错、这一点很多时候非常困难，因为编辑器的本来目的是
        提高开发效率、而不是防止出错、只是好的编辑器能够避免很多意外错误、但不能规避所有错误，开发者自己应当遵守开发规范，当然运行时代码也可以进行很多检测，但问题在于，
        这里面的很多检测可能对于游戏本身毫无意义、纯粹是为了防止将编辑时犯下的错误带到运行时中，所以开发者就应该自己做好这些事，尽量减少运行时代码在这方面的逻辑。*/
        // [SerializeField] private BagType m_BagType; //BagType含有哪些类型成员，就代表可以容纳哪些类型的物品。
        // public BagType bagType => m_BagType;
        /*Tip：反思了一下，Bag背包就不应该有什么类型，它就是单纯作为物品的容器、提供一些功能，就和标准库的Array、List这些容器没有本质区别，就是完全为物品管理服务的，
        所以使用ItemType而不是BagType、也没有BagType。*/
        [SerializeField] private ItemType m_ItemType; //TODO：BagData真的需要指定类型吗？
        public ItemType itemType => m_ItemType;
        [SerializeField] private string m_Name;
        public string name => m_Name;

        /*TODO：容量其实算是非必要字段，尤其是在单机游戏中，几乎很少出现需要扩容背包的设计，因为这种硬性限制往往是对于游戏性的直接破坏，所以容量通常都是固定的，保证始终都
        装得下（比如法环中，不仅为不同类型物品划分了各自的背包，甚至背包的格子数量都保证能够容纳该类型物品在整个游戏设计中的所有实际物品，而且物品数量都可以叠加、正常情况下
        不可能到达存储上限、只是限制随身携带的数量而已，这样的话有关背包容量的相关逻辑就可以极致简化了，而且对于游戏性是完全的促进效果）
        ————除此之外，用于背包显示的UI也是更适合容量固定的逻辑。
        ————不过如果设计玩家的背包容量不限、但是对于某些特定类型的物品所要用到的特定类型的背包必须要通过某些途径获取的话、然后再加上一个万能背包的设定，那么这确实可以扩展游戏性。*/

        // [SerializeField] private int m_Capacity;
        // public int capacity => m_Capacity;

        /*Tip：在数据中记录ID，生成运行时实例时就从ID映射到实例。而且这应该还不算是什么技巧、而是必须这样，因为这些数据类是在编辑器中编辑的，假如这里直接存储ItemData的话，那么在编辑器
        中编辑时的ItemData与直接编辑的ItemData根本就不是同一个，而从逻辑上来看应当是同一个、BagData只是引用而非克隆ItemData，所以这是必须用ID。
        背包引用物品、物品引用Buff和属性。*/
        // private List<uint> m_ItemDataIDs = new List<uint>();
        // public List<uint> itemDataIDs => m_ItemDataIDs;
        private List<ItemInfo> m_Items = new List<ItemInfo>();
        public List<ItemInfo> items => m_Items;

        [Serializable]
        // public class SlotIDPair
        public class ItemInfo //将Item的必要信息拆分便于编辑，然后运行时再根据这些信息实时生成Item实例。
        {
            /*TODO：如果要用槽位的话，其实会给逻辑带来一定的混乱度，这也与背包的容量相关联，不如就直接舍弃，就按照顺序添加就行了、或者再提供一些排序方法也行，总之在使用BagData构造背包时
            如果要考虑槽位的话，会带来大量的复杂度*/
            /*Tip：关于命名，是否要命名为ItemID或者ItemDataID呢？我暂时的判断是，看字段位于什么结构中、看访问时的链条是怎样的，比如在Item中，如果访问Item的id的话，
            直接item.id就很合适，如果是item.itemDataID的话感觉就有些多余了、没必要。*/
            // [SerializeField] private int m_Slot = -1; //槽位 
            // public int slot => m_Slot;  //TODO：由于List的下标是int类型，所以在此从uint改为了int，并且从struct改为了class，这样才能定义字段初始化值，而这里-1就表示按照顺序排列。
            [SerializeField] private uint m_ID;
            public uint id => m_ID;
            //该ID物品的数量
            [SerializeField] private int m_Amount;
            public int amount => m_Amount;

        }

#if UNITY_EDITOR
        /*Tip：发现了一个问题，因为BagData是在编辑模式下编辑的，而数据库InventoryManager是在运行模式时才使用的，所以还无法直接简单地通过数据库查找物品的类型，当然在编辑模式下
        照样有很多办法，但终究还是需要开发者遵守一定的开发规范、避免出现这些毫无意义的错误。*/
        /*Tip：将背包的物品类型纠正为背包能够存储的物品类型，实质是去掉类型不符的物品，这其实是开发者应该保证的，只是设置这样一个可以纠错的方法。当然BagData对于Bag就不应该出现这种错误了，
        说白了这是一个流水线，最多让后一个环节给前一个环节稍微纠一下错、一定要及时阻断错误传递下去，而之后的环节就会在之前的环节没有错误的基础上、来展开自己的逻辑。*/
        public void CorrectItemType()
        {
            List<ItemInfo> result = new List<ItemInfo>();

        }
#endif
    }



    #region 具体背包（被个体角色所拥有）

    /*个体对于不同类型物品会有各自特殊的处理逻辑，因此必须独立为各个具体的类型，而通过继承来最大程度上复用背包成员（并且增强结构性）、其实成员本来就是共同的、只是处理逻辑各不相同。*/

    /*捆绑包？（捆绑背包）这里就是代表个体的装备包，可以在游戏中直接通过操作访问的。*/
    public class ActorEquipmentBag
    {
        private WeaponBag m_WeaponBag;
        public WeaponBag weaponBag => m_WeaponBag;
        private ArmorBag m_ArmorBag;
        public ArmorBag armorBag => m_ArmorBag;
        private AmuletBag m_AmuletBag;
        public AmuletBag amuletBag => m_AmuletBag;
        private ConsumableBag m_ConsumableBag;
        public ConsumableBag consumableBag => m_ConsumableBag;

        public ActorEquipmentBag()
        {
            m_WeaponBag = new WeaponBag();
            m_ArmorBag = new ArmorBag();
            m_AmuletBag = new AmuletBag();
            m_ConsumableBag = new ConsumableBag(8);
        }

        //Pick相比于单纯的Add确实带有更丰富的含义。
        public bool PickItemToBag(Item _item)
        {
            if (_item == null) return false;

            switch (_item.type)
            {
                case ItemType.Weapon:
                    return m_WeaponBag.AddItem(_item);
                case ItemType.Armor:
                    return m_ArmorBag.AddItem(_item);
                case ItemType.Amulet:
                    return m_AmuletBag.AddItem(_item);
                case ItemType.Consumable:
                    return m_ConsumableBag.AddItem(_item);
                default:
                    break;
            }
            return false; //说明在该捆绑包中并没有找到合适的背包
        }
    }

    /*Tip：如果无法在字段的类型上强行限制物品类型（ItemType），那么就在背包的物品出入口（就是对于存储物品的字段的Get和Set）上进行限制。
    主要是使用枚举类型来代表物品类型之后，就是为了让各个类型的物品都公用同一个类，而另外一种常用做法是为每个类型分别创建独立的类，
    这样的话确实增加了强制性，但也因此导致结构松散、逻辑冗余，当然如果不要求特殊游戏机制的扩展性的话，这样写其实要容易得多，好的结构
    必然是更加难以编写的，说白了就是，好结构难搭建、易扩展，坏结构易搭建、难扩展。*/

    //这才是面向对象的写法
    public class WeaponBag : Bag
    {
        // public WeaponBag(BagData data) : base(data)
        // {
        //     m_ItemType = ItemType.Weapon;
        // }
        public WeaponBag() : base(ItemType.Weapon)
        {
        }
    }


    public class ArmorBag : Bag
    {
        // public ArmorBag(BagData data) : base(data)
        // {
        //     m_ItemType = ItemType.Armor;
        // }
        public ArmorBag() : base(ItemType.Armor)
        {
        }
    }

    public class AmuletBag : Bag
    {
        // public AmuletBag(BagData data) : base(data)
        // {
        //     m_ItemType = ItemType.Amulet;
        // }
        public AmuletBag() : base(ItemType.Amulet)
        {
        }
    }

    public class ConsumableBag : Bag
    {
        //一定要保证列表和字典是同步的、引用的Item是相同的而非克隆的。
        // private List<Item> m_Items;
        // private Dictionary<uint, Item> m_AllItems = new Dictionary<uint, Item>(); //字典方便快速查找，也可以用封装结构来替代。

        // private int m_Capacity;
        // private Item m_SelectedItem;
        private int m_SelectedItemIndex = 0;
        public int selectedItemIndex => m_SelectedItemIndex;
        /*Tip：这里的容量记录并非是实际存储物品的容器，因为为了动态性而选择使用List作为容器，而且这也是比较好的做法，至于限制容量、显示UI（格子数量总要明确），
        这些逻辑本来就要经过背包、而不是与背包的容器绑定，所以完全可以自由操作、自由约束。————这也带来一个问题，就是要保证m_Capacity与实际情况同步。
        就是List的Capacity和Count属性，在这里就相当于m_Capacity和ItemCount，也就是自己手动管理。*/
        private int m_Capacity;
        public int capacity => m_Capacity;
        private int m_ItemCount; //实际的物品数量
        public int itemCount => m_ItemCount;


        // public ConsumableBag(BagData data) : base(data)
        // {
        //     m_ItemType = ItemType.Consumable;
        // }

        public Item selectedItem => m_Items[m_SelectedItemIndex];
        //Tip：这两个是为了UI获取前后物品来显示而设置的属性。
        public Item rightOfSelectedItem => m_Items[RightItemIndex(m_SelectedItemIndex)];
        public Item leftOfSelectedItem => m_Items[LeftItemIndex(m_SelectedItemIndex)];

        public ConsumableBag(int _capacity) : base(ItemType.Consumable)
        {
            m_Capacity = _capacity > 0 ? _capacity : 1;
            m_ItemCount = 0;
            m_Items = new List<Item>(m_Capacity);
        }

        //扩展容量，并非设置，也不能缩小容量（可以想到一个设计是缩小背包容量、导致物品掉落，但现在不考虑）
        public void ExpandCapacity(int _capacity)
        {
            if (_capacity <= 0) return;

            m_Capacity += _capacity;
            m_Items.Capacity = m_Capacity;
        }

        public void SetSelectedItemIndex(int index)
        {
            m_SelectedItemIndex = index;
        }

        //一般就是左移右移，也没必要搞其他操作，如果是键盘的话可能用数字键直接选中，但说实话对于消耗品的话确实没必要，技能才是有那么一点必要，但对于魂游这种游戏机制的话也没必要。
        public void RightShiftSelectedItemIndex()
        {
            // m_SelectedItemIndex = (m_SelectedItemIndex + 1) % m_Capacity;
            // m_SelectedItemIndex = (m_SelectedItemIndex + 1) % m_ItemCount;
            m_SelectedItemIndex = RightItemIndex(m_SelectedItemIndex);
        }
        public void LeftShiftSelectedItemIndex()
        {
            // m_SelectedItemIndex = (m_Capacity + m_SelectedItemIndex - 1) % m_Capacity;
            // m_SelectedItemIndex = (m_ItemCount + m_SelectedItemIndex - 1) % m_ItemCount;
            m_SelectedItemIndex = LeftItemIndex(m_SelectedItemIndex);
        }

        private int RightItemIndex(int _index) => (_index + 1) % m_ItemCount;
        private int LeftItemIndex(int _index) => (m_ItemCount + _index - 1) % m_ItemCount;
        

        public void ChangeSelectedItem(int deltaIndex)
        {
            if (deltaIndex == 0) return;
            else if (deltaIndex > 0)
            {
                //正数回环，也可以设置为不能从一边到另一边
                m_SelectedItemIndex = (m_SelectedItemIndex + deltaIndex) % m_Capacity;
            }
            else if (deltaIndex < 0)
            {//这样似乎是可以实现负数时的回环
                m_SelectedItemIndex = (m_Capacity + m_SelectedItemIndex - deltaIndex) % m_Capacity;
            }
        }

        /*TODO：个体调用、使用当前选中的消耗品。这里作为背包，能够指定的逻辑就是对自己背包中的指定物品减少数量、如果没有了就及时移除，而作为引用传入的_item就是让该方法的调用者
        能够使用消耗品的数据，这才是真正意义上的“使用消耗品”。
        另一方面，也可以尝试传入该方法的调用者，在此完成逻辑，但这样的话可能会影响个体类型的成员访问权限等方面的设置。*/
        public void ConsumeItem(ref Item _item, int _amount = 1)
        {
            _item = m_Items[m_SelectedItemIndex]; //外部就可以通过_item来访问当前选中的物品。
            //取绝对值强行非负，其实正常逻辑下传入的就应该是正整数，这种虽然为了防止出现负数、但是总感觉很多余。
            if (_item.ModifyAmount((-1) * Mathf.Abs(_amount)) < 0) //通常都是消耗一个。
            {
                m_Items.Remove(_item); //TODO：对于数量归0时的释放和移除，这段逻辑还需要优化
                m_TypeItems[_item.type].Remove(_item.id);
                m_ItemCount = m_Items.Count;
            }
        }

        public override bool AddItem(Item _item)
        {
            //其实按正常逻辑就不可能出现ItemCount大于Capacity的情况
            if (m_ItemCount >= m_Capacity) return false;
            // return base.AddItem(_item);
            if (base.AddItem(_item))
            {
                //由于AddItem逻辑在基类中，而且添加元素时有可能只是改变已有Item的数量而不是新增Item，所以此处根据List自带的Count更新。
                m_ItemCount = m_Items.Count;
                return true;
            }
            else return false;
        }

        // public ConsumableBag(BagData _data)
        // {
        //     m_ItemType = ItemType.Consumable;
        //     m_Capacity = _data.capacity;
        //     m_Items = new List<Item>(m_Capacity);
        //     _data.items.ForEach(pair =>
        //     {
        //         ItemData itemData = InventoryManager.Instance.GetItemData(pair.id);
        //         if ((itemData.type & m_ItemType) != 0) //可以放入背包。
        //         {
        //             m_AllItems.Add(pair.id, new Item(itemData));
        //             m_Items.Add(new Item(itemData));
        //         }
        //     });
        // }
        // /*Tip：在设计上，这些Bag用于随身包的话、只是记录数据，这些方法都是由拥有该背包的个体调用，而个体在调用之后就会根据该背包更新自己的装备数据。*/
        // // public bool AddItem(Item item)
        // // {
        // //     if (item.type != ItemType.Consumable) return false;
        // //     return true;
        // // }
        // // public bool RemoveItem(Item item)
        // // {
        // //     if (item.type != ItemType.Consumable) return false;
        // //     // if (m_Items.Exists(item => item.id == item.id))
        // //     if (m_AllItems.ContainsKey(item.id))
        // //     {
        // //         m_AllItems[item.id].ModifyAmount(-1);
        // //     }
        // //     else //与Add不同，移除的前提就是要有。
        // //     {
        // //         return false;
        // //     }
        // //     return true;
        // // }

        #endregion
    }

    /*最新物品背包，对于小Demo来说当然没用，但是对于稍微大型一点的游戏，这个背包可以快速查看最新获得的物品，就非常实用了。
    而且在UI显示上应该是越后面的越显示在前面，这也很简单，只需要倒序遍历列表即可，列表还是按照正常顺序添加元素。*/
    public class LatestItemsBag : Bag
    {
        public LatestItemsBag() : base(ItemType.All)
        {
            
        }
    }
    //通用背包，万能背包
    public class UniversalBag : Bag
    {
        public UniversalBag() : base(ItemType.All)
        {
            
        }
    }
}