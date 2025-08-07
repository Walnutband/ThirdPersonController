using System;
using UnityEngine;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// Attribute used to specify the color of the track and its clips inside the Timeline Editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TrackColorAttribute : Attribute
    {
        /*Tip: 这里必须要有m_Color和color的原因在于，color是c#中的属性，本质上是方法而不是变量，所以不能存储数据，只有字段m_Color才可以存储数据，但是为了
        防止在构造了实例之后再对m_Color进行修改，所以就设置一个属性color限制只能读取而不是写入，这样一来m_Color就是私有的，而color是公开的。*/
        Color m_Color;

        /// <summary>
        ///
        /// </summary>
        public Color color
        {
            get { return m_Color; }
        }

        /// <summary>
        /// Specify the track color using [0-1] R,G,B values.
        /// </summary>
        /// <param name="r">Red value [0-1].</param>
        /// <param name="g">Green value [0-1].</param>
        /// <param name="b">Blue value [0-1].</param>
        public TrackColorAttribute(float r, float g, float b)
        {
            m_Color = new Color(r, g, b);
        }
    }
}
