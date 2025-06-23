using System;
using System.Collections;
using UnityEngine;

namespace SkierFramework
{
    public class LoadingData
    {
        public LoadingFunc loadingFunc;
        public bool isCleanupAsset = false;
    }

    public delegate IEnumerator LoadingFunc(Action<float, string> loadingRefresh);

    /// <summary>
    /// 实际游戏中的loading
    /// </summary>
    public class Loading : SingletonMono<Loading>
    {
        private LoadingData _loadingData;
        private Coroutine _cor; //用于保存对于进行中的协程的引用，以便进行手动停止（StopCoroutine）等操作。

        public void StartLoading(LoadingFunc loadingFunc, bool isCleanupAsset = false)
        {
            StartLoading(new LoadingData { loadingFunc = loadingFunc, isCleanupAsset = isCleanupAsset });
        }

        public void StartLoading(LoadingData loadingData)
        {
            //开启UI
            UIManager.Instance.Open(UIType.UILoadingView);

            if (loadingData.loadingFunc != null)
            {
                _loadingData = loadingData;

                if (_cor != null)
                {
                    StopCoroutine(_cor);
                }
                _cor = StartCoroutine(CorLoading());
            }
            else
            {
                Debug.LogError("加载错误,没有参数LoadingData！");
            }
        }

        private IEnumerator CorLoading()
        {
            yield return StartCoroutine(_loadingData.loadingFunc(RefreshLoading));

            if (_loadingData != null && _loadingData.isCleanupAsset)
            {
                yield return ResourceManager.Instance.CleanupAsync();
                yield return Resources.UnloadUnusedAssets();
            }

            Pool.ReleaseAll(); //释放（清空）所有对象池
            yield return null;

            GC.Collect(); //手动通知立刻进行垃圾回收，因为在上面释放之后，会留下大量未被引用的实例，此时就可以将其一并回收。
            yield return null;

            Exit();

            _cor = null;
        }

        private void RefreshLoading(float loading, string desc)
        {
            // 刷新
            // TODO：这里的GetView设计不太好，因为UIType和UIView要分开传入。由于UIType本来就是对当前存在的UI视图对象（预制体）的一个标识符，而按理来说每个这样的预制体都应该有一个UIView派生的逻辑组件，那么其实可以尝试将比如这里的UILoadingView作为类型，而将nameof(UILoadingView)作为标识符来从字典（_viewControllers）获取
            var view = UIManager.Instance.GetView<UILoadingView>(UIType.UILoadingView);
            if (view != null)
            {
                view.SetLoading(loading, desc);
            }
            if (!string.IsNullOrEmpty(desc))
            {
                Debug.Log(desc);
            }
        }

        private void Exit()
        {
            // 关闭UI
            UIManager.Instance.Close(UIType.UILoadingView);

            ObjectPool<LoadingData>.Release(_loadingData);
            _loadingData = null;
        }
    }
}
