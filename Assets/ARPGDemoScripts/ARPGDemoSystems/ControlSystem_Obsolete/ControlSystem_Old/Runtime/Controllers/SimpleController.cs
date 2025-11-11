using UnityEngine;
using Animancer;
using ARPGDemo.BattleSystem;

namespace ARPGDemo.ControlSystem_Old
{
    public class SimpleController : MonoBehaviour
    {
        public AnimancerComponent animPlayer;
        public AudioSource audioSource;
        public AnimationClip hurtAnim;
        public AudioClip hurtAudio;

        private void Awake()
        {
            animPlayer = GetComponentInChildren<AnimancerComponent>();
            audioSource = GetComponentInChildren<AudioSource>();
        }

        public void Hurt()
        {
            animPlayer.Play(hurtAnim, 0f, FadeMode.FromStart);
            // audioSource.PlayOneShot(hurtAudio);
            AudioManager.Instance.PlaySound(hurtAudio, transform.position);
        }
    }
}