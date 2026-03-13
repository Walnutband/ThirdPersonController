
using System;
using UnityEngine;

namespace ARPGDemo.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangedNumericAttribute : PropertyAttribute
    {
        public RangedNumericAttribute()
        {
            
        }
    }
}