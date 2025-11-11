using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
// using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

namespace ARPGDemo.UISystem_Old
{
    /// <summary>
    /// UI黑边类型，还是自适应的问题
    /// </summary>
    /// <remarks>基本原理是固定的，</remarks>
    public enum UIBlackType
    {
        None,       // 无黑边，全适应
        Height,     // 保持高度填满，两边黑边
        Width,      // 保持宽度填满, 上下黑边
        AutoBlack,  // 自动黑边(选中左右或上下黑边最少的一方)
    }

    public class UIManager : SingletonMono<UIManager>
    {
        //参考分辨率，通常应该是1920*1080
        // public int width = 1080;
        // public int height = 1920;
        public int width = 1920;
        public int height = 1080;
        public UIBlackType uiBlackType = UIBlackType.None;

        public bool InMainView = false;
        public UnityEngine.InputSystem.PlayerInput playerInput;

        private Transform m_Root;
        private Camera _worldCamera;
        private Camera m_UICamera;
        /// <summary>
        /// 屏幕渐变遮罩
        /// </summary>
        private CanvasGroup _blackMask; //覆盖在上面实现转变时的过渡效果
        private CanvasGroup _backgroundMask; //用于遮挡背后的UI元素，以突出当前最表面的接受交互的UI元素。
        private Tweener _fadeTweener; //保存DOTween创建的Tweener，就像保存协程方法的IEnumerator或者Coroutine一样。
        /// <summary>
        /// 黑边
        /// </summary>
        private RectTransform[] _blacks = new RectTransform[2];

        //字典和哈希集合都是提供查询UIType和UILayer的标识符集合，应该说枚举类型本身就是用作标识符的。查询操作是O(1)，非常快
        private Dictionary<UIViewType, UIViewController> m_ViewControllers; //在InitUIConfig方法中填充
        private Dictionary<UILayer, UILayerLogic> _layers; //在Initialize方法中填充
        private HashSet<UIViewType> m_OpenViews;
        private HashSet<UIViewType> _residentViews;

        public EventSystem EventSystem { get; private set; }
        /*TODO：原话是，不同UI之间如果需要交互，为了解耦通常情况下不会直接获取UI引用，而是通过事件的方式，因此还需要在UIManager上实现一下事件系统，方便UI之间的交互。
        这里指的UI交流是对于UI视图之间来说的，而UI视图以内的就是交给各个UI视图的UIView派生类来处理。所以这里的Event还算是比较重要的一个事件控制器。*/
        public EventController<UIEvent> Event { get; private set; }
        //是否已经加载过UI配置文件，因为可以从主场景退回到开始菜单，所以要避免重复加载。
        private bool hasInitConfig = false;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化方法，重点是通过这些成员来了解该管理器的职能。
        /// </summary>
        private void Initialize()
        {

            ///为数据成员分配内存///
            _layers = new Dictionary<UILayer, UILayerLogic>();
            m_ViewControllers = new Dictionary<UIViewType, UIViewController>(); //注意UIType的成员就是根据当前有哪些存在的可用的UI视图来确定的，命名为UIType确实不够准确
            m_OpenViews = new HashSet<UIViewType>();
            _residentViews = new HashSet<UIViewType>();
            Event = new EventController<UIEvent>();


            ///准备好存放UI元素的根对象，以及UI相机///
            //获取主摄像机（Tag为MainCamera）
            _worldCamera = Camera.main;
            //异或，为1时就为0，为0时就为1，所以主相机的cullingMask其他位会保持不变，而UI位会置零即一定会剔除UI层级对象，因为会有一个专门的UI相机来渲染UI元素。
            //所以一定
            _worldCamera.cullingMask &= int.MaxValue ^ (1 << Layer.UI);

            //UIRoot对象用于存放UI元素。UICamera专门用于渲染UI对象
            var root = GameObject.Find("UIRoot");
            if (root == null)
            {
                root = new GameObject("UIRoot");
            }
            root.layer = Layer.UI; //设置对象层级
            GameObject.DontDestroyOnLoad(root);
            m_Root = root.transform;

            var camera = GameObject.Find("UICamera");
            if (camera == null)
            {
                camera = new GameObject("UICamera");
            }
            m_UICamera = camera.GetOrAddComponent<Camera>(); //常见的扩展方法，没有就添加，并且返回添加的组件引用
            m_UICamera.cullingMask = 1 << Layer.UI; //只渲染UI层级对象
            m_UICamera.transform.SetParent(m_Root); //作为UIRoot子对象
            /*TODO：投影其实不一定，还是看设计，可能透视投影会更好，因为不需要透视效果的UI只要正对相机就行了，需要透视的就调整角度。*/
            m_UICamera.orthographic = true; //正交投影
            m_UICamera.clearFlags = CameraClearFlags.Depth; //只清除深度缓冲区，会保留之前的颜色。
            //Tip:在URP下，Unity相机被扩展了，需要进行以下处理，才能让UI相机的渲染内容附加在主相机上，而不是完全覆盖掉主相机的内容
            // var cameraData = _uiCamera.GetUniversalAdditionalCameraData();
            // cameraData.renderType = CameraRenderType.Overlay;
            // var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
            // mainCameraData.cameraStack.Add(_uiCamera);
            // _uiCamera.
            UpdateCameraStack();

            EventSystem = EventSystem.current; //UGUI的EventSystem

            ///生成各个UILayer的游戏对象，以及两个渐变遮罩///
            var layers = Enum.GetValues(typeof(UILayer)); //获取枚举类型的成员数组
            foreach (UILayer layer in layers)
            {
                bool is3d = layer == UILayer.SceneLayer; //SceneLayer用于显示3DUI，所以Canvas会使用World Space模式，其他均使用Screen Space模式。
                //这里意思是3DUI就用世界相机，否则就用UI相机。但是在上面世界相机不是剔除掉了UI层级吗？看了该方法后发现，如果是3D的话就会把Canvas层级设置为Default，否则就是UI。
                /*Tip：这里还有个细节，就是3DUI就应该使用透视相机来渲染，虽然正交相机也可以渲染，但是会出现很多奇怪的现象，总之就是用正交相机来渲染2DUI，使用透视相机来渲染3DUI。*/
                //这里是创建各个UILayer的游戏对象，而在框架设定上，每个UILayer都是一个Canvas，以及每个UI视图也都是一个Canvas，其实本质上是为了使用Canvas来控制渲染顺序，但可能还有通过分层来优化性能的考虑。
                Canvas layerCanvas = UIExtension.CreateLayerCanvas(layer, is3d, m_Root, is3d ? _worldCamera : m_UICamera, width, height);
                //有了Canvas之后就可以创建逻辑类了，将UILayer使用UILayerLogic封装起来，以便执行特殊操作。
                UILayerLogic uILayerLogic = new UILayerLogic(layer, layerCanvas); 
                _layers.Add(layer, uILayerLogic);
            }
            //创建渐变遮罩，实质上是在Canvas下创建一个Image对象，设置为黑色，初始alpha为0，挂载上CanvasGroup组件并且返回，后续就是通过设置CanvasGroup的alpha来调整整个图片的不透明度，从而实现渐变效果。
            //由于设置了渲染顺序，所以遮罩对象的层级关系并不强制。
            _blackMask = UIExtension.CreateBlackMask(_layers[UILayer.BlackMaskLayer].canvas.transform);
            _backgroundMask = UIExtension.CreateBlackMask(_layers[UILayer.BackgroundLayer].canvas.transform);
        }

        public void UpdateCameraStack()
        {
            //Tip:在URP下，Unity相机被扩展了，需要进行以下处理，才能让UI相机的渲染内容附加在主相机上，而不是完全覆盖掉主相机的内容
            var cameraData = m_UICamera.GetUniversalAdditionalCameraData();
            cameraData.renderType = CameraRenderType.Overlay;
            var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (!mainCameraData.cameraStack.Exists((cam) => cam == m_UICamera))
                mainCameraData.cameraStack.Add(m_UICamera);
        }

        private void Update()
        {
            // TODO：不应该Update设置应该放在屏幕状态变动事件里
            ChangeOrCreateBlack();
        }

        /// <summary>
        /// 创建或者调整黑边，需间隔触发，由于有些设备屏幕是可以转动，是动态的
        /// </summary>
        private void ChangeOrCreateBlack()
        {
            if (_layers == null) return;
            var parent = _layers[UILayer.BackgroundLayer].canvas.transform as RectTransform;
            var uIBlackType = GetUIBlackType();
            switch (uIBlackType)
            {
                case UIBlackType.Height:
                    // 高度适配时的左右黑边
                    var rect = _blacks[0];
                    if (rect == null)
                    {
                        _blacks[0] = rect = UIExtension.CreateBlackMask(parent, 1, "right").transform as RectTransform;
                    }
                    else if (Mathf.Abs(rect.anchoredPosition.x * 2 + parent.rect.width - width) < 1)
                    {
                        return;
                    }
                    rect.pivot = new Vector2(0, 0.5f);
                    rect.anchorMin = new Vector2(1, 0);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.sizeDelta = new Vector2(Mathf.Abs(width - parent.rect.width), 0);
                    rect.anchoredPosition = new Vector2((width - parent.rect.width) / 2, 0);

                    rect = _blacks[1];
                    if (rect == null)
                    {
                        _blacks[1] = rect = UIExtension.CreateBlackMask(parent, 1, "left").transform as RectTransform;
                    }
                    rect.pivot = new Vector2(1, 0.5f);
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 1);
                    rect.sizeDelta = new Vector2(Mathf.Abs(width - parent.rect.width), 0);
                    rect.anchoredPosition = new Vector2(-(width - parent.rect.width) / 2, 0);
                    break;
                case UIBlackType.Width:
                    // 宽度适配时的上下黑边
                    rect = _blacks[0];
                    if (rect == null)
                    {
                        _blacks[0] = rect = UIExtension.CreateBlackMask(parent, 1, "top").transform as RectTransform;
                    }
                    else if (Mathf.Abs(rect.anchoredPosition.y * 2 + parent.rect.height - height) < 1)
                    {
                        return;
                    }
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.sizeDelta = new Vector2(0, Mathf.Abs(height - parent.rect.height));
                    rect.anchoredPosition = new Vector2(0, (height - parent.rect.height) / 2);

                    rect = _blacks[1];
                    if (rect == null)
                    {
                        _blacks[1] = rect = UIExtension.CreateBlackMask(parent, 1, "bottom").transform as RectTransform;
                    }
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.sizeDelta = new Vector2(0, Mathf.Abs(height - parent.rect.height));
                    rect.anchoredPosition = new Vector2(0, -(height - parent.rect.height) / 2);
                    break;
                default:
                    break;
            }
        }

        public UIBlackType GetUIBlackType()
        {
            var uIBlackType = uiBlackType;
            if (uIBlackType == UIBlackType.AutoBlack)
            {
                var parent = _layers[UILayer.BackgroundLayer].canvas.transform as RectTransform;
                float widthDis = Mathf.Abs(width - parent.rect.width);
                float heightDis = Mathf.Abs(height - parent.rect.height);

                if (widthDis < 1 && heightDis < 1) //都小于1说明很接近，这里用1大概是因为作为尺寸的宽度和高度往往都是整数值，其实也应该是，并且以像素为单位，这样就相当于差距不到一个像素，也就认为完全一致了。
                    uIBlackType = UIBlackType.None;
                else if (widthDis > heightDis)
                    uIBlackType = UIBlackType.Height;
                else
                    uIBlackType = UIBlackType.Width;
            }
            return uIBlackType;
        }

        public Rect GetSafeArea()
        {
            Rect rect = Screen.safeArea;
            if (uiBlackType == UIBlackType.Width)
            {
                var parent = _layers[UILayer.BackgroundLayer].canvas.transform as RectTransform;
                float blackArea = Mathf.Abs(height - parent.rect.height) / 2;
                rect.yMin = Mathf.Max(0, rect.yMin - blackArea);
                rect.yMax = Mathf.Min(rect.yMax + blackArea, Screen.height);
            }
            else if (uiBlackType == UIBlackType.Height)
            {
                var parent = _layers[UILayer.BackgroundLayer].canvas.transform as RectTransform;
                float blackArea = Mathf.Abs(width - parent.rect.width) / 2;
                rect.xMin = Mathf.Max(0, rect.xMin - blackArea);
                rect.xMax = Mathf.Min(rect.xMax + blackArea, Screen.width);
            }
            return rect;
        }

        public void EnableBackgroundMask(bool enable)
        {
            _backgroundMask.alpha = enable ? 1 : 0;
        }

        /// <summary>
        /// 初始化UI配置，实质是加载UIConfig文件，转换为具体实例对象，然后填充_viewControllers字典。
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle InitUIConfig()
        {
            if (hasInitConfig)
            {
                Debug.Log("已经加载过UI配置");
                // var op = new AsyncOperationHandle();
                // op.Completed
                //Tip：返回表示完成的句柄。这样的话可以沿用处理异步操作的逻辑，而不需要对这种已经加载，不需要再次加载的情况专门编写额外逻辑
                return Addressables.ResourceManager.CreateCompletedOperation<Sprite>(null, null);
            }
            hasInitConfig = true;
            Debug.Log("InitUIConfig");
            // 初始化需要加载所有UI的配置
            // 本质是
            return UIConfig.GetAllConfigs((list) => //传入的回调就是在加载完成之后，可以使用加载得到的配置内容来执行一些逻辑
            {
                foreach (var cfg in list) //遍历UIConfig列表
                {
                    if (m_ViewControllers.ContainsKey(cfg.uiViewType))
                    {
                        Debug.LogErrorFormat("存在相同的uiType:{0}， 请检查UIConfig是否重复！", cfg.uiViewType.ToString());
                        continue;
                    }
                    //填充字典，就是为了复用UIViewController实例以及方便且快速地通过UIType来获取到对应的UIViewController实例。
                    m_ViewControllers.Add(cfg.uiViewType, new UIViewController
                    {
                        uiPath = cfg.path, //预制体路径
                        uiViewType = cfg.uiViewType,
                        uiLayer = _layers[cfg.uiLayer],
                        uiViewLogic = cfg.viewLogicType, 
                        isPopWindow = cfg.isWindow,
                    });
                }
            });
        }

        /// <summary>
        /// 注册常驻UI
        /// </summary>
        public void AddResidentUI(UIViewType type)
        {
            _residentViews.Add(type);
        }

        /// <summary>
        /// 打开指定的UI视图（这才是外部对象打开UI视图的直接通道）
        /// 似乎打开和关闭都应该按序通过UIManager以及UIViewController处理。
        /// </summary>
        public void Open(UIViewType type, object userData = null, Action callback = null)
        {
            if (!m_ViewControllers.ContainsKey(type))
            {//这里打印未配置是因为从流程上讲，_viewControllers在InitUIConfig方法中就已经通过读取配置文件而填充完毕，也就代表这就是之后的所有存在的能用的UI视图对象了。
                Debug.LogErrorFormat("未配置uiType:{0}， 请检查UIConfig.cs！", type.ToString());
                return;
            }

            //记录存在性，
            /*TODO：记录UIViewType，按理来说，同一个UI视图可以同时出现多个，因为能够复用，这里显然需要改变。*/
            m_OpenViews.Add(type); 
            m_ViewControllers[type].Open(userData, callback);
        }

        /// <summary>
        /// 预加载。实质上是加载指定的UI视图预制体，由于该方法在初始化时（比如Start方法）调用，所以就具有了预加载的效果
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle Preload(UIViewType type)
        {
            Debug.Log($"Preload : {type}");
            //只需要传入UIType，通过加载配置文件填充的_viewControllers可以得到对应的UIViewController，在其中就含有相关数据（比如预制体路径）以及操作方法。
            if (!m_ViewControllers.TryGetValue(type, out var controller))
            {
                Debug.LogErrorFormat("未配置uiType:{0}， 请检查UIConfig.cs！", type.ToString());
                return default;
            }
            return controller.Load();
        }

        /// <summary>
        /// 预加载所有UI视图预制体。
        /// </summary>
        public void PreloadAll()
        {
            foreach (var controller in m_ViewControllers.Values)
            {
                ResourceManager.Instance.LoadAssetAsync<GameObject>(controller.uiPath, null);
            }
        }

        public bool IsOpen(UIViewType type)
        {
            return m_OpenViews.Contains(type);
        }

        public void Close(UIViewType type, Action callback = null)
        {
            if (!m_ViewControllers.ContainsKey(type))
            {
                Debug.LogErrorFormat("未配置uiType:{0}， 请检查UIConfig.cs！", type.ToString());
                return;
            }

            m_OpenViews.Remove(type);
            m_ViewControllers[type].Close(callback);
        }

        public UnityEngine.InputSystem.PlayerInput GetPlayerInput()
        {
            if (playerInput == null)
            {
                Scene scene = SceneManager.GetActiveScene();
                //获取PlayerInput，这里是通过游戏对象名来查找的，需要注意。
                playerInput = Array.Find<GameObject>(scene.GetRootGameObjects(), (element) => element.name == "Player").GetComponent<UnityEngine.InputSystem.PlayerInput>();
            }

            return playerInput;
        }

        /// <summary>
        /// UI建议都用事件进行交互，最好不使用该接口
        /// T是UIView派生类即UI视图的逻辑组件，为了指定类型转换的目标类型。而参数是UIType即UI视图的标识符。
        /// </summary>
        public T GetView<T>(UIViewType type) where T : UIView
        {
            if (!m_ViewControllers.ContainsKey(type))
            {
                Debug.LogErrorFormat("未配置uiType:{0}， 请检查UIConfig.cs！", type.ToString());
                return null;
            }

            return m_ViewControllers[type].uiView as T;
        }

        public void CloseAll(UIViewType ignoreType = UIViewType.Max, bool closeResidentView = false)
        {
            var list = ListPool<UIViewType>.Get();

            foreach (var uiType in m_OpenViews)
            {
                if (ignoreType == uiType) continue;

                if (closeResidentView || !_residentViews.Contains(uiType))
                {
                    m_ViewControllers[uiType].Close();
                    list.Add(uiType);
                }
            }
            foreach (var uiType in list)
            {
                m_OpenViews.Remove(uiType);
            }
            ListPool<UIViewType>.Release(list);
        }

        public void ReleaseAll()
        {
            foreach (var controller in m_ViewControllers.Values)
            {
                if (!_residentViews.Contains(controller.uiViewType))
                {
                    m_OpenViews.Remove(controller.uiViewType);
                    controller.Release();
                }
            }
        }

        /// <summary>
        /// 渐入（变得透明，通常在加载开始时调用），可以传入变化时间以及结束事件
        /// </summary>
        public void FadeIn(float duration = 0.5f, TweenCallback callback = null)
        {
            //注意这里的打断，非常重要，对于有时加载过快，时间过短的时候，可以避免很多视觉上的bug。
            if (_fadeTweener != null && _fadeTweener.IsPlaying())
                _fadeTweener.Complete();
            
            _fadeTweener = _blackMask.DOFade(1.0f, duration);
            _fadeTweener.onComplete = callback;
        }

        /// <summary>
        /// 渐出（变得不透明，通常在加载结束时调用），可以传入变化时间以及结束事件
        /// </summary>
        public void FadeOut(float duration = 0.5f, TweenCallback callback = null)
        {
            if (_fadeTweener != null && _fadeTweener.IsPlaying())
                _fadeTweener.Complete();
            _fadeTweener = _blackMask.DOFade(0.0f, duration);
            _fadeTweener.onComplete = callback;
        }

        public void FadeOutIn(float duration = 1.0f, TweenCallback callbackBlack = null, TweenCallback callback = null)
        {
            if (_fadeTweener != null && _fadeTweener.IsPlaying())
                _fadeTweener.Complete();
            _fadeTweener = _blackMask.DOFade(1.0f, duration * 0.5f);
            _fadeTweener.onComplete += () =>
            {
                callbackBlack?.Invoke(); //在渐出结束即进入到纯黑时就要调用callbackBlack回调，这一步很关键，callback会由DOTween自己调用，在渐入完成即完全透明时
                _fadeTweener = _blackMask.DOFade(0.0f, duration * 0.5f);
                _fadeTweener.onComplete = callback;
            };
        }

        // public void Cancel()
        // {
        //     if (_layers.TryGetValue(UILayer.NormalLayer, out var layer) && layer.openedViews.Count > 0)
        //     {
        //         var viewController = layer.openedViews.Peek();
        //         if (viewController.uiView != null)
        //         {
        //             viewController.uiView.OnCancel();
        //         }
        //     }
        // }
    }
}
