using UnityEngine;

namespace UnityEditor.Timeline
{
    abstract class Manipulator
    {
        int m_Id;
        //各个派生类就按照自己的交互行为来重写对应的方法即可。
        protected virtual bool MouseDown(Event evt, WindowState state) { return false; }
        protected virtual bool MouseDrag(Event evt, WindowState state) { return false; }
        protected virtual bool MouseWheel(Event evt, WindowState state) { return false; }
        protected virtual bool MouseUp(Event evt, WindowState state) { return false; }
        protected virtual bool DoubleClick(Event evt, WindowState state) { return false; }
        protected virtual bool KeyDown(Event evt, WindowState state) { return false; }
        protected virtual bool KeyUp(Event evt, WindowState state) { return false; }
        protected virtual bool ContextClick(Event evt, WindowState state) { return false; }
        protected virtual bool ValidateCommand(Event evt, WindowState state) { return false; }
        protected virtual bool ExecuteCommand(Event evt, WindowState state) { return false; }

        public virtual void Overlay(Event evt, WindowState state) { }

        public bool HandleEvent(WindowState state)
        {
            Event currentEvent = Event.current;
            var type = currentEvent.GetTypeForControl(m_Id);
            return HandleEvent(type, currentEvent, state);
        }

        public bool HandleEvent(EventType type, WindowState state)
        {
            Event currentEvent = Event.current;
            return HandleEvent(type, currentEvent, state);
        }

        //根据当前的输入行为来调用对应的处理方法即可。返回值表示是否响应了事件
        /*Tip：如识别到自己负责的情形，执行对应逻辑（改变选中、修改剪辑时间、呼出菜单等），并返回 true，中断后续处理，
        否则返回 false，让下一个 Manipulator 有机会响应，因为一个Control中会存在多个Manipulator，会按照顺序来尝试响应当前的输入事件。*/
        bool HandleEvent(EventType type, Event evt, WindowState state)
        {
            if (m_Id == 0)
                m_Id = GUIUtility.GetPermanentControlID();

            bool isHandled = false;

            switch (type)
            {
                case EventType.ScrollWheel:
                    isHandled = MouseWheel(evt, state);
                    break;

                case EventType.MouseUp:
                {//Tip：这里就是保证不同鼠标按键按下和抬起的事件不会串，但是具体逻辑没有想通，因为隐藏在C++层，不过可以判断就类似于UI Toolkit中的CapturePointer捕获指针。
                    if (GUIUtility.hotControl == m_Id)
                    {
                        isHandled = MouseUp(evt, state);

                        GUIUtility.hotControl = 0;
                        evt.Use();
                    }
                }
                break;

                case EventType.MouseDown:
                {
                    //这都是Unity内置的事件系统自动处理的，只要读取数据就行了。
                    isHandled = evt.clickCount < 2 ? MouseDown(evt, state) : DoubleClick(evt, state);

                    if (isHandled)
                        GUIUtility.hotControl = m_Id;
                }
                break;

                case EventType.MouseDrag:
                {//这里与MouseUp处理类似。
                    if (GUIUtility.hotControl == m_Id)
                        isHandled = MouseDrag(evt, state);
                }
                break;

                case EventType.KeyDown:
                    isHandled = KeyDown(evt, state);
                    break;

                case EventType.KeyUp:
                    isHandled = KeyUp(evt, state);
                    break;

                case EventType.ContextClick:
                    isHandled = ContextClick(evt, state);
                    break;

                case EventType.ValidateCommand:
                    isHandled = ValidateCommand(evt, state);
                    break;

                case EventType.ExecuteCommand:
                    isHandled = ExecuteCommand(evt, state);
                    break;
            }

            if (isHandled)
                evt.Use(); //也就是该事件已经被处理掉，不再继续传播，也就实现了拦截事件的效果。

            return isHandled;
        }
    }
}
