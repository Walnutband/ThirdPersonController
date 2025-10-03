using System.Linq;
using UnityEngine;

//在这个空间下，代表是ARPGDemo脚本中共用的一些内容，也就是被各个系统单向依赖的内容
namespace ARPGDemo
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T> //必须是该类的派生类才能作为泛型参数传入，提供单例类型的信息。
    {
        protected static T m_Instance; //以便派生类内部可以切换当前的单例，但不允许外界直接访问该字段。

        public static T Instance
        {
            get
            {//TODO:还要考虑在多线程环境下应该如何修改程序
                if (m_Instance == null)
                {
                    //TODO: 注意这里是在运行时用到了C#的动态特性，这是依赖于.Net框架的，如果要以IL2CPP为后端的话，就不能在运行时使用C#的动态特性。
                    GameObject go = new GameObject(typeof(T).Name); //直接以类名作为对象名
                    m_Instance = go.AddComponent(typeof(T)) as T;
                    DontDestroyOnLoad(go);
                }
                return m_Instance;
            }
        }

        protected virtual void Awake()
        {
            //使用标签进行初步分类，然后用严格的游戏对象名来标识。注意ControlSystem本身是组件类型，不是游戏对象，所以不能传入泛型参数。
            // m_Instance = GameObject.FindGameObjectsWithTag("System").FirstOrDefault(go => go.name == "ControlSystem").GetComponent<ControlSystem> as ;
            // m_Instance = GameManager.

            /*TODO：派生类重写时，就是在游戏开始时尝试获取已经存在的实例，因为这样的获取是基于具体类的，所以无法在该基类中编写相关逻辑。
            就是为了能够在编辑器中编辑这些单例的实例，而不是只能在进入运行模式后现场生成。
            不过也可以在这里定义一个抽象方法，专门用于查找已经存在的实例，让派生类实现即可，这样才更具有逻辑性，说白了就是确定是在下面这段逻辑之前执行，如果像这样不定义方法的话，
            就没有利用到这个隐含条件。*/
            if (m_Instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name); //直接以类名作为对象名
                m_Instance = go.AddComponent(typeof(T)) as T;
                DontDestroyOnLoad(go);
            }
            else
            {
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            Debug.Log($"游戏对象名：{m_Instance.gameObject.name}");
        }
    }

    public class Singleton<T> where T : class, new()
    {
        protected static T m_Instance;
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
        }
    }
}