using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    public abstract class NodeData : ScriptableObject
    {
        public enum State {
            Running,
            Failure,
            Success
        }
        //公开，因为需要在代码中访问，但是需要限制在检视面板中可以直接进行编辑的字段。
        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        //用于与NodeView的viewDataKey对应，可以由数据节点获取到对应的视图节点。所以不是指的文件本身的guid，Unity会自动为每个文件分配一个meta文件，其中就有其独特的GUID
        [HideInInspector] public string guid; 

        /*TODO：这个值是用于编辑器中记录当前节点在GraphView中的位置，同时会影响节点的优先级，但对于运行时的本质影响就是在节点容器中的顺序，所以应该算作是编辑时成员而非运行时成员。
        不过问题核心并非这里的成员，而是整个运行时行为树的类型结构设计就有问题，*/
        [HideInInspector] public Vector2 position;
        /*TODO：context和blackboard非常重要，节点要完成任务几乎必然要从中访问自己需要的对象或数据*/
        [HideInInspector] public Context context; //作用对象（控制对象身上的各部分），也就是代行对象，委托其执行一些特定任务。
        [HideInInspector] public BehaviourTreeBlackboard blackboard; //每一个节点都能够直接访问黑板
        [TextArea] public string description; //节点描述,可以描述节点作用，以及需要的黑板变量。这就是完全靠开发者遵守开发规范了。
        public bool drawGizmos = false;

        // protected NodeData parent = null; //显式设为空。
        [HideInInspector] public NodeData parent = null; //必须要序列化，否则Undo不会记录。

        //TODO：Update改为Tick更加合适，返回值就代表此次运行的结果状态。可以认为，Tick是单独的，而Update通常会和Start之类的方法成为一个整体。
        //一个Udpate方法成功将will、ing、ed整合起来
        public State Update()
        {

            if (!started)
            {
                //在基类中统一调用获取黑板变量的方法。默认为空，各个派生节点根据自己所需要的黑板变量重写该方法即可
                /*Tip：受到值类型和引用类型的机制影响，也不一定就会在开头调用，且只在开头调用一次，也可能会在Update中刷新变量值，就是因为值类型副本不会跟随原本值的变化.
                这也是为什么要进行封装，将所有数据类型封装为各自的一个类，这样就可以获取引用，就可以随时访问其值*/
                //准备好Context和Blackboard，再执行逻辑。
                GetContextObjects();
                GetBlackboardVariables();
                OnStart();
                started = true;
            }

            state = OnUpdate();
            //通常是每帧运行一次，但是Running就会延续，而正是如此就引入了打断的机制
            /*Tip：通常来说*/
            if (state != State.Running) 
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual NodeData Clone() {
            return Instantiate(this);
        }

        /// <summary>
        /// 打断该节点
        /// </summary>
        public void Abort() {
            BehaviourTree.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        //没有继承自MonoBehaviour，所以并非消息方法
        //有些节点比如检测个体，就可以在场景中绘制一个扇形范围来示意。
        public virtual void OnDrawGizmos() { }
        /*TODO：不仅是黑板变量，还应该有从context获取自己需要的对象的方法，因为要用到的对象类型完全确定，只是实际引用的实例可能会变化，这也可以通过再次调用该方法来更新。*/
        protected virtual void GetContextObjects() { }
        protected virtual void GetBlackboardVariables() { }

        /*Tip：实际来看，应该只有OnUpdate是所有节点都需要定义的逻辑，而OnStart和OnStop主要是给动作节点的回调，因为存在持续运行的情况，而其他的节点只是在遍历行为树时执行一次Update即可。*/
        protected virtual void OnStart() {}
        // protected abstract void OnStop();
        protected virtual void OnStop() {}
        protected abstract State OnUpdate(); //返回的就是执行情况，或者说执行状态。
    }
}