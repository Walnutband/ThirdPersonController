using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace MyPlugins.AnimationPlayer
{

    /*Tip：改名为了“Animator代理”，因为终究要落实到Animator，这里就是替代了AnimatorContorller的职责。
    但是命名空间还是保持AnimationPlayer，因为作用本质上就是播放动画。*/
    // public class AnimationPlayer : MonoBehaviour
    [AddComponentMenu("ARPGDemo/MyPlugins/AnimatorAgent")]
    public class AnimatorAgent : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator;

        [SerializeField] private AnimatorAgentSettings m_Settings;

        /*TODO：甚至可以在这里留一个序列化字段，直接在检视器中指定有哪些层级，初始化时就直接创建这么多层级就行了，甚至还可以指定层级的名称，这样在个体类中访问时可以直接按照名称来访问，
        更具有可读性，但是实际上来说，要么是一层要么就是两层分为上半身和下半身，这才是通常情况。*/

        private AnimationGraph m_Graph;
        public AnimationGraph graph
        {
            get
            {
                if (m_Graph == null)
                {
                    m_Graph = new AnimationGraph(m_Animator);
                }
                return m_Graph;
            }
        }


        private void Awake()
        {
            //默认该组件就是与Animator放在同一个游戏对象上。
            if (m_Animator == null) m_Animator = GetComponent<Animator>();

            if (m_Settings != null)
            {
                ApplySettings(m_Settings);
            }
        }

        private void Start()
        {
            // if (m_Settings != null)
            // {
            //     ApplySettings(m_Settings);
            // }
        }

        private void OnEnable()
        {
            graph.Play();
        }

        private void OnDisable()
        {
            graph.Stop();
        }

        private void OnDestroy()
        {
            //有讲究，graph会自动创建实例，而m_Graph不会
            m_Graph?.Destroy();
        }

        //应用设置，虽然设置为公开，但是应当只在初始化时才会调用一次。
        public void ApplySettings(AnimatorAgentSettings _settings)
        {
            if (_settings == null) return;

            if (_settings.layerCount > 0)
            {
                graph.SetLayerCount(_settings.layerCount);
            }
            List<AnimatorAgentSettings.LayerMask> masks = _settings.layerMasks;
            if (masks != null && masks.Count > 0)
            {
                for (int i = 0; i < masks.Count; i++)
                {
                    graph.SetLayerMask((uint)i, masks[i].mask);
                }
            }
            List<AnimatorAgentSettings.LayerBlend> blends = _settings.layerBlends;
            if (blends != null && blends.Count > 0)
            {
                for (int i = 0; i < blends.Count; i++)
                {
                    graph.SetLayerAdditive((uint)i, blends[i].additive);
                }
            }
        }

        public AnimationClipState Play(AnimationClip _clip, PlayOption option = PlayOption.FromStart)
        {
            return Play(0, _clip, option);
        }

        /*Tip：参数默认值有一些限制，比如必须放在非默认参数之后，必须是编译时常量等等，这个时候就应该改为使用重载了。*/
        public AnimationClipState Play(int _layerIndex, AnimationClip _clip, PlayOption option = PlayOption.FromStart)
        {
            if (_layerIndex < 0)
            {
                Debug.LogError("指定播放的层级索引应当大于等于0，在此修正索引为0");
                _layerIndex = 0; 
            }
            return graph.Play(_layerIndex, _clip, option); 
        }

        public AnimationClipState Play(FadeAnimation _fade)
        {
            return Play(0, _fade);
        }

        public AnimationClipState Play(int _layerIndex, FadeAnimation _fade)
        {
            return graph.Play(_layerIndex, _fade);
        }

        public AnimationMixerState Play(MixerAnimation _mixer)
        {
            return graph.Play(0, _mixer);
        }

        public AnimationMixerState Play(int _layerIndex, MixerAnimation _mixer)
        {
            return graph.Play(_layerIndex, _mixer);
        }

        //对于层级的创建无需外部调用，外部只需要指定层级

        public void SetLayerMask(uint _index, AvatarMask _mask)
        {
            graph.SetLayerMask(_index, _mask);
        }

        public void SetLayerAdditive(uint _index, bool _additive)
        {
            graph.SetLayerAdditive(_index, _additive);
        }
    }
}