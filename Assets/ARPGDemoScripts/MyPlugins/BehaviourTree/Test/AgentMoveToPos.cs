
using UnityEngine;
using UnityEngine.AI;

namespace MyPlugins.BehaviourTree.Test
{
    public class AgentMoveToPos : MonoBehaviour
    {
        public Transform goal;

        void Start()
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.destination = goal.position;
        }
    }
}