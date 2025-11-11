using RPGCore.Dialogue.Runtime;
using RPGCore.UI;
public class DialogueManager : DialogueManagerTemplate<DialogueManager>
{
	public DialoguePanel dialoguePanel;
	public override void StartDialogue(DialogueGroupDataSO groupData)
	{
		base.StartDialogue(groupData);
		dialoguePanel = UIManager.Instance.ShowPanel<DialoguePanel>();
		dialoguePanel.OnMoveNext = () => MoveNext(null);
		dialoguePanel.OnChoiceSelected = param => { MoveNext(param); };
		MoveNext(null);
	}

	public override void StopDialogue()
	{
		base.StopDialogue();
		UIManager.Instance.HidePanel<DialoguePanel>();
		dialoguePanel = null;
	}

	/*TODO：使用switch-case将所有类型的处理逻辑都穷举出来了。
	不过感觉还是用多个重载方法、参数分别为各个节点类型（DgNodeType）更好，这样其实能够保留原始信息，而不是作为参数传入进来之后丢失其实际类型的信息、然后再根据其type成员来获取实际类型信息而转换。
	*/
	public override void ProcessDialogueNode(IDgNode currentNode)
	{
		DgNodeType nodeType = currentNode.Type;
		//如果上一个处理的节点是选择节点则将选择面板隐藏
		if (previousDialogueNode.Type == DgNodeType.Choice)
		{
			dialoguePanel.HideChoices();
		}
		switch (nodeType)
		{
			case DgNodeType.Start:
				break;
			case DgNodeType.End:
				StopDialogue();
				break;
			case DgNodeType.Sentence:
				DgNodeSentence sentence = currentNode.Get<DgNodeSentence>();
				dialoguePanel.sentenceContent.text = sentence.Content;
				///ATTENTION:仅测试用
				dialoguePanel.speakerContent.text = sentence.speaker.ToString();
				break;
			case DgNodeType.Choice:
				//
				DgNodeChoice choices = currentNode.Get<DgNodeChoice>();
				foreach (var choice in choices.Choices)
				{
					dialoguePanel.AddChoiceItem(choice);
				}
				dialoguePanel.ShowChoices();
				break;
			case DgNodeType.Random://执行到Random节点后立即再次执行
				MoveNext(null); //Random节点有自己重写的GetNext逻辑，总之管理器就是调度、而各个节点各自执行自己的逻辑。
				break;
			case DgNodeType.Action://执行到Action节点后立即再次执行
				currentNode.Get<DgNodeActionBase>().OnAction();
				MoveNext(null);
				break;
			case DgNodeType.Flow://执行到Flow节点后立即再次执行
				MoveNext(null);
				break;
		}
	}
}
