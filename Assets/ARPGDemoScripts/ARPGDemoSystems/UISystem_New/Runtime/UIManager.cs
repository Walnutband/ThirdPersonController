using System;
using System.Collections.Generic;
using ARPGDemo.CustomAttributes;
using ARPGDemo.UISystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace ARPGDemo.UISystem_New
{
    [AddComponentMenu("ARPGDemo/UISystem_New/UIManager")]
    public class UIManager : SingletonMono<UIManager>
    {
        public int width = 1920;
        public int height = 1080;
        private Camera m_WorldCamera;
        private Camera m_UICamera;
        private Transform m_Root;
        private EventSystem m_EventSystem;

        // private Dictionary<UILayerType, UILayerController> m_Layers = new Dictionary<UILayerType, UILayerController>();
        // private Dictionary<UIPanelType, UILayerType> m_PanelLayers = new Dictionary<UIPanelType, UILayerType>();

        private Dictionary<UIPanelType, List<UIPanelController>> m_CachedPanels = new Dictionary<UIPanelType, List<UIPanelController>>();
        private Stack<UIPanelController> m_OpenedPanels = new Stack<UIPanelController>();

        private CanvasGroup m_BlackMask;
        //Tip：保存黑色遮罩所使用的Tweener。
        private Tweener m_FadeTweener; 

        //TODO：暂时就直接引用SO。更好的做法还是定义路径来加载，而且将SO序列化为Json文件。
        [DisplayName("UI配置文件")]
        public UIConfig m_UIConfig;

        protected override void RetrieveExistingInstance()
        {
            m_Instance = GameObject.Find("UIManager").GetComponent<UIManager>();
        }

        protected override void Awake()
        {
            //Tip：准备好存放UI元素的根对象，以及UI相机
            //获取主摄像机（Tag为MainCamera）
            m_WorldCamera = Camera.main;
            //异或得到所以主相机的cullingMask其他位会保持不变，而UI位会置零即一定会剔除UI层级对象，因为会有一个专门的UI相机来渲染UI元素。
            m_WorldCamera.cullingMask &= int.MaxValue ^ (1 << Layer.UI);

            //Tip：两个硬编码，“UIRoot”代表整个UI视图，“UICamera”是专门用于渲染UI（限定UI层级对象）的相机。

            //UIRoot对象用于存放UI元素。UICamera专门用于渲染UI对象。所以UI内容都放到UIRoot下，并且一同放入到DontDestroyOnLoad场景中。
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
            m_UICamera.orthographic = true; //正交投影
            m_UICamera.clearFlags = CameraClearFlags.Depth; //只清除深度缓冲区，会保留之前的颜色。

            UpdateCameraStack();

            m_EventSystem = EventSystem.current; //UGUI的EventSystem

            // TODO：层级暂时搞不懂。。。
            // ///生成各个UILayer的游戏对象，以及两个渐变遮罩
            // var layers = Enum.GetValues(typeof(UILayerType)); //获取枚举类型的成员数组
            // foreach (UILayerType layer in layers)
            // {
            //     bool is3d = layer == UILayerType.Scene; //SceneLayer用于显示3DUI，所以Canvas会使用World Space模式，其他均使用Screen Space模式。
            //     //这里意思是3DUI就用世界相机，否则就用UI相机。但是在上面世界相机不是剔除掉了UI层级吗？看了该方法后发现，如果是3D的话就会把Canvas层级设置为Default，否则就是UI。
            //     /*Tip：这里还有个细节，就是3DUI就应该使用透视相机来渲染，虽然正交相机也可以渲染，但是会出现很多奇怪的现象，总之就是用正交相机来渲染2DUI，使用透视相机来渲染3DUI。*/
            //     //这里是创建各个UILayer的游戏对象，而在框架设定上，每个UILayer都是一个Canvas，以及每个UI视图也都是一个Canvas，其实本质上是为了使用Canvas来控制渲染顺序，但可能还有通过分层来优化性能的考虑。
            //     Canvas layerCanvas = UIExtension.CreateLayerCanvas(layer, is3d, m_Root, is3d ? m_WorldCamera : m_UICamera, width, height);
            //     //有了Canvas之后就可以创建逻辑类了，将UILayer使用UILayerLogic封装起来，以便执行特殊操作。
            //     // UILayerLogic uILayerLogic = new UILayerLogic(layer, layerCanvas);
            //     // _layers.Add(layer, uILayerLogic);
            //     UILayerController controller = new UILayerController(layer, layerCanvas);
            //     m_Layers.Add(layer, controller);
            // }

            m_BlackMask = UIExtension.CreateBlackMask(m_Root);
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

        //初始化配置，把控制器准备好，以便后续使用
        public void InitUIConfig()
        {
            List<UIConfig.PanelInfo> infos = m_UIConfig.panels;
            foreach (var info in infos)
            {
                ProcessPanelInfo(info);
            }
        }

        private void ProcessPanelInfo(UIConfig.PanelInfo _info)
        {
            UIPanelController controller = new UIPanelController()
            {
                panelType = _info.panelType,
                layerType = _info.layerType,
            };

            //UI面板的预制体路径信息记录好。
            UIResourceManager.Instance.AddPrefabPath(_info.panelType, _info.prefabPath);
        }

        public void OpenPanel(UIPanelType _panelType)
        {
            UIPanelController panel;
            if (m_CachedPanels.TryGetValue(_panelType, out var panels) && panels != null && panels.Count > 0)
            {
                panel = panels[0];
            }
            else
            {
                //放到根对象之下。
                Canvas canvas = UIExtension.CreatePanelCanvas(_panelType, m_Root, m_UICamera, width, height);
                //利用多态即可，所以不需要具体类型的信息。
                UIPanelBase panelGO = UIResourceManager.Instance.LoadPrefab(_panelType).GetComponent<UIPanelBase>();
                panelGO.transform.SetParent(canvas.transform, false);
                panel = new UIPanelController() 
                {
                    panelType = _panelType,
                    canvas = canvas,
                    panelBase = panelGO
                };
            }

            panel.Open();
            m_OpenedPanels.Push(panel);
        }

        private void ClosePanel()
        {
            var panel = m_OpenedPanels.Pop();
            if (panel == null) return;
            panel.Close();
        }

        //Tip：控制黑边实现渐入渐出效果的相关方法。

        /// <summary>
        /// 渐入（变得透明，通常在加载开始时调用），可以传入变化时间以及结束事件
        /// </summary>
        public void FadeIn(float duration = 0.5f, TweenCallback callback = null)
        {
            //注意这里的打断，非常重要，对于有时加载过快，时间过短的时候，可以避免很多视觉上的bug。
            if (m_FadeTweener != null && m_FadeTweener.IsPlaying())
                m_FadeTweener.Complete();

            m_FadeTweener = m_BlackMask.DOFade(1.0f, duration);
            m_FadeTweener.onComplete = callback;
        }

        /// <summary>
        /// 渐出（变得不透明，通常在加载结束时调用），可以传入变化时间以及结束事件
        /// </summary>
        public void FadeOut(float duration = 0.5f, TweenCallback callback = null)
        {
            if (m_FadeTweener != null && m_FadeTweener.IsPlaying())
                m_FadeTweener.Complete();
            m_FadeTweener = m_BlackMask.DOFade(0.0f, duration);
            m_FadeTweener.onComplete = callback;
        }

        public void FadeOutIn(float duration = 1.0f, TweenCallback callbackBlack = null, TweenCallback callback = null)
        {
            if (m_FadeTweener != null && m_FadeTweener.IsPlaying())
                m_FadeTweener.Complete();
            m_FadeTweener = m_BlackMask.DOFade(1.0f, duration * 0.5f);
            m_FadeTweener.onComplete += () =>
            {
                callbackBlack?.Invoke(); //在渐出结束即进入到纯黑时就要调用callbackBlack回调，这一步很关键，callback会由DOTween自己调用，在渐入完成即完全透明时
                m_FadeTweener = m_BlackMask.DOFade(0.0f, duration * 0.5f);
                m_FadeTweener.onComplete = callback;
            };
        }

    }


} 