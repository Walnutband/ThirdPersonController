using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.Test
{
    [AddComponentMenu("ARPGDemo/Test/Simple Player Controller")]
    public class SimplePlayerController : MonoBehaviour 
    {
        public InputActionAsset actions;
        public InputActionReference move;
        public float moveSpeed;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            Vector2 moveInput = move.action.ReadValue<Vector2>();
            var forward = mainCamera.transform.forward;
            forward.y = 0f;
            Vector3 moveDir = (forward * moveInput.y + mainCamera.transform.right * moveInput.x).normalized;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);
        }

        private void OnEnable()
        {
            actions.actionMaps[0].Enable();
        }

        private void OnDisable()
        {
            actions.actionMaps[0].Disable();
        }
    }
}