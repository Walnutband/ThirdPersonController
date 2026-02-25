using Physalia.Flexi;

namespace ARPGDemo.Test
{
    
    public class DefaultAbilityContainer : AbilityContainer
    {
        private GameSystem m_GameSystem;
        public GameSystem gameSystem => m_GameSystem;

        public DefaultAbilityContainer(GameSystem _gameSystem, AbilityData abilityData, int groupIndex)
            : base(abilityData, groupIndex)
        {
            m_GameSystem = _gameSystem;
        }
    }

    public abstract class DefaultEntryNode<TEventContext>
        : EntryNode<DefaultAbilityContainer, TEventContext> where TEventContext : IEventContext
    {

    }

    public abstract class DefaultEntryNode<TEventContext, TResumeContext>
        : EntryNode<DefaultAbilityContainer, TEventContext, TResumeContext>
        where TEventContext : IEventContext where TResumeContext : IResumeContext
    {

    }

    public abstract class DefaultProcessNode : ProcessNode<DefaultAbilityContainer>
    {

    }

    public abstract class DefaultProcessNode<TResumeContext>
        : ProcessNode<DefaultAbilityContainer, TResumeContext> where TResumeContext : IResumeContext
    {

    }

    public abstract class DefaultModifierNode : ModifierNode<DefaultAbilityContainer>
    {

    }

    public abstract class DefaultValueNode : ValueNode<DefaultAbilityContainer>
    {

    }
}
