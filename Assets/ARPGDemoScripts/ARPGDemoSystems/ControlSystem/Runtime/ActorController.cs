using ARPGDemo.AbilitySystem;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    
    //TODO：不同角色大概需要定制？
    public class ActorController : MonoBehaviour 
    {
        //
        // [SerializeField] private ComboAttackAbility m_NormalAttackAbility;
        [SerializeField] private AbilitySystemComponent m_ASC;

        private void Awake()
        {
            if (m_ASC == null) m_ASC = GetComponent<AbilitySystemComponent>();
        }

    }

    
}