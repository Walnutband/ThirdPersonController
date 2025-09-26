using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conceptions", menuName = "GoodUI/Conceptions", order = 0)]
public class Conceptions : ScriptableObject
{
    public struct Conception
    {
        public string Name;
        public string Description;
    }

    public List<Conception> conceptions = new List<Conception>();
}
