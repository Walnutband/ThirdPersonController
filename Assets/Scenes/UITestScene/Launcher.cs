using SkierFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public GameObject Splash;

    void Start()
    {
        if (Splash == null)
        {
            Splash = GameObject.Find(nameof(Splash));
        }

        StartCoroutine(StartCor());
    }

    //根据以下的yield return语句，该协程应该至少要用四帧执行完毕。
    IEnumerator StartCor()
    {
        //yield return后续语句不可能在当前帧执行。Unity 的协程是基于主循环调度的。当协程执行到 yield return，它会保存当前的状态，并在当前帧结束后（或到达指定的条件时）再恢复执行
        yield return StartCoroutine(ResourceManager.Instance.InitializeAsync()); //异步初始化
        //在初始化时，此处访问Instance，就会通过AddComponent来创建UIManager的实例，就会立刻调用Awake和OnEnable，随后也仍然会在同一帧调用Start。
        //UIManager的Awake、OnEnable和Start方法应该都会在IniUIConfig方法之前调用
        yield return UIManager.Instance.InitUIConfig(); //初始化（加载）所有UI的配置
        Debug.Log("InitUIConfig之后");
        //预加载UILoadingView预制体。（只是加载到内存，还没有显示，）
        yield return UIManager.Instance.Preload(UIType.UILoadingView);
        Debug.Log("UILoadingView之后");
        yield return UIManager.Instance.Preload(UIType.UILoginView);
        Debug.Log("UILoginView之后");

        Loading.Instance.StartLoading(EnterGameCor);

        if (Splash != null)
        {
            Splash.SetActive(false);
        }
    }

    /// <summary>
    /// 进入游戏时的加载。使用协程实现跨帧执行、分段执行、分条件执行
    /// </summary>
    /// <param name="loadingRefresh">用于刷新加载信息的委托方法，float参数指的是加载进度，string参数指的是加载的提示信息（描述）</param>
    IEnumerator EnterGameCor(Action<float, string> loadingRefresh)
    {//这里只是对于初始化加载界面的一个示意程序，实际上与加载过程本身无关，只是模拟出了正在加载的那种界面效果
        loadingRefresh?.Invoke(0.3f, "loading..........1");
        yield return new WaitForSeconds(0.5f);

        loadingRefresh?.Invoke(0.6f, "loading..........2");
        yield return new WaitForSeconds(0.5f);

        loadingRefresh?.Invoke(1, "loading..........3");
        yield return new WaitForSeconds(0.5f);
        //加载完之后，使用UIManager提供的Open方法打开指定的UI视图，
        UIManager.Instance.Open(UIType.UILoginView);
    }
}
