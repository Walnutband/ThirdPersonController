using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    // Does a post processing of the weights on an animation track to properly normalize
    // the mixer weights so that blending does not bring default poses and subtracks, layers and
    // layer graphs blend correctly
    class AnimationOutputWeightProcessor : ITimelineEvaluateCallback
    {
        struct WeightInfo
        {
            public Playable mixer;
            public Playable parentMixer;
            public int port;
        }

        AnimationPlayableOutput m_Output;
        AnimationMotionXToDeltaPlayable m_MotionXPlayable;
        readonly List<WeightInfo> m_Mixers = new List<WeightInfo>();

        public AnimationOutputWeightProcessor(AnimationPlayableOutput output)
        {
            m_Output = output;
            output.SetWeight(0);
            FindMixers();
        }

        //Tip：LayerMixer节点就是通过以Output节点作为起点，利用节点的连接关系，向下查找而获取到的，而在创建和连接节点的那部分逻辑中并未处理LayerMixer的记录。
        void FindMixers()
        {
            var playable = m_Output.GetSourcePlayable();
            var outputPort = m_Output.GetSourceOutputPort();

            m_Mixers.Clear();
            // only write the final output in playmode. it should always be 1 in editor because we blend to the defaults
            FindMixers(playable, outputPort, playable.GetInput(outputPort));
        }

        /*Tip：最终，WeightInfo的成员，应当是LayerMixer节点（mixer）以及TimelienPlayable节点（parentMixer），以及Mixer节点（mixer）和LayerMixer节点（parentMixer）。*/
        // Recursively accumulates mixers.
        void FindMixers(Playable parent, int port, Playable node)
        {
            if (!node.IsValid())
                return;

            var type = node.GetPlayableType(); //因为是结构体，而真正的实例是其中的GetHandle获取的PlayableHandle。
            if (type == typeof(AnimationMixerPlayable) || type == typeof(AnimationLayerMixerPlayable))
            {
                /*Tip：一种情况，node是AnimationLayerMixerPlayable，而parent是TimelinePlayable，这里的递归则是传入LayerMixer节点，以及逐个传入连接在LayerMixer上的各个轨道节点，
                可能是AnimationMixerPlayable或是（提供RootMotion）偏移节点AnimationOffsetPlayable。所以从这里会发现为何下一个分支会保留当前的parent，因为假如此刻传入的是
                偏移节点而非Mixer节点，那么就会进入下一个分支，按理来说就应该保留LayerMixer作为parent，而将偏移节点的子节点即Mixer节点作为node传入。
                如果node是AnimationMixerPlayable也是同样的逻辑。
                总之，数形结合，借助数据结构来想象过程，还是比较容易理解的，以至于能够自己写出来这些逻辑。
                */
                // use post fix traversal so children come before parents
                int subCount = node.GetInputCount();
                for (int j = 0; j < subCount; j++)
                {
                    FindMixers(node, j, node.GetInput(j));
                }

                // if we encounter a layer mixer, we assume there is nesting occuring
                //  and we modulate the weight instead of overwriting it.
                var weightInfo = new WeightInfo
                {
                    parentMixer = parent,
                    mixer = node,
                    port = port,
                };
                m_Mixers.Add(weightInfo);
            }
            else
            {
                var count = node.GetInputCount();
                for (var i = 0; i < count; i++)
                {
                    FindMixers(parent, port, node.GetInput(i));
                }
            }
        }

        public void Evaluate()
        {
            float weight = 1;
            m_Output.SetWeight(1); //Output节点始终保持为1，因为并不会在这里处理逻辑。
            for (int i = 0; i < m_Mixers.Count; i++)
            {
                var mixInfo = m_Mixers[i];
                weight = WeightUtility.NormalizeMixer(mixInfo.mixer);
                mixInfo.parentMixer.SetInputWeight(mixInfo.port, weight);
            }

            // only write the final weight in player/playmode. In editor, we are blending to the appropriate defaults
            // the last mixer in the list is the final blend, since the list is composed post-order.
            if (Application.isPlaying)
                m_Output.SetWeight(weight);
        }
    }
}
