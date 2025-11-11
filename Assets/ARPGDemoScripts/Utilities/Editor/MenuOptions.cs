
using UnityEditor;
using UnityEngine;

namespace ARPGDemo.Utilities.EditorSection
{
    public static class MenuOptions
    {
        [MenuItem("GameObject/ARPGDemo/Utilities/MouseLocker", false, -400)]
        public static void CreateMouseLocker()
        {
            new GameObject("MouseLocker").AddComponent<MouseLocker>();
        }
    }
}