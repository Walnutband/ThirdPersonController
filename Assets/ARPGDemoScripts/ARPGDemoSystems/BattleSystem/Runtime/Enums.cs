
namespace ARPGDemo.BattleSystem
{
    //主要用于可以被交互的对象，指明与自己的交互方式是什么，以便主动发起交互的对象能够执行正确的交互逻辑（调用对应方法）。
    public enum InteractionType
    {
        PickUp, //拾取
        Talk, //交谈、对话，相关词比较多（chat、discuss、conversation）
    }
}