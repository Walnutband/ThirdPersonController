namespace UnityEngine.Timeline
{//Tip：实现INotificationOptionProvider就可以自定义flags，并且在NotificationUtilities中调用AddNotification方法时作为参数传入。
    /// <summary>
    /// Implement this interface to change the behaviour of an INotification.
    /// </summary>
    /// This interface must be implemented along with <see cref="UnityEngine.Playables.INotification"/> to modify the default behaviour of a notification.
    /// <seealso cref="UnityEngine.Timeline.NotificationFlags"/>
    public interface INotificationOptionProvider
    {
        /// <summary>
        /// The flags that change the triggering behaviour.
        /// </summary>
        NotificationFlags flags { get; }
    }
}
