

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

/*底层条件：
从上往下，索引从0到1，content对象的pivot在左上角以及锚点也在左上角，初始情况为content对象的anchoredPosition为0，即上边界与ViewPort的上边界重合，
其实元素的锚点和pivot位置都随意，因为是通过作为父对象的content对象移动来带动作为子对象的各元素进行移动的，content对象的锚点和pivot位置才是需要固定好。
ScrollRect移动类型为Clamped
在滑动时，为了实现像绝区零中那样的同时变化边界元素的alpha和scale，元素本身的尺寸不会改变，即占位尺寸不变，只改变显示尺寸。并且由此也不需要自动布局系统，也就不需要LayoutGroup组件
最关键的底层条件应该是所有元素的尺寸相同，这样的话计算位置、寻找边界元素就很方便了，而且带有吸附功能的滑动区域从逻辑上、从确定性上也应当要求所有元素的尺寸相同。那么在编辑器中
就应该手动把尺寸设置好了，比如把ViewPort的高度设置为元素高度的倍数，当然也可以自定义编辑器类来让程序设置尺寸，只要指定一下相关参数就行了。
*/

//Tip:在水平和竖直的Snapper中尤其要注意的区别是，竖直中默认索引是从上到下、从0开始，而水平中却是从左到右、从0开始，这个逻辑最初是基于竖直情况写的，想要兼容的话，可以使用HorizontalLayoutGroup组件的Reverse Arrangement来让元素反转就行了。

namespace MyPlugins.GoodUI
{

    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("GoodUI/Controls/HorizontalSnapper")]
    public class HorizontalSnapper : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool hasEffect = false;
        public Action<int> toBorder; 

        public float duration = 0.1f;
        public int viewCount = 0;
        public float itemSize = 0f; //水平就是指的宽度
        

        private RectTransform content; //通过移动content位置来移动元素，边界元素在移动时基于偏移量与尺寸的比值来对scale和alpha进行插值
        private RectTransform viewport;
        private ScrollRect scrollRect;
        private RectTransform[] items; //由于运行后元素数量是固定的，所以用数组提高速度
        private CanvasGroup[] itemCanvasGroups;
        private DrivenRectTransformTracker tracker;
        private int dragDir = 0;

        //Tip:之前还真没想到，其实完全可以直接计算出content对象的y坐标的最大值（最小值就是0），然后使用Mathf.Approximately就可以判断此时是否处于边界了。这就是利用底层条件，即元素数量和尺寸始终不变。
        //还是抓住底层条件，就是可以放心使用的那些确定条件。
        private float maxX = 0f;

        private RectTransform upItem, downItem;
        private Sequence snapSequence;


        protected override void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect.content;
            viewport = scrollRect.viewport;
            items = new RectTransform[content.childCount];
            itemCanvasGroups = new CanvasGroup[content.childCount];
        }

        protected override void Start()
        {
            // foreach (RectTransform item in content)
            // items.Add(item); 
            //获取到所有元素的RectTransform
            int childCount = content.childCount; //缓存一下，避免重复访问
            for (int i = 0; i < childCount; i++)
            {
                items[i] = content.GetChild(i) as RectTransform;
                itemCanvasGroups[i] = items[i].GetOrAddComponent<CanvasGroup>();
            }

            //计算可视区域能够包含多少个item，其实最好在检视面板中指定好
            //其实我就怀疑可能会因为浮点误差而导致个数差一，或许应该用Round函数来取整数？此处就使用Round函数
            // viewCount = (int)(scrollRect.viewport.rect.width / itemSize);
            itemSize = items[0].rect.width;
            viewCount = Mathf.RoundToInt(viewport.rect.width / itemSize);

            maxX = (items.Length - viewCount) * itemSize;

            // tracker = new DrivenRectTransformTracker();
            // tracker.Add(this, content,)

            CheckToBorder();

        }



        /*之前的Snap方法，可以处理元素尺寸不同的情况，但是严格来说是吸附于上边界。不过从商业使用来看，根本不会出现元素的尺寸不同的情况，所以就弃用了，其实代码还更好写了
        /// <summary>
        /// 实现吸附功能的方法，通常在结束拖拽时调用。
        /// 不过由于可以提供边界元素信息，所以可以在滑动之前的某些地方调用，以便滑动时可以准确应用动效
        /// </summary>
        /// <remarks>利用虚实，实际情况下不存在吸附，就是正常的上下连续滑动改变位置，而虚在于以Content初始位置（上边界重合）时的数据作为参考，想象其根据边界元素的高度进行上下位移，使用当前实际
        /// 的Content的位置与虚中准确按照边界元素高度移动的位置作比较，就能确定此时位于边界的对应元素的索引了。
        /// 注意索引是从上到下的，但是这里的想象是从下到上的，不过也可以认为是从上到下的，因为初始是上边界重合，而最终是下边界重合，总之怎么想得通就怎么想</remarks>
        private void Snap(int dir) //为正就向上吸附，为负就向下吸附
        {
            int itemCount = items.Length; //求出元素数量，不能直接用foreach遍历，因为要用到索引
            float shouldPosition = 0f;
            float preShouldPosition = 0f; //两个变量记录向上吸附和向下吸附所需的位置数据
            float snapToPosition = 0f;
            //其实由于滑动区域限制，不可能
            for (int i = 0; i < itemCount; i++)
            {
                preShouldPosition = shouldPosition;
                shouldPosition += items[i].sizeDelta.y;
                float contentPosY = content.anchoredPosition.x;
                if (Mathf.Approximately(contentPosY, shouldPosition))
                {
                    content.anchoredPosition = new Vector2(0f, shouldPosition);//修正浮点位置
                    // break;
                    return; //近似的话，说明此时就刚好处于边界位置，不需要进行后续的插值移动了，也就是不需要吸附过程，如果用break的话，会发现到了下边界的时候，再拖拽会自动回到上边界，因为snapToPosition默认值为0
                } //直到找到小于或等于，才能确定此时位于边界的元素。
                else if (contentPosY < shouldPosition)
                {
                    if (dir > 0) snapToPosition = shouldPosition;
                    else snapToPosition = preShouldPosition; //这样就包含为0的情况了，不过实际来说无所谓，因为在传入的时候就应该传入1或者-1。
                    break;
                }
            }

            snapTween = content.DOAnchorPos(new Vector2(0, snapToPosition), duration, false);
        }
        */

        /// <summary>
        /// 实现附着功能。
        /// </summary>
        /// <param name="dir"></param>
        private void Snap(int dir)
        {
            if (EdgeCorrect()) return;

            int upIndex = Mathf.Clamp((int)(content.anchoredPosition.x / itemSize), 0, items.Length - 1);
            int downIndex = Mathf.Clamp(upIndex + viewCount, 0, items.Length - 1); //viewCount就相当于位于上下边界的两个元素的距离
            upItem = items[upIndex];
            downItem = items[downIndex];

            // int index = (int)(content.anchoredPosition.x / itemSize); //这里就不能用RoundToInt了，就是用int转换丢弃小数部分
            //处于边界情况，具体来说是已经完全接近于目标情况，那么就直接设置，也就是“修正”，并且不用也一定不要执行后续逻辑，要不然往往都会出一些bug。
            // if (index * itemSize - content.anchoredPosition.x <= 0.05f)
            // {
            //     content.anchoredPosition = new Vector2(0f, index * itemSize);
            //     return;
            // }
            float leftPosition = upIndex * itemSize;
            float rightPosition = Mathf.Clamp(upIndex + 1, 0, items.Length - viewCount) * itemSize; //实际能超出边界的应该除去可视部分
            // float upperPosition = (index + 1)* itemSize;
            float targetPosition;

            snapSequence = DOTween.Sequence();
            if (dir > 0)
            {
                targetPosition = rightPosition;
                //利用在OnDrag方法中已经计算得到的upItem和downItem
                if (hasEffect)
                {
                    snapSequence.Join(upItem.DOScale(0f, duration));
                    snapSequence.Join(downItem.DOScale(1f, duration));
                    snapSequence.Join(upItem.GetComponent<CanvasGroup>().DOFade(0f, duration));
                    snapSequence.Join(downItem.GetComponent<CanvasGroup>().DOFade(1f, duration));
                }
            }
            else
            {
                targetPosition = leftPosition;
                if (hasEffect)
                {
                    snapSequence.Join(upItem.DOScale(1f, duration));
                    snapSequence.Join(downItem.DOScale(0f, duration));
                    snapSequence.Join(upItem.GetComponent<CanvasGroup>().DOFade(1f, duration));
                    snapSequence.Join(downItem.GetComponent<CanvasGroup>().DOFade(0f, duration));
                }
            }

            snapSequence.Join(content.DOAnchorPosX(targetPosition, duration, true)).onComplete += CheckToBorder;
        }

        private void CheckToBorder()
        {
            if (Mathf.Approximately(content.anchoredPosition.x, maxX))
            {
                toBorder?.Invoke(-1); //到达左边界
            }
            else if (Mathf.Approximately(content.anchoredPosition.x, 0f))
            {
                toBorder?.Invoke(1); //到达右边界
            }
            else toBorder?.Invoke(0); //0表示没有触碰边界。
        }

        //Tip：由于边界修正的存在，ScrollRect的Elastic就不起作用了，就都是表现为Clamped了
        /// <summary>
        /// 边界修正。在Content位于边界位置时进行位置、缩放、alpha的修正，保证不会因为边界而出现意外情况。
        /// </summary>
        /// <returns></returns>
        private bool EdgeCorrect()
        {
            int upIndex, downIndex;

            //除了修正位置以外，由于会导致跳过（ApplyEffect）后续逻辑，所以需要同时补上对于视图内元素的scale和alpha修正
            if (Mathf.Approximately(content.anchoredPosition.x, maxX) || content.anchoredPosition.x > maxX)
            {
                toBorder?.Invoke(-1); //到达左边界
                content.anchoredPosition = new Vector2(maxX, 0f);
                // preDownIndex = items.Length - 1;
                // preUpIndex = preDownIndex - viewCount;
                //对于索引的数据预处理不同。
                downIndex = items.Length - 1;
                upIndex = downIndex - (viewCount - 1);
                ViewCorrect(upIndex, downIndex);
                return true;
            }
            else if (Mathf.Approximately(content.anchoredPosition.x, 0f) || content.anchoredPosition.x < 0f)
            {
                toBorder?.Invoke(1); //到达右边界
                content.anchoredPosition = new Vector2(0f, 0f);
                // preUpIndex = 0;
                // preDownIndex = preUpIndex + viewCount;
                upIndex = 0;
                downIndex = upIndex + (viewCount - 1);
                ViewCorrect(upIndex, downIndex);
                return true;
            }
            //Tip:这里也是一个修正，（对于恢复Flag）不要完全以边界作为边界，要留出一部分空间，否则会发现Flag闪烁的情况。
            if (content.anchoredPosition.x < maxX - itemSize / 5 && content.anchoredPosition.x > 0f + itemSize / 5)
                toBorder?.Invoke(0); //0表示没有触碰边界。
            return false;
        }

        /// <summary>
        /// 视图修正。修正视图内应该完整显示的元素
        /// </summary>
        /// <param name="upIndex"></param>
        /// <param name="downIndex"></param>
        private void ViewCorrect(int upIndex, int downIndex)
        {
            for (int i = upIndex; i <= downIndex; i++)
            {
                var item = items[i];
                item.localScale = Vector3.one;
                item.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // snapTween.Kill(); //可以选择是否要直接中断还没有结束的吸附插值
            // snapTween?.Complete(); //我选择直接让它结束，其实实际操作中，由于duration本来就很短（这里设置为了0.1s），也没必要处理吸附时又开始拖拽的情况
            snapSequence.Complete();

            //利用吸附机制，保证开始拖拽时元素贴合边界
            // int index = Mathf.RoundToInt(content.anchoredPosition.x / itemSize);

        }

        public void OnDrag(PointerEventData eventData)
        {
            if (hasEffect)
                ApplyEffect();

            if (eventData.delta.x > 0) dragDir = 1;
            else if (eventData.delta.x < 0) dragDir = -1;


        }

        private void ApplyEffect()
        {
            // if (content.anchoredPosition.x - content.anchoredPosition.x / itemSize <= 0.05f)
            // {
            //     content.anchoredPosition = new Vector2(0, Mathf.RoundToInt(content.anchoredPosition.x / itemSize) * itemSize);
            //     return;
            // }

            // int upIndex = (int)(content.anchoredPosition.x / itemSize);

            if (EdgeCorrect()) return;

            int upIndex = Mathf.Clamp((int)(content.anchoredPosition.x / itemSize), 0, items.Length - 1);
            int downIndex = Mathf.Clamp(upIndex + viewCount, 0, items.Length - 1); //viewCount就相当于位于上下边界的两个元素的距离
            //其实此处也是一个修正，是对于“跳帧”问题的另一种修正方式
            ViewCorrect(upIndex + 1, downIndex - 1); //夹在上下边界的两个元素中间的元素就是要完整显示的。

            // //Tip:如果滑动过快的话，会出现我猜测是属于“跳帧”的bug
            // if (upIndex > preUpIndex)
            // {

            // }


            // int upOverIndex = Mathf.Clamp(upIndex - 1, 0, items.Length - 1);
            // int downOverIndex = Mathf.Clamp(downIndex + 1, 0, items.Length - 1);
            // for (int i = 0; i <= upOverIndex; i++)
            // {
            //     var item = items[i];
            //     item.localScale = Vector3.zero;
            //     item.GetComponent<CanvasGroup>().alpha = 0f;
            // }
            // for (int i = downOverIndex; i <= items.Length - 1; i++)
            // {
            //     var item = items[i];
            //     item.localScale = Vector3.zero;
            //     item.GetComponent<CanvasGroup>().alpha = 0f;
            // }

            //理解好变量的意义，想象一下图形，在数学上其实就是小学算式，但是作为数学问题可以随意设置变量、随便指定类型，而在计算机中的手段很受限，必须明确可用的条件，然后思考如果编写代码，才能让计算机解决这样简单的问题
            float upItemOffset = content.anchoredPosition.x - upIndex * itemSize;
            float downItemOffset = (content.anchoredPosition.x + viewport.rect.width) - downIndex * itemSize;
            //并不需要判断此时是否向上还是向下滑动，因为利用了一个底层条件（准确来说是底层假设），上面代表出，下面代表入，所以上面计算的是超出上边界的长度， 而下面计算的是进入下边界的长度
            //这就是一个利用*对称性*的假设，即情况是等价的，只要取其一来计算即可得到正确答案。
            upItem = items[upIndex]; //upItem和downItem需要共享，所以设置为了成员变量，而非此处的局部变量
            downItem = items[downIndex];
            upItem.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, upItemOffset / itemSize);
            downItem.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, downItemOffset / itemSize);
            upItem.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, upItemOffset / itemSize);
            downItem.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0f, 1f, downItemOffset / itemSize);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Snap(dragDir);
        }



    }
}