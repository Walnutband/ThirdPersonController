
using UnityEngine;

namespace ARPGDemo.UISystem
{
    [AddComponentMenu("ARPGDemo/UISystem_New/UIManager")]
    public class UIManager : SingletonMono<UIManager>
    {
        protected override void RetrieveExistingInstance()
        {
            m_Instance = GameObject.Find("UIManager").GetComponent<UIManager>();
        }

        
    }
}