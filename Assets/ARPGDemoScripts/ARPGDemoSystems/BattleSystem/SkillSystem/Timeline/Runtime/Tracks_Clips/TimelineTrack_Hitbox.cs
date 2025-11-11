
using System.Collections.Generic;
using ARPGDemo.BattleSystem;

namespace ARPGDemo.SkillSystemtest
{
    public class TimelineTrack_Hitbox : TimelineTrack<TimelineClip_Hitbox>
    {
        // private List<TimelineClip_Hitbox> m_Clips = new List<TimelineClip_Hitbox>();
        // protected override IEnumerable<TimelineClip> clips => m_Clips;

        /*TODO：Hitbox类型待定。*/
        private CollisionDetector m_Hitbox;
        private IAttacker m_Attacker;

        public override void Initialize(TimelineContext _ctx)
        {
            m_Hitbox = _ctx.hitbox;
            m_Attacker = _ctx.attacker;
            // m_Hitbox.triggerEnter += _ctx.hitCallback; //注册回调

            foreach (var clip in m_Clips)
            {
                clip.Init(m_Hitbox, m_Attacker);
            }

            // clips.ForEach(clip =>
            // {
            //     // TimelineClip_Hitbox myclip = clip as TimelineClip_Hitbox;

                // });
        }

    }

    public class TimelineClip_Hitbox : TimelineClip
    {
        private CollisionDetector m_Hitbox;
        private IAttacker m_Attacker;

        public void Init(CollisionDetector _hitbox, IAttacker _attacker)
        {
            m_Hitbox = _hitbox;
            m_Attacker = _attacker;
        }
        
        protected override void OnBegin(double _localTime)
        {
            // m_Hitbox.triggerEnter = null;
            m_Hitbox.triggerEnter += collider =>
            {
                if (collider.TryGetComponent<IDefender>(out var defender))
                {
                    DamageHandler.Instance.DoDamage(m_Attacker, defender, m_Attacker.damage, defender.defense);
                }
            };
            m_Hitbox.EnableDetector();
        }

        protected override void Running(double _localTime)
        {
            
        }

        protected override void OnEnd()
        {
            m_Hitbox.DisableDetector();
            //TODO：按理来说应该注销回调方法。
            // m_Hitbox
        }
    }
}