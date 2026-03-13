using System;
using System.Collections.Generic;
using System.Linq;
using ARPGDemo.ControlSystem;
using ARPGDemo.CustomAttributes;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public class GameplayTag 
    {
        
    }


    /*Tip：行为管理器，个体能够执行什么行为，与决策、感知无关，专注于行为层的调度和执行。
    ASC是被注册的Ability都引用的，因为Ability的逻辑执行就依赖于各个系统，这些系统不应该也不需要各自都记录在Ability中，所以以ASC作为一个通道或者说中枢，
    使得各个Ability都能够通过ASC访问到执行逻辑所需要依赖或委托的各个系统。说白了一个完整技能的执行就是由各个系统相互协调配合实现的，所以一个Ability也算是一个调度器，
    安排各个系统如何执行。通知ASC执行（匹配标识信息的）某Ability之后，其实就进入到了Ability的调度逻辑中，而Ability并没有监测逻辑、也符合调度器的设计，
    ASC的监测逻辑只是在更新Ability的一些状态信息，因为状态是随时间流逝而不断变化的，比如Ability的CD。
    */

    public class AbilitySystemComponent : MonoBehaviour, IAbilitySystemComponent
    {
        /*
        // private List<GameplayEffect> m_Effects;

        // public void ApplyGameplayEffect(GameplayEffect _ge)
        // {

        // }

        // private void FixedUpdate()
        // {
        //     //Tip：这种最基础的遍历操作确实应该如此简洁。
        //     m_Effects.ForEach(ge => ge.OnTick());
        // }
        */
        [SerializeField] private ActorMovementComponent m_AMC; //专门处理移动逻辑
        public ActorMovementComponent AMC
        {
            get => m_AMC;
        }
        [SerializeField] private ControlSystem.Test.ActorMovementComponent m_AMCForCharged; //专门处理移动逻辑
        public ControlSystem.Test.ActorMovementComponent AMCForCharged
        {
            get => m_AMCForCharged;
        }
        //Tip：关于处理GE

        //管理属性集，暂时确定就是该属性集。
        [DisplayName("属性集")]
        [SerializeField] private ActorAttributeSet m_ActorAS;
        public ActorAttributeSet actorAS => m_ActorAS;
        public ActorAttributeSet AS => m_ActorAS;

        //不应该存在决策过程，所以就应该只有单一选项，因为只有在选项不唯一的时候才会需要决策。
        //TODO：有哪些技能、有哪些标签，都完全随游戏设计内容变化。
        // private AbilityBase[] m_Abilities = new AbilityBase[(int)AbilityType.End];
        private AbilityBase[] m_Abilities = new AbilityBase[Enum.GetValues(typeof(AbilityType)).Length];

        //Tip：似乎“默认”这个机制确实很适合于行为控制。
        private AbilityBase m_DefaultAbility;
        private AbilityBase m_CurrentAbility;
        private AbilityType m_CurrentAbilityType;
        public AbilityType currentAbilityType => m_CurrentAbilityType;

        //注意序列化对象如果自己没有可序列化的成员，反序列化过程中并不会为其创建实例，就会出现空引用错误。
        [SerializeField] private AbilityTaskExecutor m_TaskExecutor = new AbilityTaskExecutor();
        public AbilityTaskExecutor taskExecutor => m_TaskExecutor;
        private BufferedInputHandler m_BufferInput = new BufferedInputHandler();
        public BufferedInputHandler bufferInput => m_BufferInput;

        private List<GEHandle> m_GEs = new List<GEHandle>();
 
        private void FixedUpdate()
        {
            
        }

        //TODO：监测逻辑还有很多需要考虑，主要是Ability的一些共同逻辑。
        private void Update()
        {
            m_BufferInput.OnTick(Time.deltaTime);
            //始终运行TaskExecutor
            m_TaskExecutor?.OnTick(Time.deltaTime); //Ques：或许有另外的位置？
            // CheckAbilityCompleted(); //检查Ability是否完成

            OnTickGE();
        }

        private void OnTickGE()
        {
            List<GEHandle> handlesToRemove = new List<GEHandle>();
            m_GEs.ForEach(handle =>
            {
                if (handle.OnTick(Time.deltaTime))
                {
                    handlesToRemove.Add(handle); //记录要移除的GE
                }
            });

            handlesToRemove.ForEach(handle => m_GEs.Remove(handle));

        }

        //Tip：由ASC来监测当前行为是否执行完成，而行为本身只需要实现IsCompleted向ASC返回自己是否已经执行完成的信息。
        // private void CheckAbilityCompleted()
        // {
        //     if (m_CurrentAbility != null && m_CurrentAbility.IsCompleted())
        //     {//调用TryDeactivate就是想要打断当前的Ability，调用Deactivate就像这里是已经确认完成了，由ASC调用其Deactivate。
        //         m_CurrentAbility.Deactivate();
        //     }
        // }

        public void SetDefaultAbility(AbilityBase _ability)
        {
            m_DefaultAbility = _ability;
            m_DefaultAbility.AddToASC(this);
        }

        //Tip：处理技能。注册和注销技能
        //注册技能，代表可以执行。也可以用“LoadAbility载入技能”
        public void AddAbility(AbilityBase _ability)
        {
            _ability.AddToASC(this);
            m_Abilities[(int)_ability.Tag] = _ability;
            //TODO：是否考虑覆盖的情况？现在是默认直接覆盖。大概是应该再加一些逻辑的。
        }
        public void RemoveAbility(AbilityBase _ability)
        {
            //要存在，才能注销。
            if (m_Abilities[(int)_ability.Tag] != _ability) return;

            _ability.RemoveFromASC();
            m_Abilities[(int)_ability.Tag] = null;

        }

        /*Tip：Ability就是行为，始终都在执行某个行为，所以当调用Ability的TryDeactivate时，就必然是想要执行另一个行为。*/
        public void ExecuteAbility(AbilityType _type)
        {
            //数组没有对应槽位，或者是槽位上为空，就直接返回了
            if (m_Abilities.Count() <= (int)_type || m_Abilities[(int)_type] == null) return;

            ExecuteAbility(m_Abilities[(int)_type]);
        }
        //可以临时执行一个Ability
        public void ExecuteAbility(AbilityBase _ability)
        {
            /*Tip：这里算是一点决策逻辑，决策在于是否要指定所指定的Ability。*/
            //先看看当前执行的Ability
            if (m_CurrentAbility != null)
            {
                //现在无法退出，那就不会执行指定的Ability。如果返回为true，则同时执行了Deactivate方法，所以随后就直接执行Activate了。
                if (m_CurrentAbility.TryDeactivate() == false)
                {
                    return;
                }
            }

            //找到，然后激活，就进入到了Ability的逻辑中。记录当前正在执行的Ability。
            m_CurrentAbility = _ability;
            m_CurrentAbility.Activate();
            //监测完成事件。这里意思是，默认都要在完成时执行默认行为。
            m_CurrentAbility.SetCompletedCallback(ExecuteDefaultAbility);
            m_CurrentAbilityType = m_CurrentAbility.Tag;
        }

        //默认行为，应当是ASC自己维护的，当然要么在编辑器中指定，要么就是运行时由外部传入。
        public void ExecuteDefaultAbility()
        {
            // Debug.Log("执行默认行为");
            
            if (m_DefaultAbility == null)
            {
                Debug.LogError("ASC没有设置默认行为，请检查");
                return;
            }

            //先看看当前执行的Ability
            if (m_CurrentAbility != null)
            {
                if (m_CurrentAbility.TryDeactivate() == false)
                {
                    return;
                }
            }

            m_CurrentAbility = m_DefaultAbility;
            m_CurrentAbility.Activate();
            m_CurrentAbilityType = AbilityType.None;
        }

        //Tip:处理效果
        //伤害也是通过构造一个GE来作用。
        
        public void ApplyGameplayEffect(GameplayEffect _ge)
        {
            //作用于属性集
            switch (_ge.effectType)
            {
                case EffectType.Instant:
                    _ge.Apply(m_ActorAS);
                    break;
                case EffectType.HasDuration:
                case EffectType.Infinite:
                    m_GEs.Add(new GEHandle(_ge, m_ActorAS));
                    break;

            }
        }

        public void ApplyGameplayEffects(GameplayEffect[] _ges)
        {
            foreach (var ge in _ges)
            {
                ApplyGameplayEffect(ge);
            }
        }

    }

}