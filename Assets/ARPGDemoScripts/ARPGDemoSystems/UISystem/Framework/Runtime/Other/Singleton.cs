using UnityEngine;

/*单例类往往都是一个独立类，不会搞什么继承之类的关系，所以在此专门定义实现单例的类，这样的话就可以把实现单例的逻辑从实际的单例类中移除了，使得单例类内部逻辑更加集中，显然
并不具有必要性，不过通过让单例类显式继承Singleton，具有一个提示性，表明其为单例类。*/

namespace ARPGDemo.UISystem_Old
{
    //非Mono单例
    public abstract class Singleton<T> where T : new() //泛型参数必须具有无参构造函数
    {
        private static T m_Instance;

        /// <summary>
        /// 禁止外部进行实例化
        /// </summary>
        protected Singleton()
        {
            OnInitialize();
        }

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new T();
                return m_Instance;
            }
        }
        public virtual void OnInitialize() { }

        // public void Init() { }
    }

    //Mono单例，由于组件必须挂载在游戏对象上，所以才会有getter中的那一段逻辑。
    //由于Mono组件的生命周期由Unity底层管理，所有没有定义初始化方法，只要自主实现生命周期方法即可。
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    //FindObjectOfType会在当前加载的所有场景中查找指定类型的对象（指的是GameObject及其组件），显然开销很大，因为要遍历游戏对象，判断其是否挂载有指定类型的组件。
                    m_Instance = (T)FindObjectOfType(typeof(T));
                    if (m_Instance == null)
                    {//没有找到就新建一个（会位于Active场景中）
                        GameObject singleton = new GameObject();
                        m_Instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString(); //直接以类型名称作为游戏对象名
                        ExtensionMethods.DontDestroyOnLoad(singleton); //放在DontDestroyOnLoad场景中，也比较合理，因为是单例，理应存在于全局空间。
                    }
                }
                return m_Instance;
            }
        }

        //定义销毁的周期方法
        public virtual void OnDestroy()
        {
            m_Instance = null;
        }
    }
}

































