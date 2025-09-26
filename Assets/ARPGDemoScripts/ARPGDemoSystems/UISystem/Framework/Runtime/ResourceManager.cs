using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
//Addressables是一个静态类，using static
using static UnityEngine.AddressableAssets.Addressables;

namespace MyPlugins.GoodUI
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        /// <summary>
        /// 已加载/正在加载中资源名对应的句柄
        /// 基本就是，记录预制体路径以及句柄，从AsyncOperationHandle的Result成员可以获取到加载得到的对象，只不过是object类型，但这里可以确定实际类型是GameObject，所以直接强制转换即可
        /// </summary>
        private Dictionary<string, AsyncOperationHandle> _handleCaches = new Dictionary<string, AsyncOperationHandle>();
        /// <summary>
        /// 正在进行加载状态中的资源的数量（因为加入了异步操作，也就是多线程编程，这类变量都是因为线程间交流而产生的）
        /// </summary>
        private int _loadingAssetCount = 0;
        /// <summary>
        /// 资源的引用个数（实例个数？）
        /// 键：通常是预制体路径
        /// 值：引用次数
        /// </summary>
        private Dictionary<string, int> _loadedAssetInstanceCountDic = new Dictionary<string, int>();
        /// <summary>
        /// 已实例化对象对应的Key
        /// </summary>
        /// <remarks>
        /// key: instanceId实例ID，系统自动分配                 
        /// value: path通常是预制体路径
        /// </remarks>
        private Dictionary<int, string> _objectInstanceIdKeyDic = new Dictionary<int, string>();
        /// <summary>
        /// 对象池
        /// </summary>
        private InstancePool _instancePool;
        /// <summary>
        /// 更新列表
        /// </summary>
        private List<string> updateKeys;

        /// <summary>
        /// 是否正在加载资源
        /// </summary>
        public bool IsProcessLoading
        {
            get => _loadingAssetCount > 0;
        }

        /// <summary>
        /// 在分配单例时调用，执行初始化，
        /// </summary>
        /// <remarks>注意这是"资源管理器"，在这里“资源”通常指的是预制体，即游戏对象，所以此处的初始化就是分配一个InstancePool实例，用于放置GameObject实例</remarks>
        public override void OnInitialize()
        {
            base.OnInitialize();
            _instancePool = new InstancePool();
        }

        #region 初始化/清除
        /// <summary>
        /// 初始化（确保Addressables系统已经准备好）
        /// </summary>
        public IEnumerator InitializeAsync()
        {
            //手动调用InitializeAsync方法确保Addressables 资源系统已经准备好，避免首次加载资源时的隐式初始化(就是惰性初始化，在这种非常基本、底层、频繁使用的系统中不应该使用惰性初始化，就是该在初始化阶段就完成自己的初始化)
            /*这里yield return会等待该方法返回的异步操作句柄AsyncOperationHandle<IResourceLocator>执行完成之后，才会执行后续。这里一定要理解协程的核心机制，
            Addressables.InitializeAsync中使用的是return而不是yield return，因为它返回的AsyncOperationHandle本来就是IEnumerator类型，*/
            // yield return Addressables.InitializeAsync();
            return Addressables.InitializeAsync();
        }



        #endregion


        #region 实例化和回收对象

        /// <summary>
        /// 异步实例化
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle InstantiateAsync(string path, Action<UnityEngine.GameObject> callback, bool active = true)
        {
            AsyncOperationHandle operationHandle = default;
            if (!_handleCaches.ContainsKey(path))
            {
                //未加载过此资源
                operationHandle = LoadAssetAsync<GameObject>(path, (obj) =>
                {
                    //_loadedAssetInstanceCountDic[path]--;
                    if (obj != null)
                    {
                        InternalInstantiate(path, callback, active);
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                });
            }
            else
            {
                operationHandle = _handleCaches[path];
                //已加载此资源且加载完成
                if (operationHandle.IsDone)
                {
                    InternalInstantiate(path, callback, active);
                }
                else
                {//正在加载
                    operationHandle.Completed += (result) =>
                    {
                        InternalInstantiate(path, callback, active);
                    };
                }
            }
            return operationHandle;
        }

        /// <summary>
        /// 回收游戏对象
        /// </summary>
        /// <param name="instanceObject"></param>
        /// <param name="forceDestroy"></param>
        public void Recycle(UnityEngine.GameObject instanceObject, bool forceDestroy = false)
        {
            if (instanceObject == null)
            {
                return;
            }

            int id = instanceObject.GetInstanceID();
            if (_objectInstanceIdKeyDic.ContainsKey(id))
            {
                _instancePool.Recycle(_objectInstanceIdKeyDic[id], instanceObject, forceDestroy);
                _loadedAssetInstanceCountDic[_objectInstanceIdKeyDic[id]]--;
                _objectInstanceIdKeyDic.Remove(id);
            }
            else
            {
                Debug.LogErrorFormat("此模块不回收不是从这个模块实例化出去的对象：{0}", instanceObject.name);
                GameObject.Destroy(instanceObject);
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback">以实例化对象作为参数的回调方法</param>
        private void InternalInstantiate(string path, Action<UnityEngine.GameObject> callback, bool active = true)
        {
            GameObject result = _instancePool.Get(path); //尝试从对象池中获取
            GameObject invokeResult = null;

            if (result == null)
            {
                //异步操作的结果对象,不过是object类型(C#类的共同基类),所以要转换为GameObject
                if (_handleCaches[path].Result != null)
                {
                    invokeResult = _handleCaches[path].Result as GameObject;
                    //此处通过实例化之后才会真正地分配内存，以及出现在场景中，在此之前都只是记录了类的信息。
                    invokeResult = GameObject.Instantiate(invokeResult);
                }
            }
            else
            {
                invokeResult = result;
            }

            if (invokeResult != null)
            {
                _instancePool.InitInst(invokeResult, active); //放在对象池对象之下。
                _objectInstanceIdKeyDic[invokeResult.GetInstanceID()] = path; //记录实例ID及其资源路径
                _loadedAssetInstanceCountDic[path]++; //记录引用次数
            }
            callback?.Invoke(invokeResult);
        }

        #endregion

        #region 资源加载/卸载   

#if UNITY_EDITOR
        public void SimpleLoadAsset<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
            }
            AsyncOperationHandle handle;
            handle = Addressables.LoadAssetAsync<T>(path);
            Debug.Log("正在加载");
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("加载成功");
                    onComplete?.Invoke(op.Result as T); //传入加载好的资产引用（内存地址）
                }
                else
                {
                    Debug.LogErrorFormat($"[LoadAssetAsync] {path} 加载失败！");
                    onComplete?.Invoke(null);
                }
            };
        }

#endif
        /// <summary>
        /// 异步加载
        /// </summary>
        public AsyncOperationHandle LoadAssetAsync<T>(string path, Action<T> onComplete, bool autoUnload = false) where T : class
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return default;
            }
            AsyncOperationHandle handle;
            //已加载或在加载中
            if (_handleCaches.TryGetValue(path, out handle))
            {
                if (handle.IsDone)
                {
                    onComplete?.Invoke(_handleCaches[path].Result as T);
                }
                else
                {
                    handle.Completed += (result) =>
                    {
                        if (result.Status == AsyncOperationStatus.Succeeded)
                        {
                            onComplete?.Invoke(result.Result as T);
                            if (autoUnload)
                            {
                                UnLoadAsset(path);
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat("[LoadAssetAsync] {0} 加载失败！", path);
                            onComplete?.Invoke(null);
                        }
                    };
                }
                return handle;
            }
            else //未加载过
            {
                _loadingAssetCount++;
                _loadedAssetInstanceCountDic.Add(path, 1);
                //处理异步加载
                handle = Addressables.LoadAssetAsync<T>(path);
                handle.Completed += (op) =>
                {
                    _loadingAssetCount--; //完成时调用，所以此处确定减一
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        onComplete?.Invoke(op.Result as T);
                        if (autoUnload)
                        {
                            UnLoadAsset(path);
                        }
                    }
                    else
                    {
                        //Debug.LogErrorFormat("[LoadAssetAsync] {0} 加载失败！", path);
                        onComplete?.Invoke(null);
                    }
                };
                _handleCaches.Add(path, handle);
                return handle;
            }
        }

        /// <summary>
        /// 直接卸载资源
        /// </summary>
        public void UnLoadAsset(string path)
        {
            // //判断卸载是否是一个常驻资源
            // if (_residentAssetsHashSet.Contains(path))
            // {
            //     Debug.LogErrorFormat("[UnLoadAsset] 禁止卸载常驻资源：{0} ！", path);
            //     return;
            // }

            // AsyncOperationHandle handle;
            // if (_handleCaches.TryGetValue(path, out handle))
            // {
            //     if (!handle.IsDone)
            //     {
            //         Debug.LogErrorFormat("[UnLoadAsset] 卸载了一个未加载完成的资源：{0} ！", path);
            //     }
            //     Debug.Log(string.Format("[UnLoadAsset] 卸载资源：{0} ！", path));

            //     if (_spriteCache.TryGetValue(path, out SpriteAtlas spriteAtlas))
            //     {
            //         spriteAtlas.Cleanup();
            //         _spriteCache.Remove(path);
            //     }
            //     _handleCaches.Remove(path);
            //     _loadedAssetInstanceCountDic.Remove(path);
            //     Addressables.Release(handle);
            // }
            // else
            // {
            //     Debug.LogErrorFormat("[UnLoadAsset] 卸载未加载资源：{0} ！", path);
            // }
        }

        #endregion
    }
}