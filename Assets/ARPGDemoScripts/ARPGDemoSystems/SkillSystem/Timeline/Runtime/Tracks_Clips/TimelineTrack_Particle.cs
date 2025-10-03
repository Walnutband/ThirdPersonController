
using System.Collections.Generic;

namespace ARPGDemo.SkillSystemtest
{
    public class TimelineTrack_Particle : TimelineTrack<TimelineClip_Particle>
    {
        // private List<TimelineClip_Particle> m_Clips;
        // protected override IEnumerable<TimelineClip> clips => m_Clips;

        public override void Initialize(TimelineContext _ctx)
        {
            throw new System.NotImplementedException();
        }

    }

    public class TimelineClip_Particle : TimelineClip
    {
        protected override void OnBegin(double _localTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void Running(double _localTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEnd()
        {
            throw new System.NotImplementedException();
        }
    }
}