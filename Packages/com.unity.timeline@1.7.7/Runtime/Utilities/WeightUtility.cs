using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    static class WeightUtility
    {
        

        // Given a mixer, normalizes the mixer if required
        //  returns the output weight that should be applied to the mixer as input
        public static float NormalizeMixer(Playable mixer)
        {
            if (!mixer.IsValid())
                return 0;
            int count = mixer.GetInputCount();
            float weight = 0.0f;
            for (int c = 0; c < count; c++)
            {
                weight += mixer.GetInputWeight(c);
            }

            /*Tip：具体解读一下，结合现象，也就是在单个动画轨道上，如果当前片段处于Ease状态，那么总权重就会小于1，而此处会将其Mixer节点权重归一化，并且LayerMixer节点在该端口的权重
            就会设置为总权重。*/
            //归一化权重的基本方法：按比例分配权重。
            if (weight > Mathf.Epsilon && weight < 1) //Ques：总权重大于1的话就不会进行归一化，不知道这个处理怎样，因为似乎按照正常情况也不会出现这种总权重大于1的情况。
            {
                for (int c = 0; c < count; c++)
                {
                    mixer.SetInputWeight(c, mixer.GetInputWeight(c) / weight);
                }
            }
            return Mathf.Clamp01(weight); //总权重，夹到0~1
        }
    }
}
