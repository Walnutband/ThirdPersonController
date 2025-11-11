using System;
using System.Collections.Generic;
using DG.Tweening;
using MyPlugins.GoodUI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Old
{
    /// <summary>
    /// 这是所有UI视图的逻辑组件的基类，
    /// </summary>
    public abstract class UIView : MonoBehaviour, IBindableUI //IBindableUI这种空接口，可以提供的信息是存在性以及数量，即有没有这种类型的对象以及有多少。
    {
        private UIViewController _controller;
        private GameObject _lastSelect;
        private Canvas _canvas;
        protected CanvasGroup canvasGroup;
        
        //在OnResume即恢复时会尝试将该变量引用的游戏对象设置为选中对象，如果有的话。比如在当前UI视图中有什么按钮即Selectable类型的组件，那就可以被该变量所引用
        public GameObject DefaultSelect; 

        public UIViewController Controller => _controller;

        /// <summary>
        /// UI视图对象的初始化方法，在实例化预制体之后，会添加对应的UIView组件，并且立刻调用该方法进行初始化
        /// </summary>
        public virtual void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            //Tip:在基类UIView这里的OnInit方法中就已经绑定好组件引用，所以在派生类中可以重写OnInit方法调用base实现，然后补充初始化逻辑，可以放心使用UI组件，不用担心出现空引用。
            if (uIControlData != null)
            {
                //在检视器中从UIControlData组件的菜单项复制声明字段的代码，放到UIView派生类中，然后在此处的绑定就可以将UIControlData组件中记录的引用传递到UIView派生类中声明的对应字段，也就实现了绑定功能
                uIControlData.BindDataTo(this); //绑定控件
            }
            _controller = controller;

            //为每个预制体根对象添加Canvas相关组件。显然会将每个UI视图本身的根对象视为一个Canvas画布，这样在同一UI层下的不同UI视图就可以通过Canvas组件来设置渲染层级和渲染顺序了。
            _canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<CanvasScaler>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>(); //使用CanvasGroup主要方便做一些动效，还有防止异常的连续响应之类的。
        }


        /// <summary>
        /// 事件监听（打开时注册）
        /// </summary>
        protected virtual void OnAddListener() { }

        /// <summary>
        /// 事件移除（关闭时注销）
        /// </summary>
        protected virtual void OnRemoveListener() { }

        /// <summary>
        /// 打开
        /// </summary>
        public virtual void OnOpen(object userData)
        {

            SortOrder();

            //当 overrideSorting = true 时，该 Canvas 将脱离 Sorting Layer 规则，仅依据 sortingOrder 进行排序。
            _canvas.overrideSorting = true;
            //
            _canvas.sortingOrder = _controller.order;

            OnAddListener();

            _lastSelect = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            // OnOpenAnim();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true; 
        }

        //Tip:由于在UIViewController中必然调用OnOpenAnim和OnCloseAnim，并且还要通过OnCloseAnim来调用TrueClose方法关闭视图，所以设置为抽象
        public abstract void OnOpenAnim();

        protected virtual void SortOrder()
        {
            SortOrder(transform, _controller.order + 1);
        }

        /// <summary>
        /// 递归的将所有孩子层级设置正确：一些默认摆在UI上的特效等正确分配层级
        /// </summary>
        protected int SortOrder(Transform target, int order)
        {
            var canvas = target.GetComponent<Canvas>();
            if (canvas != null && canvas != _canvas)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = order++;
                canvas.gameObject.layer = Layer.UI;
            }
            var psr = target.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingOrder = order++;
                psr.gameObject.layer = Layer.UI;
            }
            var sortGroup = target.GetComponent<SortingGroup>();
            if (sortGroup != null)
            {
                sortGroup.sortingOrder = order++;
                sortGroup.gameObject.SetLayerRecursively(Layer.UI);
            }
            var sr = target.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = order++;
                sr.gameObject.layer = Layer.UI;
            }
            for (int i = 0; i < target.childCount; i++)
            {
                order = SortOrder(target.GetChild(i), order);
            }
            return order;
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public virtual void OnResume()
        {
            if (DefaultSelect != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(DefaultSelect);
            }
        }

        /// <summary>
        /// 被覆盖（在TrueClose中调用，或者是）
        /// </summary>
        public virtual void OnPause()
        {
        }

        /// <summary>
        /// 被关闭
        /// </summary>
        public virtual void OnClose()
        {
            OnRemoveListener();

            if (_lastSelect != null && _lastSelect.activeInHierarchy)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_lastSelect);
            }

            // OnCloseAnim();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // public abstract void OnCloseAnim(TweenCallback complete);
        public virtual void OnCloseAnim(TweenCallback complete)
        {
            canvasGroup.interactable = false;
            // canvasGroup.blocksRaycasts = false; //播放结束动画时，不能交互，但是会阻挡射线
        }

        /// <summary>w
        /// 取消按钮响应（就是关闭自己这个视图，返回到上一个视图，而且需要通过UIManager来关闭，因为要走一段流程，不能自己调用OnClose，逻辑不全）
        /// </summary>
        public virtual void OnCancel()
        {
            UIManager.Instance.Close(_controller.uiViewType);
        }

        /// <summary>
        /// 被卸载释放
        /// </summary>
        public virtual void OnRelease() { }
    }
}
