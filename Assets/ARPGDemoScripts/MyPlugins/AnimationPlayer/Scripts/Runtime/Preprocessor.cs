using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    /*Tip：就是处理权重，*/

    public class Preprocessor : PlayableBehaviour
    {
        private AnimationGraph m_Graph;
        internal AnimationGraph graph {set => m_Graph = value;}

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
                { //将本轮次执行完成的对象记录下来，之后统一移除。
                    toRemove.Add(u);
                }
            }
            toRemove.ForEach(u => RemoveUpdatable(u));

            ProcessLayerMixerWeight();
        }

        public void AddUpdatable(IUpdatable _updatable)
        {
            m_Updatables.Add(_updatable);
            /*BUG：因为存在强制结束FadeHandler的情况，所以这里必须要注册回调，在结束时就把这个FadeHandler移除掉，否则本来节点都已经断开了，
            还继续调用Fadehandler的Update方法设置节点的权重，在AnimationLayer的SetStateWeight方法中传入的索引为负，即游离节点。*/
            _updatable.onComplete = () => RemoveUpdatable(_updatable);
        }
        //TODO：这里移除还可以使用算法优化。
        public void RemoveUpdatable(IUpdatable _updatable) => m_Updatables.Remove(_updatable);

        //Tip：参考Timeline对于分层动画的处理。
        private void ProcessLayerMixerWeight()
        {
            AnimationLayerMixer layerMixer = m_Graph.layerMixer;
            List<AnimationLayer> layers = layerMixer.layers; 
            if (layers == null || layers.Count == 0) return;

            for (int i = 0; i < layers.Count; i++)
            {
                AnimationLayer layer = layers[i];
                if (layer == null) continue;

                float weight = NormalizeMixer(layer);
                layerMixer.SetLayerWeight(layer, weight);
            }

            // for (int i = 0; i < layers.Count; i++)
            // {
            //     AnimationLayer layer = layers[i];
            //     if (layer == null) continue;
                
            //     NormalizeMixer(layer);
            // }
        }

        // Given a mixer, normalizes the mixer if required
        //  returns the output weight that should be applied to the mixer as input
        // public static float NormalizeMixer(AnimationLayer _layer)
        public static float NormalizeMixer(IAnimationMixer _mixer)
        {
            Playable mixer = _mixer.playable;

            if (!mixer.IsValid())
                return 0;
            int count = mixer.GetInputCount();
            float weight = 0.0f;
            for (int c = 0; c < count; c++)
            {
                if (!mixer.GetInput(c).IsValid())
                    continue;

                weight += mixer.GetInputWeight(c);
                // Debug.Log($"端口{c}权重为{mixer.GetInputWeight(c)}");
            }

            /*Tip：具体解读一下，结合现象，也就是在单个动画轨道上，如果当前片段处于Ease状态，那么总权重就会小于1，而此处会将其Mixer节点权重归一化，并且LayerMixer节点在该端口的权重
            就会设置为总权重。*/
            //归一化权重的基本方法：按比例分配权重。
            // if (weight > Mathf.Epsilon && weight < 1) //Ques：总权重大于1的话就不会进行归一化，不知道这个处理怎样，因为似乎按照正常情况也不会出现这种总权重大于1的情况。
            if (weight > Mathf.Epsilon)
            {
                for (int c = 0; c < count; c++)
                {
                    if (!mixer.GetInput(c).IsValid())
                    {
                        mixer.SetInputWeight(c, 0);
                        continue;
                    }
                    mixer.SetInputWeight(c, mixer.GetInputWeight(c) / weight);
                }
            }
            return Mathf.Clamp01(weight); //总权重，夹到0~1
        }
    }

}