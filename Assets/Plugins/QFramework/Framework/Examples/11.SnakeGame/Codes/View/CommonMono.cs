using UnityEngine;
using System;

namespace SnakeGame
{
    public class CommonMono : MonoBehaviour
    {
        private static Action mUpdateAction;
        public static void AddUpdateAction(Action fun) => mUpdateAction += fun;
        public static void RemoveUpdateAction(Action fun) => mUpdateAction -= fun;
        //准确来说，这才是游戏的主循环
        private void Update()
        {//在这个示例中注册了两个方法，一个是接收方向输入，一个是SnakeSystem的AutoMove方法也就是移动蛇身
            mUpdateAction?.Invoke();
        }
    }
}