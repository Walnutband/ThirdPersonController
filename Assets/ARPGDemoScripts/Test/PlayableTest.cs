
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace ARPGDemo.Test
{

    [RequireComponent(typeof(Animator))]
    public class PlayWithTimeControlSample : MonoBehaviour
    {
        public AnimationClip clip;
        public float time;
        PlayableGraph playableGraph;
        AnimationClipPlayable playableClip;

        void Start()
        {
            playableGraph = PlayableGraph.Create();
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

            // Wrap the clip in a playable.
            playableClip = AnimationClipPlayable.Create(playableGraph, clip);

            // Connect the Playable to an output.
            playableOutput.SetSourcePlayable(playableClip);

            // Plays the Graph.
            playableGraph.Play();

            // Stops time from progressing automatically.
            playableClip.Pause();
        }

        void Update()
        {
            // Control the time manually.
            playableClip.SetTime(time);
        }

        void OnDisable()
        {
            // Destroys all Playables and Outputs created by the graph.
            playableGraph.Destroy();
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