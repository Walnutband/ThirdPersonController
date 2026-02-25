using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.ControlSystem
{
    public class InputController : MonoBehaviour 
    {
        [SerializeField] private InputActionAsset m_IAA;

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        public void EnableInput()
        {
            m_IAA.Enable();
        }
        public void DisableInput()
        {
            m_IAA.Disable();
        }
    }
}