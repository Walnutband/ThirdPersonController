using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.DialogueSystem
{

    [CreateAssetMenu(fileName = "SO_DialogueTreeGroup", menuName = "ARPGDemo/DialogueSystem/SO_DialogueTreeGroup", order = -300)]
    public class SO_DialogueTreeGroup : ScriptableObject
    {
        public List<SO_DialogueTree> dialogueTrees = new List<SO_DialogueTree>();

        public DialogueTreeGroup GetGroup()
        {
            return new DialogueTreeGroup(dialogueTrees.ConvertAll(x => x.GetTree()));
        }
    }
}