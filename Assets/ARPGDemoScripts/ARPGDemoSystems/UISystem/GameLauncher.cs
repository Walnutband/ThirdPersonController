using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyPlugins.GoodUI;

namespace ARPGDemo.UISystem_Old
{

    public class GameLauncher : MonoBehaviour
    {
        public GameObject splash;
        // public UIConfigData uiConfig;

        private void Awake()
        {
            //都是启动时的处理，那么把对象直接放在一起方便处理。
            // 这里的Splash在游戏中通常指的就是一开始界面，此处只是一个黑色幕布，等待Loading场景加载完毕后，就过渡到透明，显示加载界面，启动加载进度条。
            splash = transform.Find("Splash").gameObject;
            if (splash == null)
            {//在子对象中没有找到，就直接从场景中寻找了。
                splash = GameObject.Find("Splash");
            }
        }

        /*在所有生命周期方法中只有Start可以设置为协程方法，大概因为初始化过程不一定是在一帧内就能够完成的，而且Awake只是负责初始化方面的简单工作（获取引用，准备好引用对象），
        */
        IEnumerator Start()
        {
            Debug.Log("Start刚开始");
            ///三个异步操作，首先是Addressables初始化，然后是加载并初始化UI配置数据，其实最必要的就是这两部分，后面预加载UILoadingView是因为随后就要进入到该UI视图中。///
            //yield return后续语句不可能在当前帧执行。Unity 的协程是基于主循环调度的。当协程执行到 yield return，它会保存当前的状态，并在当前帧结束后（或到达指定的条件时）再恢复执行
            /*Tip:这些yield return语句的条件调用的方法返回的都是IEnumerator类型，通过yield return，Unity内部就会等待其IEnumerator执行完毕也就是MoveNext方法返回false，*/
            yield return ResourceManager.Instance.InitializeAsync(); //异步初始化
            //在初始化时，此处访问Instance，就会通过AddComponent来创建UIManager的实例，就会立刻调用Awake和OnEnable，随后也仍然会在同一帧调用Start。
            //UIManager的Awake、OnEnable和Start方法应该都会在IniUIConfig方法之前调用
            yield return UIManager.Instance.InitUIConfig(); //初始化（加载）所有UI的配置

            // UIManager.Instance.InitializeUIConfig();

            //预加载UIStartView预制体。（只是加载到内存，还没有显示，）
            yield return UIManager.Instance.Preload(UIViewType.UIStartView);

            EnterLoading(); //进入到加载界面，渐入后正式开始加载。
        }


        /*进入加载界面，从黑布Splash渐入，透明之后就开始加载。*/
        private void EnterLoading()
        {
            //直接打开UILoadingView，不过还不开始加载，EnterLoading方法被调用时
            UIManager.Instance.Open(UIViewType.UIStartView);

            Image image = splash.GetComponentInChildren<Image>();

            image.DOFade(0f, 0.5f).onComplete += () =>
            {
                image.raycastTarget = false; //此时已经完全透明，切勿阻挡射线。

            };


        }
    }
}