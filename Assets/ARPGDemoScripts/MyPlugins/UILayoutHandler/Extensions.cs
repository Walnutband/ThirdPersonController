
using UnityEngine;

namespace MyPlugins.UILayout
{
    public static class ExtensionMethods
    {
        public static Vector3 Multiply(this Vector3 left, Vector3 right)
        {
            return new Vector3(left.x * right.x, left.y * right.y, left.z * right.z);
        }
    }
}
