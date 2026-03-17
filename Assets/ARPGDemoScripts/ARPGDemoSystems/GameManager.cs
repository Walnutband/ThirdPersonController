
using ARPGDemo.AbilitySystem;
using ARPGDemo.ControlSystem;
using UnityEngine;

namespace ARPGDemo.UISystem_Test
{
    public class GameManager : SingletonMono<GameManager>
    {
        public AbilitySystemComponent Player;

        protected override void RetrieveExistingInstance()
        {
            base.RetrieveExistingInstance();
            m_Instance = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }
}