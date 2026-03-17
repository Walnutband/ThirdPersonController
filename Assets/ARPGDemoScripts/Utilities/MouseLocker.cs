using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.Utilities
{


    [AddComponentMenu("ARPGDemo/Utilities/MouseLocker")]
    public class MouseLocker : SingletonMono<MouseLocker> 
    {
        public bool lockCursor = true; //默认锁定。

        private void Update()
        {
            if (lockCursor == false)
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