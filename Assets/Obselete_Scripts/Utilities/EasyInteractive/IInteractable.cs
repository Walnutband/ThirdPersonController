using System;

namespace HalfDog.EasyInteractive
{
    public interface IInteractable
    {
        /// <summary>
        /// 交互对象标识（本身的标签）
        /// </summary>
        public Type interactTag { get; }
    }

    /// <summary>
    /// 交互：可聚焦接口
    /// </summary>
    public interface IFocusable : IInteractable
    {
        public bool enableFocus { get; } //可以获取焦点，就是是否可以聚焦的开关，在实际游戏中可想而知非常有用

        /// <summary>
        /// 焦点进入
        /// </summary>
        public void OnFocus();

        /// <summary>
        /// 焦点离开
        /// </summary>
        public void EndFocus();
    }

    public interface ISelectable : IFocusable, IInteractable //可以获取焦点才可以被选择
    {
        public bool enableSelect { get; }

        /// <summary>
        /// 选择
        /// </summary>
        public void OnSelect();

        /// <summary>
        /// 取消选择
        /// </summary>
        public void EndSelect();
    }

    public interface IDragable : IFocusable, IInteractable
    {
        public bool enableDrag { get; }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        public void OnDrag();

        /// <summary>
        /// 拖拽中
        /// </summary>
        public void ProcessDrag();

        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void EndDrag(IFocusable target);
    }
}