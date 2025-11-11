using RPGCore.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//Tip：对话面板似乎确实就只需要这样一个UI对象。推进对话流程就是给该UI对象填充数据。
public class DialoguePanel : BasePanel
{
	public TMP_Text sentenceContent;
	public TMP_Text speakerContent;
	public GameObject choicesContainer;
	public GameObject choiceItem;
	public Action<int> OnChoiceSelected;
	public Action OnMoveNext;
	private int choiceCount = 0;

	public override void Init()
	{
		OnMoveNext?.Invoke();
	}

	public void ShowChoices()
	{
		choicesContainer.SetActive(true);
	}
	/*Tip：Choices具有特殊逻辑是因为它在UI面板上有单独的UI元素，除此之外就是承载对话文本内容的UI元素。
	这里隐藏选项意思就是此时没有分支选项，那么就将其UI元素禁用，同时清空选项，不过更好的方式应该是复用这些选项的UI对象，因为只需要改变Text文本和Button的点击回调即可。
	*/
	public void HideChoices()
	{
		choicesContainer.SetActive(false);
		for (int i = 0; i < choicesContainer.transform.childCount; i++)
		{
			Destroy(choicesContainer.transform.GetChild(i).gameObject);
		}
		choiceCount = 0;
	}

	public void AddChoiceItem(string content)
	{
		GameObject ci = GameObject.Instantiate(choiceItem);
		ci.GetComponent<Button>().onClick.AddListener(() => { OnChoiceSelected?.Invoke(ci.GetComponent<RPGCore.ChoiceItem>().Id); });
		ci.GetComponent<TMP_Text>().text = content;
		ci.GetComponent<RPGCore.ChoiceItem>().Id = choiceCount;
		ci.transform.SetParent(choicesContainer.transform);
		choiceCount++;
	}

	public void SetSentenceContent(string content)
	{
		sentenceContent.text = content;
	}

	protected override void Update()
	{
		base.Update();
		// if (Input.GetKeyDown(KeyCode.Space))
		if (Keyboard.current.spaceKey.wasPressedThisFrame)
		{
			OnMoveNext?.Invoke();
		}
	}
}
