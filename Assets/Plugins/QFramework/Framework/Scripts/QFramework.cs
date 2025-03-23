/****************************************************************************
 * Copyright (c) 2015 ~ 2024 liangxiegame MIT License
 *
 * QFramework v1.0
 *
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Author:
 *  liangxie        https://github.com/liangxie
 *  soso            https://github.com/so-sos-so
 *
 * Contributor
 *  TastSong        https://github.com/TastSong
 *  京产肠饭         https://gitee.com/JingChanChangFan/hk_-unity-tools
 *  猫叔(一只皮皮虾) https://space.bilibili.com/656352/
 *  misakiMeiii     https://github.com/misakiMeiii
 *  New一天
 *  幽飞冷凝雪～冷
 *
 * Community
 *  QQ Group: 623597263
 * 
 * Latest Update: 2024.5.12 20:17 add UnRegisterWhenCurrentSceneUnloaded(Suggested by misakiMeiii) 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QFramework
{
    #region Architecture


    public interface IArchitecture
    {
        //注册系统
        void RegisterSystem<T>(T system) where T : ISystem; //意为泛型参数T必须是泛型参数ISystem的派生类
        //注册模型
        void RegisterModel<T>(T model) where T : IModel;
        //注册实用程序（与Tools工具区别，Utility专注于单一任务，更加简单、轻量，而Tools提供一系列功能，覆盖多个任务，复杂、多模块）
        void RegisterUtility<T>(T utility) where T : IUtility;
        //
        T GetSystem<T>() where T : class, ISystem; //意为泛型参数必须是引用类型且是ISystem的派生类

        T GetModel<T>() where T : class, IModel; //T是引用类型，且继承自IModel

        T GetUtility<T>() where T : class, IUtility;

        //发送命令，一个无返回类型，一个有返回类型（命令的执行内容是一个非常专门非常具体的操作。就是对Controller的逻辑的再抽象）
        void SendCommand<T>(T command) where T : ICommand;
        TResult SendCommand<TResult>(ICommand<TResult> command);
        //查询，返回结果。
        TResult SendQuery<TResult>(IQuery<TResult> query);
        //发送事件
        void SendEvent<T>() where T : new();//意为泛型参数T必须是new()的，即必须有无参构造函数。也就是不用传入实例
        void SendEvent<T>(T e); //需要传入实例，即实例可以进行一些自定义
        //注册和注销事件
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(Action<T> onEvent);

        void Deinit(); //反初始化？为什么会在这里定义？而且还没有对应的Init方法？
    }

    /*
    定义了一个 泛型抽象类，并结合了泛型约束
    这是一个自引用的泛型约束，要求 T 必须是 Architecture 类的子类，并且类型参数 T 也是具体的自身类型。
    例如，如果你定义一个 MyArchitecture 继承自 Architecture<T>，那么 T 必须是 MyArchitecture。
    */
    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        private bool mInited = false;

        //抽象类中定义的静态成员也可以直接通过类名访问
        //没看懂这个字段的用处，patch是补丁的意思，难道是用于修复的？为架构注册一个补丁？
        public static Action<T> OnRegisterPatch = architecture => { }; //参数类型为T（其实就是派生类类型），参数名为architecture的Action委托

        //设置为静态，因为每个架构都是一个单独的类，并且只有一个实例
        //好像也不是这回事，因为这个字段定义在抽象类中，所以是所有派生类共享的，也就是说所有派生类都共享这个字段，那么就不是各自都有一个了
        protected static T mArchitecture;
        //这样的话，整个架构应该就代表整个应用，
        public static IArchitecture Interface //只读，返回该类本身的实例
        {
            get
            {
                if (mArchitecture == null) MakeSureArchitecture(); //没有就创造一个
                return mArchitecture;
            }
        }

        /// <summary>
        /// 创建架构实例并初始化
        /// </summary>
        static void MakeSureArchitecture()
        {
            if (mArchitecture == null)
            {
                mArchitecture = new T(); //创建一个T类型的对象
                mArchitecture.Init();

                OnRegisterPatch?.Invoke(mArchitecture);
                //遍历所有未初始化的model和system，然后初始化。Utility则不需要初始化，因为本来就应该提前写好，直接用即可
                foreach (var model in mArchitecture.mContainer.GetInstancesByType<IModel>().Where(m => !m.Initialized))
                {
                    model.Init();
                    model.Initialized = true;
                }

                foreach (var system in mArchitecture.mContainer.GetInstancesByType<ISystem>()
                             .Where(m => !m.Initialized))
                {
                    system.Init();
                    system.Initialized = true;
                }

                mArchitecture.mInited = true; //至此才算架构本身初始化完成
            }
        }

        //对于接口方法的实现必须是public，所以为了将Init设置为protected，就没有在IArchitecture中定义，而是在此定义为一个抽象方法
        protected abstract void Init();

        public void Deinit() //反初始化
        {
            OnDeinit();
            foreach (var system in mContainer.GetInstancesByType<ISystem>().Where(s => s.Initialized)) system.Deinit();
            foreach (var model in mContainer.GetInstancesByType<IModel>().Where(m => m.Initialized)) model.Deinit();
            mContainer.Clear();
            mArchitecture = null;
        }

        protected virtual void OnDeinit()
        {
        }

        private IOCContainer mContainer = new IOCContainer(); //管理实例的容器

        /// <summary>
        /// 注册系统并初始化
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <param name="system"></param>
        public void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            //为系统设置架构，然后注册进架构的容器
            system.SetArchitecture(this);
            mContainer.Register<TSystem>(system);
            //注册时（如果架构已经初始化，否则就是等架构进行统一初始化）初始化
            if (mInited)
            {
                system.Init();
                system.Initialized = true;
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.SetArchitecture(this);
            mContainer.Register<TModel>(model);

            if (mInited)
            {
                model.Init();
                model.Initialized = true;
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility =>
            mContainer.Register<TUtility>(utility);

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem => mContainer.Get<TSystem>();

        public TModel GetModel<TModel>() where TModel : class, IModel => mContainer.Get<TModel>();

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility => mContainer.Get<TUtility>();

        //发送，然后执行。封装了一层，为了把发送命令和执行命令从逻辑上分开
        public TResult SendCommand<TResult>(ICommand<TResult> command) => ExecuteCommand(command);
        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand => ExecuteCommand(command);

        //执行命令，设置架构并执行命令，往往在派生类中会重写以便添加额外逻辑
        protected virtual TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            command.SetArchitecture(this); //命令是在架构之外独立定义的类，所以在执行命令前需要设置架构，然后才能作用于指定架构中的模块
            return command.Execute();
        }
        protected virtual void ExecuteCommand(ICommand command)
        {
            command.SetArchitecture(this);
            command.Execute(); //显式实现，通过对应接口的引用来调用
        }

        //发送查询
        public TResult SendQuery<TResult>(IQuery<TResult> query) => DoQuery<TResult>(query);

        protected virtual TResult DoQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }

        private TypeEventSystem mTypeEventSystem = new TypeEventSystem();

        public void SendEvent<TEvent>() where TEvent : new() => mTypeEventSystem.Send<TEvent>();

        public void SendEvent<TEvent>(TEvent e) => mTypeEventSystem.Send<TEvent>(e);

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent) => mTypeEventSystem.Register<TEvent>(onEvent);

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent) => mTypeEventSystem.UnRegister<TEvent>(onEvent);
    }

    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct =>
            TypeEventSystem.Global.Register<T>(self.OnEvent);

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct =>
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
    }

    #endregion

    #region Controller
    //注意到没有继承ICanSetArchitecture，大概因为控制器的架构是固定不变的，由所实现的GetArchitecture方法所返回的架构决定
    public interface IController : IBelongToArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel,
        ICanRegisterEvent, ICanSendQuery, ICanGetUtility
    {
    }

    #endregion

    #region System

    public interface ISystem : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetUtility,
        ICanRegisterEvent, ICanSendEvent, ICanGetSystem, ICanInit
    {
    }

    public abstract class AbstractSystem : ISystem
    {
        private IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        public bool Initialized { get; set; }
        void ICanInit.Init() => OnInit();

        public void Deinit() => OnDeinit();

        protected virtual void OnDeinit()
        {
        }

        protected abstract void OnInit();
    }

    #endregion

    #region Model
    //其实定义了一个Model需要具备的一些基本功能。（获取）归属架构、设置架构、获取工具、发送事件、初始化
    public interface IModel : IBelongToArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent, ICanInit
    {
    }

    public abstract class AbstractModel : IModel
    {
        private IArchitecture mArchitecturel; //所属架构

        //没看懂，这里定义为私有的，那么派生类如何访问该方法？
        /*实际上，这里的获取和设置架构方法分别是在IBelongToArchitectureh和ICanSetArchitecture中定义的接口方法，从接口名加上点访问符
        就表明这里是对接口方法的显式实现，实际上是private的，只有通过接口引用才能访问它，也就是说这样的方法对类实例不可见，
        看来还是封装性，或者说是隔离，为了让访问更加明确，避免各个方法都混合在一起。
        经测试，通过IModel引用也可以调用这里显式实现的方法，所以其实接口引用也包含了其子接口？
        */
        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecturel;
        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecturel = architecture;

        public bool Initialized { get; set; } //是否已初始化
        void ICanInit.Init() => OnInit();
        public void Deinit() => OnDeinit(); //进行封装是为了统一命名？
        //以下就是给派生类实现的方法，注意设置为abstract也可以视为是对接口方法的实现，即可以起到延迟实现的作用，留到下一个派生类中实现
        //可见对于接口方法的“实现”和一般理解的实现确实也有所区别，这就是语法细节了。
        protected virtual void OnDeinit() //为啥这个不设置为abstract？反正也不需要在此处实现
        {
        }

        protected abstract void OnInit();
    }

    #endregion

    #region Utility

    public interface IUtility //此时只是个架构作用，即通过命名和继承层次来表明程序结构。
    {
    }

    #endregion

    #region Command泛型命令（有返回值，用于执行返回值的类型）和非泛型命令

    public interface ICommand : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility,
        ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }

    public interface ICommand<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel,
        ICanGetUtility,
        ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        TResult Execute(); //泛型命令，区别在于有返回值，且返回类型为泛型参数类型
    }

    public abstract class AbstractCommand : ICommand
    {
        private IArchitecture mArchitecture; //所属架构

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;
        //显式实现，通过Architecture类中的ExecuteCommand方法来引用，因为其参数类型就是ICommand，所以虽然传入的是AbstractCommand的派生类，但是可以调用该方法
        void ICommand.Execute() => OnExecute(); //命令执行内容。在派生类中实现OnExecute方法，通过调用Execute方法来执行命令

        protected abstract void OnExecute();
    }

    public abstract class AbstractCommand<TResult> : ICommand<TResult>
    {
        private IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        TResult ICommand<TResult>.Execute() => OnExecute();

        protected abstract TResult OnExecute();
    }

    #endregion

    #region Query
    //这里的泛型参数就是所查询的对象类型
    public interface IQuery<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetSystem,
        ICanSendQuery
    {
        TResult Do(); //do查询
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        //公开，由发送查询调用，而具体实现在OnDo中也就是在派生类中实现，就是通过封装调整了调用和定义的位置
        public T Do() => OnDo();

        protected abstract T OnDo();


        private IArchitecture mArchitecture;
        //这里是可以直接通过查询类本身获取和设置架构，而AbstractModel类是根据对应的接口进行显式实现的，所以只能通过接口引用才能调用
        public IArchitecture GetArchitecture() => mArchitecture;

        public void SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;
    }

    #endregion

    #region Rule

    public interface IBelongToArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public interface ICanGetModel : IBelongToArchitecture
    {
    }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel =>
            self.GetArchitecture().GetModel<T>(); //通过所属架构获取Model（通常在调用前，都会设置所属架构）
    }

    public interface ICanGetSystem : IBelongToArchitecture
    {
    }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem =>
            self.GetArchitecture().GetSystem<T>();
    }

    public interface ICanGetUtility : IBelongToArchitecture
    {
    }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility =>
            self.GetArchitecture().GetUtility<T>();
    }

    public interface ICanRegisterEvent : IBelongToArchitecture
    {
    }

    public static class CanRegisterEventExtension
    {//派生类都可以访问扩展方法，因为这里是公开的，当然私有的就不能访问了，但其实也不会设置为私有的，因为毫无意义
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent) =>
            self.GetArchitecture().RegisterEvent<T>(onEvent);

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent) =>
            self.GetArchitecture().UnRegisterEvent<T>(onEvent);
    }

    public interface ICanSendCommand : IBelongToArchitecture
    {
    }

    /// <summary>
    /// 扩展了发送命令的方法（其实是封装了架构的发送命令方法）
    /// </summary>
    public static class CanSendCommandExtension //扩展方法（只能通过类的实例调用，而不能通过类名直接调用），用于发送命令，一共有三种重载方法应对不同情况，泛用性极强
    {
        //this就是用于定义扩展方法，表示该方法将附加到实现了 ICanSendCommand 接口的类或对象上，使你可以直接通过这些类/对象调用它（类似实例方法）
        //大概就和C++的友元类似。
        //注意这里发送命令实质上是通过架构对象的SendCommand方法来发送命令的

        //用于发送 不需要额外配置参数的命令。比如某些命令类有固定的默认逻辑，开发者不需要手动构造。传入类名即可
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new() => //无参构造函数的命令
            self.GetArchitecture().SendCommand<T>(new T()); //临时实例，用完就丢
        //用于发送 需要额外配置的命令，即命令实例需要根据具体场景进行自定义。
        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : ICommand => //（可以）有参构造函数的命令
            self.GetArchitecture().SendCommand<T>(command);
        //命令接口扩展： command 类型是 ICommand<TResult>，这意味着命令本身是一个泛型接口，其执行结果将返回特定类型的值。
        //用于 带有返回值的命令，通常用于更复杂的场景，例如获取某些状态或计算结果。比如命令中具有玩家ID，执行后返回玩家的分数。
        public static TResult SendCommand<TResult>(this ICanSendCommand self, ICommand<TResult> command) =>
            self.GetArchitecture().SendCommand(command);
    }

    public interface ICanSendEvent : IBelongToArchitecture
    {

    }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new() =>
            self.GetArchitecture().SendEvent<T>();

        public static void SendEvent<T>(this ICanSendEvent self, T e) => self.GetArchitecture().SendEvent<T>(e);
    }

    public interface ICanSendQuery : IBelongToArchitecture
    {
    }

    public static class CanSendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self, IQuery<TResult> query) =>
            self.GetArchitecture().SendQuery(query);
    }

    public interface ICanInit //Init初始化和Deinit反初始化
    {
        bool Initialized { get; set; } //是否已经初始化。这里只是接口属性，具体的访问器还要在派生类中实现
        void Init();
        void Deinit();
    }

    #endregion

    #region TypeEventSystem

    public interface IUnRegister
    {
        void UnRegister();
    }

    public interface IUnRegisterList
    {
        List<IUnRegister> UnregisterList { get; }
    }

    public static class IUnRegisterListExtension
    {
        public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList) =>
            unRegisterList.UnregisterList.Add(self);

        public static void UnRegisterAll(this IUnRegisterList self) //注销列表中所有IUnregister对象（派生类对象）
        {
            foreach (var unRegister in self.UnregisterList)
            {
                unRegister.UnRegister();
            }

            self.UnregisterList.Clear(); //同步，列表只是对集合的记录，实际的注销操作是在上面的foreach循环中进行的
        }
    }

    public struct CustomUnRegister : IUnRegister //自定义注销器
    {
        //Action是一个委托，这里的构造方法就是传入一个（匿名）方法，传入方法应该就是用于注销，然后调用这里的UnRegister方法就可以进行注销了。
        private Action mOnUnRegister { get; set; } //注销方法，创建时传入自定义的注销方法（就是为了能够在单独的-=注销以外添加额外逻辑）
        public CustomUnRegister(Action onUnRegister) => mOnUnRegister = onUnRegister;

        public void UnRegister()
        {
            mOnUnRegister.Invoke();
            mOnUnRegister = null;
        }
    }

#if UNITY_5_6_OR_NEWER
    public abstract class UnRegisterTrigger : UnityEngine.MonoBehaviour
    {
        private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

        public IUnRegister AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
            return unRegister;
        }

        public void RemoveUnRegister(IUnRegister unRegister) => mUnRegisters.Remove(unRegister);

        public void UnRegister() //统一注销，然后清空容器
        {
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }

            mUnRegisters.Clear();
        }
    }

    public class UnRegisterOnDestroyTrigger : UnRegisterTrigger
    {
        private void OnDestroy()
        {
            UnRegister();
        }
    }

    public class UnRegisterOnDisableTrigger : UnRegisterTrigger
    {
        private void OnDisable()
        {
            UnRegister();
        }
    }

    public class UnRegisterCurrentSceneUnloadedTrigger : UnRegisterTrigger
    {
        private static UnRegisterCurrentSceneUnloadedTrigger mDefault;

        public static UnRegisterCurrentSceneUnloadedTrigger Get
        {
            get
            {
                if (!mDefault)
                {
                    mDefault = new GameObject("UnRegisterCurrentSceneUnloadedTrigger")
                        .AddComponent<UnRegisterCurrentSceneUnloadedTrigger>();
                }

                return mDefault;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            hideFlags = HideFlags.HideInHierarchy;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy() => SceneManager.sceneUnloaded -= OnSceneUnloaded;
        void OnSceneUnloaded(Scene scene) => UnRegister();
    }
#endif

    public static class UnRegisterExtension
    {
#if UNITY_5_6_OR_NEWER
        /// <summary>
        /// 获取传入对象上的指定组件（没有就添加一个并且获取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var trigger = gameObject.GetComponent<T>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<T>();
            }

            return trigger;
        }

        //在对象销毁时注销，一个是传入对象本身，一个是传入组件，其实第二个方法就是对第一个方法的封装，也是重载，总之就是在传参时多一个选择
        public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister,
            UnityEngine.GameObject gameObject) =>
            GetOrAddComponent<UnRegisterOnDestroyTrigger>(gameObject)
                .AddUnRegister(unRegister);
        public static IUnRegister UnRegisterWhenGameObjectDestroyed<T>(this IUnRegister self, T component)
            where T : UnityEngine.Component =>
            self.UnRegisterWhenGameObjectDestroyed(component.gameObject);

        public static IUnRegister UnRegisterWhenDisabled(this IUnRegister unRegister,
            UnityEngine.GameObject gameObject) =>
            GetOrAddComponent<UnRegisterOnDisableTrigger>(gameObject)
                .AddUnRegister(unRegister);
        public static IUnRegister UnRegisterWhenDisabled<T>(this IUnRegister self, T component)
            where T : UnityEngine.Component =>
            self.UnRegisterWhenDisabled(component.gameObject);

        public static IUnRegister UnRegisterWhenCurrentSceneUnloaded(this IUnRegister self) =>
            UnRegisterCurrentSceneUnloadedTrigger.Get.AddUnRegister(self);
#endif


#if GODOT
		public static IUnRegister UnRegisterWhenNodeExitTree(this IUnRegister unRegister, Godot.Node node)
		{
			node.TreeExiting += unRegister.UnRegister;
			return unRegister;
		}
#endif
    }

    public class TypeEventSystem
    {
        private readonly EasyEvents mEvents = new EasyEvents(); //不是EasyEvent，而是一个字典（封装类），用于存储类型和事件的映射关系

        public static readonly TypeEventSystem Global = new TypeEventSystem(); //全局实例，其实类似于单例

        public void Send<T>() where T : new() => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());//这里传入类型随意，因为都会被转换为Easyevent类型，所以不会出现类型不匹配的情况

        //这里可以只传入函数参数而不传入泛型参数，因为泛型参数可以通过函数参数推断出来，这就是C#的类型推导（Type Inference）
        //但必须要有<T>这才表明是一个泛型方法，否则泛型T就会被当作一个普通的类型参数，如果没有T类则就会报错
        //不过也可以比如泛型传入接口，函数参数是实现了接口的类或结构体，但不能是接口，因为接口无法实例化，而传入的应该是实例
        public void Send<T>(T e) => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);

        public IUnRegister Register<T>(Action<T> onEvent) => mEvents.GetOrAddEvent<EasyEvent<T>>().Register(onEvent);

        public void UnRegister<T>(Action<T> onEvent)
        {
            var e = mEvents.GetEvent<EasyEvent<T>>();
            e?.UnRegister(onEvent);
        }
    }

    #endregion

    #region IOC

    public class IOCContainer //Inversion of Control控制反转容器
    {
        //本质就是一个字典，管理类型和实例的映射关系即管理实例
        private Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        //可以注册各种类型，主要是ISystem、IModel、IUtility类型的实例
        //注意：这里是类型（键）对应实例（值），所以一个类型只能有一个实例，但不要误认为是比如System只能有一个实例，因为可以有很多种不同的System，那么各自都可以存储一个实例
        //所以其实说明这里注册的类型，都是单例。
        public void Register<T>(T instance)
        {
            var key = typeof(T);

            if (mInstances.ContainsKey(key))
            {
                mInstances[key] = instance;
            }
            else
            {
                mInstances.Add(key, instance);
            }
        }
        //注册时全部由基类Type和object引用，获取时转换为对应的类型
        public T Get<T>() where T : class //必须为引用类型
        {
            var key = typeof(T);

            if (mInstances.TryGetValue(key, out var retInstance))
            {
                return retInstance as T;
            }

            return null;
        }

        /*返回一系列 T 类型的对象。
        泛型方法非常灵活，允许调用者动态指定目标类型。
        type.IsInstanceOfType(instance) 适用于运行时类型检查，包括多态场景。
        .Cast<T>() 保证了返回的集合类型安全。
        */
        public IEnumerable<T> GetInstancesByType<T>()
        {
            var type = typeof(T);
            //使用 LINQ 的 Where 方法进行筛选，判断 instance 是否为类型 T 的实例（包括继承关系）。.Cast<T>(): 将筛选后的对象列表强制转换为类型 T 的集合。
            return mInstances.Values.Where(instance => type.IsInstanceOfType(instance)).Cast<T>();
        }

        public void Clear() => mInstances.Clear();
    }

    #endregion

    #region BindableProperty

    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
    }

    public interface IReadonlyBindableProperty<T> : IEasyEvent
    {
        T Value { get; }

        IUnRegister RegisterWithInitValue(Action<T> action);
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }

    /// <summary>
    /// 适用于单个数据+变更事件的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableProperty<T> : IBindableProperty<T> //属性mValue的数据类型
    {//由此可以很好启发对于基本对象的封装即自定义，比如像这里对于int、float、string等基本数据类型的封装
        //defaultValue = default 意味着调用这个构造函数时，如果没有显式提供参数 defaultValue，它将使用该类型的默认值。比如，对数值类型，default 是 0；对引用类型，default 是 null。
        public BindableProperty(T defaultValue = default) => mValue = defaultValue; //构造函数，初始化mValue

        protected T mValue; //就是代表字段原本的值
        //System.Func 是一个泛型委托，最后一个泛型参数表示返回值类型，其余的泛型参数是方法的输入参数类型，支持最多 16 个输入参数，也可以没有输入参数即只有返回值
        public static Func<T, T, bool> Comparer { get; set; } = (a, b) => a.Equals(b); //比较a和b是否相等。同类型通用比较器，所以直接设为静态

        public BindableProperty<T> WithComparer(Func<T, T, bool> comparer) //设置比较器
        {
            Comparer = comparer;
            return this;
        }

        public T Value //属性，用于对字段mValue的读写添加额外的逻辑
        {
            get => GetValue();
            set
            {
                //value为空，但mValue不为空则就是将mValue置空？
                //其实就是只要相等就跳过set过程，分两种情况：空和非空
                if (value == null && mValue == null) return;
                if (value != null && Comparer(value, mValue)) return;

                SetValue(value);
                mOnValueChanged.Trigger(value); //值变化触发事件
            }
        }

        protected virtual void SetValue(T newValue) => mValue = newValue;

        protected virtual T GetValue() => mValue;

        //不触发事件的赋值
        public void SetValueWithoutEvent(T newValue) => mValue = newValue;

        private EasyEvent<T> mOnValueChanged = new EasyEvent<T>(); //值变化事件

        /// <summary>
        /// 将传入方法注册到值变化的事件中
        /// </summary>
        /// <param name="onValueChanged"></param>
        /// <returns></returns>
        public IUnRegister Register(Action<T> onValueChanged) //注册的方法可以接收一个T类型的参数，其实就是封装的数据类型
        {
            return mOnValueChanged.Register(onValueChanged);
        }

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(mValue); //先调用一次将要注册的方法，即初始化赋值
            return Register(onValueChanged);
        }

        public void UnRegister(Action<T> onValueChanged) => mOnValueChanged.UnRegister(onValueChanged);

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);
            void Action(T _) => onEvent();
        }

        public override string ToString() => Value.ToString();
    }

    internal class ComparerAutoRegister
    {
#if UNITY_5_6_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoRegister()
        {
            BindableProperty<int>.Comparer = (a, b) => a == b;
            BindableProperty<float>.Comparer = (a, b) => a == b;
            BindableProperty<double>.Comparer = (a, b) => a == b;
            BindableProperty<string>.Comparer = (a, b) => a == b;
            BindableProperty<long>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Vector2>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Vector3>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Vector4>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Color>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Color32>.Comparer =
                (a, b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            BindableProperty<UnityEngine.Bounds>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Rect>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Quaternion>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Vector2Int>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.Vector3Int>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.BoundsInt>.Comparer = (a, b) => a == b;
            BindableProperty<UnityEngine.RangeInt>.Comparer = (a, b) => a.start == b.start && a.length == b.length;
            BindableProperty<UnityEngine.RectInt>.Comparer = (a, b) => a.Equals(b);
        }
#endif
    }

    #endregion

    #region EasyEvent

    public interface IEasyEvent
    {
        IUnRegister Register(Action onEvent);
    }

    //EasyEvent就是对System.Action事件类型的封装
    public class EasyEvent : IEasyEvent
    {
        private Action mOnEvent = () => { };

        public IUnRegister Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public IUnRegister RegisterWithACall(Action onEvent)
        {
            onEvent.Invoke();
            return Register(onEvent);
        }

        public void UnRegister(Action onEvent) => mOnEvent -= onEvent;

        public void Trigger() => mOnEvent?.Invoke();
    }

    public class EasyEvent<T> : IEasyEvent
    {
        private Action<T> mOnEvent = e => { }; //初始为空

        //一层层一直跳转到这里才是真正地注册方法，并且返回一个注销器
        public IUnRegister Register(Action<T> onEvent) //这里是自定义方法，并不是实现接口方法，因为接口方法的Action是无参的
        {
            mOnEvent += onEvent;
            //这里的参数类型就是一个空参数的Action
            return new CustomUnRegister(() => { UnRegister(onEvent); }); //这里使用CustomUnRegister又对注销方法进行了一层封装
        }

        public void UnRegister(Action<T> onEvent) => mOnEvent -= onEvent; //注销方法


        public void Trigger(T t) => mOnEvent?.Invoke(t); //触发事件

        //这里是对接口方法的显式实现，为了满足该类的多态性即泛型，所以定义了一个局部方法
        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);
            void Action(T _) => onEvent(); //局部方法，只能在当前方法中调用
        }
    }

    public class EasyEvent<T, K> : IEasyEvent
    {
        private Action<T, K> mOnEvent = (t, k) => { };

        public IUnRegister Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K> onEvent) => mOnEvent -= onEvent;

        public void Trigger(T t, K k) => mOnEvent?.Invoke(t, k);

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);
            void Action(T _, K __) => onEvent();
        }
    }

    public class EasyEvent<T, K, S> : IEasyEvent
    {
        private Action<T, K, S> mOnEvent = (t, k, s) => { };

        public IUnRegister Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K, S> onEvent) => mOnEvent -= onEvent;

        public void Trigger(T t, K k, S s) => mOnEvent?.Invoke(t, k, s);

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);
            void Action(T _, K __, S ___) => onEvent();
        }
    }

    public class EasyEvents
    {
        private static readonly EasyEvents mGlobalEvents = new EasyEvents();

        public static T Get<T>() where T : IEasyEvent => mGlobalEvents.GetEvent<T>();

        public static void Register<T>() where T : IEasyEvent, new() => mGlobalEvents.AddEvent<T>();

        private readonly Dictionary<Type, IEasyEvent> mTypeEvents = new Dictionary<Type, IEasyEvent>();

        public void AddEvent<T>() where T : IEasyEvent, new() => mTypeEvents.Add(typeof(T), new T());

        public T GetEvent<T>() where T : IEasyEvent
        {
            return mTypeEvents.TryGetValue(typeof(T), out var e) ? (T)e : default;
        }

        /// <summary>
        /// 获取或添加事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetOrAddEvent<T>() where T : IEasyEvent, new()
        {
            var eType = typeof(T);
            if (mTypeEvents.TryGetValue(eType, out var e))
            {
                return (T)e;
            }

            var t = new T();
            mTypeEvents.Add(eType, t); //类型映射实例
            return t;
        }
    }

    #endregion


    #region Event Extension

    public class OrEvent : IUnRegisterList
    {
        public OrEvent Or(IEasyEvent easyEvent)
        {
            easyEvent.Register(Trigger).AddToUnregisterList(this);
            return this;
        }

        private Action mOnEvent = () => { };

        public IUnRegister Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public IUnRegister RegisterWithACall(Action onEvent)
        {
            onEvent.Invoke();
            return Register(onEvent);
        }

        public void UnRegister(Action onEvent)
        {
            mOnEvent -= onEvent;
            this.UnRegisterAll();
        }

        private void Trigger() => mOnEvent?.Invoke();

        public List<IUnRegister> UnregisterList { get; } = new List<IUnRegister>();
    }

    public static class OrEventExtensions
    {
        public static OrEvent Or(this IEasyEvent self, IEasyEvent e) => new OrEvent().Or(self).Or(e);
    }

    #endregion

#if UNITY_EDITOR
    internal class EditorMenus
    {
        [UnityEditor.MenuItem("QFramework/Install QFrameworkWithToolKits")]
        public static void InstallPackageKit() => UnityEngine.Application.OpenURL("https://qframework.cn/qf");
    }
#endif
}