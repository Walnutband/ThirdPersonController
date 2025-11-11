using RPGCore.Dialogue.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour
{
    public DialogueGroupDataSO currentExecuteDialogueGroup;
    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.D))
        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            DialogueManager.Instance.StartDialogue(currentExecuteDialogueGroup);
        }
    }
}
