using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{//Ques：既然派生类只有一个TimelineWindow，那么为何要单独分出来这一个抽象类作为基类呢？
    /// <summary>
    /// Base class of the TimelineWindow.
    /// </summary>
    public abstract class TimelineEditorWindow : EditorWindow
    {
        /// <summary>
        /// Use this interface to navigate between Timelines and Sub-Timelines. (RO，ReadOnly只读，所以只声明了一个get)
        /// 在Timeline和SubTimeline之间进行导航。
        /// </summary>
        public abstract TimelineNavigator navigator { get; }

        /// <summary>
        /// Use this interface to control the playback behaviour of the Timeline window. (RO)
        /// 控制Timline窗口的playback行为
        /// </summary>
        public abstract TimelinePlaybackControls playbackControls { get; }
        /// <summary>
        /// Retrieves and sets the Timeline Window lock state. When disabled (false), the window focus follows the Unity selection.
        /// </summary>
        /// <remarks>When the lock state transitions from true to false, the focused timeline is synchronized with the Unity selection.</remarks>>
        public abstract bool locked { get; set; } //整个窗口的锁定，一旦lock就不随Unity选中对象变化了
        /// <summary>
        /// Sets which TimelineAsset is shown in the TimelineWindow.
        /// </summary>
        /// <param name="sequence">The TimelineAsset to show. Specify a null to clear the TimelineWindow.</param>
        /// <remarks>When you call this method, the TimelineWindow is placed in asset edit mode. This mode does not support all features. For example, bindings are not available and the timeline cannot be evaluated.
        /// You can use this method when the TimelineWindow is locked.</remarks>
        public abstract void SetTimeline(TimelineAsset sequence);  //点击TimlineAsset资产就是调用这个
        /// <summary>
        /// Sets which TimelineAsset is shown in the TimelineWindow based on the PlayableDirector.
        /// </summary>
        /// <param name="director">The PlayableDirector associated with the TimelineAsset to show in the TimelineWindow. Specify a null to clear the TimelineWindow.</param>
        /// <remarks>You can use this method when the TimelineWindow is locked.</remarks>
        public abstract void SetTimeline(PlayableDirector director); //点击挂载有PlayableDirector组件的游戏对象就是调用这个。
        /// <summary>
        /// Clears the TimelineAsset that is shown in the TimelineWindow.
        /// </summary>
        /// <remarks>You can use this method when the TimelineWindow is locked.</remarks>>
        public abstract void ClearTimeline(); //就是取消选中的效果，但是通常扩展的编辑窗口只会随着选中另一个正确的目标对象才会切换面板内容，而不是只要选中当前对象以外的对象就取消选中
    }
}
