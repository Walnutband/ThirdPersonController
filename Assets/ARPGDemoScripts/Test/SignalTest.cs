using UnityEngine;

namespace ARPGDemo.Test
{
    public class SignalTest : MonoBehaviour
    {

        private SignalTest()
        {
            Debug.Log("Constructor called");
        }

        void Awake()
        {
            Debug.Log("Awake called");
        }
    }
}