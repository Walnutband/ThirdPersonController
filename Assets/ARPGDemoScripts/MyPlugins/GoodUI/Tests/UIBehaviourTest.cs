using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIBehaviourTest : MonoBehaviour
{
    public static UIBehaviourTest instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //这是两个静态事件成员，所以在热重载之后就会清空。
            Canvas.preWillRenderCanvases += PreWillRenderCanvases;
            Canvas.willRenderCanvases += WillRenderCanvases;
            RectTransform.reapplyDrivenProperties += ReapplyDrivenProperties;
        }
        Debug.Log($"在{Time.frameCount}帧触发了Awake");
    }
 
    private void PreWillRenderCanvases()
    {
        Debug.Log($"在{Time.frameCount}帧触发了Canvas.PreWillRenderCanvases");
    }

    private void WillRenderCanvases()
    {
        Debug.Log($"在{Time.frameCount}帧触发了Canvas.willRenderCanvases");
    }

    private void ReapplyDrivenProperties(RectTransform driven)
    {
        Debug.Log($"在{Time.frameCount}帧触发了RectTransform.reapplyDrivenProperties");
    }
 
    private void Start()
    {
        Debug.Log($"在{Time.frameCount}帧触发了Start");
    }

    private void Update()
    {
        Debug.Log($"在{Time.frameCount}帧触发了Update");

    }

    private void FixedUpdate()
    {
        Debug.Log($"在{Time.frameCount}帧触发了FixedUpdate");

    }

    private void LateUpdate()
    {
        Debug.Log($"在{Time.frameCount}帧触发了LateUpdate");

    }

    private void OnPreCull()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnPreCull");

    }

    private void OnBecameVisible()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnBacameVisible");

    }

    private void OnWillRenderObject()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnWillRenderObject");

    }

    private void OnPreRender()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnPreRender");

    }

    private void OnRenderObject()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnRenderObject");

    }

    private void OnPostRender()
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnPostRender");

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Debug.Log($"在{Time.frameCount}帧触发了OnRenderImage");

    }

    // public float test;
    // public bool trigger;

    // private void Update()
    // {
    // if (trigger)
    // {
    // trigger = false;
    // RectTransform rect = transform as RectTransform;
    // rect.sizeDelta = new Vector2(rect.sizeDelta.x + 1, rect.sizeDelta.y);
    // }
    // }

    // protected void OnRectTransformDimensionsChange()
    // {
    // Debug.Log($@"游戏对象{name}调用了RectTransformDimensionsChange
    // 在第{Time.frameCount}帧时调用
    // 当前的RectTransform相关信息：{(transform as RectTransform).ToString()}");
    // }

    // protected   void OnBeforeTransformParentChanged()
    // {
    // Debug.Log($@"游戏对象{name}调用了OnBeforeTransformParentChanged
    // 在第{Time.frameCount}帧时调用
    // 当前父对象是{transform.parent.name}");
    // }

    // protected   void OnTransformParentChanged()
    // {
    // Debug.Log($@"游戏对象{name}调用了OnTransformParentChanged
    // 在第{Time.frameCount}帧时调用
    // 当前父对象是{transform.parent.name}");
    // }

    // protected   void OnDidApplyAnimationProperties()
    // {
    // Debug.Log($@"游戏对象{name}调用了OnDidApplyAnimationProperties
    // 在第{Time.frameCount}帧时调用
    // 当前的test值为{test}");
    // }

    // protected   void OnCanvasGroupChanged()
    // {
    // Debug.Log($@"游戏对象{name}调用了OnCanvasGroupChanged
    // 在第{Time.frameCount}帧时调用");
    // }

    // protected   void OnCanvasHierarchyChanged()
    // {
    // Debug.Log($@"游戏对象{name}调用了OnCanvasHierarchyChanged
    // 在第{Time.frameCount}帧时调用");
    // }
}