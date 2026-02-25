
using System.Collections.Generic;
using MyPlugins.AnimationPlayer.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyPlugins.AnimationPlayer.Utility
{
    [RequireComponent(typeof(AnimatorAgent))]
    [AddComponentMenu("ARPGDemo/MyPlugins/AnimationPlayer/SimpleAnimPlayer")]
    public class SimpleAnimPlayer : MonoBehaviour
    {
        private AnimatorAgent player;
        public List<FadeAnimation> anims;
        public bool playOnAwake;
        private int curAnimIndex;

        private void Awake()
        {
            player = GetComponent<AnimatorAgent>();
            // if (playOnAwake)
            // {
            //     PlayCurrentAnim();
            // }
        }
        private void Start()
        {
            if (playOnAwake)
            {
                PlayCurrentAnim();
            }
        }

        private void PlayCurrentAnim()
        {
            if (curAnimIndex >= 0 && curAnimIndex <= anims.Count - 1 && anims[curAnimIndex].clip != null)
            {
                player.Play(anims[curAnimIndex]);
            }
        }

        public void PlayNextAnim()
        {
            curAnimIndex++; //curAnimIndex = curAnimIndex / anims.Count
            if (curAnimIndex >= anims.Count)
            {
                curAnimIndex = 0;
            }
            PlayCurrentAnim();

        }
        public void PlayLastAnim()
        {
            curAnimIndex--;
            if (curAnimIndex < 0)
            {
                curAnimIndex = Mathf.Max(anims.Count - 1, 0);
            }
            PlayCurrentAnim();
        }

    }
}

#if UNITY_EDITOR
namespace MyPlugins.AnimationPlayer.EditorSection
{
    [CustomEditor(typeof(SimpleAnimPlayer))]
    public class SAPEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SimpleAnimPlayer tar = target as SimpleAnimPlayer;
            Button lastBtn = new Button(tar.PlayLastAnim) { text = "上一个动画" };
            Button nextBtn = new Button(tar.PlayNextAnim) { text = "下一个动画" };
            root.Add(lastBtn);
            root.Add(nextBtn);
            var p = serializedObject.GetIterator();
            p.NextVisible(true);
            while (p.NextVisible(false))
            {
                var pf = new PropertyField(p);
                if (p.name == "m_Script")
                {
                    pf.SetEnabled(false);
                }
                root.Add(pf);
            }
            // do
            // {
            //     root.Add(new PropertyField(p));
            // }while (p.NextVisible(true));

            return root;
        }
    }    
}
#endif