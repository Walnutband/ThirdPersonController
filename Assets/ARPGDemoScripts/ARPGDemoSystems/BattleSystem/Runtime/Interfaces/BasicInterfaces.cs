
public interface IBattleHandler
{
    void OnFixedUpdate(); //被周期方法FixedUpdate所调用，主要是为了能够手动控制调用的顺序，而不是让Unity自动调用。
}