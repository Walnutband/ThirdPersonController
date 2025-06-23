
using DG.Tweening;
using MyPlugins.GoodUI;
using UnityEngine.UI;

public class UIMainView : UIView
{

    #region 控件绑定变量声明，自动生成请勿手改
#pragma warning disable 0649
    [ControlBinding]
    private Button button_Exit;
    [ControlBinding]
    private Button button_Settings;
    [ControlBinding]
    private Button button_Attribute;
    [ControlBinding]
    private Button button_Activities;

#pragma warning restore 0649
    #endregion



    protected override void OnAddListener()
    {
        base.OnAddListener();
        button_Exit.onClick.AddListener(OpenExitView);
    }

    protected override void OnRemoveListener()
    {
        base.OnRemoveListener();
        button_Exit.onClick.RemoveListener(OpenExitView);
    }

    public override void OnCloseAnim(TweenCallback complete)
    {
        complete?.Invoke();
    }

    public override void OnOpenAnim()
    {

    }

    private void OpenExitView()
    {
        UIManager.Instance.Open(UIViewType.UIExitView, "是否退回到开始界面？");
    }
}