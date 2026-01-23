using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Implement this interface in a PlayableAsset to specify which properties will be modified when Timeline is in preview mode.
    /// 编辑时预览功能的重要接口，获取属性、驱动属性。
    /// </summary>
    public interface IPropertyPreview
    {
        /// <summary>
        /// Called by the Timeline Editor to gather properties requiring preview.
        /// </summary>
        /// <param name="director">The PlayableDirector invoking the preview</param>
        /// <param name="driver">PropertyCollector used to gather previewable properties</param>
        void GatherProperties(PlayableDirector director, IPropertyCollector driver);
    }
}
