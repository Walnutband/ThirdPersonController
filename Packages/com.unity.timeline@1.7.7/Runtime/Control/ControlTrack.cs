using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /*Tip：ControlTrack非常重要，功能极其强大，*/

    /// <summary>
    /// A Track whose clips control time-related elements on a GameObject.
    /// </summary>
    [TrackClipType(typeof(ControlPlayableAsset), false)]
    [ExcludeFromPreset]
    [TimelineHelpURL(typeof(ControlTrack))]
    public class ControlTrack : TrackAsset
    {
#if UNITY_EDITOR
        private static readonly HashSet<PlayableDirector> s_ProcessedDirectors = new HashSet<PlayableDirector>();

        /// <inheritdoc/>
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director == null)
                return;

            // avoid recursion
            if (s_ProcessedDirectors.Contains(director))
                return;

            s_ProcessedDirectors.Add(director);

            //这里代表的是可以控制的四种对象类型，ParticleSystem粒子系统的播放即特效的控制，GameObject游戏对象的启用和禁用，（实现ITimeControl）MonoBehaviour组件的控制（触发回调），PlayableDirector控制的是SubTimeline
            var particlesToPreview = new HashSet<ParticleSystem>();
            var activationToPreview = new HashSet<GameObject>();
            var timeControlToPreview = new HashSet<MonoBehaviour>();
            var subDirectorsToPreview = new HashSet<PlayableDirector>();

            foreach (var clip in GetClips())
            {
                var controlPlayableAsset = clip.asset as ControlPlayableAsset;
                if (controlPlayableAsset == null)
                    continue;

                var gameObject = controlPlayableAsset.sourceGameObject.Resolve(director);
                if (gameObject == null)
                    continue;

                //Tip：四个成员就是在检视器中看到的四个选项，只是经过了编辑器脚本处理，统一改成了“Control ···”
                if (controlPlayableAsset.updateParticle)
                    particlesToPreview.UnionWith(gameObject.GetComponentsInChildren<ParticleSystem>(true));
                if (controlPlayableAsset.active)
                    activationToPreview.Add(gameObject);
                if (controlPlayableAsset.updateITimeControl)
                    timeControlToPreview.UnionWith(ControlPlayableAsset.GetControlableScripts(gameObject));
                if (controlPlayableAsset.updateDirector)
                    subDirectorsToPreview.UnionWith(controlPlayableAsset.GetComponent<PlayableDirector>(gameObject));
            }

            ControlPlayableAsset.PreviewParticles(driver, particlesToPreview);
            ControlPlayableAsset.PreviewActivation(driver, activationToPreview);
            ControlPlayableAsset.PreviewTimeControl(driver, director, timeControlToPreview);
            ControlPlayableAsset.PreviewDirectors(driver, subDirectorsToPreview);

            s_ProcessedDirectors.Remove(director);

            particlesToPreview.Clear();
            activationToPreview.Clear();
            timeControlToPreview.Clear();
            subDirectorsToPreview.Clear();
        }

#endif
    }
}
