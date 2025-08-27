using UnityEngine;

public abstract class Singleton<T> where T : new() //泛型参数必须具有无参构造函数
{
    private static T _ServiceContext;
    private readonly static object lockObj = new object();

    /// <summary>
    /// 禁止外部进行实例化
    /// </summary>
    protected Singleton()
    {
        OnInitialize();
    }

    /// <summary>
    /// 获取唯一实例，双锁定防止多线程并发时重复创建实例
    /// </summary>
    /// <returns></returns>
    public static T Instance
    {
        get 
        {
            if (_ServiceContext == null)
            {//双重检查锁定
                lock (lockObj)
                {
                    if (_ServiceContext == null)
                    {
                        _ServiceContext = new T();
                    }
                }
            }
            return _ServiceContext;
        }
    }
    public virtual void OnInitialize() { }

    public void Init() { }
}


public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{//这样一来，关于单例部分的代码就直接交给作为基类的该类了，而不用出现在派生类之中。
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
                if (_instance == null)
                {
                    GameObject singleton = new GameObject(); //该游戏对象会出现在Active场景
                    _instance = singleton.AddComponent<T>(); //立刻执行组件实例的Awake和OnEnable方法
                    singleton.name = typeof(T).ToString(); //类型名（会包含命名空间）作为游戏对象名，比如UIManager
                    DontDestroyOnLoad(singleton); //放到DontDestroyOnLoad场景中。单例的管理器往往都是如此。
                }
            }
            return _instance;
        }
    }

    public virtual void OnDestroy()
    {
        _instance = null;
    }
}

