using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MyPlugins.GoodUI
{
    //UI视图控制器，将UI视图数据和UI视图行为集中在一个该类中，使得定义的方法可以在内部对操作的数据对象即UI视图直接进行处理，并且供外部调用，所以该类本身也属于被操作对象。
    //通过UIManager中的字典成员_viewControllers来实现UIType和UIViewController实例映射关系的记录，以便快捷获取对应视图的控制器而对视图执行某些行为。
    public class UIViewController
    {
        // 配置
        public UIViewType uiType; //应该把该成员视作UIViewController的核心，标识了该控制器的独立性，而下面的UIView本质上是挂载在整个UI视图对象的根对象上的逻辑组件，在命名上确实有些迷惑
        public string uiPath; //UI视图预制体路径
        public Type uiViewType;
        public UILayerLogic uiLayer; //就是该UI视图对象所在的UI层级，用UILayerLogic而不是UILayer，就像用UIViewController而不是UIView，都是将数据和行为封装起来的对象，便于直接调用提供的方法通知对其内部数据进行操作
        public bool isPopWindow;

        public UIView uiView; //在实际加载预制体时添加该组件，并且在此引用。所以通过该组件的存在性就可以获取其UI视图对象的存在性。
        // public UIViewAnim uiViewAnim;
        //UI视图所处状态
        public bool isLoading = false;
        public bool isOpen = false;
        public bool isPause = false; //暂停就类似于"被覆盖"
        public int order; //默认为0
        /// <summary>
        /// 在该UI视图上面的界面(非窗口界面，也就是全屏界面，会直接完全遮盖背后UI视图的界面)的数量（每个UI视图都会记录该值）
        /// </summary>
        public int topViewNum;

        /// <summary>
        /// 加载UI视图对象，以及初始化。
        /// </summary>
        /// <param name="userData">额外的数据</param>
        public AsyncOperationHandle Load(object userData = null, Action callback = null)
        {
            isLoading = true;

            if (isOpen)
            {
                order = uiLayer.PopOrder(this);
            }
            /*关于InstantiateAsync方法，就是传入预制体路径，然后异步加载，还利用了对象池和缓存的技巧来节省性能*/
            return ResourceManager.Instance.InstantiateAsync(uiPath, (go) => //实例化UI视图对应的预制体，go就是加载完成即预制体的实例化游戏对象
            {
                if (!isLoading)
                {
                    ResourceManager.Instance.Recycle(go);
                    callback?.Invoke();
                    Release();
                    return;
                }

                isLoading = false; //该方法就是在加载结束后调用，所以在此设置标记
                //添加用于控制UI视图的逻辑组件。这个非常关键，按理来说每个UI视图都应该有一个对应的逻辑组件挂载在其根对象上，起到管理和协调的作用。
                uiView = (UIView)go.GetOrAddComponent(uiViewType);
                // uiViewAnim = go.GetComponent<UIViewAnim>();
                //从这里可以看到运行时实际处理的UI就是一个个直接位于层级画布下作为直接子对象的游戏对象，至于更深层级，都是在开发时确定好、制作成预制体的。
                uiView.transform.SetParentEx(uiLayer.canvas.transform); //本质上和go.transform相同
                RectTransform rectTransform = uiView.transform as RectTransform;

                switch (UIManager.Instance.GetUIBlackType())
                {
                    case UIBlackType.None:
                        // 全适配
                        rectTransform.SetAnchor(AnchorPresets.StretchAll);
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.sizeDelta = Vector2.zero;
                        break;
                    case UIBlackType.Height:
                        // 保持高度填满，两边留黑边
                        rectTransform.SetAnchor(AnchorPresets.VertStretchCenter);
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.sizeDelta = new Vector2(UIManager.Instance.width, 0);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, UIManager.Instance.width);
                        break;
                    case UIBlackType.Width:
                        // 保持宽度填满，上下留黑边
                        rectTransform.SetAnchor(AnchorPresets.HorStretchMiddle);
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIManager.Instance.height);
                        rectTransform.sizeDelta = new Vector2(0, UIManager.Instance.height);
                        break;
                }

                //这是使用的UIContorlData插件
                uiView.OnInit(go.GetComponent<UIControlData>(), this);
                uiView.transform.SetAsLastSibling(); //设置为同层最后位置的对象，没有什么特殊含义，

                //如果通过Open方法调用的话，那么会标记isOpen为true，这样在加载完成之后就会进入该分支调用Open方法了
                if (isOpen)
                {
                    Open(userData, callback, true); //加载后打开，当然是首次打开。
                }
                else
                {
                    Close(callback);
                }
            });
        }

        //将打开动画和关闭动画分别放在Open和Close方法中。
        public void Open(object userData = null, Action callback = null, bool isFirstOpen = false)
        {
            isOpen = true; //标记已经打开
            if (isLoading) return; //避免执行重复逻辑，可能主要都不是性能方面，而是可能产生难以预料的意外结果

            /*Tip：看了Load方法的实现之后，才懂了为何会有Open和TrueOpen的分别，因为Open方法中会判断此时是否存在所要打开的UI视图对象，如果不存在的话就要先加载即此处的Load方法，然后
            在加载完成后的回调方法中会再次调用该Open方法，此时就会进入else分支，即执行TrueOpen方法以及*/
            if (uiView == null)
            {
                Load(userData, callback);
            }
            else
            {
                if (!isFirstOpen && isOpen && order > 0)
                {
                    TrueClose(); //应该是走个流程，避免遗漏必要逻辑
                }
                TrueOpen(userData, callback);
                uiView.OnOpenAnim();
                // if (uiViewAnim != null)
                // {
                //     uiViewAnim.Open();
                // }
            }
        }

        public void Close(Action callback = null)
        {
            isOpen = false;
            if (isLoading) return;

            uiView.OnCloseAnim(() => TrueClose(callback));

            // if (uiView != null)
            // {
            //     if (uiViewAnim != null)
            //     {
            //         uiViewAnim.Close(() => { TrueClose(callback); }); //结束时回调
            //     }
            //     else
            //     {
            //         TrueClose(callback);
            //     }
            // }
        }

        /// <summary>
        /// 销毁UIView实例
        /// </summary>
        public void Release()
        {
            if (uiView != null)
            {
                if (isOpen)
                    TrueClose();
                uiView.OnRelease();
                GameObject.Destroy(uiView.gameObject); //销毁UI视图对象
            }
            uiView = null;
            // uiViewAnim = null;
            isLoading = false;
            isOpen = false;
            order = 0;
        }

        /// <summary>
        /// 确定场景中已经存在了UI视图对象，然后执行实际的打开UI视图对象的逻辑
        /// </summary>
        /// <remarks></remarks>
        private void TrueOpen(object userData = null, Action callback = null)
        {
            uiLayer.OpenUI(this); //通知所在层级。为何要通知？因为有联系，当前操作会发生牵涉，所以通知其进行相应的变化。
            SetVisible(true); //很朴素,直接设置为true
            // 刷新一下显示
            AddTopViewNum(0);
            uiView.OnOpen(userData);
            uiView.OnResume();
            callback?.Invoke();
        }

        private void TrueClose(Action callback = null)
        {
            uiLayer.CloseUI(this);
            // 刷新一下显示
            AddTopViewNum(-100000); //表示其为负值范围。
            SetVisible(false); //Tip：直接使用SetActive来管理UI视图的开关，其实非常不适于制作开关时的动画。
            uiView.OnPause();
            uiView.OnClose();
            callback?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            if (uiView != null)
            {//TODO：这里是通过直接SetActive进行隐藏和显示的，但是我看到有关UGUI优化的文章说的是，SetActive切换状态其实是一个很大的消耗，应该采用其他方式比如scale设置为0、alpha设置为0之类的，来实现显示和隐藏
                uiView.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 记录在该UI视图上面的视图数量，并且适时调整可见性
        /// </summary>
        /// <param name="num"></param>
        public void AddTopViewNum(int num)
        {
            topViewNum += num;
            topViewNum = Mathf.Max(0, topViewNum); //最小为0
            //TODO：不太理解，这里没有其他UI对象在上面时就SetActive为true，反之则为false，但是一般来说UI叠层是不会直接隐藏背后元素的，只是会加上半透明的蒙版，以便突出当前最表面的UI对象
            SetVisible(topViewNum <= 0);
        }
    }

}
