using QFramework;
using UnityEngine;

namespace SnakeGame
{
    public class InitGameCommand : AbstractCommand
    {
        private readonly int mapW, mapH; //地图的宽度和高度（离散值）

        public InitGameCommand(int w, int h)
        {
            mapW = w;
            mapH = h;
        }
        protected override void OnExecute()
        {//该命令设置了相机位置，创造了网格，设置了蛇和食物的初始位置，然后发送GameInitEndEvent事件表明游戏初始化结束
            CenterCamera(mapW, mapH); //设置相机位置
            var map = this.GetSystem<IGridNodeSystem>();
            map.CreateGrid(mapW, mapH); //创造网格（确定整张地图上各个节点的类型，此时确定的是边界的Wall和内部的Block）
            //使用FindBlockPos方法分别随机寻找一个Block节点作为蛇和食物的初始位置（返回的是二维下标）
            var p = map.FindBlockPos(mapW, mapH);
            this.GetSystem<ISnakeSystem>().CreateSnake(p.x, p.y);
            //因为CreateFoodEvent类中的pos是public，所以可以直接在此用初始化列表赋值，如果是private，则要通过构造函数赋值了。
            this.SendEvent(new CreateFoodEvent() { pos = map.FindBlockPos(mapW, mapH) });
            this.SendEvent<GameInitEndEvent>();
        }
        /// <summary>
        /// 居中摄像机
        /// </summary>
        private void CenterCamera(int w, int h)
        {
            Camera.main.transform.localPosition = new Vector3((w - 1) * 0.5f, (h - 1) * 0.5f, -10f);
            Camera.main.orthographicSize = w > h ? w * 0.5f : h * 0.5f; //将就大的，也就是保证将整体包含在视图中
        }
    }
}