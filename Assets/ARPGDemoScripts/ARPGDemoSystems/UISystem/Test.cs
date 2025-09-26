
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class Test_ : MonoBehaviour
{
    public RectTransform target;

    [ContextMenu("测试DOTween动画")]
    public void DOTweenTest()
    {//Tip:突然忘了，DOTween动画设置Ease是通过链式调用，而不是创建Tween时传参
     // (transform as RectTransform).rect.height
     // target.DOAnchorPosY(new Vector2(0f, (transform as RectTransform).rect.height), 1f).SetEase(Ease.OutBounce).From();
     float h = (transform as RectTransform).rect.height;
     target.anchoredPosition = new Vector2(target.anchoredPosition.x, h);
     target.DOAnchorPosY(0f, 1f).SetEase(Ease.OutElastic);

     // target.DOMoveY(0f, 5f);
        // target.DOAnchorPosX(540f, 1f, true).SetEase(Ease.OutElastic);
    }
}