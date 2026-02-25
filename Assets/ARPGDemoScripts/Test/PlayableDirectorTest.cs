using UnityEngine;
using UnityEngine.Playables;

namespace ARPGDemo.Test
{
    
    public class PlayableDirectorTest : MonoBehaviour 
    {
        public bool fixedUpdate;
        public bool update;
        private PlayableDirector m_Director;
        
        private void Awake()
        {
            m_Director = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            // m_Director.timeUpdateMode = DirectorUpdateMode.Manual;
        }


        private void FixedUpdate()
        {
            Debug.Log($"在{Time.frameCount}帧触发了FixedUpdate，位置：{transform.position}");
            if (fixedUpdate)
            {
                m_Director.time += Time.fixedDeltaTime;
                m_Director.Evaluate();
            }
        }

        private void Update()
        {
            Debug.Log($"在{Time.frameCount}帧触发了Update，位置：{transform.position}");
            if (update)
            {
                m_Director.time += Time.deltaTime;
                m_Director.Evaluate();
            }
        }

        private void LateUpdate()
        {
            Debug.Log($"在{Time.frameCount}帧触发了LateUpdate，位置：{transform.position}");
        }
    }
}