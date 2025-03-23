using System;
//这个思路就是将交互行为从整个交互过程中抽象出来，并且具体化为一个个的交互情景，这样就可以在不同的交互对象之间进行交互，而不用在每个对象中写一大堆的交互代码
//总之还是根本思想：明确目标，找出其决定因素，抽象出来并且具体化，然后交互对象都只是临时的引用，只有交互行为本身才是交互过程的核心
//决定必隐，无关必显。

namespace HalfDog.EasyInteractive
{
    /// <summary>
    /// 交互情景接口
    /// </summary>
    public interface IInteractCase
    {
        public Type subject { get; }
        public Type target { get; }
        public bool enable { get; set; }
        /// <summary>
        /// 执行交互情景
        /// </summary>
        /// <param name="focusable">当前聚焦对象</param>
        /// <param name="selectable">当前选择对象</param>
        /// <param name="dragable">当前拖拽对象</param>
        /// <returns></returns>
        public bool Execute(IFocusable focusable, ISelectable selectable, IDragable dragable);
    }

    /// <summary>
    /// 交互情景抽象类
    /// </summary>
    public abstract class AbstractInteractCase : IInteractCase
    {
        private Type _subject; //交互主体
        private Type _target; //交互目标
        private bool _enable;  //是否开启
        public Type subject => _subject;
        public Type target => _target;

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool enable
        {
            get => _enable;
            set => _enable = value;
        }

        public AbstractInteractCase(Type subject, Type target)
        {
            _subject = subject;
            _target = target;
        }

        public abstract bool Execute(IFocusable focusable, ISelectable selectable, IDragable dragable);
    }
}
