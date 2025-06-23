using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SkierFramework
{
    /// <summary>
    /// 这是所有UI视图的逻辑组件的基类，
    /// </summary>
    public class UIView : MonoBehaviour, IBindableUI //IBindableUI这种空接口，可以提供的信息是存在性以及数量，即有没有这种类型的对象以及有多少。
    {
        private UIViewController _controller;
        private GameObject _lastSelect;
        private Canvas _canvas;
        //在OnResume即恢复时会尝试将该变量引用的游戏对象设置为选中对象，如果有的话。比如在当前UI视图中有什么按钮即Selectable类型的组件，那就可以被该变量所引用
        public GameObject DefaultSelect; 

        public UIViewController Controller => _controller;

        /// <summary>
        /// UI视图对象的初始化方法，在实例化预制体之后，添加对应的UIView组件，然后就会调用该方法进行初始化
        /// </summary>
        /// <param name="uIControlData"></param>
        /// <param name="controller"></param>
        public virtual void OnInit(UIControlData uIControlData, UIViewController controller)
        {
            if (uIControlData != null)
            {
                uIControlData.BindDataTo(this); //绑定控件
            }
            _controller = controller;

            //显然会将每个UI视图本身的根对象视为一个Canvas画布，这样在同一UI层下的不同UI视图就可以通过Canvas组件来设置渲染层级和渲染顺序了。
            _canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<CanvasScaler>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 事件监听（打开时注册）
        /// </summary>
        public virtual void OnAddListener() { }

        /// <summary>
        /// 事件移除（关闭时注销）
        /// </summary>
        public virtual void OnRemoveListener() { }

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
        }

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
        /// 被覆盖
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
        }

        /// <summary>
        /// 取消按钮响应
        /// </summary>
        public virtual void OnCancel()
        {
            UIManager.Instance.Close(_controller.uiType);
        }

        /// <summary>
        /// 被卸载释放
        /// </summary>
        public virtual void OnRelease() { }
    }
}
