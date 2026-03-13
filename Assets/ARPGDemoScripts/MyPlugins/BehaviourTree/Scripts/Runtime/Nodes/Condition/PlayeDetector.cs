
using UnityEngine;

namespace MyPlugins.BehaviourTree
{
    public class PlayerDetector : ConditionNode
    {
        private float radius = 5f;
        public Vector3 centerOffset = Vector3.zero;
        public LayerMask playerLayer = 1 << 9; // 默认为Player层

        // 检测到的玩家列表
        private Collider[] results = new Collider[10];
        private GameObject targetPlayer;

        protected override State OnUpdate()
        {
            // 计算检测中心点
            Vector3 detectCenter = context.transform.position + context.transform.TransformDirection(centerOffset);

            // 执行球形检测
            int numColliders = Physics.OverlapSphereNonAlloc(
                detectCenter,
                radius,
                results,
                playerLayer.value
            );

            // 检查是否找到玩家
            if (numColliders > 0)
            {
                // 获取第一个找到的玩家
                GameObject player = results[0].gameObject;

                // 验证是否为有效玩家（可选：检查是否有Player组件）
                if (IsValidPlayer(player))
                {
                    Debug.Log($"检测到玩家：{targetPlayer.name}");
                    targetPlayer = player;
                    return State.Success;
                }
            }

            targetPlayer = null;
            return State.Failure;
        }

        private bool IsValidPlayer(GameObject player)
        {
            // return player.CompareTag("Player") || player.GetComponent<PlayerController>() != null;
            return player.CompareTag("Player");
        }
    }
}