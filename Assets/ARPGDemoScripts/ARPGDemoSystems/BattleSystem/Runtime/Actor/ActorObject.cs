using System;
using System.Collections.Generic;
using ARPGDemo.CustomAttributes;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    /*TODO：ActorProfile代表个体本身的一切，大概接近于ActorData、ActorStats、ActorSheet，具体采用什么命名，也只有基于更多的开发经验才能确定，
    比如Profile更多指的是角色档案，那么可能作为资产类命名更好，而对应的运行时类型就叫ActorStats或者其他。。。*/
    // public class ActorProfile : MonoBehaviour
    //Tip：开发之后，感觉还是Object更加符合运行时代表的概念，Profile、Model这样的更像编辑时、持久化的对象。
    [AddComponentMenu("ARPGDemo/BattleSystem/ActorObject")]
    public class ActorObject : MonoBehaviour, IActor
    {
        private int m_Level;
        private ActorAbility m_Ability;

        [Header("属性值")]
        [SerializeField] private ActorProperty m_Property;
        public ActorProperty property { get => m_Property; }
        [SerializeField] private ActorProperty m_BaseProperty;
        public ActorProperty baseProperty { get => m_BaseProperty; }
        [SerializeField] private ActorProperty m_BuffProperty;
        public ActorProperty buffProperty { get => m_BuffProperty; }
        /*TODO：这才是真正参与战斗系统的数据。*/
        // [Header("资源")]
        [DisplayName("资源量")]
        [SerializeField] private ActorResource m_Resource;
        public ActorResource resource { get => m_Resource; }

        public event Action<ActorResource> resourceChangedEvent;

        /*Ques：总感觉Buff应该用一个专门的特殊容器来存储，方便触发一些回调、Buff叠层、Buff更新和替换，等等功能，这些都是应该复用的，但在容器之外似乎又不太好复用。
        Buff本身倒是直接存储数据就行了，但是一旦要作用到个体身上的话，就要处理很多情况、而这些情况往往联系着Buff相关的游戏设计。*/
        private List<BuffObj> m_Buffs;
        public List<BuffObj> buffs { get => m_Buffs; }
        // private Dictionary<uint, Buff> m_Buffs;
        // public Dictionary<uint, Buff> buffs { get => m_Buffs; }

        /*Tip：一些特殊变量*/
        private bool m_IsDead;
        public bool isDead { get => m_IsDead; }


        [Header("背包")]
        /*TODO：从UI的角度来看，肯定要根据类型划分各个UI部分，这样就肯定要访问对应的背包、然后背包中的物品、物品的相关信息（主要是名称、描述、图标），从而在UI中显示物品信息，所以
        必然要对背包进行划分；而*/
        // private Dictionary<BagType, Bag> m_Bags = new Dictionary<BagType, Bag>();
        // private WeaponBag m_WeaponBag;
        // private ArmorBag m_ArmorBag;
        // private AmuletBag m_AmuletBag;
        // private ConsumableBag m_ConsumableBag;
        private ActorEquipmentBag m_EquipmentBag = new ActorEquipmentBag();


        /*TODO: 实际上应该在一个管理器中获取ActorCareerData，然后作为参数传入ActorProfile的初始化方法，因为会用到职业数据往往是在创建新档的时候——主要还是在资源管理方面，
        对于这种静态数据大多应该是通过资源加载技术将硬盘中的资产加载到内存中、使用临时变量存储，将其数据读取到运行时类型的成员中，然后释放该临时变量所占用的内存。*/
        public ActorCareerData m_CareerData;

        [Header("物理检测")]
        [SerializeField] private CollisionDetector m_InteractableDetector; //需要结合Collider的Layer指定与哪些层级对象进行检测，比如Item、NPC等等。

        [Header("交互对象")]
        // private List<IInteractable> m_Interactables = new List<IInteractable>();
        /*似乎使用双向链表更好，因为可以快速*/
        // private LinkedList<IInteractable> m_Interactables = new LinkedList<IInteractable>();
        private InteractableRecord m_InteractableRecord = new InteractableRecord();
        private class InteractableRecord
        {//空间换时间，记录最高优先级（priority值越小越优先）以及其索引，这样List添加元素时就可以直接往后添加，不需要通过插入等方式来保证顺序，因为已经存储了额外信息必然可以直接访问到对应元素
            private int m_TopPriority;
            private int m_TopPriorityIndex;
            public IInteractable target => m_TopPriorityIndex != -1 ? m_Interactables[m_TopPriorityIndex] : null;
            private List<IInteractable> m_Interactables;

            public InteractableRecord()
            {
                m_TopPriority = int.MaxValue;
                m_TopPriorityIndex = -1; //-1代表无效索引。
                m_Interactables = new List<IInteractable>();
            }

            public void Reset()
            {
                m_TopPriority = int.MaxValue;
                m_TopPriorityIndex = -1; //-1代表无效索引。
                m_Interactables.Clear();
            }

            public void Add(IInteractable _object)
            {
                if (_object == null) return;

                //更新。
                if (_object.priority <= m_TopPriority)
                {
                    m_TopPriority = _object.priority;
                    m_TopPriorityIndex = m_Interactables.Count; //因为是连续往后添加。
                }
                m_Interactables.Add(_object);
            }
        }

        private void Awake()
        {
            m_InteractableDetector = transform.Find("___Colliders").Find("Interaction").GetComponent<CollisionDetector>();
        }

        // [ContextMenu("找找")]
        // private void Find()
        // {
        //     if (transform.Find("___Colliders").Find("Interaction").GetComponent<CollisionDetector>())
        //         Debug.Log("找到了");
        //     else
        //         Debug.Log("没找到");
        // }

        private void Start()
        {
            Initialize(m_CareerData);
        }

        private void OnEnable()
        {
            // m_Detector.triggerEnter += OnPickUpItem;
            m_InteractableDetector.triggerStay += CollectInteractable;
        }

        private void OnDisable()
        {
            m_InteractableDetector.triggerStay -= CollectInteractable;
        }

        private void LateUpdate()
        {
            /*TODO：设想是，每帧最后都要将记录的交互对象清空，只有在FixedUpdate物理检测时记录交互对象、然后在Update中处理交互行为（如果有的话）、最后在LateUpdate清空。
            但这样的问题是，断绝了后续与此相关的渲染流程，因为通常需要在UI中显示当前的可交互对象，不过可能UI在Update方法中就可以读取容器、设置自己的数据？
            而且还可能出现的更严重问题是，如果渲染帧率比较高的话，也就是多个Update帧之后才会执行一次FixedUpdate，那么在按下交互键时，这一次输入如果没有位于
            执行FixedUpdate的那一帧中的话（比如在相邻的后面几帧，这几帧没有执行FixedUpdate方法），在同一帧这里的LateUpdate方法就会将其清空，那么这一次的交互行为就会丢失，
            也就是出现“交互键失灵”的现象，这肯定是不应该出现的。*/
            // m_Interactables.Clear();
            // m_InteractableRecord.Reset();
        }

        private void FixedUpdate()
        {//似乎放在FixedUpdate中进行重置就可以与检测保持同步，不会丢失交互行为
            m_InteractableRecord.Reset();
        }

        private void Update()
        {

        }

        public void Initialize(ActorCareerData _careerData)
        {
            if (_careerData == null) return;

            m_Level = _careerData.level;
            m_Ability = _careerData.ability;
            UpdateBaseProperty();
        }

        private void UpdateBaseProperty()
        {
            m_BaseProperty.hp = m_Ability.vigor * 30;
            m_BaseProperty.mp = m_Ability.mind * 15;
            m_BaseProperty.sp = m_Ability.endurance * 10;
            m_BaseProperty.physicsAttack = m_Ability.strength * 10;
            m_BaseProperty.poison = m_Ability.arcane * 10;
            m_BaseProperty.bleed = m_Ability.arcane * 10;
            m_BaseProperty.poisonEffect = m_Ability.arcane * 2;
            m_BaseProperty.bleedEffect = m_Ability.arcane * 2;
            m_BaseProperty.physicsResist = m_Ability.vigor * 1;
            // m_BaseProperty.poisonResist = m_Ability.arcane * 2;
            // m_BaseProperty.bleedResist = m_Ability.arcane * 2;
            UpdateTotalProperty();
        }

        private void UpdateTotalProperty()
        {
            m_Property = m_BaseProperty + m_BuffProperty;
        }

        /*Tip：使用消耗品，设计是--有一个按键可以使用在当前的消耗品背包（随身包即装备包）中选中的消耗品（没有），或者是打开背包UI、选中指定消耗品点击使用，这是两种最常见做法、通常
        也只有这两种做法，*/
        /*Ques：比较纠结的是，到底使用Item还是ItemData呢？其实Item是含有层数信息的，但是这里实际上又没有用到、而是另外指定的数量，不过好像这样才是对的？！*/
        public void UseConsumable(Item _item = null, int _amount = 1) //参数默认值的妙用，传入就是依你、不传入就是依我
        {
            if (_item == null) //传入为空，说明就是默认的使用自身的消耗品背包中的选中的消耗品
            {
                m_EquipmentBag.consumableBag.ConsumeItem(ref _item);
                // _item = m_EquipmentBag.consumableBag.selectedItem;
                m_Resource = m_Resource + _item.resource; //简单相加资源。
                // m_Buffs.AddRange(_item.buffs); //简单添加Buff
                _item.buffs.ForEach(buff =>
                {
                    m_Buffs.Add(buff);
                    buff.onAdded?.Invoke(buff);
                });
            }
        }

        //收集可交互对象，物理检测是基于每一个单独的游戏对象的，所以这里就是对每一个检测到的对象单独处理。
        private void CollectInteractable(Collider _collider)
        {
            m_InteractableRecord.Add(_collider.GetComponent<IInteractable>());
            Debug.Log($"检测到交互对象：{_collider.gameObject.name}");
            // interactables.Add(_interactable);
        }

        /*Tip：作为对交互命令的直接响应，在此内部根据当前检测到的可交互对象、取其中最高优先级的对象作为此次交互行为的作用对象，由此决定执行哪种交互行为。*/
        public void Interact()
        {
            //每次要
            IInteractable target = m_InteractableRecord.target;
            if (target != null)
            {
                switch (target.type)
                {
                    case InteractionType.PickUp:
                        // PickUpItem((target as IInteractable<Item>).InteractableTarget());
                        PickUpItem(target);
                        break;
                    case InteractionType.Talk:

                        break;
                }
            }
        }

        //这算是一个用于“拾取物品”交互行为的执行行为。
        // private void PickUpItem(Item _item)
        // {
        //     /*TODO：其实应该放到库存背包中而不是这里的随身包（装备背包）*/
        //     // m_EquipmentBag.consumableBag.AddItem(_item);
        //     m_EquipmentBag.PickItemToBag(_item);
        //     Debug.Log($"物品ID：{_item.id}，名称：{_item.name}，数量：{_item.amount}");
        // }
        private void PickUpItem(IInteractable _target)
        {
            /*TODO：其实应该放到库存背包中而不是这里的随身包（装备背包）*/
            // m_EquipmentBag.consumableBag.AddItem(_item);
            Item _item = (_target as IInteractable<Item>).InteractableTarget();
            if (m_EquipmentBag.PickItemToBag(_item)) //如果确实将物品收集到了背包中
            {
                _target.InteractionEnd(); //交互成功结束。
            }
            Debug.Log($"物品ID：{_item.id}，名称：{_item.name}，数量：{_item.amount}");
        }

        public void AddBuff(BuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveBuff(BuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public void ModResource(ActorResource value)
        {
            m_Resource += value;
            resourceChangedEvent?.Invoke(m_Resource);
        }

        public Damage damage => new Damage(m_Property.physicsAttack, m_Property.poisonEffect, m_Property.bleed); 

        public Defense defense => throw new System.NotImplementedException();

        public bool CanBeAttack()
        {
            throw new System.NotImplementedException();
        }

        /*死亡时调用的方法，处理自身的死亡逻辑*/
        public void Die()
        {
            m_IsDead = true;

            
        }

        /*复活*/
        // public void Revive()
        public void ReCover()
        {
            m_IsDead = false;

            
        }

        public bool BeKilled()
        {
            return m_IsDead;
        }

        public void OnHit(IDefender defender, DamageInfo _damage)
        {
            
        }

        public void OnBeHurt(IAttacker attacker, DamageInfo _damage)
        {
            ActorResource resource = _damage.FinalDamage();
            ModResource(resource);
        }
    }
}

namespace ARPGDemo.AbilitySystem
{
    public class ActorObject : MonoBehaviour
    {
        
    }
}