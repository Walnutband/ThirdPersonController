using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ARPGDemo.Test.GameplayTest
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public float moveSpeed;
        public InputActionReference followAction;
        public float progress;
        public Vector3 startPos;
        // public InputActionReference teleportAction;

        public bool isFollowing;
        public ParticleSystem particle;

        private void OnTriggerEnter(Collider other)
        {
            particle.transform.position = transform.position;
            particle.Play();
        }

        private void Update()
        {
            // if (isFollowing && target != null && Vector3.Distance(transform.position, target.position) >= 0.0001f)
            if (isFollowing && target != null)
            {
                if (Vector3.Distance(transform.position, target.position) <= 0.0001f)
                {
                    isFollowing = false;
                    transform.position = target.position;
                    return;
                }

                // progress = Mathf.Clamp01(progress + moveSpeed * Time.deltaTime);
                // transform.position = Vector3.Lerp(startPos, target.position, progress);

                Vector3 dir = target.position - transform.position;
                transform.Translate(dir.normalized * moveSpeed * Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            if (followAction == null) return;

            // inputAction.action.Enable();
            followAction.action.started += ctx =>
            {
                isFollowing = true;
                startPos = transform.position;
                progress = 0f;
            };
            followAction.action.canceled += ctx => isFollowing = false;

            // teleportAction.action.started += ctx =>
            // {
            //     Vector2 mousePos = Mouse.current.position.ReadValue();
            //     Debug.Log($"鼠标位置：{mousePos}");
            //     transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            // };
        }
    }
}