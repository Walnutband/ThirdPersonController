using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using Object = UnityEngine.Object;

namespace MyPlugins.SkillEditor
{
    /*Tip：每一个需要使用Timeline存储内容的个体就会使用一个TimleinePlayer用于播放Timeline。*/
    
    public class TimelinePlayer : MonoBehaviour, IExposedPropertyTable
    {

        //TODO：应当具有缓存功能，因为会频繁交替使用。
        // private TimelineObj m_CachedTimeline;
        
        //TODO：应该需要上下文，因为技能是特定于角色的，而不是（像剧情编排）控制场景中各个对象的。

        public void Load(TimelineAsset asset) {}

        /*TODO：字典可以很容易地自定义序列化，只是在检视器中直接操作的话就不好保证其Key的唯一性，而Timeline的编辑器是在创建一个新的且带有Binding目标的Track时就会同时在SceneBindings
        的字典中创建一个键值对，所以必然能够保证唯一性，而且在检视器中只能操作值、无法操作键，这就很自然地实现了字典的序列化及其使用。*/
        [SerializeField] private Dictionary<TrackAsset, SceneBinding> m_TrackBindings = new Dictionary<TrackAsset, SceneBinding>(); 
        [SerializeField] private Dictionary<int, Reference> m_References = new Dictionary<int, Reference>();

        public void SetReferenceValue(PropertyName _id, Object _value)
        {
            int id = _id.GetHashCode();
            if (m_References.ContainsKey(id))
            {
                m_References[id] = new Reference(id, _value);
            }
            else
            {
                m_References.Add(id, new Reference(id, _value));
            }
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            int _id = id.GetHashCode();
            if (m_References.ContainsKey(_id))
            {
                idValid = true;
                return m_References[_id].target;
            }
            else
            {
                idValid = false;
                return null;
            }
        }

        public void ClearReferenceValue(PropertyName id)
        {
            int _id = id.GetHashCode();
            if (m_References.ContainsKey(_id))
            {
                m_References.Remove(_id);
            }
        }

        public void SetTrackBinding(TrackAsset track, Object target)
        {
            m_TrackBindings.Add(track, new SceneBinding(track, target));
        }

        public Object GetTrackBinding(TrackAsset track)
        {
            if (m_TrackBindings.ContainsKey(track))
            {
                return m_TrackBindings[track].target;
            }
            else return null;
        }

        [Serializable]
        public struct SceneBinding
        {
            public TrackAsset track;
            public Object target;

            public SceneBinding(TrackAsset _track, Object _target)
            {
                track = _track;
                target = _target;
            }
        }

        [Serializable]
        public struct Reference
        {
            public int id;
            public Object target;

            public Reference(int _id, Object _target)
            {
                id = _id;
                target = _target;
            }
        }
    }

}