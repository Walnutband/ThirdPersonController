using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyPlugins.GoodUI
{
    //需要LayoutGroup相关组件来对作为子对象的若干AccordionElement进行布局重建以实现动态的展开和折叠效果，ContentSizeFitter组件来自动调整作为父对象的组对象尺寸
    [DisallowMultipleComponent]
    [AddComponentMenu("GoodUI/Controls/AccordionGroup")]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class AccordionGroup : UIBehaviour //没有ExecuteAlways标记，则只在运行时起到控制组中元素状态的作用
    {

        /// <summary>
        /// 存储属于该组的AccordionElement
        /// </summary>
        /// <remarks>对于AccordionElement应该自行在禁用时退出组，这样在组中存储的列表中就不会存在可能未启用的元素了，方便其他逻辑处理</remarks>
        [SerializeField]
        // [ContextMenuItem("自动获取子元素", nameof(AutoFindChildrenElements))]
        private List<AccordionElement> elements = new List<AccordionElement>();

        /// <summary>
        /// 是否允许多个元素同时处于On状态，默认为true，比如任务面板，也可以设置为false，比如属性面板，因为在false时带有一个自动功能，即展开下一个就会自动折叠上一个，操作更加方便
        /// </summary>
        [SerializeField]
        private bool m_AllowMultiOn = true;
        public bool allowMultiOn { get { return m_AllowMultiOn; } set { if (m_AllowMultiOn != value) { m_AllowMultiOn = value; EnsureValidState(); } } }

        public enum Transition
        {
            Instant,
            Tween
        }

        [SerializeField] private Transition m_Transition = Transition.Instant;
        [SerializeField] private float m_TransitionDuration = 0.3f;

        /// <summary>
        /// Gets or sets the transition.
        /// </summary>
        /// <value>The transition.</value>
        public Transition transition
        {
            get { return this.m_Transition; }
            set { this.m_Transition = value; }
        }

        /// <summary>
        /// Gets or sets the duration of the transition.
        /// </summary>
        /// <value>The duration of the transition.</value>
        public float transitionDuration
        {
            get { return this.m_TransitionDuration; }
            set { this.m_TransitionDuration = value; }
        }

        protected override void Start()
        {
            base.Start();
            FindChildrenElements();
            EnsureValidState();
        }

        //没有实现OnDisable方法是因为该组件仅仅是起到一个组合的辅助作用，只是控制组中元素的状态，而不应该控制组中元素的可用性
        //Tip:或许该考虑在进入运行模式时自动查找一下子元素，但我始终认为这些内容本来就应该在编辑时设置好，不需要在运行时逻辑中添加这些代码，当然如果要想实现比如UGC让玩家自己尝试制作UI之类的话，那可以实现，但这是另一回事。
        protected override void OnEnable()
        {
            base.OnEnable();
            // FindChildrenElements();
            EnsureValidState();

        }

        /// <summary>
        /// 确定有效状态，就是判断是否符合限制条件，此处唯一可能存在的限制条件就是字段allowMultiOn指定的是否允许多个元素处于On状态即展开状态，而始终都是允许全部关闭的
        /// </summary>
        /// <remarks>在ToggleGroup中限制条件更多，主要是其要求同时最多有一个Toggle处于On状态，然后可以用allowSwitchOff指定是否允许所有Toggle都处于off状态</remarks>
        public void EnsureValidState() //TODO:为了让Editor类调用而设置为公开，其实不太好。
        {
            // Debug.Log("EnsureValidState");
            if (false == m_AllowMultiOn)
            {
                bool hasOn = false;
                foreach (AccordionElement element in elements)
                {
                    //遇到第一个为On的元素后，就要将后续的所有元素设置为false了
                    if (hasOn)
                        element.isExpand = false;
                    // element.ChangeState(false);
                    else if (true == element.isExpand)
                        hasOn = true;
                }
            }
        }

        /// <summary>
        /// 实现互斥性，保证在唯一模式下，展开另一元素时自动折叠当前展开的元素
        /// </summary>
        /// <param name="target"></param>
        public void EnsureValidState(AccordionElement target)
        {
            if (target == null) return;

            if (false == m_AllowMultiOn)
            {
                foreach (AccordionElement element in elements)
                {
                    if (element.isExpand == true && element != target)
                        element.isExpand = false;
                }
            }
        }
        /// <summary>
        /// 组中元素是否可以设置为On状态，位于组中的AccordionElement在设置状态时都会通过该方法检查，就是为了保证受到组限制。
        /// </summary>
        /// <returns></returns>
        /// <remarks>其实用不上，因为在互斥状态下，应该是点击其他会导致当前展开的元素自动折叠，而不是无法点击其他元素</remarks>
        public bool CanElementSetOn()
        {
            if (true == m_AllowMultiOn)
                return true;
            else
            {
                foreach (var element in elements)
                {
                    if (element.isExpand == true)
                        return false;
                }
                return true;
            }

        }

        private void OnTransformChildrenChanged()
        {

        }

        public void AddElement(AccordionElement element)
        {
            if (element != null && element.IsActive())
            {
                elements.Add(element);
                element.group = this;
            }
        }
        public void RemoveElement(AccordionElement element)
        {
            elements.Remove(element);
            element.group = null;
        }

#if UNITY_EDITOR
        #region 菜单方法
        /// <summary>
        /// 自动查找该组件所挂载对象下的直接子对象中挂载有AccordionElement的对象，即可以作为该组元素的对象。
        /// </summary>
        [ContextMenu("获取子元素")]
        private void FindChildrenElements()
        {
            elements.Clear();

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                AccordionElement element = child.GetComponent<AccordionElement>();
                //注意双向的，将成员加入该组，以及通知成员属于该组。
                if (null != element)
                    AddElement(element);
            }
            //获取到子元素后，就掌握了控制权，立刻检查状态。
            EnsureValidState();
        }
        [ContextMenu("清空子元素")]
        private void ClearElements()
        {
            elements.ForEach(element => element.group = null);
            elements.Clear();
        }

        [ContextMenu("一键设置Header标签")]
        private void SetHeaderLabels()
        {
            FindChildrenElements();
            int count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                elements[i].transform.FindRecursively("Label").GetComponent<TextMeshProUGUI>().text = $"属性 {i + 1}";
            }
        }

        #endregion
#endif

    }
}