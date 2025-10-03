using System;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    /*Tip：泛型须注意，泛型不属于一个具体类型，作为类型时必须指明泛型参数，所以泛型类型是不能作为基类的，所以还要专门设置一个非泛型类型作为泛型类型的基类，这样就可以引用
    各个派生的泛型类型了，而且除了这一点以外，另一点在于应该用具体类型继承泛型类型、实际使用具体类型而不是泛型类型。*/

    // [CreateAssetMenu(fileName = "TimelineTrack_SO", menuName = "TimelineTrack_SO", order = 0)]
    public class TimelineTrack_SO<T> : TimelineTrack_SO where T : TimelineTrack
    {
        /*Tip：突然发现，tmd序列化是按照引用类型来的，所以对于持久化需求、压根就不可能完全用基类指向派生类，否则编辑工作毫无意义.
        而且更细致思考的话，应该看的是所依附的UnityEngine.Object类型，主要是MonoBehaviour和ScriptableObject。*/
        // public TimelineTrack Track;
        // public T Track;
        [ContextMenuItem("DebugRunning", nameof(DebugRunning))]
        [SerializeField] private T m_Track;
        // public override T track => Track;
        //只有直接定义为TimelineTrack才能匹配基类的track，不过由于指定了泛型约束，所以能够直接返回泛型类型的字段了。
        public override TimelineTrack track
        {
            get => m_Track;
            // set => m_Track = value as T;
        }
        public void DebugRunning()
        {
            m_Track.DebugRunning();
        }
        // private T Track => m_Track;
        // public Type type; //
        // public string TrackTypeName; //因为是使用同一个类型SO来引用所有派生轨道，在此记录类型的一些信息，以便在生成编辑器时能够找到其具体类型。

    }

    /*Tip：设置为ScriptableObject，就是为了能够以派生类型来序列化保存，而不是以基类类型来序列化*/
    public class TimelineTrack_SO : ScriptableObject
    {
        /*Ques：我草了，写到工厂方法的时候才想到，tm需要通过该类来获取Track，但是我的字段又不能存储在这里，最后想到定义虚属性，让派生类型重写，而正好*/
        // public virtual TimelineTrack track { get; set; }
        public virtual TimelineTrack track { get;}
        // public Type type;
    }

    // public class TimelineTrack_Animation_SO : TimelineTrack_SO<TimelineTrack_Animation> { }
}