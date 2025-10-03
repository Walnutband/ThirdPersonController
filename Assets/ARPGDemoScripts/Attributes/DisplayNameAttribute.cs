
using UnityEngine;

namespace ARPGDemo
{
    public class DisplayNameAttribute : PropertyAttribute
    {
        public string Name { get; private set; }
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}