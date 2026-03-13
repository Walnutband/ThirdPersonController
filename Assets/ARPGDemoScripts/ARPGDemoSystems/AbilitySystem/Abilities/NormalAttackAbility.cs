using System;
using System.Linq;
using ARPGDemo.BattleSystem;
using ARPGDemo.ControlSystem;
using ARPGDemo.ControlSystem.InputActionBindings;
using ARPGDemo.CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.AbilitySystem
{

    /*Ques：似乎不需要搞成资产复用，那是实例复用，类型和数据完全相同，似乎代表的是一个攻击模组。但是应该也不能直接复用，因为会有一些数据发生调整，而且最关键的是，
    关于复用攻击模组，有没有可能，真正提高效率和降低成本的根本就不是引用同一个这样的资产，而是已经做好适配的那些动画等资产和测试好的固有逻辑？？
    */
    //连段攻击，逻辑有所不同。
    // public class NormalAttackAbility : AbilityBase
    [Serializable]
    public class ComboAttackAbility : AbilityBase //Tip：似乎ComboAttack才是“行为逻辑”，NormalAttack偏向“行为表现”了，而行为表现应该是由变量名来表达。 
    {
        /*Tip：要不要从ASC或者其他对象获取InputAction而不是直接在检视器中单独编辑？这是早期想法，现在完全不这么认为，因为这些输入绑定是纯数据配置的，就是策划的事，就是应该
        这样分开独立地编辑，这样策划就有最灵活的编辑空间。*/
        // [SerializeField] private InputActionReference m_AttackAction;
        // [DisplayName("普通攻击输入")]
        [ExpandInlineProperties("普通攻击输入")]
        [SerializeField] private InputActionBinder<InvokeSpecifiedCallback> m_ComboAttackInput; //普攻连段。
        [ExpandInlineProperties("重击输入")]
        [SerializeField] private InputActionBinder<InvokeSpecifiedCallback> m_HeavyAttackInput; //重击。
        [Header("连段攻击")] 
        // [ExpandInlineProperties("连段攻击")] //TODO：该特性现在对容器字段无效，暂时加个Header补充。
        [SerializeField] private SingleAttack[] m_ComboAttacks;
        [ExpandInlineProperties("重击")]
        [SerializeField] private SingleAttack m_HeavyAttack; //Tip：其实重击也可以有多段，不过这里就默认只有单独的一段了。

        /*Tip：如何理解“一段伤害”所需的信息是关键——应该考虑到逻辑层和表现层，表现层主要就是基于时间轴的内容以及用于UI显示的信息，逻辑层主要就是初始伤害来源，*/
        //每一段伤害，来自于特定基础属性乘以一定倍率，在属性相同的情况下就直接比较倍率即可得知伤害高低
        // [SerializeField] private ActorAttributeSet.AttributeValueAcquirer[] acquirers;

        private ComboCounter m_ComboCounter;

        //对于连段攻击来说，Activate就是第一段攻击开始，之后连段就是在Ability内部处理。

        private AbilityTask_SingleAttack.TaskHandle m_CurrentTaskHandle; //始终都在执行对应的AbilityTask。
        //记录Attack，主要还是为了访问acquirer，因为这不属于Task的逻辑，而TaskHandle只负责提供Task的相关信息（执行情况）。
        private SingleAttack m_CurrentAttack;

        /*Tip：Activate和Deactivate就相当于State的OnEnter和OnExit，即行为准备和行为收尾的逻辑，而行为执行的监测逻辑就是将数据存储在了AbilityTask、委托给AbilityTaskExecutor进行
        监测执行了。
        */
        public override void Activate()
        {
            EnableInput();
            ComboAttackStarted(); //开始执行第一段攻击。
            m_ASC.AMC.SetMoveAndRotate(false, false);
        }

        //Tip：返回false就是不能Deactivate，返回true就是可以且已经执行了Deactivate。
        public override bool TryDeactivate()
        {
            // Debug.Log("TryDeactivate普攻");
            //不能退出，直接返回false。
            if (m_CurrentTaskHandle.CanExit() != true) return false;

            Deactivate();
            return true;
        }

        public override void Deactivate()
        {
            // Debug.Log(" Deactivate普攻");
            DisableInput();
            //Tip：如果是正常结束（完成），在Executor中就会触发完成事件，如果非完成而进入了该方法，说明是因为外部指令，那么这里就不要触发完成事件了，但是SubTask的End照样触发。
            m_CurrentTaskHandle.CompleteTask(false); 
            m_ASC.AMC.SetMoveAndRotate(true,true);
        }

        //激活Ability时执行，处理一些准备逻辑，就像状态机中State的OnEnter一样，然后是直接执行第一段攻击。
        private void ComboAttackStarted()
        {
            //攻击都没有就直接结束了。
            if (m_ComboAttacks.Count() <= 0)   
            {
                Debug.LogError($"在激活ComboAttackAbility时，没有存储任何攻击数据，请检查");
                return;
            }
            m_ComboCounter = new ComboCounter(m_ComboAttacks.Count());
            // Debug.Log("执行第一段攻击，索引：" + m_ComboCounter.index);
            ExecuteSpecifiedAttack(m_ComboAttacks[m_ComboCounter.index]);
        }

        /*Tip：这就是要作用于检测到的攻击目标的方法，通过注册到检测事件上调用。也就是在Hitbox检测到目标时，就应该回到这里处理逻辑。
        这也算是伤害流程的开端。
        */
        private void ActOnTarget(GameObject _target)
        {
            Debug.Log($"检测到目标{_target.name}");
            // AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("39_Block_03 1"), Vector3.zero);
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("blade_hit_07"), m_ASC.transform.position);
            // return;

            // return;
            //TODO：确定要作用于对方的ASC。应该直接提供获取，也就是对方自行指定，不应该自己获取。
            // AbilitySystemComponent targetASC;
            IAbilitySystemComponent targetASC;
            if ((targetASC = _target.GetComponentInParent<IAbilitySystemComponent>()) == null) return;

            //从属性集获取初始值。
            float initialValue = m_CurrentAttack.acquirer.GetValue(m_ASC.actorAS);
            Debug.Log("伤害值：" + initialValue);
            //说明暴击了。乘以暴击伤害。
            if (UnityEngine.Random.Range(0f, 1f) <= m_ASC.actorAS.GetAttributeCurrentValue(ActorAttributeSet.AttributeType.CritRate))
            {
                initialValue *= 1 + m_ASC.actorAS.GetAttributeCurrentValue(ActorAttributeSet.AttributeType.CritDamage);
                Debug.Log("发生暴击！暴击后伤害为：" + initialValue);
            }
            //给对方造成伤害。
            targetASC.ApplyGameplayEffect(GEBuilder.CreateDamageEffect(initialValue));
            //TODO：额外效果，其实应该和伤害一起处理的。
            targetASC.ApplyGameplayEffects(m_CurrentAttack.effects);
        }

        private void EnableInput()
        {
            // Debug.Log($"NormalAttackAbility，EnableInput");
            m_ComboAttackInput.Enable();
            m_HeavyAttackInput.Enable();
            m_ComboAttackInput.bindedEvent.AddCallback(TriggerComboAttack);
            m_HeavyAttackInput.bindedEvent.AddCallback(TriggerHeavyAttack);
        }
        private void DisableInput()
        {
            m_ComboAttackInput.Disable();
            m_HeavyAttackInput.Disable();
            m_ComboAttackInput.bindedEvent.RemoveCallback(TriggerComboAttack);
            m_HeavyAttackInput.bindedEvent.RemoveCallback(TriggerHeavyAttack);
        }

        //Tip：触发连段攻击和触发重击，分别注册到对应的输入
        // private void TriggerComboAttack()
        private void TriggerComboAttack()
        {
            // if (m_ComboCounter.isEnded == true || m_CurrentTaskHandle.CanCombo() != false)
            //Tip：之前看错了，其实原神中最后一段可以继续回到第一段开始，因为也是按照连击输入（Tap抬起鼠标左键）来触发的。
            if (m_CurrentTaskHandle.CanCombo() != true) //如果未处于可连击区间。
            {
                //已经在执行最后一段攻击，所以不再响应，直到这最后一段的CanExit为true，则外部可以再次激活该Ability，也就是回到第一段攻击。
                //当前正在进行的攻击，未处于可连击区间。
                m_ASC.bufferInput.SetBuffer(CachedTriggerComboAttack);
                return;
            }

            m_ComboCounter.Next(false);
            // Debug.Log($"连段索引：{m_ComboCounter.index}");
            ExecuteSpecifiedAttack(m_ComboAttacks[m_ComboCounter.index]);

            return;
        }

        private bool CachedTriggerComboAttack()
        {
            if (m_CurrentTaskHandle.CanCombo() != true) //如果未处于可连击区间。
            {
                return false;
            }

            m_ComboCounter.Next(false);
            ExecuteSpecifiedAttack(m_ComboAttacks[m_ComboCounter.index]);

            return true;
        }

        // private void TriggerHeavyAttack()
        private void TriggerHeavyAttack()
        {
            //基于原神的设计，在最后一段普攻无法触发重击。
            //Tip：注意，参考原神，能够触发重击的区间并不结束于当前的CanExit，而是在
            // if (m_ComboCounter.isEnded == true || m_CurrentTaskHandle.CanExit() == true) return;
            // if (m_ComboCounter.isEnded == true || m_CurrentTaskHandle.IsEnded() == true) return;

            //TODO：暂定，只要在这个Ability期间，都可以触发重击。

            if (m_CurrentTaskHandle.CanCombo() != true)
            {
                m_ASC.bufferInput.SetBuffer(CachedTriggerHeavyAttack, 0.35f);
                return;
            }

            /*Tip：触发重击后，就不应该连段了，这里就通过连段判断所使用的ComboCounter来实现，不需要另外设置一个标记，这就是利用数据变化带来的额外信息。
            所以这样看，连段攻击和重击都会用到ComboCounter。
            */
            m_ComboCounter.Ended();
            ExecuteSpecifiedAttack(m_HeavyAttack);

            return;
        }

        private bool CachedTriggerHeavyAttack()
        {
            if (m_CurrentTaskHandle.CanCombo() != true)
            {
                return false;
            }
            m_ComboCounter.Ended();
            ExecuteSpecifiedAttack(m_HeavyAttack);

            return true;
        }

        //在调用之前一段逻辑，就是确定要执行哪个Attack，确定好之后就传入该方法即可。
        private void ExecuteSpecifiedAttack(SingleAttack _attack)
        {
            // Debug.Log("执行特定攻击ExecuteSpecifiedAttack");
            if (_attack == null)
            {
                Debug.LogError("在执行指定Attack时，发现传入的Attack为空，请检查");
                return;
            }
            //开始执行，记录handle以便获取执行的相关状态。
            m_CurrentAttack = _attack;
            m_CurrentTaskHandle = m_CurrentAttack.Execute(ActOnTarget, Completed, m_ASC.taskExecutor);
        }

    }

    //简单封装，仅用于拆分，方便管理。
    [Serializable]
    public class SingleAttack //Tip：在这里才遇到有必要
    {
        [DisplayName("伤害值描述")]
        public string valueDescription; //值描述，即倍率。（可以通过固定规则直接从requirer中获取然后解析，但是从编辑角度来说，我感觉不如直接编辑清楚算了。）
        //Tip：被Ability所使用，用于产生伤害，因为这部分逻辑本来就要在Ability所处的环境下（Context）才能成功执行，这里只是暂存数据而已。
        [ExpandInlineProperties("伤害计算规则")]
        public ActorAttributeSet.AttributeValueAcquirer acquirer; //从属性值按照一定倍率获取初始值。
        [DisplayName("作用效果")]
        public GameplayEffect[] effects;
        //Tip：
        [ExpandInlineProperties("<b>表现层内容</b>")]
        public AbilityTask_SingleAttack task; //每段攻击都是不同的Task，但是可能使用的是同一个Hitbox，所以要注意注册的Hit回调方法不要冲突。
        public AbilityTask_SingleAttack.TaskHandle Execute(HitCallback _action, Action _completed, AbilityTaskExecutor _executor)
        {
            //监测命中事件
            task.SetHitCallback(_action); //直接覆盖设置自己的击中回调，不管之前到底有什么回调。
            //监测完成事件
            //Tip：注意并非Ability自己要监测Task完成，本来Task代表的就是Ability的执行内容，只是ASC要监测Ability的完成，而Ability是委托给Task执行，所以将完成回调传递到Task。
            task.SetCompletedCallback(_completed);
            //Tip：注意这里传入Task时就已经包含了Handle的信息，非常便捷。
            // return _executor.ExecuteTask(task); //委托执行器执行任务（Task），返回记录可以获取运行状态的句柄（Handle） 
            return _executor.ExecuteTask(task, true); //委托执行器执行任务（Task），返回记录可以获取运行状态的句柄（Handle） 
        }

    }

    // /*Tip：并非只是存储数据，而是作为Ability尤其是（几乎专用于）ComboAttackAbility的一个子执行器，而Ability就是负责调用和传入所需数据，。*/
    // //TODO：单个攻击，就是代表一段攻击行为，“攻击行为”应该只是“行为”的一个子集，需要进一步探究所谓“行为”本质。

    // //Ques：有点变态，之前没有这样写过，难道说，就应该将Handle作为泛型参数，而在变量中直接指定AbilityTask_AttackBase？
    // [Serializable]
    // public class SingleAttack<THandle> //Tip：在这里才遇到有必要
    // {
    //     [DisplayName("伤害值描述")]
    //     public string valueDescription; //值描述，即倍率。（可以通过固定规则直接从requirer中获取然后解析，但是从编辑角度来说，我感觉不如直接编辑清楚算了。）
    //     //Tip：被Ability所使用，用于产生伤害，因为这部分逻辑本来就要在Ability所处的环境下（Context）才能成功执行，这里只是暂存数据而已。
    //     [ExpandInlineProperties("伤害计算规则")]
    //     public ActorAttributeSet.AttributeValueAcquirer acquirer; //从属性值按照一定倍率获取初始值。
    //     //Tip：
    //     [ExpandInlineProperties("<b>表现层内容</b>")] 
    //     public AbilityTask_AttackBase<THandle> task; //每段攻击都是不同的Task，但是可能使用的是同一个Hitbox，所以要注意注册的Hit回调方法不要冲突。

    //     /*Tip：在执行时传入回调，而不是先在某处注册回调，然后再调用这里的Execute方法，很明显这里的处理更加优雅，因为本来就只有在执行时才会用到该信息。
    //     根本逻辑时，在调用该Execute方法时，才会开始执行，同时注册回调就意味着监视此次执行的实际情况，及时响应，而不是一开始就注册回调，之后才调用执行。
    //     还有这个SingleAttack封装了Task，所以应该将completed回调作为参数一并传入，如果Ability自己直接掌握Task的话，则应该自己先后Execute执行任务以及给Task设置回调。
    //     SingleAttack只需要通过方法参数接收来自于（使用者）Ability的信息，就是执行Task所需要的一些附带信息，本身并不需要也不应该引用Ability，因为本质上是一个代行对象，
    //     就像AnimatorAgent不会引用使用者一样。
    //     */
    //     //Tip：需要深思，为何在这里定义方法，而不是让Ability访问Task然后注册方法？当然本质上还是信息，这就是封装的意义。
    //     public THandle Execute(HitCallback _action, Action _completed, AbilityTaskExecutor _executor)
    //     {
    //         //监测命中事件
    //         task.SetHitCallback(_action); //直接覆盖设置自己的击中回调，不管之前到底有什么回调。
    //         //监测完成事件
    //         task.SetCompletedCallback(_completed);
    //         //Tip：注意这里传入Task时就已经包含了Handle的信息，非常便捷。
    //         return _executor.ExecuteTask(task); //委托执行器执行任务（Task），返回记录可以获取运行状态的句柄（Handle） 
    //     }

    // }

    //Tip：专门用于Combo计数。
    
    public class ComboCounter
    {
        private int m_CurrentIndex;
        public int index => m_CurrentIndex;
        private int m_Count;
        public bool isEnded => m_CurrentIndex >= m_Count;

        public ComboCounter(int _count)
        {
            Started(_count);
        }

        //直接结束。
        public void Ended() => m_CurrentIndex = m_Count;

        public void Next(bool _tobegin = false)
        {
            if (_tobegin == true) m_CurrentIndex = (m_CurrentIndex + 1) / m_Count;
            else if (++m_CurrentIndex >= m_Count) m_CurrentIndex = m_Count; //保持在最后一个索引的下一个，作为已经结束的标志。
        }

        public void Started(int _count)
        {
            m_CurrentIndex = 0; //当然从0开始。
            m_Count = _count;
        }
    }
}