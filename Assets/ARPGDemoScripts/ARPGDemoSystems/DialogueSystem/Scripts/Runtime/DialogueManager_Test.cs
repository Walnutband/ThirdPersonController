
using System.Collections.Generic;
using ARPGDemo.UISystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPGDemo.DialogueSystem
{
    [AddComponentMenu("ARPGDemo/DialogueSystem/DialogueManager_Test")]
    public class DialogueManager_Test : SingletonMono<DialogueManager_Test>
    {
        public DialoguePanel m_DialoguePanel;

        public List<string> contents = new List<string>();
        public int index = -1;
        public List<string> choices = new List<string>();

        protected override void RetrieveExistingInstance()
        {
            m_Instance = GameObject.Find("DialogueManager_Test").GetComponent<DialogueManager_Test>();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                MoveNext();
            }
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                AddChoices();
            }
        }

        private void MoveNext()
        {
            if (contents.Count <= 0) return;

            if (index < 0)
            {//开始对话
                index = 0;
                // m_DialoguePanel.gameObject.SetActive(true);
                
                // m_DialoguePanel.Show();
                m_DialoguePanel.SetContent(contents[index]);
            }
            else if (index < contents.Count - 1)
            {//继续对话
                // index++;
                // m_DialoguePanel.SetContent(contents[index]);
                // if (!m_DialoguePanel.SetContent(contents[index]))
                if (m_DialoguePanel.SetContent(contents[index + 1]))
                {//Tip：如果确实成功进行到了下一段对话的话，那么就可以推进index了，否则就不用推进index了。我感觉这比先++再--的逻辑意义更加清晰。
                    index++;
                }
            }
            else
            {//结束对话
                // index = -1;
                /*Tip：其实本来就应该在面板组件中定义方法，而不是像这样直接访问其游戏对象、调用SetActive方法，这是很明显的越权访问以及曲解逻辑意义。*/
                // m_DialoguePanel.gameObject.SetActive(false);

                // if (m_DialoguePanel.Hide())
                // {
                //     index = -1;
                // }
            }
        }

        private void AddChoices()
        {
            // m_DialoguePanel.AddChoices(choices);
        }
    }
}