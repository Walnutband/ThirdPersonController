using UnityEngine;
using UnityEngine.Playables;

namespace MyPlugins.AnimationPlayer
{
    public class Preprocessor : PlayableBehaviour
    {
        private FadeHandler m_FadeHandler;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);

            if (m_FadeHandler != null && m_FadeHandler.Update(info.deltaTime) == true)
            {//过渡结束。
                m_FadeHandler = null;
            }
        }

        public void RegisterFadeHandler(FadeHandler _fadeHandler)
        {
            // Debug.Log("注册过渡处理器");
            //同一时间只能存在一个过渡过程。
            m_FadeHandler?.End(); //直接结束
            m_FadeHandler = _fadeHandler;
            /*BUG：出现的一个问题是，在AnimationLayer中创建Fade之后，调用这里的注册方法，然后会首先调用之前的FadeHandler的End，再设置为新的FadeHandler，但是问题来了，有可能
            之前的FadeHandler可能过渡还没完成，即转出节点的权重还没有变到1，在这个时候就创建了新的FadeHandler实例，显然不合理、因为实际上应该以上一个结束之后的数据，再来创建
            新的FadeHandler实例。所以此处只是一个修正方法，但其实也够用了，甚至可能直接把转出的开始权重设置为1，转入的开始权重设置为0。*/
            m_FadeHandler.RefreshData();
        }

        public void EndFading()
        {
            m_FadeHandler?.End(); //直接结束
            m_FadeHandler = null;
        }
    }

}