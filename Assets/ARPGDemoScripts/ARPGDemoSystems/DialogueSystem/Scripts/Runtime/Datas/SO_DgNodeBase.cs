using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{
    
    // [CreateAssetMenu(fileName = "DgNodeBase", menuName = "ARPGDemo/DialogueSystem/DgNodeBase", order = -250)]
    public abstract class SO_DgNodeBase : ScriptableObject
    {
        public abstract DgNodeBase GetNode();
    }
}