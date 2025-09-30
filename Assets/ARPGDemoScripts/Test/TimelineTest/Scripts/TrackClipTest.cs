
using System.Collections.Generic;
using System.Drawing;

namespace ARPGDemo.Test.Timeline
{
    public abstract class Track
    {
        public string name;
        public float height;
        public List<Clip> clips = new List<Clip>();
    }

    public class AnimationTrack : Track
    {
        public AnimationTrack()
        {
            name = "Animation";
        }
    }

    public class AudioTrack : Track
    {
        public AudioTrack()
        {
            name = "Audio";
        }
    }

    public class EventTrack : Track
    {
        public EventTrack()
        {
            name = "Event";
        }
    }

    public class CinemachineTrack : Track
    {
        public CinemachineTrack()
        {
            name = "Cinemachine";
        }
    }

    public class ParticleTrack : Track
    {
        public ParticleTrack()
        {
            name = "Particle";
        }
    }

    public class HitboxTrack : Track
    {
        public HitboxTrack()
        {
            name = "Hitbox";
        }
    }

    public class Clip
    {
        public double beginTime;
        public double duration;
        public Color color;
    }
}