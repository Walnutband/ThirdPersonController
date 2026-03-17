
using System;
using System.Collections.Generic;
using System.Linq;
using ARPGDemo.AbilitySystem;
using ARPGDemo.BattleSystem;
using ARPGDemo.ControlSystem;
using ARPGDemo.Utilities;
using DG.Tweening;
using MyPlugins.GoodUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARPGDemo.UISystem_Test
{
    public class UIAttributePanel : UIPanelBase
    {
        // [HideInInspector] public List<AttributeUI> uis = new List<AttributeUI>();
        public AttributeInfo[] infos;
        // public Text[] texts;
        public Transform AttributeGroup;
        public RectTransform rect_Content;
        public SimpleButton sbutton_Return;
        public CanvasGroup canvasGroup;

        private Vector2 position;
        private float offset;

        protected override void Awake()
        {
            base.Awake();
            position = rect_Content.anchoredPosition;
            offset = rect_Content.rect.width / 6; //取比例。
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            // Get(); //获取属性信息，更新面板。
            // Time.timeScale = 0f;
            MouseLocker.Instance.lockCursor = false;
            UIManager.Instance.GetComponent<InputController>().iaa.FindActionMap("Player").Disable();
        }

        protected override void OnClose()
        {
            // Time.timeScale = 1f;
            MouseLocker.Instance.lockCursor = true;
            UIManager.Instance.GetComponent<InputController>().iaa.FindActionMap("Player").Enable();
        }

        protected override void OpenAnim()
        {
            rect_Content.anchoredPosition = position + new Vector2(offset, 0f); //取比例，而不是绝对数值，可能更合适。
            rect_Content.DOAnchorPos(position, 0.2f).SetEase(Ease.OutSine);
            canvasGroup.DOFade(1f, 0.2f);
        }

        protected override void CloseAnim(TweenCallback _hide)
        {
            rect_Content.DOAnchorPos(position - new Vector2(offset, 0f), 0.2f).SetEase(Ease.OutSine);
            canvasGroup.DOFade(0f, 0.1f).onComplete += _hide; 
        }

        //显示属性信息，以及普通攻击的信息。        
        private void Get()
        {
            // AbilitySystemComponent player = GameManager.Instance.Player;
            //获取属性信息，获取普通攻击信息。
            // ActorAttributeSet playerAS =player.actorAS;
            //生命值上限
            // texts[0].text = playerAS.GetAttributeCurrentValue(ActorAttributeSet.AttributeType.HPMax).;

            for (int i = 0; i < infos.Count(); i++)
            {
                AttributeGroup.GetChild(i).FindRecursively("Label").GetComponent<TextMeshProUGUI>().text = infos[i].header;
                // AttributeGroup.GetChild(i).FindRecursively("Content").GetComponent<Text>().text = infos[i].description;
                AttributeGroup.GetChild(i).Find("Content").GetComponent<Text>().text = infos[i].description;

            }
            
        }

        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            // sbutton_Return.AddListener(Close);
            sbutton_Return.AddListener(UIManager.Instance.ClosePanel);
        }

        protected override void UnregisterCallbacks()
        {
            base.UnregisterCallbacks();
            sbutton_Return.RemoveListener(UIManager.Instance.ClosePanel);
        }

        [ContextMenu("获取UI")]
        public void GetUI()
        {
            // AttributeGroup.GetChild(i).FindRecursively("Label").GetComponent<TextMeshProUGUI>().text = infos[i].header;
            // // AttributeGroup.GetChild(i).FindRecursively("Content").GetComponent<Text>().text = infos[i].description;
            // AttributeGroup.GetChild(i).Find("Content").GetComponent<Text>().text = infos[i].description;
            Get();
        }
    }

    //Tip：测试就该直接填内容，不用考虑动态获取。
    [Serializable]
    public struct AttributeInfo
    {
        public string header;
        public string description;
    }

    [Serializable]
    public struct AttributeUI
    {
        public TextMeshProUGUI header;
        public Text content;
    }
}