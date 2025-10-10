using System.Collections.Generic;
using UnityEngine;  

namespace ARPGDemo.SkillSystemtest
{

    /*运行时*/
    [CreateAssetMenu(fileName = "TimelineModel_SO", menuName = "ARPGDemo/TimelineModel_SO", order = 0)]
    public class TimelineModel_SO : ScriptableObject
    {
        // public TimelineModel model;
        [SerializeField] private TimelineModel Model;
        /*为了以实际类型序列化保存，不得不在这里单独分出来放在一个独立的容器中，运行时再*/
        /*TODO：暂时先保持公开，其实可以在创建之后立刻让该资产类查找自己的子资产，同样可以获取引用，而且能够避免公开。*/
        public List<TimelineTrack_SO> Tracks;
        // [SerializeField] private List<TimelineTrack_SO> Tracks;

        //调整一下，将这些逻辑全部放在属于数据层的ScriptableObject资产类中，尽量做到让运行时属于逻辑层的类型完全不依赖于这些数据类。
        public TimelineModel GetModel()
        {
            List<TimelineTrack> tracks = new List<TimelineTrack>(Tracks.Count); //已知数量，直接分配足够内存
            Tracks.ForEach(trackSO =>
            {
                tracks.Add(trackSO.track); //将数据类中的数据提取出来。
            });
            /*Tip：当然可以直接用该资产类中原本就有的*/
            return Model.Clone(tracks);
        }
        //不仅是Model本身要实例化克隆，自身所含的Track资产也要实例化克隆
        public TimelineModel_SO Clone()
        {
            TimelineModel_SO clone = Instantiate(this);
            List<TimelineTrack_SO> tracks = new List<TimelineTrack_SO>(Tracks.Count); //已知数量，直接分配足够内存
            Tracks.ForEach(track =>
            {
                tracks.Add(Instantiate(track));
            });
            clone.Tracks = tracks;
            return clone;
        }

    }
}