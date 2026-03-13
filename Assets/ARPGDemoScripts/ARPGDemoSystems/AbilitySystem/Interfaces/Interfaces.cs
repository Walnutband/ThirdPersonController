
namespace ARPGDemo.AbilitySystem
{
    public interface IAbilitySystemComponent
    {
        ActorAttributeSet AS {get;}
        void ApplyGameplayEffect(GameplayEffect _ge);
        void ApplyGameplayEffects(GameplayEffect[] _ges);
    }
}