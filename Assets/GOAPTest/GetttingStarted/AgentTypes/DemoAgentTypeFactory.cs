using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace CrashKonijn.Docs.GettingStarted.AgentTypes
{
    public class DemoAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ScriptDemoAgent");

            factory.AddCapability<Capabilities.IdleCapabilityFactory>();
            factory.AddCapability<Capabilities.PearCapability>();

            return factory.Build();
        }
    }
}