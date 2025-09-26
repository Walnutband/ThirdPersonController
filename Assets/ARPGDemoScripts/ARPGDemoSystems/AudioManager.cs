
using UnityEngine;

namespace ARPGDemo.BattleSystem
{
    [AddComponentMenu("ARPGDemo/系统与管理器/AudioManager")]
    public class AudioManager : SingletonMono<AudioManager>
    {
        protected override void Awake()
        {
            m_Instance = GameObject.Find("AudioManager").GetComponent<AudioManager>();
            base.Awake();
        }

        /*注意参数默认值必须是编译时常量，而Vector3.zero在运行时才会确定，即不符合要求，可以使用default对于Vector3来说就是为zero，而更好的做法可能就是重载、而不是用参数默认值。  */
        // public void PlaySound(AudioClip clip, Vector3 position = default)
        // public void PlaySound(AudioClip clip, Vector3 position = Vector3.zero)
        public void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Vector3.zero);
            }
        }

    }
}