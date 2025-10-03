using System.Collections.Generic;
using ARPGDemo.SkillSystemtest;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{
    [AddComponentMenu("ARPGDemo/系统与管理器/TimelineManager")]
    public class TimelineManager : SingletonMono<TimelineManager>
    {
        //TODO：正式情况不应该这样专门划分字段存储，而是应该在初始化方法中直接通过路径或标识符加载资源、使用局部变量存储、读取数据之后直接销毁资产类即可。
        [SerializeField] private List<TimelineModel_SO> m_Timelines = new List<TimelineModel_SO>();

        protected override void Awake()
        {
            m_Instance = GameObject.Find("TimelineManager").GetComponent<TimelineManager>();
            base.Awake();
        }

        private void Start()
        {
            BuildTimelineFactory();
        }

        /*TODO：建造工厂这种方法，可能要转换为接口，由各自传入具体类型来建造对应的工厂，才是更好的写法*/
        private void BuildTimelineFactory()
        {
            Debug.Log("01：创建工厂");
            List<TimelineModel> models = new List<TimelineModel>();
            m_Timelines.ForEach(modelSO =>
            {
                /*BugFix：tmd我忘了在编辑器中直接使用资产的话是会污染资产的，这和加载资产不同，加载资产是从外存的序列化文件反序列化生成内存实例的，而在编辑器中直接引用
                资产的话，就是直接将已经生成好的资产实例拿来用，而且这个资产实例是与对应外存上的序列化文件相关联的。*/
                // models.Add(modelSO.GetModel());
                // models.Add(Instantiate(modelSO).GetModel());
                models.Add(modelSO.Clone().GetModel());
            });
            TimelineFactory.Instance.AddTimelineModels(models);
        }
    }
}