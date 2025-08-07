using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Samples
{
    // The runtime instance of a Tween track. It is responsible for blending and setting
    // the final data on the transform binding.
    public class TweenMixerBehaviour : PlayableBehaviour
    {
        static AnimationCurve s_DefaultCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        bool m_ShouldInitializeTransform = true;
        Vector3 m_InitialPosition;
        Quaternion m_InitialRotation;

        // Performs blend of position and rotation of all clips connected to a track mixer
        // The result is applied to the track binding's (playerData) transform.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Transform trackBinding = playerData as Transform;

            if (trackBinding == null)
                return;

            // Get the initial position and rotation of the track binding, only when ProcessFrame is first called
            //Ques：如何控制只有第一次调用时才会这样获取初始值呢？
            //Tip：利用m_ShouldInitializeTransform标记，第一次调用之后就永远为false，即不再进入初始化分支了。
            InitializeIfNecessary(trackBinding);

            Vector3 accumPosition = Vector3.zero;
            Quaternion accumRotation = QuaternionUtils.zero;

            float totalPositionWeight = 0.0f;
            float totalRotationWeight = 0.0f;

            // Iterate on all mixer's inputs (ie each clip on the track)
            //该MixerBehaviour绑定在一个ScriptPlayable上，各个ClipPlayable就连接在该ScriptPlayable上，所以由此遍历连接的ClipPlayable
            //（不过其实这里说的ClipPlayable也是ScriptPlayable，只是其Behaviour为TweenBehaviour）
            int inputCount = playable.GetInputCount(); 
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                //正数权重才有意义，并且这里似乎没有对权重进行归一化（由Mixer自己决定如何处理）。关于权重就是这两点问题要考虑：正负问题、归一化问题
                if (inputWeight <= 0)
                    continue;

                Playable input = playable.GetInput(i);
                //归一化时刻，也就是当前走到了全过程的多少。注意每个Clip的当前time都是各自独立的，
                float normalizedInputTime = (float)(input.GetTime() / input.GetDuration()); 

                // get the clip's behaviour and evaluate the progression along the curve
                TweenBehaviour tweenInput = GetTweenBehaviour(input);
                float tweenProgress = GetCurve(tweenInput).Evaluate(normalizedInputTime); //从时刻求曲线值，前提是该曲线的横轴范围就是0~1

                // calculate the position's progression along the curve according to the input's (clip) weight
                if (tweenInput.shouldTweenPosition)
                {
                    totalPositionWeight += inputWeight;
                    accumPosition += TweenPosition(tweenInput, tweenProgress, inputWeight);
                }

                // calculate the rotation's progression along the curve according to the input's (clip) weight
                if (tweenInput.shouldTweenRotation)
                {
                    totalRotationWeight += inputWeight;
                    accumRotation = TweenRotation(tweenInput, accumRotation, tweenProgress, inputWeight);
                }
            }

            // Apply the final position and rotation values in the track binding
            // 一般情况下与初始位置应该无关，所以各个Clip的总权重往往就是1。只要片段结束了，则total就会变为0，那么就会恢复到初始位置，算是一个小特性
            trackBinding.position = accumPosition + m_InitialPosition * (1.0f - totalPositionWeight);
            //这里是利用Quaternion自带的一些处理方法。而且四元数的计算也不是向量那样的线性计算。
            trackBinding.rotation = accumRotation.Blend(m_InitialRotation, 1.0f - totalRotationWeight); 
            trackBinding.rotation.Normalize();
        }

        void InitializeIfNecessary(Transform transform)
        {
            if (m_ShouldInitializeTransform)
            {
                m_InitialPosition = transform.position;
                m_InitialRotation = transform.rotation;
                m_ShouldInitializeTransform = false;
            }
        }

        /*Tip：只要知道，本质上是两点定一线，只是在从起点移动到终点的过程中不是匀速的，而是根据TweenBehaviour的curve变量指定的曲线来运动，
        对于权重的理解，应该是本来只有一个片段，但是它将自己的值根据不同比例分配到了多个分开的Clip中，带权计算只是为了从这些Clip恢复最原本的那一个Clip。*/
        Vector3 TweenPosition(TweenBehaviour tweenInput, float progress, float weight)
        {
            Vector3 startPosition = m_InitialPosition;
            if (tweenInput.startLocation != null)
            {
                startPosition = tweenInput.startLocation.position;
            }

            Vector3 endPosition = m_InitialPosition;
            if (tweenInput.endLocation != null)
            {
                endPosition = tweenInput.endLocation.position;
            }

            return Vector3.Lerp(startPosition, endPosition, progress) * weight;
        }

        Quaternion TweenRotation(TweenBehaviour tweenInput, Quaternion accumRotation, float progress, float weight)
        {
            Quaternion startRotation = m_InitialRotation;
            if (tweenInput.startLocation != null)
            {
                startRotation = tweenInput.startLocation.rotation;
            }

            Quaternion endRotation = m_InitialRotation;
            if (tweenInput.endLocation != null)
            {
                endRotation = tweenInput.endLocation.rotation;
            }

            Quaternion desiredRotation = Quaternion.Lerp(startRotation, endRotation, progress);
            return accumRotation.Blend(desiredRotation.NormalizeSafe(), weight);
        }

        static TweenBehaviour GetTweenBehaviour(Playable playable)
        {
            ScriptPlayable<TweenBehaviour> tweenInput = (ScriptPlayable<TweenBehaviour>)playable;
            return tweenInput.GetBehaviour();
        }

        static AnimationCurve GetCurve(TweenBehaviour tween)
        {
            if (tween == null || tween.curve == null)
                return s_DefaultCurve;
            return tween.curve;
        }
    }
}
