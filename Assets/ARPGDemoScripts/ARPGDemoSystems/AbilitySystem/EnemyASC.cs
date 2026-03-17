
using System.Collections.Generic;
using ARPGDemo.ControlSystem.Enemy;
using ARPGDemo.UISystem_Test;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public class EnemyASC : MonoBehaviour, IAbilitySystemComponent
    {
        // public EnemyAttributeSet AS;
        public ActorAttributeSet m_AS;
        public ActorAttributeSet AS {get => m_AS;}

        private List<GEHandle> m_GEs = new List<GEHandle>();

        public SimpleEnemyContorller controller;

        public void ApplyGameplayEffect(GameplayEffect _ge)
        {
            //作用于属性集
            switch (_ge.effectType)
            {
                case EffectType.Instant:
                    _ge.Apply(AS);
                    break;
                case EffectType.HasDuration:
                case EffectType.Infinite:
                    m_GEs.Add(new GEHandle(_ge, AS));
                    // m_AS.RegisterHPChangedEvent(damage => DamageNumberGenerator.Instance.SpawnDamageNumber(damage, transform.position, false));
                    m_AS.RegisterHPChangedEvent(Temp_DoDamageNumber);
                    break;

            }
            
            controller.OnHurt();
        }

        private void Temp_DoDamageNumber(float _damage)
        {
            DamageNumberGenerator.Instance.SpawnDamageNumber(_damage, transform.position, false);
        }

        private void Update()
        {
            OnTickGE();
        }

        private void OnTickGE()
        {
            List<GEHandle> handlesToRemove = new List<GEHandle>();
            m_GEs.ForEach(handle =>
            {
                handle.OnEffect = () => controller.OnHurt();
                if (handle != null)
                {
                    if (handle.OnTick(Time.deltaTime))
                    {
                        handlesToRemove.Add(handle); //记录要移除的GE
                        m_AS.UnregisterHPChangedEvent(Temp_DoDamageNumber);
                    }
                    // controller.OnHurt();
                }
            });

            handlesToRemove.ForEach(handle => m_GEs.Remove(handle));

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