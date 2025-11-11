using System;
using System.Linq;

namespace RPGCore.Dialogue.Runtime
{
	[Serializable]
	public class DgNodeActionBase : DgNodeBase, IDgNode
	{
        public Action OnAction { get; protected set; }

        public DgNodeActionBase() : base(DgNodeType.Action)
        {

        }
		public override IDgNode GetNext(object param)
		{
			if (NextNodes.Count() >= 1)
			{
				return NextNodes[0];
			}
			return null;
		}

		public void SetAction(Action action) 
		{
			OnAction = null; //先清空再注册就可以使用匿名方法，因为不会被之前注册的方法影响。
			OnAction = action;
		}
	}
}