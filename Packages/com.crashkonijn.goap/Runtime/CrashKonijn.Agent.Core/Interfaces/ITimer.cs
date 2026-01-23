namespace CrashKonijn.Agent.Core
{
    public interface ITimer
    {
        //Touch就是更新时间。
        void Touch(); 
        //从上一次Touch到现在经过了多长时间。
        float GetElapsed();
        //是否已经经过了time时间。
        bool IsRunningFor(float time);
    }
}