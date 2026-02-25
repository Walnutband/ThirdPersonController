using Physalia.Flexi;
using UnityEngine;

namespace ARPGDemo.Test
{
    [NodeCategory("Test/Value")]
    public class RandomIntNode : DefaultValueNode
    {
        public Inport<int> min;
        public Inport<int> max;
        public Outport<int> result;

        protected override void EvaluateSelf()
        {
            result.SetValue(Random.Range(min, max));
        }
    }
}