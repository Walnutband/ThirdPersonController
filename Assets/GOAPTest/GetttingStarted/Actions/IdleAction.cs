using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using Random = UnityEngine.Random;

namespace CrashKonijn.Docs.GettingStarted
{
    // The GoapId attribute is used to identify the action, even when you change the name
    // This is used when using the Scriptable Object method of configuring actions
    [GoapId("Idle-ccc6f46c-1626-44aa-b90d-1b2741642166")]
    public class IdleAction : GoapActionBase<IdleAction.Data>
    {
        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = Random.Range(0.5f, 1.5f);
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Timer <= 0f)
                // Return completed to stop the action
                return ActionRunState.Completed;

            // Lower the timer for the next frame
            data.Timer -= context.DeltaTime;

            // Return continue to keep the action running
            return ActionRunState.Continue;
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer { get; set; }
        }
    }
}


// using CrashKonijn.Agent.Core;
// using CrashKonijn.Goap.Runtime;
// using UnityEngine;

// namespace CrashKonijn.Docs.GettingStarted
// {
//     [GoapId("Idle-4b29d893-08a2-4691-b764-ec0fec2cba1e")]
//     public class IdleAction : GoapActionBase<IdleAction.Data>
//     {
//         // This method is called when the action is created
//         // This method is optional and can be removed
//         public override void Created()
//         {
//         }

//         // This method is called every frame before the action is performed
//         // If this method returns false, the action will be stopped
//         // This method is optional and can be removed
//         public override bool IsValid(IActionReceiver agent, Data data)
//         {
//             return true;
//         }

//         // This method is called when the action is started
//         // This method is optional and can be removed
//         public override void Start(IMonoAgent agent, Data data)
//         {
//         }

//         // This method is called once before the action is performed
//         // This method is optional and can be removed
//         public override void BeforePerform(IMonoAgent agent, Data data)
//         {
//         }

//         // This method is called every frame while the action is running
//         // This method is required
//         public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
//         {
//             return ActionRunState.Completed;
//         }

//         // This method is called when the action is completed
//         // This method is optional and can be removed
//         public override void Complete(IMonoAgent agent, Data data)
//         {
//         }

//         // This method is called when the action is stopped
//         // This method is optional and can be removed
//         public override void Stop(IMonoAgent agent, Data data)
//         {
//         }

//         // This method is called when the action is completed or stopped
//         // This method is optional and can be removed
//         public override void End(IMonoAgent agent, Data data)
//         {
//         }

//         // The action class itself must be stateless!
//         // All data should be stored in the data class
//         public class Data : IActionData
//         {
//             public ITarget Target { get; set; }
//         }
//     }
// }