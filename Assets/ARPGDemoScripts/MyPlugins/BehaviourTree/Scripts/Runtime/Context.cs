using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MyPlugins.BehaviourTree
{

    // The context is a shared object every node has access to.这是每个节点都有权访问的共享对象
    // Commonly used components and subsytems should be stored here共用的组件和子系统都应该存储在这里
    // It will be somewhat specific to your game exactly what to add here.显然这根据要控制的游戏对象本身，决定要添加什么内容
    // Feel free to extend this class 所以根据具体游戏定制，自由扩展。
    public class Context {
        public GameObject gameObject;
        public Transform transform;
        public Animator animator;
        public Rigidbody physics;
        public NavMeshAgent agent;
        public SphereCollider sphereCollider;
        public BoxCollider boxCollider;
        public CapsuleCollider capsuleCollider;
        public CharacterController characterController;
        // Add other game specific systems here
        
        /*TODO：从结构性来看，最好是在构造函数中*/

        public static Context CreateFromGameObject(GameObject gameObject)
        {
            // Fetch all commonly used components
            Context context = new Context();
            context.gameObject = gameObject;
            context.transform = gameObject.transform;
            context.animator = gameObject.GetComponent<Animator>();
            context.physics = gameObject.GetComponent<Rigidbody>();
            context.agent = gameObject.GetComponent<NavMeshAgent>();
            context.sphereCollider = gameObject.GetComponent<SphereCollider>();
            context.boxCollider = gameObject.GetComponent<BoxCollider>();
            context.capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            context.characterController = gameObject.GetComponent<CharacterController>();

            // Add whatever else you need here...

            return context;
        }
    }
}