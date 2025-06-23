using System.Collections.Generic;
using UnityEngine;

/*实例对象池，注意区别，这里应该专门针对的游戏对象的对象池，而其他的对象池是针对无关游戏对象的实例的，大概可以认为是非Mono对象？说白了就是组件对象与非组件对象，因为组件对象
的生命期绑定在游戏对象上，所以存储组件对象的实例没有意义，而是应该存储其所在游戏对象的实例，而非组件对象没有游戏对象作为持续引用源，为了防止其在不用时因为没有任何引用而被
垃圾回收，所以就应该用一个对象池容器来专门引用，从而保持其存在，以便随后就可以取用，不用重新分配内存而浪费性能。*/

namespace MyPlugins.GoodUI
{

    public class InstancePool
    {
        private static string GameObjectName = "GameObjectPool";
        private static string RecycleName = "RecyclePool";
        private Dictionary<string, Stack<GameObject>> _allInstances = new Dictionary<string, Stack<GameObject>>();
        private Transform _instancePoolTransRoot = null;
        private Transform _recyclePoolTransRoot = null;

        public InstancePool()
        {
            var go = new GameObject(GameObjectName);
            // GameObject.DontDestroyOnLoad(go);
            ExtensionMethods.DontDestroyOnLoad(go);

            _instancePoolTransRoot = go.transform;
            go.SetActive(true); //保证激活

            _recyclePoolTransRoot = _instancePoolTransRoot.Find(RecycleName); //Transform.Find仅限于直接子对象
            if (_recyclePoolTransRoot == null)
            {//显然RecyclePool是GameObjectPool的直接子对象
                _recyclePoolTransRoot = new GameObject(RecycleName).transform;
                _recyclePoolTransRoot.SetParent(_instancePoolTransRoot);
            }
            _recyclePoolTransRoot.gameObject.SetActive(false);
        }

        public GameObject Get(string key)
        {
            Stack<GameObject> objects = null;
            //首先有无这个键值对，之后是该栈是否分配了内存以及是否确实存储了对象
            if (!_allInstances.TryGetValue(key, out objects))
            {
                return null;
            }
            else
            {
                if (objects == null || objects.Count == 0)
                {
                    return null;
                }

                return objects.Pop(); //弹出栈顶元素
            }
        }

        public void Recycle(string key, GameObject obj, bool forceDestroy = false)
        {
            //强制删除
            if (forceDestroy)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(obj);
                }
                else
                {
                    GameObject.DestroyImmediate(obj); //编辑模式下没有延迟销毁的功能，则直接销毁
                }
                return;
            }
            //不销毁，就是放入对象池，以便后续复用
            Stack<GameObject> objects = null;
            //如果还没有，就要分配一个专门的栈。从预制体来看，_allInstances的键就是预制体资产路径，而值也就是这个对象栈，就是存储的该预制体的各个实例化对象
            if (!_allInstances.TryGetValue(key, out objects))
            {
                objects = new Stack<GameObject>();
                _allInstances[key] = objects;
            }

            InitInst(obj, false); //放入回收对象池
            objects.Push(obj);
        }

        public void Clear(string key)
        {
            Stack<GameObject> objects = null;
            if (_allInstances.TryGetValue(key, out objects))
            {
                while (objects.Count > 0)
                {
                    GameObject objToDestroy = objects.Pop();
                    UnityEngine.AddressableAssets.Addressables.ReleaseInstance(objToDestroy);
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(objToDestroy);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(objToDestroy);
                    }
                }
            }
        }

        public void ClearAll()
        {
            foreach (var item in _allInstances)
            {
                while (item.Value.Count > 0)
                {
                    GameObject objToDestroy = item.Value.Pop();
                    UnityEngine.AddressableAssets.Addressables.ReleaseInstance(objToDestroy);
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(objToDestroy);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(objToDestroy);
                    }
                }
            }
            _allInstances.Clear();
        }

        /// <summary>
        /// 初始化实例（就是放入作为对象池的游戏对象之下）
        /// </summary>
        public void InitInst(GameObject inst, bool active = true)
        {
            if (inst != null)
            {
                if (active)
                {
                    inst.transform.SetParent(_instancePoolTransRoot, true); //世界位置不变，只是设置层级关系
                }
                else
                {
                    inst.transform.SetParent(_recyclePoolTransRoot, true);
                }
            }
        }
    }
}