using System;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Use these flags to specify the notification behaviour.
    /// </summary>
    /// <see cref="UnityEngine.Playables.INotification"/>
    [Flags]
    [Serializable]
    //Tip：附加性枚举，从这些枚举常量其实揭示出一些默认的机制。
    public enum NotificationFlags : short //指定常量的数据类型为short即int16
    {
        /// <summary>
        /// Use this flag to send the notification in Edit Mode.
        /// </summary>
        /// <remarks>
        /// Sent on discontinuous jumps in time.
        /// </remarks>
        TriggerInEditMode = 1 << 0,

        /// <summary>
        /// Use this flag to send the notification if playback starts after the notification time.
        /// </summary>
        Retroactive = 1 << 1,

        /// <summary>
        /// Use this flag to send the notification only once when looping.
        /// </summary>
        TriggerOnce = 1 << 2,
    }
}
