using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
// using ARPGDemo;

namespace ARPGDemo.BattleSystem
{
    public class BattleSystem : SingletonMono<BattleSystem>
    {
        private List<IBattleHandler> battleHandlers;

        protected override void Awake()
        {
            //从所有加载的场景中查找带有标签“System”的游戏对象，取出其中第一个名为“ControlSystem”的（注意有可能为空），获取其组件ControlSystem。
            //TODO：这里要求严格的游戏对象Tag和name，到底合不合适还值得思考，不过具体来说，对于这种单例类，本来就是少数，所以用肯定是可以用的，稍微注意一下就行。
            m_Instance = GameObject.FindGameObjectsWithTag("System").FirstOrDefault(go => go.name == "BattleSystem")?.GetComponent<BattleSystem>();
            base.Awake();
        }

        private void FixedUpdate()
        {
            battleHandlers.ForEach(handler => handler.OnFixedUpdate());
        }
    }
}