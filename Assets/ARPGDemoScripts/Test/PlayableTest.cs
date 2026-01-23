using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.UIElements;

namespace ARPGDemo.Test
{
    [RequireComponent(typeof(Animator))]
    public class PlayableTest : MonoBehaviour
    {
        public ExposedReference<Transform> trans;
        public AnimationClip clip;
        public float time;
        public bool autoRun;
        public PlayableGraph playableGraph;
        AnimationClipPlayable playableClip;
        Playable playable;
        public bool playInUpdate;
        public bool playInLateUpdate;
        public bool debugLateUpate;

        // void Start()
        // {
        //     playableGraph = PlayableGraph.Create();
        //     var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        //     // Wrap the clip in a playable.
        //     playableClip = AnimationClipPlayable.Create(playableGraph, clip);

        //     // Connect the Playable to an output.
        //     playableOutput.SetSourcePlayable(playableClip);

        //     // Plays the Graph.
        //     // playableGraph.Play();

        //     // Stops time from progressing automatically.
        //     playableClip.Pause();
        // }

        void Update()
        {
            // Control the time manually.
            // if (playableGraph.IsValid() && autoRun == false) playableClip.SetTime(time);
            if (playableGraph.IsValid() && autoRun == false) playable.SetTime(time);
            if (playInUpdate && playableGraph.IsValid())
            {
                playInUpdate = false;
                playableGraph.Play();
            }
        }

        void OnDisable()
        {
            // Destroys all Playables and Outputs created by the graph.
            playableGraph.Destroy();
        }

        private void LateUpdate()
        {
            if (debugLateUpate) Debug.Log($"在{Time.frameCount}帧触发了LateUpdate");
            if (playInLateUpdate && playableGraph.IsValid())
            {
                playInLateUpdate = false;
                playableGraph.Play();
            }
        }

        [ContextMenu("创建Graph")]
        public void CreateGraph()
        {
            playableGraph = PlayableGraph.Create();
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

            // Wrap the clip in a playable.
            playableClip = AnimationClipPlayable.Create(playableGraph, clip);

            // Connect the Playable to an output.
            // playableOutput.SetSourcePlayable(playableClip);

            Debug.Log("在" + Time.frameCount + "帧触发了CreateBehaviour");
            // var output = ScriptPlayableOutput.Create(playableGraph, "Behaviour");
            var playable = ScriptPlayable<TestBehaviour>.Create(playableGraph, 1);
            playable.SetOutputCount(3);
            // playableGraph.Connect(playable, 0, playableClip, 0);
            // output.SetSourcePlayable(playable);
            playableOutput.SetSourcePlayable(playable);
            // playableGraph.Connect(playableClip, 0, playable, 0);

            // playableClip.SetLeadTime(1f);
            // playable.SetLeadTime(1f);
            // playable.SetDuration(3f);
            // playableClip.SetDuration(clip.length);
            // playable.SetDuration(5f);

            // CreateBehaviour(_evt);

            // playable.SetPropagateSetTime(true);
            var playable2 = ScriptPlayable<TestBehaviour>.Create(playableGraph, 1);
            playable2.SetOutputCount(1);
            playableGraph.Connect(playable2, 0, playable, 0);
            playableGraph.Connect(playableClip, 0, playable2, 0);

            this.playable = playable;
        }

        public void CreateGraph(ClickEvent _evt)
        {
            playableGraph = PlayableGraph.Create();
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

            // Wrap the clip in a playable.
            playableClip = AnimationClipPlayable.Create(playableGraph, clip);

            // Connect the Playable to an output.
            // playableOutput.SetSourcePlayable(playableClip);

            Debug.Log("在" + Time.frameCount + "帧触发了CreateBehaviour");
            // var output = ScriptPlayableOutput.Create(playableGraph, "Behaviour");
            var playable = ScriptPlayable<TestBehaviour>.Create(playableGraph, 1);
            playable.SetOutputCount(3);
            // playableGraph.Connect(playable, 0, playableClip, 0);
            // output.SetSourcePlayable(playable);
            playableOutput.SetSourcePlayable(playable);
            // playableGraph.Connect(playableClip, 0, playable, 0);

            // playableClip.SetLeadTime(1f);
            // playable.SetLeadTime(1f);
            // playable.SetDuration(3f);
            // playableClip.SetDuration(clip.length);
            // playable.SetDuration(5f);

            // CreateBehaviour(_evt);

            // playable.SetPropagateSetTime(true);
            var playable2 = ScriptPlayable<TestBehaviour>.Create(playableGraph, 1);
            playable2.SetOutputCount(1);
            playableGraph.Connect(playable2, 0, playable, 0);
            playableGraph.Connect(playableClip, 0, playable2, 0);

            this.playable = playable;
        }

        public void CreateBehaviour(ClickEvent _evt)
        {
            Debug.Log("在" + Time.frameCount + "帧触发了CreateBehaviour");
            var output = ScriptPlayableOutput.Create(playableGraph, "Behaviour");
            var playable = ScriptPlayable<TestBehaviour>.Create(playableGraph);
            // playableGraph.Connect(playable, 0, playableClip, 0);
            output.SetSourcePlayable(playable);
        }

        public void PlayClip(ClickEvent _evt)
        {
            Debug.Log("在" + Time.frameCount + "帧触发了PlayClip");
            if (playableClip.IsValid()) playableClip.Play();
        }

        public void PauseClip(ClickEvent _evt)
        {
            Debug.Log("在" + Time.frameCount + "帧触发了PauseClip");
            if (playableClip.IsValid()) playableClip.Pause();
        }

        
        public void PlayGraph(ClickEvent _evt)
        {
            Debug.Log("在" + Time.frameCount + "帧触发了PlayGraph");
            if (playableGraph.IsValid()) playableGraph.Play();
        }
        [ContextMenu("播放Graph")]
        public void PlayGraph()
        {
            Debug.Log("在" + Time.frameCount + "帧触发了PlayGraph");
            if (playableGraph.IsValid()) playableGraph.Play();
        }

        public void StopGraph(ClickEvent _evt)
        {
            Debug.Log("在" + Time.frameCount + "帧触发了StopGraph");
            if (playableGraph.IsValid()) playableGraph.Stop();
        } 

        [ContextMenu("设置为手动")]
        public void SetManual()
        {
            if (playableGraph.IsValid()) playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        [ContextMenu("手动更新")]
        public void ManualUpdate()
        {
            if (playableGraph.IsValid()) playableGraph.Evaluate(Time.deltaTime);
            // if (playableGraph.IsValid()) playableGraph.Evaluate();
        }
    }

    public class TestBehaviour : PlayableBehaviour
    {
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            Debug.Log($"在{Time.frameCount}帧触发了OnBehaviourPause");
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            Debug.Log($"在{Time.frameCount}帧触发了OnBehaviourPlay");
        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            Debug.Log($"在{Time.frameCount}帧触发了OnGraphStart");
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
            Debug.Log($"在{Time.frameCount}帧触发了OnGraphStop");
        }

        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
            Debug.Log($"在{Time.frameCount}帧触发了OnPlayableCreate");
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
            Debug.Log($"在{Time.frameCount}帧触发了OnPlayableDestroy");
        }

        public override void PrepareData(Playable playable, FrameData info)
        {
            base.PrepareData(playable, info);
            Debug.Log($"在{Time.frameCount}帧触发了PrepareData");
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);
            Debug.Log($"在{Time.frameCount}帧触发了PrepareFrame");
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            Debug.Log($"在{Time.frameCount}帧触发了ProcessFrame");
            Animator target = playerData as Animator;
            if (target != null) Debug.Log("playerData是Aniamtor");
            else Debug.Log("playerData不是Aniamtor");
        }
    }
}

/*PauseSubGraphAnimationSample
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PauseSubGraphAnimationSample : MonoBehaviour
{
    public AnimationClip clip0;
    public AnimationClip clip1;

    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.

        playableGraph = PlayableGraph.Create();

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.

        var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);
        mixerPlayable.SetInputWeight(0, 1.0f);
        mixerPlayable.SetInputWeight(1, 1.0f);
        clipPlayable1.Pause();

        // Plays the Graph.
        playableGraph.Play();
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}
*/

/*MultiOutputSample
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class MultiOutputSample : MonoBehaviour
{
    public AnimationClip animationClip;
    public AudioClip audioClip;
    PlayableGraph playableGraph;

    void Start()
    {
        playableGraph = PlayableGraph.Create();

        // Create the outputs.
        var animationOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        var audioOutput = AudioPlayableOutput.Create(playableGraph, "Audio", GetComponent<AudioSource>());

        // Create the playables.
        var animationClipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
        var audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, true);

        // Connect the playables to an output.
        animationOutput.SetSourcePlayable(animationClipPlayable);
        audioOutput.SetSourcePlayable(audioClipPlayable);

        // Plays the Graph.
        playableGraph.Play();
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}
*/

/*RuntimeControllerSample
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class RuntimeControllerSample : MonoBehaviour
{
    public AnimationClip clip;
    public RuntimeAnimatorController controller;
    public float weight;

    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        var ctrlPlayable = AnimatorControllerPlayable.Create(playableGraph, controller);

        playableGraph.Connect(clipPlayable, 0, mixerPlayable, 0);
        playableGraph.Connect(ctrlPlayable, 0, mixerPlayable, 1);

        // Plays the Graph.
        playableGraph.Play();
    }

    void Update()
    {
        weight = Mathf.Clamp01(weight);
        mixerPlayable.SetInputWeight(0, 1.0f - weight);
        mixerPlayable.SetInputWeight(1, weight);
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}
*/

/*MixAnimationSample
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class MixAnimationSample : MonoBehaviour
{

    public AnimationClip clip0;
    public AnimationClip clip1;
    public float weight;
    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

        // Plays the Graph.
        playableGraph.Play();
    }

    void Update()
    {
        weight = Mathf.Clamp01(weight);
        mixerPlayable.SetInputWeight(0, 1.0f - weight);
        mixerPlayable.SetInputWeight(1, weight);
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}
*/

/*PlayAnimationUtilitiesSample
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayAnimationUtilitiesSample : MonoBehaviour
{

    public AnimationClip clip;
    PlayableGraph playableGraph;

    void Start()
    {
        AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out playableGraph);
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}
*/

/*PlayAnimationSample
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayAnimationSample : MonoBehaviour
{
    public AnimationClip clip;
    PlayableGraph playableGraph;

    void Start()
    {
        playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable.
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);

        // Connect the Playable to an output.
        playableOutput.SetSourcePlayable(clipPlayable);

        // Plays the Graph.
        playableGraph.Play();
    }

    void OnDisable()
    {
        // Destroys all Playables and PlayableOutputs created by the graph.
        playableGraph.Destroy();
    }
}
*/