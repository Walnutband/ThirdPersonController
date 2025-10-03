
using System.Collections.Generic;

namespace ARPGDemo.SkillSystemtest
{
    public class TimelineTrack_Audio : TimelineTrack<TimelineClip_Audio>
    {
        // private List<TimelineClip_Audio> m_Clips = new List<TimelineClip_Audio>();
        // protected override IEnumerable<TimelineClip> clips => m_Clips;

        public override void Initialize(TimelineContext _ctx)
        {
            throw new System.NotImplementedException();
        }

    }

    public class TimelineClip_Audio : TimelineClip
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