using UnityEngine;

namespace ARPGDemo.Test.GameplayTest
{
    public class AutoMove : MonoBehaviour
    {
        public Vector3 moveDir = new Vector3(0f, 1f, 0f);
        public float moveSpeed = 1f;
        public bool isFollowing;

        private void FixedUpdate()
        {
            if (isFollowing)
            {
                transform.Translate(moveDir * Time.fixedDeltaTime);
            }

        }
    }
}