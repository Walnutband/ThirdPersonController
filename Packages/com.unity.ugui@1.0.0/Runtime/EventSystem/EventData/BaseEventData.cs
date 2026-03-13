namespace UnityEngine.EventSystems
{
    /// <summary>
    /// A class that can be used for sending simple events via the event system.
    /// </summary>
    public abstract class AbstractEventData
    {
        protected bool m_Used;

        /// <summary>
        /// Reset the event.
        /// </summary>
        public virtual void Reset()
        {
            m_Used = false;
        }

        //用于外部访问，是否已经在逻辑上使用过该事件，可以据此做出一些决策。

        /// <summary>
        /// Use the event.
        /// </summary>
        /// <remarks>
        /// Internally sets a flag that can be checked via used to see if further processing should happen.
        /// </remarks>
        public virtual void Use()
        {
            m_Used = true;
        }

        /// <summary>
        /// Is the event used?
        /// </summary>
        public virtual bool used
        {
            get { return m_Used; }
        }
    }

    /// <summary>
    /// A class that contains the base event data that is common to all event types in the new EventSystem.
    /// </summary>
    public class BaseEventData : AbstractEventData
    {
        //Tip：说明在构造之后就不能改变所属的事件系统了。
        private readonly EventSystem m_EventSystem;
        public BaseEventData(EventSystem eventSystem)
        {
            m_EventSystem = eventSystem;
        }

        /// <summary>
        /// >A reference to the BaseInputModule that sent this event.
        /// </summary>
        public BaseInputModule currentInputModule
        {
            get { return m_EventSystem.currentInputModule; }
        }

        /// <summary>
        /// The object currently considered selected by the EventSystem.
        /// </summary>
        public GameObject selectedObject
        {
            get { return m_EventSystem.currentSelectedGameObject; }
            set { m_EventSystem.SetSelectedGameObject(value, this); }
        }
    }
}
