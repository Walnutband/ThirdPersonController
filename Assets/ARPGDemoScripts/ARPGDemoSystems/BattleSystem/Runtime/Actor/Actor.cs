using UnityEngine;


namespace ARPGDemo.BattleSystem
{
    /*Tip：这里的Actor代表的是个体的一切，包括角色、怪物、NPC、玩家等等，也就是个体本身。但并不代表控制，将控制系统和战斗系统解耦是一方面，只是顺带的，另一方面主要是动静结构的划分，
    也就是个体本身具有各种属性，然后还会用到各种动画，还有各方面的数据等等，但是这都是静态的，这里的静态指的是并不主动参与任何逻辑（除了读取数据以外，准确来说是不参与运行时监测逻辑），
    只有在接收输入时才会参与逻辑，而从更加创新的游戏设计来看，接收输入并不是读取数值然后使用 就完了，而是从产生输入到接收输入的过程中，还存在大量变化，这些变化是在控制系统层面的
    游戏设计的源头。*/
    public class Actor : MonoBehaviour
    {
        /*TODO：每个个体都有自己的属性，但是从我设想的控制系统来看，可以控制的对象不一定是这里所指的个体，按理来说可以是一切实体（Entity），而除了个体之外的实体可能就没有这样的属性，
        当然它们也可能有其他属性，比如定义EntityProperty类，然后派生若干，也可以，后续也可能新增接口来增强扩展性。*/
        [DisplayName("属性值")]
        [SerializeField] private ActorProperty m_Property; //这种纯C#类的序列化就是将其序列化成员排列出来，而不是拖拽改变所引用的实例。
        public ActorProperty property { get { return m_Property; } set { m_Property = value; } }
        //TODO: 看后面如何结合资源管理系统来调整加载数据的方式。也可能单独设置一个静态类来存放相关路径之类的。还有后续加入存档系统之后，肯定还会有所改动。
        // [SerializeField] private SO_ActorProperty m_Data;

        // [SerializeField] private ActorEquipment

        private void Awake()
        {
            // m_Property = new ActorProperty();
        }
    }

    
}

namespace ARPGDemo.AbilitySystem
{
    public class ActorProfile
    {
                
    }
}
