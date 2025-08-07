using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools.BehaviourTreeTool
{
    public abstract class Node : ScriptableObject {
        public enum State {
            Running,
            Failure,
            Success
        }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Context context; //作用对象（控制对象身上的各部分）
        [HideInInspector] public Blackboard blackboard;
        [TextArea] public string description; //节点描述
        public bool drawGizmos = false;

        //一个Udpate方法成功将will、ing、ed整合起来
        public State Update() {

            if (!started) {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual Node Clone() {
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

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
    }
}