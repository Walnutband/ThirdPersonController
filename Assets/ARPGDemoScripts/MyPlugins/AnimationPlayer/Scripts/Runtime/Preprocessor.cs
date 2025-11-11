using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public class Preprocessor : PlayableBehaviour
    {
        private List<IUpdatable> m_Updatables = new List<IUpdatable>();

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);

            //不过在遍历过程中改变容器。
            List<IUpdatable> toRemove = new List<IUpdatable>(m_Updatables.Count);
            foreach (var u in m_Updatables)
            {
                // u.Update(info.deltaTime);
                if (u.Update(info.deltaTime))
                {
                    toRemove.Add(u);
                }
            }
            toRemove.ForEach(u => RemoveUpdatable(u));
        }

        public void AddUpdatable(IUpdatable _updatable)
        {
            m_Updatables.Add(_updatable);
            _updatable.onComplete = () => RemoveUpdatable(_updatable);
        }
        public void RemoveUpdatable(IUpdatable _updatable) => m_Updatables.Remove(_updatable);
    }

}