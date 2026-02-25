using System.Collections.Generic;

namespace Physalia.Flexi
{
    public abstract class ProcessNode<TContainer> : ProcessNode
        where TContainer : AbilityContainer
    {
        public TContainer Container => GetContainer<TContainer>();
    }

    public abstract class ProcessNode<TContainer, TResumeContext> : ProcessNode<TContainer>
        where TContainer : AbilityContainer
        where TResumeContext : IResumeContext
    {
        internal sealed override bool CheckCanResume(IResumeContext resumeContext)
        {
            if (resumeContext != null && resumeContext is TResumeContext context)
            {
                return CanResume(context);
            }
            return false;
        }

        protected abstract bool CanResume(TResumeContext resumeContext);

        internal sealed override FlowState ResumeInternal(IResumeContext resumeContext)
        {
            TResumeContext context = resumeContext is TResumeContext resumeContextTyped ? resumeContextTyped : default;
            return OnResume(context);
        }

        protected abstract FlowState OnResume(TResumeContext resumeContext);
    }

    public abstract class ProcessNode : FlowNode
    {
        internal Inport<FlowNode> previous;
        internal Outport<FlowNode> next;

        public sealed override FlowNode Next
        {
            get
            {
                IReadOnlyList<Port> connections = next.GetConnections();
                return connections.Count > 0 ? connections[0].Node as FlowNode : null;
            }
        }

        private protected sealed override FlowState ExecuteInternal()
        {
            return OnExecute();
        }

        protected virtual FlowState OnExecute()
        {
            return FlowState.Success;
        }
    }
}
