using RPGCore.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCore.Dialogue.Runtime
{
    [DialogueNode(Path = "Example/Action/Change Dialogue Item")]
    public class DgNodeActionChangeDialogueItem : DgNodeActionBase
    {
        /*Tip：固定的Action方法就是改变当前Group中进行的Item，可以在编辑器中编辑这里ItemName字段的值而设置要切换到哪个Item。
        不过最好是在编辑器中直接列举出当前Group中的Item名称，直接选择即可，无需手动输入。
        */
        public string ItemName;

        public DgNodeActionChangeDialogueItem()
        {
            Name = "Change Dialogue Item";
            SetAction(() =>
            {
                DialogueManager.Instance.ChangeExecutingGroupActiveItem(ItemName);
            });
        }
    }
}
