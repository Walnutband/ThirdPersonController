using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ARPGDemo.Test
{
    
    public class AnimationJobTest : MonoBehaviour 
    {
        public AnimationClip clip1;
        public AnimationClip cilp2;
        [Range(0,1)]
        public float weight = 1f;
        public Vector3 velocity;
        public bool processInputs;
        public bool copy;
        AnimationScriptPlayable sp;
        AnimationMixerPlayable mixer;
        PlayableGraph graph;

        private void Awake()
        {
            graph = PlayableGraph.Create("AnimationJobTest");
            var playableOutput = AnimationPlayableOutput.Create(graph, "Animation", GetComponent<Animator>());
            var clipPlayable1 = AnimationClipPlayable.Create(graph, clip1);
            var clipPlayable2 = AnimationClipPlayable.Create(graph, cilp2);
            // var root = GetComponent<Animator>().BindStreamTransform(transform);
            // var job = new Job() {id = 001, rootHandle = root};
            // sp = AnimationScriptPlayable.Create(graph, job, 2);
            mixer = AnimationMixerPlayable.Create(graph, 2);
            graph.Connect(clipPlayable1, 0, mixer, 0);
            graph.Connect(clipPlayable2, 0, mixer, 1);

            // var sp2 = AnimationScriptPlayable.Create(graph, new Job(){id = 002}, 1);
            // playableOutput.SetSourcePlayable(sp2);
            playableOutput.SetSourcePlayable(mixer);
            // graph.Connect(sp, 0, sp2, 0);
            // playableOutput.SetSourcePlayable(sp);
            graph.Play();

            // job.rootHandle = root;

        }

        // private void Awake()
        // {
        //     var root = GetComponent<Animator>().BindStreamTransform(transform);
        //     job.rootHandle = root;
        // }

        private void Update()
        {
            mixer.SetInputWeight(0, weight);
            mixer.SetInputWeight(1, 1 - weight);
            // if (processInputs != sp.GetProcessInputs()) sp.SetProcessInputs(processInputs);
            // Job job = sp.GetJobData<Job>();
            // job.velocity = velocity;
            // job.copy = copy;
            // sp.SetJobData(job);
        }

        private void OnDestroy()
        {
            graph.Destroy();
        }

        [ContextMenu("DebugClipInfo")]
        private void DebugClipInfo()
        {
            if (clip1 == null) return;

            Debug.Log($"clip1.hasGenericRootTransform为{clip1.hasGenericRootTransform}\nclip1.hasMotionCurves为{clip1.hasMotionCurves}\nclip1.hasRootCurves为{clip1.hasRootCurves}");
        }

        public struct Job : IAnimationJob
        {
            public TransformStreamHandle rootHandle;

            public Vector3 velocity;
            public bool copy;
            public int id;

            public float weight;

            private Vector3 lastPos;

            public void ProcessAnimation(AnimationStream stream)
            {

                // Debug.Log($"ID: {id}   ProcessAnimation");
                // AnimationStream stream1 = stream.GetInputStream(1);
                // Vector3 curPos = rootHandle.GetPosition(stream1);
                // Vector3 deltaPos = curPos - lastPos;
                // lastPos = curPos;
                // Debug.Log($"ProcessAnimation\n当前根位置：{curPos}， 计算速度：{deltaPos.magnitude/stream1.deltaTime}，实际速度：{stream1.velocity}" + 
                // $"\nrootMotionPosition: {stream1.rootMotionPosition}\nrootMotionRotation: {stream1.rootMotionRotation}");
                // Debug.Log($"deltaTime: {stream.deltaTime}\ninputStreamCount: {stream.inputStreamCount}\nisHumanStream: {stream.isHumanStream}\n" 
                // + $"velocity: {stream.velocity}\nrootMotionPosition: {stream.rootMotionPosition}\nrootMotionRotation: {stream.rootMotionRotation}");

            }

            public void ProcessRootMotion(AnimationStream stream)
            {

                // Debug.Log($"ID: {id}   ProcessRootMotion");
                // if (copy) stream.CopyAnimationStreamMotion(stream.GetInputStream(0));

                // AnimationStream stream1 = stream.GetInputStream(1);
                // Vector3 curPos = rootHandle.GetPosition(stream1);
                // Vector3 deltaPos = curPos - lastPos;
                // lastPos = curPos;
                // Debug.Log($"ProcessRootMotion\n当前根位置：{curPos}， 计算速度：{deltaPos.magnitude / stream1.deltaTime}，实际速度：{stream1.velocity}" +
                // $"\nrootMotionPosition: {stream1.rootMotionPosition}\nrootMotionRotation: {stream1.rootMotionRotation}");

                // stream.velocity = velocity;
                // stream.velocity = stream.GetInputStream(0).velocity * stream.GetInputWeight(0) + stream.GetInputStream(1).velocity * stream.GetInputWeight(1);
                // stream.velocity = ;

                // Debug.Log($"deltaTime: {stream.deltaTime}\ninputStreamCount: {stream.inputStreamCount}\nisHumanStream: {stream.isHumanStream}\n"
                // + $"velocity: {stream.velocity}\nrootMotionPosition: {stream.rootMotionPosition}\nrootMotionRotation: {stream.rootMotionRotation}");
            }
        }
    }

}