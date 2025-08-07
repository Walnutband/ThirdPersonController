using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool
{
    public class BehaviourTreeRunner : MonoBehaviour { //行为树执行器，挂载到游戏对象上，执行指定的行为树来控制对象的行为

        // The main behaviour tree asset
        public BehaviourTree tree; //执行的行为树（资产文件）

        // Storage container object to hold game object subsystems
        Context context;

        // Start is called before the first frame update
        void Start() {
            context = CreateBehaviourTreeContext(); //创建环境（运行时）
            tree = tree.Clone();  //将tree转换为对于运行时副本的引用，避免污染原本的资产文件
            tree.Bind(context); //绑定上下文和黑板
        }

        // Update is called once per frame
        void Update() {
            if (tree) {
                tree.Update();
            }
        }

        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject); //创建该对象的各个控制对象
        }

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