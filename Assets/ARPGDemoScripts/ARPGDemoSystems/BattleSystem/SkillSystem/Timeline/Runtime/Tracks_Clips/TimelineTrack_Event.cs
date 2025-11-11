
namespace ARPGDemo.SkillSystemtest
{
    /*TODO：使用一个*/

    public class TimelineTrack_Event : TimelineTrack<TimelineClip_Event>
    {
        public override void Initialize(TimelineContext ctx)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TimelineClip_Event : TimelineClip
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