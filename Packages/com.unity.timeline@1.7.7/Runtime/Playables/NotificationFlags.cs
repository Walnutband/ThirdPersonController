using System;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Use these flags to specify the notification behaviour.
    /// </summary>
    /// <see cref="UnityEngine.Playables.INotification"/>
    [Flags]
    [Serializable]
    /*Tip：附加性枚举，从这些枚举常量其实揭示出一些默认的机制。
    细看这些选项，非常地自然，因为只要去实际观察运行情况，就会发现存在以下这些不确定的情况，所以给出一个设置，也就是选择一个确定的方式来执行。
    */
    //指定常量的数据类型为short即int16，就是节省内存，因为压根用不到那么大范围，其实这个范围都很大，只是能用的数据类型确实也就这些。
    public enum NotificationFlags : short 
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
