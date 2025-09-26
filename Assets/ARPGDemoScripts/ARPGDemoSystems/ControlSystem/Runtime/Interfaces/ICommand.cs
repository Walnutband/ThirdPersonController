
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public interface ICommand
    {
        bool Execute(ICommandConsumer consumer);
    }

    public interface ICommandProducer
    {
        void OnStart();
        List<ICommand> Produce();
        void OnEnd();
    }

    public interface ICommandConsumer //作为标记，不需要规定任何接口成员，也是作为每一个命令的接收者的接口的根接口
    {
        // bool Consume(List<ICommand> command);
        
    }

    /*Tip：在设计上，所有受控对象都是通过命令来控制，所以理应带上该接口。而受控对象在绝大多数游戏中都是玩家角色、怪物、NPC之类的，但我在设想另外的更加广泛的受控对象，
    所以在此用一个IController，用于规定通常受控对象的一些独有的方法，其实主要就是Update每帧刷新，只是用周期方法的话会被引擎底层控制，无法手动控制先后顺序，因为
    应该先执行命令，然后才是每帧刷新。
    带上ICommandConsumer，这样在ControlSystem中可以直接通过ICommandConsumer进行类型转换，然后调用规定的OnUpdate方法*/
    public interface IController : ICommandConsumer
    {
        void OnUpdate();
    }

    #region 所有命令生产者接口
    // public interface IMove_Producer : 
    #endregion


    #region 所有命令接收者接口
    public interface IMove_Consumer : ICommandConsumer
    {
        bool Move(Vector2 moveInput, MoveCommand.MoveType moveType);
    }

    /*TODO：预计不会单独设置Sprint冲刺和Walk（按下Ctrl的慢走，正常其实是Run），而是根据是否在移动时按下了Ctrl或者Shift作为Move方法的第二个参数（枚举类型Walk、Run、Sprint），
    然后在命令接收者实现的周期方法中处理Walk、Run和Sprint，而且这样也具有复用性，比如不只是角色，在NPC、怪物的AI上其实Walk、Run和Sprint这三种动作都极其常见，甚至直接对应其状态。*/
    // public interface ISprint_Consumer : ICommandConsumer
    // {
    //     bool Sprint(Vector2 moveInput);
    // }

    public interface IJump_Consumer : ICommandConsumer
    {
        bool Jump();
    }

    public interface IDodge_Consumer : ICommandConsumer
    {
        bool Dodge();
    }

    public interface ILightAttack_Consumer : ICommandConsumer
    {
        bool LightAttack();
    }

    public interface IHeavyAttack_Consumer : ICommandConsumer
    {
        //重攻击通常就是一个蓄力，在交互行为上就是Tap + SlowTap轻松实现。
        /*Tip：接口方法也可以规定为重载，因为charging为true的时候，也就是蓄力，无所谓full。但是C#中支持这样的语法糖，使用"?"即可让结构体能够为空（实际上是转换为了System.Nullable<T>类型）,
        不过这也只是为了增强逻辑性，实际上在该接口方法的具体实现中，在charging为true时，不管full就行了，本来自由决定逻辑。*/
        bool HeavyAttack(bool charging, bool? full);  
    }

    // public interface IMouseScroll_Consumer : ICommandConsumer
    // {

    // }
    #endregion

    #region 个体命令接口

    //就是为了将这些要用到的命令接口放在同一个接口处，以免放在具体类的声明中造成不必要的干扰。
    //捆绑命令？
    public interface IPlayerConsumer : IController,
    IMove_Consumer, IJump_Consumer, IDodge_Consumer,
    ILightAttack_Consumer, IHeavyAttack_Consumer
    {
        //添加一些定制命令
        bool Zoom(Vector2 zoomInput);
        bool ResetCamera();
        bool ChangePlayer(CommandProducer producer);
        bool UseItem();
        bool Interact();
    }
    #endregion
}