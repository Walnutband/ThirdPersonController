
using System;
using UnityEngine;

namespace ARPGDemo
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangedNumericAttribute : PropertyAttribute
    {
        public RangedNumericAttribute()
        {
            
        }
    }
}