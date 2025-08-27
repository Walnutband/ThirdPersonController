using Animancer;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    [AddComponentMenu("ARPGDemo/ControlSystem/States/PlayerHeavyAttackState", 50)]
    public class PlayerHeavyAttackState : PlayerStateBehaviour
    {
        protected bool m_IsEnd;
        public override bool isEnd => m_IsEnd;

        //分为了两个部分，其实是从一个整体动画切出来的，Pre就是蓄力的部分，一旦松开就是Post释放的部分。
        [SerializeField] protected ClipTransition attackPre;
        [SerializeField] protected ClipTransition attackPost;

        protected bool m_IsCharging;
        public bool isCharging { get => m_IsCharging; set => m_IsCharging = value; }


        public override void OnEnterState()
        {
            base.OnEnterState();

            animPlayer.Play(attackPre).Events(this).OnEnd = () =>
            {
                animPlayer.Play(attackPost).Events(this).OnEnd = () =>
                {
                    m_IsEnd = true;
                };
            }; //满蓄
            m_IsCharging = true; //因为重击一开始就是蓄力
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (m_IsCharging == false) //说明蓄力完成
            {
                animPlayer.Play(attackPost).Events(this).OnEnd = () =>
                {
                    m_IsEnd = true;
                };
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();

            m_IsEnd = false;
            m_IsCharging = true;
        }
    }
}