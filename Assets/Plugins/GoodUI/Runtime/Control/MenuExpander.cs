
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("GoodUI/Controls/MenuExpander")]
[RequireComponent(typeof(Image))] //主要是需要一个Graphic，接收射线检测，该组件应该挂载在作为父对象的面板上，也就是点击按钮之外的区域即面板中的区域，就会调用这里实现的OnPointerClick方法
public class MenuExpander : UIBehaviour, IPointerClickHandler
{
    public Button menuButton;
    public RectTransform options;
    public float foldSize;
    public float expandSize;
    public float expandDuration = 0.3f;
    public float showDuration = 0.2f;

    private bool isExpand = false;
    private Sequence expandSeq;
    private Sequence foldSeq;
    private CanvasGroup[] canvasGroups;


    protected override void Awake()
    {
        int childCount = options.childCount;
        canvasGroups = new CanvasGroup[childCount];
        for (int i = 0; i < childCount; i++)
        {
            canvasGroups[i] = options.GetChild(i).GetOrAddComponent<CanvasGroup>();
        }


    }

    protected override void Start()
    {
        menuButton.onClick.AddListener(OpenMenu);
        SetTween(); //设置好Tween动画的数据，到时候再播放
        options.GetOrAddComponent<CanvasGroup>().alpha = 0f;
        options.gameObject.SetActive(false);
        foreach (var c in canvasGroups)
        {
            c.alpha = 0f;
        }
    }

    /// <summary>
    /// 因为要多次使用，所以在初始化时就设置好，并且使用SetAutoKill设置不要销毁，以及Pause暂停即不要立刻执行，后续直接使用Restart方法就可以多次执行了。
    /// </summary>
    /// <remarks>由于初始化时就确定好了，所以会发现在检视器中修改duration会没有效果</remarks>
    private void SetTween()
    {
        expandSeq = DOTween.Sequence();
        expandSeq.Append(menuButton.GetOrAddComponent<CanvasGroup>().DOFade(0f, 0.2f));
        expandSeq.Join(options.GetOrAddComponent<CanvasGroup>().DOFade(1f, 0.2f));
        // expandSeq.Append(options.DOSizeDelta(new Vector2(options.sizeDelta.x, expandSize), expandDuration));

        int count = 0;
        foreach (var c in canvasGroups)
        {
            count++;
            expandSeq.Append(c.DOFade(1f, count * showDuration));
        }
        expandSeq.Insert(0.4f, options.DOSizeDelta(new Vector2(options.sizeDelta.x, expandSize), expandDuration));



        // sequence.Join()
        expandSeq.onComplete += () => isExpand = true;
        expandSeq.SetAutoKill(false).Pause();

        foldSeq = DOTween.Sequence();
        foldSeq.Append(options.DOSizeDelta(new Vector2(options.sizeDelta.x, foldSize), expandDuration));
        foldSeq.Append(options.GetOrAddComponent<CanvasGroup>().DOFade(0f, 0.2f));
        foldSeq.Join(menuButton.GetOrAddComponent<CanvasGroup>().DOFade(1f, 0.2f));
        foldSeq.onComplete += () =>
        {
            options.gameObject.SetActive(false);
            foreach (var c in canvasGroups)
            {
                c.alpha = 0f;
            }
        };
        foldSeq.SetAutoKill(false).Pause();


    }

    private void OpenMenu()
    {
        options.gameObject.SetActive(true);
        expandSeq.Restart();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isExpand)
        {
            isExpand = false;
            foldSeq.Restart();
        }
    }

}