using System;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.Example
{


    // 1. 定义一个 Model 对象
    public interface ICounterAppModel : IModel
    {
        //这里只定义了一个Count变量，主要基于具体的Model对象的需求，这里只是一个简单的计数器
        BindableProperty<int> Count { get; } //只读
    }
    public class CounterAppModel : AbstractModel, ICounterAppModel
    {
        //左边是声明类型，只需要且只能指定泛型类型，右边是创建实例，才是可以传入参数
        public BindableProperty<int> Count { get; } = new BindableProperty<int>(0); //int类型参数

        protected override void OnInit()
        {
            //获取存储工具，初始化读取，实时修改并存储（数据多了之后可能会累计在一起进行统一存储，而不是每次都存储）
            var storage = this.GetUtility<IStorage>(); //通过GetUtility这个方法，就可以将各种封装好的工具统一起来（逻辑含义，并非语法含义），还是可读性

            // 设置初始值（不触发事件）
            Count.SetValueWithoutEvent(storage.LoadInt(nameof(Count))); //读取存储数据

            // 当数据变更时 存储数据
            Count.Register(newCount => //写入存储数据
            {
                storage.SaveInt(nameof(Count), newCount);
            });
        }
    }

    public interface IAchievementSystem : ISystem //成就系统
    {

    }
    //成就系统，就是给那些触发成就的对象注册实现成就的方法，注册也就是添加监听。这确实具有独立性，因为只是响应数据变化（即某些特定情况），不需要和其他系统进行交互
    public class AchievementSystem : AbstractSystem, IAchievementSystem
    {
        protected override void OnInit() //初始化，成就系统在自己的初始化方法中注册了自己的方法
        {
            this.GetModel<ICounterAppModel>() // -+
                .Count
                .Register(newCount =>
                {
                    if (newCount == 10)
                    {
                        Debug.Log("触发 点击达人 成就");
                    }
                    else if (newCount == 20)
                    {
                        Debug.Log("触发 点击专家 成就");
                    }
                    else if (newCount == -10)
                    {
                        Debug.Log("触发 点击菜鸟 成就");
                    }
                });
        }
    }


    public interface IStorage : IUtility //定义存储工具的接口
    {
        void SaveInt(string key, int value);
        int LoadInt(string key, int defaultValue = 0);
    }

    /// <summary>
    /// 用于存储的类
    /// </summary>
    public class Storage : IStorage //使用Unity内置的PlayerPrefs类实现存储功能（保存在注册表中，非常不便于查看）
    {
        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }


    // 2.定义一个架构（提供 MVC、分层、模块管理等）也就是一个应用程序的总体框架。该架构就代表该应用，System、Model、Utility都是该应用的各个模块，还有处理各个模块的Controller
    public class CounterApp : Architecture<CounterApp>
    {
        protected override void Init() //注册各个对象
        {
            // 注册 System 
            this.RegisterSystem<IAchievementSystem>(new AchievementSystem());

            // 注册 Model
            this.RegisterModel<ICounterAppModel>(new CounterAppModel());

            // 注册存储工具的对象
            this.RegisterUtility<IStorage>(new Storage());
        }

        //Command日志功能，就是重写了基类Architecture<T>的该方法，在前后添加了日志输出，除了调试以外，也可以添加其他各种功能，比如日志拦截
        protected override void ExecuteCommand(ICommand command)
        {//标记命令执行前和执行后
            Debug.Log("Before " + command.GetType().Name + "Execute");
            base.ExecuteCommand(command);
            Debug.Log("After " + command.GetType().Name + "Execute");
        }

        protected override TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            Debug.Log("Before " + command.GetType().Name + "Execute");
            var result = base.ExecuteCommand(command);
            Debug.Log("After " + command.GetType().Name + "Execute");
            return result;
        }
    }

    // 引入 Command
    public class IncreaseCountCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var model = this.GetModel<ICounterAppModel>();

            model.Count.Value++;
        }
    }

    public class DecreaseCountCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<ICounterAppModel>().Count.Value--; //和上面一样，只是这里没有用一个临时变量存储model对象
        }
    }

    // Controller
    public class CounterAppController : MonoBehaviour, IController /* 3.实现 IController 接口 */
    {//挂载在Canvas对象上，这里是通过名字查找子对象，感觉更方便的是直接拖拽到Inspector面板上
        // View
        private Button mBtnAdd; //加号按钮
        private Button mBtnSub; //减号按钮
        private Text mCountText; //显示数字的文本

        // 4. Model
        private ICounterAppModel mModel;

        void Start()
        {
            // 5. 获取模型
            mModel = this.GetModel<ICounterAppModel>();

            // View 组件获取
            mBtnAdd = transform.Find("BtnAdd").GetComponent<Button>();
            mBtnSub = transform.Find("BtnSub").GetComponent<Button>();
            mCountText = transform.Find("CountText").GetComponent<Text>();


            // 监听输入（按钮点击）
            mBtnAdd.onClick.AddListener(() =>
            {
                // 交互逻辑
                this.SendCommand<IncreaseCountCommand>(); //来自于IController继承的ICanSendCommand接口，这里是作为的泛型参数传入的
            });

            mBtnSub.onClick.AddListener(() =>
            {
                // 交互逻辑
                this.SendCommand(new DecreaseCountCommand(/* 这里可以传参（如果有） */)); //这里是作为实例传入的，与上面区别
            });

            // 表现逻辑（初始就要显示，这里指的是初始化赋值，所谓表现其实都是指的更新数据，因为实际的显示是由引擎底层的渲染系统完成的）
            mModel.Count.RegisterWithInitValue(newCount => // -+
            { //匿名函数，这里只是调用一下UpdateView方法，其实并没有用到newCount这个参数，因为该方法可以直接获取到mModel.Count的值，
              //如果把用于显示的该方法放到该类以外可能就需要由各个调用它的不同对象传入各自的参数了，当然这样它本身用于表现的逻辑就会复杂得多
                UpdateView();

            }).UnRegisterWhenGameObjectDestroyed(gameObject); //当游戏对象销毁时，注销
        }

        /// <summary>
        /// 更新视图数据
        /// </summary>
        void UpdateView()
        {
            mCountText.text = mModel.Count.ToString();
        }

        // 3.获取该控制器所在架构
        public IArchitecture GetArchitecture()
        {
            return CounterApp.Interface;
        }

        private void OnDestroy()
        {
            // 8. 将 Model 设置为空
            mModel = null;
        }
    }
}
