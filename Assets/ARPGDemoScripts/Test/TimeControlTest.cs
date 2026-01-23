
using UnityEngine;
using UnityEngine.Timeline;

namespace ARPGDemo.Test
{
    public class TimeControlTest : MonoBehaviour, ITimeControl
    {
        public void SetTime(double time)
        {
            
        }

        public void OnControlTimeStart()
        {
            Debug.Log("OnControlTimeStart");
        }

        public void OnControlTimeStop()
        {
            Debug.Log("OnControlTimeStop");
        }
    }
}