using System.Collections.Generic;
using UnityEngine;

namespace UIToolkitExamples
{
    [CreateAssetMenu(menuName = "UIToolkitExamples/GameSwitchList")]
    public class GameSwitchListAsset : ScriptableObject
    {
        public List<GameSwitchList> switches;

        public void Reset()
        {
            switches = new() //初始化器，注意容器的初始化器就是逐个按序地指定元素
            {
                new() { name = "Use Local Server", enabled = false },
                new() { name = "Show Debug Menu", enabled = false },
                new() { name = "Show FPS Counter", enabled = true },
            };
        }

        public bool IsSwitchEnabled(string switchName) => switches.Find(s => s.name == switchName).enabled;
    }
}