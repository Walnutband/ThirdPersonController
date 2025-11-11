using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace ARPGDemo.UISystem_Old
{
    public class SceneController : SingletonMono<SceneController>
    {

        /// <summary>
        /// 同步加载指定场景（场景名必须已添加至 Build Settings）。
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 同步加载指定场景（通过 Build Index），适合有 Build Settings 索引的情况。
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        // public AsyncOperation LoadSceneAsync(string sceneName)
        // {
        //     AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        //     return asyncOperation;
        // }

        /// <summary>
        /// 异步加载指定场景，可以结合进度条显示加载进度。
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        public IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
            // 在加载期间输出加载进度
            while (!asyncOperation.isDone)
            {
                // float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                // Debug.Log($"Loading {sceneName}: {progress * 100f}%");
                yield return null;
            }

        }


        public void TransitionToSceneSingle(string newSceneName, Action loadStart = null, Action<float> loading = null, Action loadComplete = null)
        {
            StartCoroutine(TransitionSceneCoroutineSingle(newSceneName, loadStart, loading, loadComplete));
        }

        private IEnumerator TransitionSceneCoroutineSingle(string newSceneName, Action loadStart, Action<float> loading, Action loadComplete)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
            loadStart?.Invoke();

            while (!loadOperation.isDone)
            {
                float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                loading?.Invoke(progress);
                yield return null;
            }
            loading?.Invoke(1f);
            loadComplete?.Invoke();
            UIManager.Instance.UpdateCameraStack(); //每次切换场景时都应该更新，因为有可能主相机变了。
            //Ques:在加载场景之后，发现阴影部分变成了纯黑，因为没有反射光，但本来是应该有的，而调用这一行代码更新一下就正常了，但是具体原因，就必须要深入到URP渲染管线以及计算机图形学了
            DynamicGI.UpdateEnvironment(); 
        }

        /// <summary>
        /// 调用该方法进行场景过渡：加载新场景（Additive 模式），切换激活场景，并卸载旧场景
        /// </summary>
        /// <param name="newSceneName">需要加载的新场景名称，需要确保该场景已添加到 Build Settings 中并启用</param>
        /// <param name="loadStart">新场景开始加载时调用</param>
        /// <param name="loading">加载过程中每帧调用（传入进度，主要是用于更新进度条）</param>
        /// <param name="loadComplete">新场景加载完成时调用</param>
        /// <param name="unloadComplete">旧场景加载完成时调用</param>
        public void TransitionToSceneAdditive(string newSceneName, Action loadStart = null, Action<float> loading = null, Action<AsyncOperation> loadComplete = null, Action unloadComplete = null)
        {
            StartCoroutine(TransitionSceneCoroutineAdditive(newSceneName, loadStart, loading, loadComplete, unloadComplete));
        }

        /// <summary>
        /// 协程实现的场景过渡逻辑：
        /// 1. 记录当前活动场景作为旧场景
        /// 2. 异步加载新场景（采用 Additive 模式，确保旧场景依然保持活动，避免短暂“无场景”）
        /// 3. 等待新场景加载完成，然后设置新场景为活动场景
        /// 4. 异步卸载旧场景
        /// </summary>
        private IEnumerator TransitionSceneCoroutineAdditive(string newSceneName, Action loadStart, Action<float> loading, Action<AsyncOperation> loadComplete, Action unloadComplete)
        {
            // 记录当前活动场景，作为后续卸载的旧场景
            Scene oldScene = SceneManager.GetActiveScene();
            Debug.Log($"旧场景名为{oldScene.name}");

            // 开始异步加载新场景（Additive 模式）
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            // loadOperation.allowSceneActivation = true;
            loadOperation.allowSceneActivation = false;
            loadStart?.Invoke();
            // 实际上，loadOperation.progress最大值是 0.9，当达到 0.9 时表示已加载完成等待激活，所以这里除以0.9就可以取到0~1的范围，也就是归一化
            //BugFix：这里有个坑，isDone只会在场景真正被激活时才会为true，所以如果提前将allowSceneActivation设置为false的话，就会导致无限循环。所以要使用progress来作为循环条件
            // while (!loadOperation.isDone)
            while (!(loadOperation.progress >= 0.9f))
            {
                float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                Debug.Log($"加载场景 [{newSceneName}] 进度：{progress * 100f:F2}%");
                loading?.Invoke(progress);
                yield return null;
            }
            Debug.Log("新场景加载完成");
            loading?.Invoke(1f);
            loadComplete?.Invoke(loadOperation);

            //Tip:注意层级，UILoadingView往往是会覆盖在其他场景（主场景）上面的，所以不用担心

            loadOperation.allowSceneActivation = true;
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            Scene newScene = SceneManager.GetSceneByName(newSceneName);
            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
                Debug.Log($"新场景 [{newSceneName}] 已设为活动场景");
            }
            else
            {
                Debug.LogError($"加载场景失败：{newSceneName} 无法获取");
                yield break;
            }
            
            // 异步卸载旧场景
            // if (oldScene.IsValid()) Debug.Log("oldScene有效");
            // else Debug.Log("oldScene无效");
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(oldScene);
            // while (!unloadOperation.isDone)
            while (!unloadOperation.isDone)
            {
                Debug.Log($"正在卸载旧场景 [{oldScene.name}] 中...");
                yield return null;
            }
            unloadComplete?.Invoke();

            Debug.Log("场景切换完成！");
        }
    }
}