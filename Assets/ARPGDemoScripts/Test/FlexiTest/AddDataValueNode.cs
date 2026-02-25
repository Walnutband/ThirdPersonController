using Physalia.Flexi;

namespace ARPGDemo.Test
{
    [NodeCategory("Test/Process")]
    public class AddDataValueNode : DefaultProcessNode
    {
        public Inport<int> value; 

        protected override FlowState OnExecute()
        {
            Container.gameSystem.AddDataValue(value);
            return FlowState.Success;  // Always return FlowState.Success for normal cases.
        }
    }
}