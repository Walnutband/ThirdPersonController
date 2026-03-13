
using System;
using UnityEngine;

namespace ARPGDemo.AbilitySystem
{
    [Serializable]
    public class BufferedInputHandler
    {
        private Func<bool> buffer;
        public float defaultBufferTime = 0.15f; //缓存时间
        private float bufferTime;
        private float timer;

        public void OnTick(float _deltaTime)
        {
            if (buffer == null) return;
            timer += _deltaTime;
            // Debug.Log($"timer计时器时间：{timer}");
            if (timer >= bufferTime)
            {
                buffer = null;
                timer = 0f;
                return;
            }

            if (buffer.Invoke() == true)
            {
                buffer = null;
            }

        }

        public void SetBuffer(Func<bool> _buffer) => SetBuffer(_buffer, defaultBufferTime);

        public void SetBuffer(Func<bool> _buffer, float _bufferTime)
        {
            buffer = _buffer;
            bufferTime = _bufferTime;
            timer = 0f;
        }
    }
}