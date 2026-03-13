using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.AddressableAssets;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyPlugins.GoodUI
{
    [ExecuteAlways] //Tip:一旦允许在编辑模式下运行一些生命周期方法的话，可能会出现很多意外bug，而且会增加冗余逻辑。
    [DisallowMultipleComponent]
    [AddComponentMenu("GoodUI/Controls/AccordionElement")]
    //这里的
    // [RequireComponent(typeof(VerticalLayoutGroup), typeof(LayoutElement))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class AccordionElement : UIBehaviour, IPointerClickHandler
    {

        // [SerializeField]
        // private float minHeight;

        [SerializeField]
        private float m_MinHeight;
        public float minHeight
        {
            get
            {
                // float headerPH = LayoutUtility.GetPreferredHeight(headerRect);
                //直接用标题对象的首选高度作为该元素的最小高度，因为这里的最小高度含义其实是折叠时的高度
                // return LayoutUtility.GetPreferredHeight(headerRect);
                // return LayoutUtility.GetMinHeight(headerRect);
                m_MinHeight = LayoutUtility.GetMinHeight(headerRect);
                return m_MinHeight;
            }
        }

        [SerializeField]
        [ContextMenuItem("打印m_IsExpand值", nameof(DebugIsExpand))]
        private bool m_IsExpand;
        public bool isExpand { get { return m_IsExpand; } set { ChangeState(value); } } //将所有setter逻辑都放在ChangeState中集中处理

        void DebugIsExpand()
        {
            Debug.Log($"m_IsExpand值为{m_IsExpand}");
        }

        [SerializeField]
        private RectTransform headerRect; //标题对象的Rect，用于获取最小高度即折叠状态下的AccordionElement对象的整体高度
        [SerializeField]
        private RectTransform contentRect;
        private LayoutElement m_ContentLayout;
        private LayoutElement contentLayout
        {
            get
            {
                if (m_ContentLayout == null)
                {
                    m_ContentLayout = contentRect.GetComponent<LayoutElement>();
                }
                return m_ContentLayout;
            }
        }
        // private LayoutElement contentLayout;
        [SerializeField]
        private float contentPreferredHeight;

        [SerializeField]
        private RectTransform expandFlag;
        private Image expandImage; //用于表示折叠状态的Image对象
        [SerializeField] private Sprite expandTopSprite; //折叠时图片
        [SerializeField] private Sprite expandBottomSprite; //展开时图片

        [SerializeField]
        private AccordionGroup m_Group;
        /// <summary>
        /// 当前AccordionGroup元素所属的AccordionGroup组
        /// </summary>
        public AccordionGroup group
        {
            get
            {
                //尝试从父对象获取组对象，就是说对于UGUI及其扩展的UI控件都会有一个默认的游戏对象层级关系，就可以根据这个关系来对代码添加一些自动化逻辑
                if (m_Group == null)
                    m_Group = GetComponentInParent<AccordionGroup>();
                return m_Group;
            }
            set { m_Group = value; }
        }

        private RectTransform m_RectTransform;
        public RectTransform rectTransform { get { return m_RectTransform; } set { SetPropertyUtility.SetClass(ref m_RectTransform, value); } }

        private LayoutElement m_LayoutElement;
        public LayoutElement layoutElement { get { return m_LayoutElement; } set { SetPropertyUtility.SetClass(ref m_LayoutElement, value); } }

        [Serializable]
        public class AccordionGroupElementEvent : UnityEvent<bool> //仿照UGUI控件比如Toggle定义一个内部的事件类型，就是指定了参数类型
        { }
        /// <summary>
        /// 定义值改变事件，也就是折叠和展开的二分状态
        /// </summary>
        /// <remarks>暂时只注册了OnValueChanged方法改变布局，而后续肯定需要注册播放音效的相关方法</remarks>
        public AccordionGroupElementEvent onValueChanged = new AccordionGroupElementEvent();

        private Graphic m_TargetGraphic;
        /// <summary>
        /// 该属性实质是代表该元素的显示和交互的可视化元素
        /// </summary>
        /// <remarks>组合优于继承</remarks>
        public Graphic targetGraphic { get { return m_TargetGraphic; } set { SetPropertyUtility.SetClass(ref m_TargetGraphic, value); } }
        

        /// <summary>
        /// 用于实现尺寸的过渡变化
        /// </summary>
        [NonSerialized]
        private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

        protected AccordionElement()
        {
            if (this.m_FloatTweenRunner == null)
                this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
            this.m_FloatTweenRunner.Init(this);
        }

        //须注意，无论是否启用，只要有实例，Awake都会按时调用
        protected override void Awake()
        {
            base.Awake();

            // onValueChanged.AddListener(OnValueChanged);
            // this.m_Group = this.gameObject.GetComponentInParent<AccordionGroup>(); //说明AccordionGroup应该位于父对象上，不过该方法也会查找当前对象
            this.m_RectTransform = this.transform as RectTransform;
            this.m_LayoutElement = this.gameObject.GetComponent<LayoutElement>();
        }

        //（编辑模式下）在选中组件所在对象时会调用OnEnable。不过测试了一下，似乎也不是，就是在热重载之后就会立刻统一调用
        protected override void OnEnable()
        {
            Debug.Log("OnEnable");
            onValueChanged.AddListener(OnValueChanged);
            AddToGroup();
            Init();
        }

        protected override void OnDisable()
        {
            onValueChanged.RemoveListener(OnValueChanged);
            RemoveFromGroup();
        }

        private void Init()
        {
            //基于规定的层级结构来初始化获取相关引用。
            headerRect = (RectTransform)transform.Find("Header"); //标题对象
            if (headerRect != null)
            {
                expandFlag = (RectTransform)headerRect.Find("Flag");
                // expandFlag.name = "Flag";
            }

            contentRect = (RectTransform)transform.Find("Content"); //结构明确，那么直接通过命名查找，简单高效
            // contentRect.name = "Content";

            if (expandFlag != null)
            {
                expandFlag.anchorMin = new Vector2(1f, 0f);
                expandFlag.anchorMax = new Vector2(1f, 1f);
                expandFlag.pivot = new Vector2(1f, 0.5f);
                expandImage = expandFlag.GetComponent<Image>();
                expandFlag.rotation = Quaternion.identity;
            }

#if UNITY_EDITOR
            //TODO：修改后的版本，这里的资产处理与UI视图预制体相关的资产处理比较割裂，还需要后续更进一步的迭代
            if (!Application.isPlaying)
            {
                LoadAssetsInEditor();
                return;
            }
            else
#endif
            { LoadAssets(); }
            // if (expandBottomSprite == null || expandTopSprite == null)
            // {
            //     LoadAssets();
            // }

            // //异步加载的同步执行写法。
            // expandTopSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Arrow Top (64x).png").Result;
            // expandBottomSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Arrow Bottom (64x).png").Result;

            // if (expandBottomSprite == null || expandTopSprite == null)

            //     //异步加载的同步执行写法。
            //     // expandBottomSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Arrow Bottom (64x).png").Result;
            //     // expandTopSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Arrow Top (64x).png").Result;
            //     //TODO: 这里只是简单加载资产，没有记录相关信息，只是在加载完成之后将结果赋值给这里变量引用即可。
            //     /*BugFix：这里有个坑，整个过程是，AccordionElement的该Init方法会在OnEnable中调用，而该类标记了ExecuteAlways，导致在编辑模式下也会调用OnEnable方法，那么此时
            //     就是在编辑哦模式下访问ResourceManager.Instance，由于SingleMono，会将单例组件放在DontDestroyOnLoad场景中，而DontDestroyOnLoad场景只能在运行模式下使用，所以
            //     就会报错（如下）。
            //     “InvalidOperationException: The following game object is invoking the DontDestroyOnLoad method: GameObjectPool. 
            //     Notice that DontDestroyOnLoad can only be used in play mode and, as such, cannot be part of an editor script.”
            //     */
            //     // ResourceManager.Instance.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Top (64x).png", (result) => expandTopSprite = result);
            //     // ResourceManager.Instance.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Bottom (64x).png", (result) => expandBottomSprite = result);
            //     // ResourceManager.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Top (64x).png", (result) => expandTopSprite = result);
            //     // ResourceManager.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Bottom (64x).png", (result) => expandBottomSprite = result);
            //     /*BugFix：这里还有个坑，不过准确来说是知识缺漏，在编辑时使用Addressables进行异步加载资产，会发现其句柄的Complete始终不会调用，暂时不清楚什么原因，但是要知道
            //     编辑模式下应该使用AssetDatabase处理资产，而不是Addressables。*/

            // }

        }

        private void LoadAssets()
        {
            Debug.Log("LoadAssets");
            // expandTopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/Arts/Textures/Icons/Navigation/Arrow Top (64x).png");
            // expandTopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/Arts/Textures/Icons/Navigation/Arrow Bottom (64x).png");
            //异步加载的同步执行写法。
            // SimpleLoadAsset<SpriteAtlas>("Textures/Icons/Navigation/Flags.spriteatlasv2", (atlas) =>
            // {
            //     expandTopSprite = atlas.GetSprite("Arrow Top (64x)");
            //     expandBottomSprite = atlas.GetSprite("Arrow Bottom (64x)");
            // });
            // expandTopSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Flags.spriteatlasv2[Arrow Top (64x)]").Result;
            // expandBottomSprite = Addressables.LoadAssetAsync<Sprite>("Textures/Icons/Navigation/Flags.spriteatlasv2[Arrow Bottom (64x)]").Result;
            // ResourceManager.Instance.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Top (64x).png", (result) => expandTopSprite = result);
            // ResourceManager.Instance.SimpleLoadAsset<Sprite>("Textures/Icons/Navigation/Arrow Bottom (64x).png", (result) => expandBottomSprite = result);

        }

#if UNITY_EDITOR
        private void LoadAssetsInEditor()
        {
            //异步加载的同步执行写法。
            Debug.Log("LoadAssetsInEditor");
            expandTopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/Arts/Textures/Icons/Navigation/Arrow Top (64x).png");
            expandBottomSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetsPackage/Arts/Textures/Icons/Navigation/Arrow Bottom (64x).png");
        }

        //Tip:取消[ExecuteAlways]标记之后，就在Reset方法中处理一下Awake和OnEnable的初始化逻辑
        protected override void Reset()
        {
            base.Reset();
            this.m_RectTransform = this.transform as RectTransform;
            this.m_LayoutElement = this.gameObject.GetComponent<LayoutElement>();
            AddToGroup();
            Init();
            LoadAssetsInEditor();
            m_MinHeight = LayoutUtility.GetMinHeight(headerRect);
        }

        public void SimpleLoadAsset<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
            }
            AsyncOperationHandle handle;
            handle = Addressables.LoadAssetAsync<T>(path);
            Debug.Log("正在加载");
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("加载成功");
                    onComplete?.Invoke(op.Result as T); //传入加载好的资产引用（内存地址）
                }
                else
                {
                    Debug.LogErrorFormat($"[LoadAssetAsync] {path} 加载失败！");
                    onComplete?.Invoke(null);
                }
            };
        }


#endif

        public void UpdateHeaderHeight()
        {
            if (headerRect != null)
            {
                Debug.Log("header");
                //通过AccordionElement检视器直接控制Header高度
                headerRect.GetComponent<LayoutElement>().minHeight = m_MinHeight;
                //BugFix:在实现单个AccordionElement即不位于组中时，因为不需要LayoutElement，就将其移除了，结果就出现空引用错误，并且检视面板直接黑屏了，最后发现是这里没有判空，因为之前都是默认有LayoutElement组件的，而且这次很迷惑的是，我印象中空引用错误是会打印出具体哪个变量空引用了，而且也仅仅是个空引用错误而已，结果这次竟然检视面板直接黑屏，简直离谱，前所未见。
                if (m_IsExpand == false && m_LayoutElement != null)
                {
                    m_LayoutElement.preferredHeight = m_MinHeight;
                }
            }

            if (expandFlag != null)
            {
                Debug.Log("flag");
                // expandFlag.sizeDelta = new Vector2(m_MinHeight, 0);
                // expandFlag.anchoredPosition = Vector2.zero;
            }
        }

        private void AddToGroup() //需要处理换组的问题
        {
            Transform parent = transform.parent;
            //注意GetComponentInParent方法包含了所有上层对象，而此处只是访问父对象，所以不能直接用。
            if (parent != null)
            {
                AccordionGroup parentGroup = parent.GetComponent<AccordionGroup>();
                if (parentGroup != null && parentGroup != m_Group)
                {
                    RemoveFromGroup();
                    parentGroup.AddElement(this);
                }
            }
        }
        private void RemoveFromGroup()
        {
            if (m_Group != null)
                m_Group.RemoveElement(this);
        }


        /// <summary>
        /// 用于改变状态，其实就是为了确定组的限制。
        /// </summary>
        /// <param name="isExpand"></param>
        /// <param name="byGroup">是否由Group调用</param>
        /// <remarks>这里的逻辑稍微复杂点，其实Toggle和ToggleGroup中的相关逻辑更加复杂，而且完全耦合，我这里还极大简化而且思路更清晰了</remarks>
        public void ChangeState(bool isExpand)
        {
            if (m_IsExpand == isExpand)
                return;
            //当设置为true时即展开时，确定合理性，也就是在唯一模式下的互斥性。
            if (isExpand == true && m_Group != null && m_Group.IsActive())
                m_Group.EnsureValidState(this);
            // Debug.Log($"{this.gameObject.name}调用ChangeState方法");
            m_IsExpand = isExpand;
            onValueChanged.Invoke(m_IsExpand);
        }

        public void OnValueChanged(bool isExpand)
        {
            // Debug.Log("OnVlueChanged");
            //需要处理在组中以及不在组中的情况
            /*不在组中时，结构有所不同，应该将AccordionElement对象固定尺寸，并且挂载的LayoutGroup组件应该控制子对象尺寸，
            在Content对象上挂载一个LayoutElement组件，然后通过调整该LayoutElement组件的首选尺寸，使得LayoutGroup组件会控制Content对象尺寸设置为指定的首选尺寸
            不过也可以*/
            if (group == null) //不在组中时，通常自身就不用挂载LayoutElement组件了
            {
                if (contentLayout != null)
                {
                    if (isExpand)
                    {
                        this.StartTween(contentRect.rect.height, contentPreferredHeight);
                    }
                    else
                    {
                        this.StartTween(contentRect.rect.height, 0f);
                    }
                }

                return;
            }
            else if (this.m_LayoutElement != null)
            {
                //BugFix：由于“?.”运算符是直接操作的C# null检查，不会使用重载逻辑，所以如果是UnityEngine.Object类型的话，可能会出现错误
                // expandFlag?.Rotate(new Vector3(0, 0, 180), Space.Self);
                // if (expandFlag != null) expandFlag.Rotate(new Vector3(0, 0, 180), Space.Self);

                //如果没有组的话，就会默认为Instant方式
                AccordionGroup.Transition transition = (this.m_Group != null) ? this.m_Group.transition : AccordionGroup.Transition.Instant;

                if (transition == AccordionGroup.Transition.Instant)
                {
                    if (isExpand)
                    {
                        // this.m_LayoutElement.preferredHeight = -1f;
                        this.m_LayoutElement.preferredHeight = GetExpandedHeight();
                        // expandFlag?.Rotate(new Vector3(0, 0, 180), Space.Self);
                        // if (expandFlag != null) expandFlag.localEulerAngles = Vector3.zero;
                        if (expandFlag != null) expandImage.sprite = expandBottomSprite;
                    }
                    else
                    {
                        this.m_LayoutElement.preferredHeight = this.minHeight;
                        // if (expandFlag != null) expandFlag.localEulerAngles = new Vector3(0f, 0f, 180f);
                        if (expandFlag != null) expandImage.sprite = expandTopSprite;

                    }
                }
                //Tip:注意在编辑模式下用于实现Tween动画的协程并不会按照预期工作，会看到直接变为目标值即Instant的效果。
                else if (transition == AccordionGroup.Transition.Tween)
                {
                    if (isExpand)
                    {
                        //TODO：这样改的话还有个问题是在过渡总量只有原本的一部分时，过渡时间还是与总量一致，那么中途再切换就会发现速度稍微慢了点，理应是更快切换的。
                        // this.StartTween(this.minHeight, this.GetExpandedHeight());
                        this.StartTween(this.m_RectTransform.rect.height, this.GetExpandedHeight());
                        // if (expandFlag != null) expandFlag.localEulerAngles = Vector3.zero;
                        if (expandFlag != null) expandImage.sprite = expandBottomSprite;
                    }
                    else
                    {
                        this.StartTween(this.m_RectTransform.rect.height, this.minHeight);
                        // if (expandFlag != null) expandFlag.localEulerAngles = new Vector3(0f, 0f, 180f);
                        if (expandFlag != null) expandImage.sprite = expandTopSprite;
                    }
                }
            }
            
        }

        /// <summary>
        /// 获取展开状态下的总高度
        /// </summary>
        /// <returns></returns>
        protected float GetExpandedHeight()
        {
            if (this.m_LayoutElement == null)
                return this.minHeight;
            float originalPrefH = this.m_LayoutElement.preferredHeight;
            this.m_LayoutElement.preferredHeight = -1f;
            //TODO：这里其实是获取该LayoutGroup对象下的子对象本来需要的总高度。其实可以考虑单独写一个专门的组件来实现这个功能。
            float h = LayoutUtility.GetPreferredHeight(this.m_RectTransform);
            this.m_LayoutElement.preferredHeight = originalPrefH; //此时preferredHeight不变是因为随后就会进行直接赋值（Instant）或者是使用协程过渡到目标值即此处的返回值
            return h;
        }

        /// <summary>
        /// 开始尺寸变化，实质上就是展开和折叠的状态切换
        /// </summary>
        /// <param name="startFloat"></param>
        /// <param name="targetFloat"></param>
        protected void StartTween(float startFloat, float targetFloat)
        {
            float duration = (this.m_Group != null) ? this.m_Group.transitionDuration : 0.1f;

            //初始化列表，初始值、目标值、过渡时间
            FloatTween info = new FloatTween
            {
                duration = duration,
                startFloat = startFloat,
                targetFloat = targetFloat
            };
            if (group != null)
                info.AddOnChangedCallback(SetHeight); //注册过渡时相应改变尺寸的方法
            else info.AddOnChangedCallback(SetContentHeight);
            info.ignoreTimeScale = true; //默认当然为true，通常来说UI元素都不可能会考虑Time.timeScale影响，可能3DUI才会需要受其影响
            this.m_FloatTweenRunner.StartTween(info);
            // Debug.Log("StartTween");
        }

        protected void SetHeight(float height)
        {
            if (this.m_LayoutElement == null)
                return;
            //在该属性的setter中会标记脏，即会注册到布局重建容器中
            this.m_LayoutElement.preferredHeight = height;
        }

        protected void SetContentHeight(float height)
        {
            if (this.contentLayout == null)
                return;
            this.m_ContentLayout.preferredHeight = height;
        }

        /// <summary>
        /// 指针点击事件，切换状态，触发事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            isExpand = !isExpand;
        }

    }

}