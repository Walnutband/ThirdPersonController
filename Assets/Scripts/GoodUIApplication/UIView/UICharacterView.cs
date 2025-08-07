
using DG.Tweening;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterView : UIView
{
    #region 控件绑定变量声明，自动生成请勿手改
#pragma warning disable 0649
    [ControlBinding]
    private Button button_Close;
    [ControlBinding]
    private Button button_Attribute;
    [ControlBinding]
    private Image image_Left;
    [ControlBinding]
    private Image image_Right;
    [ControlBinding]
    private HorizontalSnapper snapper;

#pragma warning restore 0649
    #endregion

    private RectTransform left, right;
    // private Tween leftTween, rightTween;
    private Sequence sequence;

    public override void OnInit(UIControlData uIControlData, UIViewController controller)
    {
        base.OnInit(uIControlData, controller);
        left = image_Left.GetComponent<RectTransform>();
        right = image_Right.GetComponent<RectTransform>();
        Tween leftTween = left.DOAnchorPosX(0 - left.rect.width, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        Tween rightTween = right.DOAnchorPosX(right.rect.width, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        sequence.Append(leftTween).Join(rightTween).Pause();
    }

    public override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        sequence.Play();
    }

    public override void OnClose()
    {
        base.OnClose();
        sequence.Pause();
    }



    protected override void OnAddListener()
    {
        base.OnAddListener();
        button_Close.onClick.AddListener(OnCancel);
        button_Attribute.onClick.AddListener(OpenAttributeView);
        snapper.toBorder += ChangeFlag;
    }

    protected override void OnRemoveListener()
    {
        base.OnRemoveListener();
        button_Close.onClick.RemoveListener(OnCancel);
        button_Attribute.onClick.RemoveListener(OpenAttributeView);
        snapper.toBorder -= ChangeFlag;


    }

    public override void OnCloseAnim(TweenCallback complete)
    {
        UIManager.Instance.FadeOutIn(0.5f, complete);
    }

    public override void OnOpenAnim()
    {

    }

    private void OpenAttributeView()
    {
        UIManager.Instance.Open(UIViewType.UIAttributeView);
    }

    private void ChangeFlag(int dir) //0代表左，1代表右
    {
        if (dir == -1)
        {
            image_Left.SetAlpha(0f);
        }
        else if (dir == 1)
        {
            image_Right.SetAlpha(0f);
        }
        else //其实dir为0，就是没有触碰到边界，注意要重新显示。
        {
            image_Left.SetAlpha(1f);
            image_Right.SetAlpha(1f);
        }
    }
}