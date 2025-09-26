
using System.Collections.Generic;

namespace ARPGDemo.SkillSystemtest
{
    public class TimelineTrack_Audio : TimelineTrack
    {
        private List<TimelineClip_Audio> m_Clips = new List<TimelineClip_Audio>();
        protected override IEnumerable<TimelineClip> clips => m_Clips;

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

    }

    public class TimelineClip_Audio : TimelineClip
    {
        protected override void OnStart(float _localTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Running(float _localTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEnd()
        {
            throw new System.NotImplementedException();
        }
    }

}