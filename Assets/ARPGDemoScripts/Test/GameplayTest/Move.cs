using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.Test.GameplayTest
{
    public class Move : MonoBehaviour
    {
        public float moveSpeed = 1f;
        public float flashDistance = 1f; 

        public InputActionReference moveAction;
        public InputActionReference flashAction;
        public Vector2 moveInput;
        public InputActionReference teleportAction;

        private void Update()
        {
            moveInput = moveAction.ToInputAction().ReadValue<Vector2>();
            transform.Translate(new Vector3(0f, moveInput.y, moveInput.x) * moveSpeed * Time.deltaTime);
        }

        private void OnEnable()
        {
            moveAction.asset.Enable();
            flashAction.action.started += ctx =>
            {
                transform.Translate(new Vector3(0f, moveInput.y, moveInput.x).normalized * flashDistance);
            };

            // teleportAction.action.started += ctx =>
            // {
            //     Vector2 mousePos = Mouse.current.position.ReadValue();
            //     Debug.Log($"鼠标位置：{mousePos}");
            //     transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            // };
        }
    }
}