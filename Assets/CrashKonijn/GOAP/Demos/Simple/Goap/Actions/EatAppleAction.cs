using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Demos.Simple.Behaviours;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace CrashKonijn.Goap.Demos.Simple.Goap.Actions
{
    [GoapId("Simple-EatAppleAction")]
    public class EatAppleAction : GoapActionBase<EatAppleAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {//开始时，从库存中取出一个苹果。
            data.Apple =  data.Inventory.Hold();
        }
        
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            //没有苹果吃啊。
            if (data.Apple == null)
                return false;
            //记录饥饿值的组件。
            if (data.SimpleHunger == null)
                return false;

            return true;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Apple == null)
                return ActionRunState.StopAndLog("Apple is null.");
            
            if (data.SimpleHunger == null)
                return ActionRunState.StopAndLog("SimpleHunger is null.");

            //补充营养，只是这里的逻辑有点不合常理。。。
            var eatNutrition = context.DeltaTime * 20f;

            data.Apple.nutritionValue -= eatNutrition;
            data.SimpleHunger.hunger -= eatNutrition;
            /*TODO：持续进行的动作，应当有专门的计时器，因为本来就是按照Update每帧的频率来执行的，显然不会以相邻帧间隔作为执行间隔，否则与1帧执行应该没有任何区别（除非专门设计，
            但是我真没想到有什么设计会这样做），而且动作执行肯定还会牵涉到动画控制，还会引出一堆问题。*/
            if (data.Apple.nutritionValue <= 0)  
            {
                return ActionRunState.Completed;
            }
            
            return ActionRunState.Continue;
        }
        
        public override void Stop(IMonoAgent agent, Data data)
        {
            this.Finish(agent, data);
        }

        public override void Complete(IMonoAgent agent, Data data)
        {
            this.Finish(agent, data);
        }

        private void Finish(IMonoAgent agent, Data data)
        {
            if (data.Apple == null)
                return;
            
            //苹果已经没有营养了，就是全部用来消除饥饿值了，就直接从库存清理了，如果还有营养的话就放回去。
            if (data.Apple.nutritionValue <= 0)
            {
                data.Inventory.Drop(data.Apple);
                Object.Destroy(data.Apple.gameObject);
                return;
            }
            
            data.Inventory.Put(data.Apple);
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public AppleBehaviour Apple { get; set; }
            
            [GetComponent]
            public SimpleHungerBehaviour SimpleHunger { get; set; }
            
            [GetComponent]
            public InventoryBehaviour Inventory { get; set; }
        }
    }
}