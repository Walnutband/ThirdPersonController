using UnityEngine;

namespace ARPGDemo.ControlSystem
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private ControlSystem m_ControlSystem;
        public ControlSystem controlSystem
        {
            get
            {
                return m_ControlSystem;
            }
        }
    }
}