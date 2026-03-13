
using System;

namespace MyPlugins.AnimationPlayer
{
    public interface IFadeTarget
    {
        //提供给外部设置自己权重的方法。getter是用于创建时获取权重信息作为开始权重。
        float weight { get; set; }

        //两个回调方法。
        void StartFadeOut();
        void StartFadeIn();
    }

    public interface IUpdatable
    {
        //自己实现，只是预处理器在PrepareFrame中负责把经过的时间（deltaTime）传入。
        bool Update(float _deltaTime); //返回是否结束的信息是关键，因为预处理器需要使用该信息，而只有自己才能获取该信息，所以将其通过返回值传递给预处理器。
        Action onComplete { get; set; }
    }

    public interface IAnimationInfo
    {
        int key {get;} //作为创建的状态的Key。
    }
}