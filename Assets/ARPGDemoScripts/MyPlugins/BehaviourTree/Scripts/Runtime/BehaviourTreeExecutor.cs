using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    [AddComponentMenu("BehaviourTree/BehaviourTreeExecutor")]
    public class BehaviourTreeExecutor : MonoBehaviour { //行为树执行器，挂载到游戏对象上，执行指定的行为树来控制对象的行为

        // The main behaviour tree asset
        public BehaviourTree tree; //执行的行为树（资产文件）
        public BehaviourTreeBlackboard blackboard; //使用的黑板（资产文件）

        // Storage container object to hold game object subsystems
        Context context;

        //在成员比较少的时候，比较方便地监测字段值改变，就可以直接写在OnValidate方法中。
        private void OnValidate()
        {   //及时更新为当前行为树的黑板。
            blackboard = tree == null ? null : tree.blackboard;
        }

        // Start is called before the first frame update
        void Start() {
            context = CreateBehaviourTreeContext(); //创建环境（运行时）
            //Tip：并非是所谓的防止污染，这是以前的错误理解。
            tree = tree.Clone();  //将tree转换为对于运行时副本的引用，避免污染原本的资产文件
            blackboard = blackboard.Clone();
            tree.Bind(context, blackboard); //绑定上下文和黑板
        }

        //运行行为树，
        void Update() {
            if (tree) {
                tree.Update();
            }
        }

        //Tip：按照规则，从所在的GO获取所需（组件）对象。
        /*TODO：具体是什么规则，还要看行为树中的那些节点到底要用到哪些对象，这些就属于“代行对象”，用于被委托执行一些特定任务，因为节点的逻辑主要还是调度而非自己执行，行为树本来做的就是决策，
        具体的执行内容就交给相应的对象去做了。
        */
        /*TODO：不过可能更加灵活的方式是让节点自己引用所要委托的代行对象，但是这样的话，就是资产引用场景对象，就需要ExposedReference了，而ExposedReference也需要专门的组件提供映射信息，
        所以从实际来说，或许这样设置固定的上下文，确实能满足基本需求。*/
        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject); //创建该对象的各个控制对象
        }

        //消息方法，在被选中所在GO时调用，每个节点可以自己实现OnDrawGizmos在场景中绘制自己的Gizmos
        private void OnDrawGizmosSelected() {
            if (!tree) {
                return;
            }

            
            BehaviourTree.Traverse(tree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }
    }

}