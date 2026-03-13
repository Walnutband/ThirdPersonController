
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ARPGDemo.Test
{
    public class AnimationLayerMixerTest : MonoBehaviour
    {
        public AnimationClip clip1;
        public AnimationClip clip2;
        [Range(0f, 1f)]
        public float weight;
        public bool weightForBase = false;
        public bool normalized = true;

        private PlayableGraph graph;
        private AnimationLayerMixerPlayable mixer;

        public AvatarMask fullBody;
        public AvatarMask upperBody;

        private void Start() 
        {
            graph = PlayableGraph.Create("AnimationLayerMixerTest");
            var playableOutput = AnimationPlayableOutput.Create(graph, "Animation", GetComponent<Animator>());
            var clipPlayable1 = AnimationClipPlayable.Create(graph, clip1);
            var clipPlayable2 = AnimationClipPlayable.Create(graph, clip2);
            mixer = AnimationLayerMixerPlayable.Create(graph, 2);
            graph.Connect(clipPlayable1, 0, mixer, 0);
            graph.Connect(clipPlayable2, 0, mixer, 1);
            playableOutput.SetSourcePlayable(mixer);
            graph.Play();

            mixer.SetLayerMaskFromAvatarMask(0, fullBody);
            mixer.SetLayerMaskFromAvatarMask(1, upperBody);
        }

        private void Update()
        {
            if (weightForBase)
            {
                mixer.SetInputWeight(0, weight);
                if (normalized) mixer.SetInputWeight(1, 1 - weight);
            }
            else
            {
                if (normalized) mixer.SetInputWeight(0, 1 - weight);
                mixer.SetInputWeight(1, weight);
            }
        }

        private void OnDestroy()
        {
            graph.Destroy();
        }
    }
}