using System;

namespace UnityEditor.Timeline
{
    //播放范围，就是起点时间和终点时间。
    [Serializable]
    struct PlayRange : IEquatable<PlayRange>
    {
        public bool Equals(PlayRange other)
        {
            return other != null && start.Equals(other.start) && end.Equals(other.end);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayRange other && Equals(other);
        }

        public static bool operator ==(PlayRange left, PlayRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayRange left, PlayRange right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (start.GetHashCode() * 397) ^ end.GetHashCode();
            }
        }

        public PlayRange(double a, double b)
        {
            start = a;
            end = b;
        }

        public double start;
        public double end;
    }
}
