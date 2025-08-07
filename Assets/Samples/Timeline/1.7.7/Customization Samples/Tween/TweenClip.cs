using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    // Represents the serialized data for a clip on the Tween track
    //PlayableBehaviour的数据不会被序列化，并且在graph销毁时就会丢失，为了保存该数据，就要定义一个新的PlayableAsset如下。
    [Serializable]
    [DisplayName("Tween Clip")]
    public class TweenClip : PlayableAsset, ITimelineClipAsset, IPropertyPreview
    {
        //因为资产无法直接引用场景对象，所以使用ExposedReference。不过其实在检视器中确实可以直接拖拽场景对象，这是编辑器功能，在运行时必须要通过ExposedReference解析才行。
        public ExposedReference<Transform> startLocation;
        public ExposedReference<Transform> endLocation;

        [Tooltip("Changes the position of the assigned object")]
        public bool shouldTweenPosition = true;

        [Tooltip("Changes the rotation of the assigned object")]
        public bool shouldTweenRotation = true;

        [Tooltip("Only keys in the [0,1] range will be used")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        // Implementation of ITimelineClipAsset. This specifies the capabilities of this timeline clip inside the editor.
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; } 
        }

        // Creates the playable that represents the instance of this clip.
        //一个PlayableAsset的主要目的就是构建一个PlayableBehaviour，这正是该方法所做的，使用TweenClip的数据来初始化一个TweenBehaviour。
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            // create a new TweenBehaviour
            //Tip：在Playables系统的手册中始终没有说清楚ScriptPlayable的用处，可以简单理解为拥有一个PlayableBehaviour成员，在运行时会周期性调用该成员的一些接口方法，从而实现自定义逻辑。
            ScriptPlayable<TweenBehaviour> playable = ScriptPlayable<TweenBehaviour>.Create(graph);
            TweenBehaviour tween = playable.GetBehaviour();

            // set the behaviour's data
            /*Tip: Resolve方法就是调用IExposedPropertyTable的GetReference方法来尝试根据记录的exposedName（标识符ID）来获取对应的场景对象或组件,
            问了AI，然后发现，这里GetResolver返回的就是PlayableDirector本身！！可以看到其实现了IExposedPropertyTable接口，不过具体实现都隐藏在C++层，不过可以
            合理猜想，ExposedReference调用Resolve方法，在PlayableDirector的GetReferenceValue方法中查表，获取到*/
            tween.startLocation = startLocation.Resolve(graph.GetResolver());
            tween.endLocation = endLocation.Resolve(graph.GetResolver());
            tween.curve = curve;
            tween.shouldTweenPosition = shouldTweenPosition;
            tween.shouldTweenRotation = shouldTweenRotation;

            return playable;
        }

        // Defines which properties are changed by this playable. Those properties will be reverted in editmode
        // when Timeline's preview is turned off.
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            const string kLocalPosition = "m_LocalPosition";
            const string kLocalRotation = "m_LocalRotation";

            driver.AddFromName<Transform>(kLocalPosition + ".x");
            driver.AddFromName<Transform>(kLocalPosition + ".y");
            driver.AddFromName<Transform>(kLocalPosition + ".z");
            driver.AddFromName<Transform>(kLocalRotation + ".x");
            driver.AddFromName<Transform>(kLocalRotation + ".y");
            driver.AddFromName<Transform>(kLocalRotation + ".z");
            driver.AddFromName<Transform>(kLocalRotation + ".w");
        }
    }
}

namespace Test
{
    public interface TestInter
    {
        void Test();
    }

    public class TestBase : TestInter
    {
        void TestInter.Test()
        { }
    }

    public class TestSub : TestBase, TestInter
    {

    }
}