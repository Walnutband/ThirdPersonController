using System;
using UnityEngine;
using UnityEngine.Playables;

public class TransformTweenMixerBehaviour : PlayableBehaviour
{
    //纯粹用来处理没有设置StartLocation对象的情况，只在第一次执行时设置，之后就不需要设置了（而且不应该，因为就是将当前位置和旋转作为开始值，显然后续的当前值是会变化的），所以设置该变量作为一个标记。
    bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if(trackBinding == null)
            return;

        //其实应该叫当前值，currentPosition和currentRotation
        Vector3 defaultPosition = trackBinding.position;
        Quaternion defaultRotation = trackBinding.rotation;

        int inputCount = playable.GetInputCount();

        float positionTotalWeight = 0f;
        float rotationTotalWeight = 0f;

        //混合结果的位置和旋转。
        Vector3 blendedPosition = Vector3.zero;
        Quaternion blendedRotation = new Quaternion(0f, 0f, 0f, 0f);

        for (int i = 0; i < inputCount; i++)
        {
            //首先取出当前片段的Behaviour（每一个Input就是一个Clip）
            ScriptPlayable<TransformTweenBehaviour> playableInput = (ScriptPlayable<TransformTweenBehaviour>)playable.GetInput(i);
            TransformTweenBehaviour input = playableInput.GetBehaviour();

            //Start可以为空，但是End不可以为空
            if(input.endLocation == null)
                continue;

            //获取当前片段的权重，也就是获取当前片段的播放状态。
            float inputWeight = playable.GetInputWeight(i);

            //如果是第一帧（第一次执行该行为），且没有设置StartLocation，那么就以默认值作为开始值，也就是所控制的对象本身的位置和旋转作为开始值，非常符合现实直觉。
            if (!m_FirstFrameHappened && !input.startLocation)
            {
                input.startingPosition = defaultPosition;
                input.startingRotation = defaultRotation;
            }

            //获取当前片段的播放进度，注意中间存在曲线的作用，所以播放进度并不一定是线性的，只是时间推进（作为自变量x）是表现为线性。
            float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
            float tweenProgress = input.EvaluateCurrentCurve(normalisedTime);

            //自由开关，是否插值位置，是否插值旋转。
            if (input.tweenPosition)
            {
                positionTotalWeight += inputWeight;

                blendedPosition += Vector3.Lerp(input.startingPosition, input.endLocation.position, tweenProgress) * inputWeight;
            }

            if (input.tweenRotation)
            {
                rotationTotalWeight += inputWeight;

                Quaternion desiredRotation = Quaternion.Lerp(input.startingRotation, input.endLocation.rotation, tweenProgress);
                desiredRotation = NormalizeQuaternion(desiredRotation);

                //如果点积为负，取反其中一个（保证走最短路径）
                if (Quaternion.Dot (blendedRotation, desiredRotation) < 0f)
                {
                    desiredRotation = ScaleQuaternion (desiredRotation, -1f);
                }

                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);

                blendedRotation = AddQuaternions(blendedRotation, desiredRotation);
            }
        }

        //defaultPosition = defaultPosition * (1f - positionTotalWeight) + blendedPosition;
        //正常情况下，总权重就是1，所以此处不变，而如果遇到Ease In或Ease Out这种情况，总权重就会小于1，
        blendedPosition += defaultPosition * (1f - positionTotalWeight);
        //算法逻辑相同，只是代码指令要多一些，但是我并没有完全理解四元数的运算法则及其意义。
        Quaternion weightedDefaultRotation = ScaleQuaternion(defaultRotation, 1f - rotationTotalWeight);
        blendedRotation = AddQuaternions(blendedRotation, weightedDefaultRotation);

        trackBinding.position = blendedPosition;
        trackBinding.rotation = blendedRotation;
        
        m_FirstFrameHappened = true;
    }

    public override void OnPlayableDestroy (Playable playable)
    {
        //Ques：倒是有点好奇，都已经销毁节点了，还设置内部成员有什么意义呢？
        /*Tip：我草，貌似反应过来了，因为在TrackAsset中会定义PlayableBehaviour的模版，然后创建节点时是将模版传入而非新建，所以该Behaviour实例的Playable销毁后自己仍然存在，
        所以就应该在这个销毁的周期方法中处理善后工作，以便下一次使用。*/
        m_FirstFrameHappened = false;
    }

    static Quaternion AddQuaternions (Quaternion first, Quaternion second)
    {
        first.w += second.w;
        first.x += second.x;
        first.y += second.y;
        first.z += second.z;
        return first;
    }

    //写成一个方法，是因为Quaternion不能直接乘以float，只能乘以Quaternion表示进行一次旋转，而对于每个值的放缩只能四个成员逐一计算，所以集中于此以便复用。
    static Quaternion ScaleQuaternion (Quaternion rotation, float multiplier)
    {
        rotation.w *= multiplier;
        rotation.x *= multiplier;
        rotation.y *= multiplier;
        rotation.z *= multiplier;
        return rotation;
    }

    static float QuaternionMagnitude (Quaternion rotation)
    {
        return Mathf.Sqrt(Quaternion.Dot(rotation, rotation));
    }

    static Quaternion NormalizeQuaternion (Quaternion rotation)
    {
        float magnitude = QuaternionMagnitude (rotation);

        //放缩到自身长度的倒数，就是归一化。
        if (magnitude > 0f)
            return ScaleQuaternion (rotation, 1f / magnitude);

        Debug.LogWarning ("Cannot normalize a quaternion with zero magnitude.");
        return Quaternion.identity;
    }
}