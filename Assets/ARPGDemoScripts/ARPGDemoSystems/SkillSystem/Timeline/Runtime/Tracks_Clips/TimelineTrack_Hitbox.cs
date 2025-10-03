
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
        
        public override void Initialize(TimelineContext _ctx)
        {
            // clips.ForEach(clip =>
            // {
            //     // TimelineClip_Hitbox myclip = clip as TimelineClip_Hitbox;

            // });
        }

    }

    public class TimelineClip_Hitbox : TimelineClip
    {
        private CollisionDetector m_Hitbox;

        public void Init(CollisionDetector _hitbox)
        {
            m_Hitbox = _hitbox;
        }
        
        protected override void OnBegin(double _localTime)
        {
            m_Hitbox.EnableDetector();
        }

        protected override void Running(double _localTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEnd()
        {
            m_Hitbox.DisableDetector();
        }
    }
}