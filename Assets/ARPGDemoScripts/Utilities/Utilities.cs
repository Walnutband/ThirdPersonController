
using UnityEngine;

namespace ARPGDemo
{
    public static class MathUtilities
    {
        public static float FloatLerp(float a, float b, float t)
        {
            float result = Mathf.Lerp(a, b, t);
            // if (Mathf.Approximately(result, b)) result = b;
            // if (Vector2.Equals(result, b)) result = b;
            //Tip：发现用前面两种都不好使。。。
            if (Mathf.Abs(result - b) <= 0.0001f) result = b;

            return result;
        }
    }
}