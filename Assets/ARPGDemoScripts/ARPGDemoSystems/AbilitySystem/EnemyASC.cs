
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    public class EnemyASC : MonoBehaviour, IAbilitySystemComponent
    {
        // public EnemyAttributeSet AS;
        public ActorAttributeSet m_AS;
        public ActorAttributeSet AS {get => m_AS;}

        private List<GEHandle> m_GEs = new List<GEHandle>();

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
                    break;

            }
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
                if (handle.OnTick(Time.deltaTime))
                {
                    handlesToRemove.Add(handle); //记录要移除的GE
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