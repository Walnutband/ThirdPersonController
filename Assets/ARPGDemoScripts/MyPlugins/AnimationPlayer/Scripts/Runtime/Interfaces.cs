
using System;

namespace MyPlugins.AnimationPlayer
{
    public interface IFadeTarget
    {
        float weight { get; set; }

        void StartFadeOut();
        void StartFadeIn();
    }

    public interface IUpdatable
    {
        bool Update(float _deltaTime);
        Action onComplete { get; set; }
    }
}