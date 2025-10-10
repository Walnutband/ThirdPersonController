using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.SkillSystemtest
{

    /*TODO：可以全部搞成接口，比如IFactory<T>，ICloneable*/
    public class TimelineFactory : Singleton<TimelineFactory>
    {
        /*TODO：注意这里是Model“模型数据”，实际使用会根据Model创建TimlelineObj来使用。而工厂常常会使用对象池来实现对于实例的复用，但对象池对于实例是有限制要求的，就是实例之间
        往往是相同的，因为入池时都会经过初始化过程，而对于TimelineModel来说，显然每个实例就不相同，虽然是同一类型，但是每个实例都会在编辑时编辑数据以实现功能，不可能随便从池中
        取一个实例就行了。当然也可以用ID来存储，比如Dictionary<uint, List<TimelineModel>>但是TimelineModel的实例在运行时其实不会频繁创建和销毁（但是技能的作用比如发射的子弹、
        火球之类的对象，这种就很适合使用对象池，入池时初始化，出池时修改一些指定数据就能直接使用了），大多实例都是在初始化过程中就创建好了，设置工厂的目的就是为了同一ID创建多个实例，
        以免对Model的修改影响到彼此。*/
        private Dictionary<uint, TimelineModel> m_TimelineModels = new Dictionary<uint, TimelineModel>(); //这里存储的原始数据，没有Context。



        // public TimelineObj GetTimeline(uint _id)
        // {
        //     if (m_TimelineModels.TryGetValue(_id, out TimelineModel model) == false)
        //     {//工厂中没有所需要的TimelineModel
        //         return null;
        //     }
        //     else
        //     {
        //         return new TimelineObj(model.Clone());
        //     }
        // }
        /*Tip：正常来说总是应该调用下面这个方法而非上面那个。
        id是查找对应的Timeline，Context则是让Timeline进入自己的环境中为自己服务。
        */
        public TimelineObj GetTimeline(uint _id, TimelineContext _ctx)
        {
            Debug.Log("获取Timeline， ID：" + _id);
            if (m_TimelineModels.TryGetValue(_id, out TimelineModel model) == false)
            {//工厂中没有所需要的TimelineModel
                return null;
            }
            else
            {//返回TimelineObj而不是TimelineModel，防止外部通过Model构造。
                return new TimelineObj(model.Clone(), _ctx);
            }
        }
        //添加Model，就是给工厂增加配方
        public void AddTimelineModels(List<TimelineModel> _models)
        {
            Debug.Log("添加Model到工厂，ID：" + _models[0].id);
            _models.ForEach(model => m_TimelineModels.Add(model.id, model));
        }
    }
}