using ARPGDemo.ControlSystem;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public class GameplayTag 
    {
        
    }
    /*Tip：行为管理器，个体能够执行什么行为，与决策、感知无关，专注于行为层的调度和执行。*/

    public class AbilitySystemComponent : MonoBehaviour 
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
        [SerializeField] private ActorMovementComponent m_MoveComp; //专门处理移动逻辑
        public ActorMovementComponent moveComp
        {
            get => m_MoveComp;
        }

        //管理属性集，暂时确定就是该属性集。
        private ActorAttributeSet m_ActorAS;
        public ActorAttributeSet actorAS => m_ActorAS;

        public void GiveAbility(AbilityBase _ability)
        {
            _ability.AddToASC(this);
        }
    }
}