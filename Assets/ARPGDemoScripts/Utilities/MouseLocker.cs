using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.Utilities
{


    [AddComponentMenu("ARPGDemo/Utilities/MouseLocker")]
    public class MouseLocker : MonoBehaviour 
    {
        public bool dontLock;

        private void Update()
        {
            if (dontLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            if (Keyboard.current.altKey.isPressed)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}