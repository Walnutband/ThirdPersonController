
namespace ARPGDemo.AbilitySystem
{
    public static class GEBuilder
    {
        public static GameplayEffect CreateDamageEffect(float _value)
        {
            return new GameplayEffect(){effectType = EffectType.Instant, 
            modifier = new ActorAttributeSet.AttributeValueModifier(ActorAttributeSet.AttributeType.HP, ActorAttributeSet.ModifierType.SubByValue, _value)};
        }
    }
}