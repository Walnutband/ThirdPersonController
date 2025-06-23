using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkierFramework
{
    public enum UILayer
    {//这里的数值会直接作为Canvas的sortingOrder渲染顺序（在检视面板中是Order In Layer）。这个系统中的各个Canvas的Sorting Layer都是Default，就是根据Order In Layer来设置渲染顺序的。
        SceneLayer = 1000, //用于显示3DUI，画布使用World Space模式
        BackgroundLayer = 2000, //用于显示UI背景，如主界面，或者黑边图等。
        NormalLayer = 3000, //游戏中没有特殊层级要求的UI都放这层
        InfoLayer = 4000, //游戏中一些需要显示在普通界面上面的信息
        TopLayer = 5000, //顶层，显示Loading等
        TipLayer = 6000, //提示层，看情况设计，有时Loading中也需要弹出提示框等信息
        BlackMaskLayer = 7000,
    }

    public class UILayerLogic
    {
        public UILayer layer; //对应层级，只是一个标识符，不过其int值也提供了顺序值的信息。
        public Canvas canvas; //所属幕布
        private int maxOrder; // 最大排序号。排序号就是渲染顺序，这里就是当前层级中最大的
        private HashSet<int> orders; // 已分配的排序号
        //存储在这个UI层中打开的UI视图对象。对于UI来说，都是用栈来存储，因为其后进先出的特性，正好对应从里到外打开、从外到里关闭。这其实是数据结构应用的一个典型体现。
        public Stack<UIViewController> openedViews; // 所有已打开的UI，栈的最上面就是当前显示在最上层的UI，所以在方法调用上必须为了实现该功能而保持与实际情况同步。

        /// <summary>
        /// 从构造函数看出核心成员就是layer和canvas，其他就是初始化分配实例内存
        /// </summary>
        public UILayerLogic(UILayer layer, Canvas canvas)
        {
            this.layer = layer;
            this.canvas = canvas;
            maxOrder = (int)layer; //将枚举常量值即层级的预设order直接作为最大order，所以在UILayer中就要注意调整数值
            orders = new HashSet<int>();
            openedViews = new Stack<UIViewController>();
        }

        /// <summary>
        /// 关闭指定的UI视图对象，本质上是处理一些引用关系，并非销毁内存对象，UI视图本身始终被UIManager的_viewControllers字典所引用。
        /// </summary>
        /// <remarks>可见，实际在使用中，比如此处的参数传递，都是封装好的UIViewController类型，而不是原本的UIType或者UIView</remarks>
        public void CloseUI(UIViewController closedUI)
        {
            int order = closedUI.order;
            PushOrder(closedUI); //处理order以及移出openedViews容器
            closedUI.order = 0;

            //善后工作，如果还有打开的UI视图的话，移除UI视图会对各自产生的一些影响。
            if (openedViews.Count > 0)
            {
                // 拿到最上层UI，如果被暂停的话，则恢复，
                var topViewController = openedViews.Peek();
                // 暂停和恢复不影响其是否被覆盖隐藏，只要不是最上层UI都应该标记暂停状态
                if (topViewController != null && topViewController.isPause)
                {
                    topViewController.isPause = false;
                    if (topViewController.uiView != null)
                    {
                        topViewController.uiView.OnResume();
                    }
                }

                if (!closedUI.isWindow) //isWindow应该指的是否为弹出窗口，弹出窗口就只是有一个蒙版遮住背后UI，但并不会导致其完全不可见，蒙版会随着自己的消失而消失，而全屏窗口就会直接导致背后的UI视图不可见，所以就需要在关闭自己时通知被遮盖的UI视图重新显示。
                {
                    foreach (var viewController in openedViews)
                    {
                        if (viewController != closedUI
                            && viewController.isOpen
                            && viewController.order < order) //顺序更小就说明被遮挡了，现在将其关闭了当然就减去该遮挡物。
                        {
                            viewController.AddTopViewNum(-1);
                        }
                    }
                }
            }
        }

        public void OpenUI(UIViewController openedUI)
        {
            //UIViewController在创建实例时（UIManager的InitUIConfig方法）没有指定order值，那么就是默认int值为0，order确实应该在打开该UI视图时才确定。
            if (openedUI.order == 0)
            {
                openedUI.order = PopOrder(openedUI); //分配Order。
            }

            foreach (var viewController in openedViews)
            {
                if (viewController != openedUI
                    && viewController.isOpen
                    && viewController.order < openedUI.order
                    && viewController.uiView != null)
                {
                    if (!viewController.isPause)
                    {
                        viewController.isPause = true;
                        /*TODO:这里感觉UILayerLogic越级了,不应该直接访问UIView然后调用其方法,大概应该在UIViewController中封装好对应UIView的OnPause方法的调用,然后这里就是
                        直接调用UIViewController的封装方法,来执行暂停的逻辑,就是为了保证UILayerLogic只会通知UIViewController执行任务，而不是越级通知UIView执行任务，因为
                        UIViewController本来就定义了引用UIVew的成员，这就应该视为其内部任务，不应该被外部对象即此处的UILayerLogic插手。*/
                        viewController.uiView.OnPause();  
                    }
                    if (!openedUI.isWindow)
                    {
                        viewController.AddTopViewNum(1);
                    }
                }
            }
        }

        /*TODO:这里命名有些迷惑，实质上做的事就是，传入将要关闭的UI视图，更新相关数据，即orders、maxOrder、openedViews。就是说它的命名与其实际的任务不太一致，而且与CloseUI方法有所歧义。*/
        public void PushOrder(UIViewController closedUI)
        {
            int order = closedUI.order;
            if (orders.Remove(order)) //如果HashSet中有的话，就移除并且返回true。
            {
                // 重新计算最大值
                maxOrder = (int)layer;
                foreach (var item in orders) //遍历集合，更新取最大。
                {
                    maxOrder = Mathf.Max(maxOrder, item);
                }

                // 移除界面，list用于保存在closedUI上面的UI视图，在弹出了closedUI之后再按照正确顺序推入回栈。
                // 因为栈的特性，无法直接遍历查找，不过其实本质也一样。
                // 虽然这里从算法上确实支持了关闭UI层中间的UI，但其实实际应用中应当确定限制只能关闭最顶层UI（不过似乎有时难以完全保证？），按顺序从外到内，或者是一键关闭当前UI层的所有UI视图，基本不会设置可以关闭中间本来就被覆盖了的UI的操作。
                List<UIViewController> list = ListPool<UIViewController>.Get();
                while (openedViews.Count > 0)
                {
                    var view = openedViews.Pop();
                    if (view != closedUI)
                    {
                        list.Add(view);
                    }
                    else //直到弹出了closedUI就退出循环。
                    {
                        break;
                    }
                }
                //因为栈特性，所以倒过来遍历才是正确顺序还原。
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    openedViews.Push(list[i]);
                }
                ListPool<UIViewController>.Release(list);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uIViewController"></param>
        /// <returns></returns>
        public int PopOrder(UIViewController uIViewController)
        {
            maxOrder += 10;
            orders.Add(maxOrder);
            openedViews.Push(uIViewController);
            return maxOrder;
        }
    }

}
