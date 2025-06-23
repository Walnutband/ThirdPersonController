using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] footSteps;
    public AudioClip[] jumpEfforts;
    public AudioClip[] landing;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayFootStep()
    {
        int i = Random.Range(0, footSteps.Length);
        source.PlayOneShot(footSteps[i]);
    }

    public void PlayJumpEffort()
    {
        int i = Random.Range(0, jumpEfforts.Length);
        source.PlayOneShot(jumpEfforts[i]);
    }

    public void PlayLanding()
    {
        int i = Random.Range(0, landing.Length);
        source.PlayOneShot(landing[i]);
    }

}
