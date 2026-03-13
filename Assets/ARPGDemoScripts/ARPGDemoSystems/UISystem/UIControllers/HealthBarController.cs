using ARPGDemo.AbilitySystem;
using Codice.CM.Client.Differences.Graphic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem.UIControllers
{

    public class HealthBarController : MonoBehaviour
    {
        [Header("血条组件")]
        [SerializeField] private Image fillImage;  // 填充图片
        [SerializeField] private Image tweenImage; //过渡图片。
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("血条位置偏移")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

        [Header("跟随设置")]
        [SerializeField] private bool alwaysFaceCamera = true;  // 始终面向相机

        [SerializeField] private Transform mainCamera;
        [SerializeField] private Transform followTarget;  // 要跟随的目标对象

        private IAbilitySystemComponent m_TargetASC;

        private void Awake()
        {
            m_TargetASC = followTarget.GetComponentInParent<IAbilitySystemComponent>();
        }

        private void Start()
        {
            // 获取主相机
            mainCamera = Camera.main.transform;

            // 如果没有指定跟随目标，则跟随父对象
            if (followTarget == null)
                followTarget = transform.parent;

            // 如果没有指定填充图片，尝试获取
            if (fillImage == null)
                fillImage = GetComponent<Image>();

            InitializeHealthBar();
        }

        private void InitializeHealthBar()
        {
            float fillAmount = 1f;
            fillImage.fillAmount = fillAmount;
            tweenImage.fillAmount = fillAmount;
            healthText.text = $"{m_TargetASC.AS.GetHPCurrent()}/{m_TargetASC.AS.GetHPMax()}";

            currentHealth = m_TargetASC.AS.GetHPMax();
        }

        private void OnEnable()
        {
            m_TargetASC.AS.RegisterHPChangedEvent(UpdateHealthBar); 
        }
        private void OnDisable()
        {
            m_TargetASC.AS.RegisterHPChangedEvent(UpdateHealthBar);
        }


        private void LateUpdate()
        {
            if (followTarget == null) return;

            // 1. 位置跟随
            UpdatePosition();

            // 2. 面向相机
            if (alwaysFaceCamera && mainCamera != null)
                UpdateRotation();
        }

        [ContextMenu("更新位置")]
        /// <summary>
        /// 更新血条位置
        /// </summary>
        private void UpdatePosition()
        {
            // 计算世界空间中的位置：目标位置 + 偏移
            Vector3 worldPos = followTarget.position + offset;
            transform.position = worldPos;
        }

        /// <summary>
        /// 更新血条旋转（始终面向相机）
        /// </summary>
        private void UpdateRotation()
        {
            // 方法1：直接面向相机（简单，但可能导致倾斜）
            // transform.LookAt(mainCamera);

            // 方法2：只绕Y轴旋转（推荐，保持血条水平）
            //Tip：注意这里看起来反向，其实是因为UGUI本来就是反向的，内容向其Z轴负方向进行显示。
            Vector3 direction = (transform.position - mainCamera.position).normalized;
            direction.y = 0;  // 保持水平
            transform.rotation = Quaternion.LookRotation(direction);

            // 方法3：如果Canvas需要反向旋转（取决于设置）
            // transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        }

        /// <summary>
        /// 设置要跟随的目标
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>
        /// 设置血条偏移量
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        private float currentHealth;

        private void UpdateHealthBar(float _value)
        {
            Debug.Log($"当前生命值：{_value}");
            //没变就啥都不干。
            if (Mathf.Abs(_value - currentHealth) < 0.001f) return;

            UpdateHealth(_value, m_TargetASC.AS.GetHPMax(), _value > currentHealth? true : false);
            currentHealth = _value;
        }

        /// <summary>
        /// 更新血条显示（0-1之间）
        /// </summary>
        private void UpdateHealth(float currentHealth, float maxHealth, bool increase)
        {
            if (fillImage != null && maxHealth > 0)
            {
                float fillAmount = currentHealth / maxHealth;
                Debug.Log($"fillAmount: {fillAmount}, maxHealth: {maxHealth}, currentHealth: {currentHealth}");
                if (increase == false)
                {
                    //填充血条
                    fillImage.fillAmount = Mathf.Clamp01(fillAmount);

                    //用作Tween过渡的血条
                    DOTween.To(() => tweenImage.fillAmount, x => tweenImage.fillAmount = x, fillAmount, 0.6f);
                }
                else
                {
                    DOTween.To(() => fillImage.fillAmount, x => fillImage.fillAmount = x, fillAmount, 0.2f);
                    DOTween.To(() => tweenImage.fillAmount, x => tweenImage.fillAmount = x, fillAmount, 0.2f);
                }

                healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";

            }

            //Tip：含义是受到了攻击。
            if (increase == false)
            {
                
            }
        }
    }
}