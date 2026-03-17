using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace ARPGDemo.UISystem_Test
{
    
    public class DamageNumber : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("动画设置")]
        [SerializeField] private float floatHeight = 0.5f;
        [SerializeField] private float floatDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private Ease floatEase = Ease.OutQuad;

        [Header("暴击设置")]
        [SerializeField] private float critFontSizeMultiplier = 1.5f; //暴击时字体的放大倍数
        [SerializeField] private Color critColor = Color.yellow;
        [SerializeField] private Vector3 critScale = new Vector3(1.2f, 1.2f, 1f);

        private Sequence animationSequence;

        public void Initialize(float damage, bool isCrit, float normalFontSize)
        {
            if (damageText == null)
                damageText = GetComponent<TextMeshProUGUI>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // 设置文本
            damageText.text = Mathf.RoundToInt(damage).ToString();
            damageText.fontSize = normalFontSize;

            // 设置透明度
            canvasGroup.alpha = 1f;

            // 暴击效果
            if (isCrit)
            {
                damageText.fontSize = normalFontSize * critFontSizeMultiplier;
                damageText.color = critColor;
                transform.localScale = critScale;
            }

            PlayAnimation();
        }

        private void PlayAnimation()
        {
            // 取消之前的动画
            animationSequence?.Kill();

            // 获取起始位置
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + new Vector3(0, floatHeight, 0);

            // 创建动画序列
            animationSequence = DOTween.Sequence();

            // 向上漂浮动画
            animationSequence.Join(transform.DOMove(endPos, floatDuration).SetEase(floatEase));

            // 淡出动画
            animationSequence.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetDelay(floatDuration - fadeOutDuration));

            // 动画完成后销毁对象
            // animationSequence.OnComplete(() =>
            // {
            //     Destroy(gameObject);
            // });
        }

        private void LateUpdate()
        {
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            if (DamageNumberGenerator.Instance.mainCamera == null) return;
            Vector3 direction = (transform.position - DamageNumberGenerator.Instance.mainCamera.position).normalized;
            direction.y = 0;  // 保持水平
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnDestroy()
        {
            animationSequence?.Kill();
        }
    }
}
