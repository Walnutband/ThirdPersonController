namespace ARPGDemo.ControlSystem_New
{
    public interface ICommand
    {
        //每个命令的唯一ID，由于具有特殊性，所以使用大驼峰而不是小驼峰
        uint ID { get; }
    }


    /*Ques：偶然想到，如果稍微初具规模的话，命令之间是否会存在一些特殊关系？所以是否可以利用这种关系，而使用泛型来精确所能够处理的命令？
    我都已经想到了可能的情景，给命令进行分类，而每个类型都会有对应的泛型类型的派生类型，然后还可以实现多个这种接口，那么就会实现HandleCommand的多个重载版本，似乎完全OK？
    因为一般来说需要尽量把接口方法浓缩、但是就可能带来杂糅的情况，而根据具体情形这样适当地增加接口方法，就很舒服了。
    */
    /*Tip：通过重载，可以统一函数名，而不是像之前那样全部硬编码为对应的行为名。而且这样利用泛型，还将之前接口的隐性依赖替换成了现在对于具体命令的显性依赖，更具有结构性。
    利用泛型，还不需要定义多个接口，就可以实现多个重载*/
    public interface ICommandHandler<T> where T : ICommand
    {
        void HandleCommand(T _command);
    }
    //Tip：只是占个位，其实直接单独写也可以。
    public interface ICommandHandler : ICommandHandler<CommandBase>
    {
        // void HandleCommand(ICommand _command);
    }

    public interface ICommandProducer
    {
        //开始生产和结束生产
        void OnStart();
        void OnEnd();
    }

    public interface ICommandConsumer
    {
        
    }
}