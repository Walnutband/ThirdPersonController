using System;
using System.Collections;
using System.Collections.Generic;

namespace ARPGDemo.UISystem_Old
{
    public static class Pool
    {
        //存储所有存在的对象池
        public readonly static List<PoolBase> AllPool = new List<PoolBase>();

        public static void ReleaseAll()
        {
            foreach (var pool in AllPool)
            {
                pool.Dispose();
            }
            AllPool.Clear();
        }
    }

    public interface IObject
    {
        void OnRelease();
    }

    public interface PoolBase
    {
        void Dispose();
    }

    public class ObjectPool<T> : PoolBase where T : new()
    {
        private static ObjectPool<T> Instance;

        private Stack<T> _pool; //因为没有将该对象池容器设置为静态，所以才专门提供了一个静态单例Instance，其实也不是入口，因为静态方法Get才是直接的入口。

        private ObjectPool() { }


        private static void Init()
        {
            if (Instance == null)
            {
                Instance = new ObjectPool<T>();
                Instance._pool = new Stack<T>();
                Pool.AllPool.Add(Instance); //相当于注册、登记
            }
        }

        public static T Get()
        {
            Init(); //惰性初始化，在获取时才进行初始化

            if (Instance._pool.Count > 0)
            {
                return Instance._pool.Pop();
            }
            else
            {
                return new T();
            }
        }

        public static void Release(T obj)
        {
            //这里就注意与Get区别，并不会进行初始化，因为这本来就应该是Get的任务，默认前提就是首先执行过Get，之后才会有Release来释放。
            if (obj == null || Instance == null) return;

            if (obj is IObject interfac)
            {
                interfac.OnRelease();
            }
            Instance._pool.Push(obj);
        }

        public void Dispose()
        {
            if (Instance != null)
            {
                if (Instance._pool != null)
                {
                    Instance._pool.Clear();
                    Instance._pool = null;
                }
                Instance = null;
            }
        }
    }

    /// <summary>
    /// 列表对象池。其实Unity自己也有ListPool类，不过这里是自定义的实现。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListPool<T> : PoolBase
    {
        private static ListPool<T> Instance; //这引用的是从当前对象池中取出的其中一个实例

        private Stack<List<T>> _pool;

        private ListPool() { }

        private static void Init()
        {
            if (Instance == null)
            {
                Instance = new ListPool<T>();
                Instance._pool = new Stack<List<T>>();
                Pool.AllPool.Add(Instance);
            }
        }

        public static List<T> Get()
        {
            Init();

            if (Instance._pool.Count > 0)
            {
                return Instance._pool.Pop();
            }
            else
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// 放回对象池，就是清空（初始化）实例，并且添加到对象池容器中
        /// </summary>
        /// <param name="list"></param>
        public static void Release(List<T> list)
        {
            if (list == null || Instance == null) return;
            list.Clear();
            Instance._pool.Push(list);
        }

        public void Dispose()
        {
            if (Instance != null)
            {
                if (Instance._pool != null)
                {
                    Instance._pool.Clear();
                    Instance._pool = null;
                }
                Instance = null;
            }
        }
    }

    public class DictionaryPool<Key, Value> : PoolBase
    {
        private static DictionaryPool<Key, Value> Instance;

        private Stack<Dictionary<Key, Value>> _pool;

        private DictionaryPool() { }

        private static void Init()
        {
            if (Instance == null)
            {
                Instance = new DictionaryPool<Key, Value>();
                Instance._pool = new Stack<Dictionary<Key, Value>>();
                Pool.AllPool.Add(Instance);
            }
        }

        public static Dictionary<Key, Value> Get()
        {
            Init();

            if (Instance._pool.Count > 0)
            {
                return Instance._pool.Pop();
            }
            else
            {
                return new Dictionary<Key, Value>();
            }
        }

        public static void Release(Dictionary<Key, Value> dict)
        {
            if (dict == null || Instance == null) return;
            dict.Clear();
            Instance._pool.Push(dict);
        }

        public void Dispose()
        {
            if (Instance != null)
            {
                if (Instance._pool != null)
                {
                    Instance._pool.Clear();
                    Instance._pool = null;
                }
                Instance = null;
            }
        }
    }
}
