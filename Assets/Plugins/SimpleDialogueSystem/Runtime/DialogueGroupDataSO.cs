using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGCore.Dialogue.Runtime
{
    public class DialogueGroupDataSO : ScriptableObject
    {
        public string Description;
#if UNITY_EDITOR
        private DialogueItemDataSO currentOpenedItemInEditor = null;
#endif
        private DialogueItemDataSO currentActiveItem = null;
        public List<DialogueItemDataSO> dialogueItems = new List<DialogueItemDataSO>();

#if UNITY_EDITOR
        public DialogueItemDataSO GetOpenedEditorItem()
        {
            if (currentOpenedItemInEditor == null)
            {
                if (dialogueItems.Count() > 0)
                {
                    currentOpenedItemInEditor = dialogueItems[0];
                }
            }
            return currentOpenedItemInEditor;
        }
        public void SetOpenedEditorItem(string itemName)
        {
            currentOpenedItemInEditor = dialogueItems.Find(item => item.name == itemName) ?? currentOpenedItemInEditor;
        }
#endif
        public DialogueItemDataSO GetActiveItem()
        {
            if (currentActiveItem == null)
            {
                if (dialogueItems.Count() > 0)
                {//一般对于DialogueItem的命名就是DialogueGroup的名字加上“-”然后就是该Item的名字
                    currentActiveItem = dialogueItems.Find(item => item.name.Split("-").Last() == "default");
                }
            }
            return currentActiveItem;
        }
        public bool SetActiveItem(string itemName)
        {//就是处理两种情况，一个是全名，另一个就是“-”后面的Item的名字
            currentActiveItem = dialogueItems.Find(item => item.name == itemName);
            if (currentActiveItem == null)
            {
                currentActiveItem = dialogueItems.Find(item => item.name.Split("-").Last() == itemName);
            }
            return currentActiveItem != null;
        }
    }
}
