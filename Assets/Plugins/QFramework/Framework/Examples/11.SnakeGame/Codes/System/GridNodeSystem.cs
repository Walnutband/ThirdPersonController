using QFramework;
using System.Text;
using UnityEngine;

namespace SnakeGame
{
    public class Node
    {
        //节点属性，整个地图完全由节点组成，节点就分为这四类。这里Wall是墙，Block原意应该是阻挡的意思，这里实际代表的就是可以走的节点
        public enum E_Type { Block, Wall, Food, Snake }
        public E_Type type;
        //重写的是object的ToString方法，即该类的实例.ToString返回枚举常量type的字符串值
        public override string ToString() => $"[{type.ToString()}]"; //$表示这是一个插值字符串，{}内是插值内容放入变量或表达式
    }
    public interface IGridNodeSystem : ISystem
    {
        void CreateGrid(int w, int h); //创建网格
        Node GetNode(int x, int y); //根据二维坐标获取节点
        Vector2Int FindBlockPos(int w, int h);
    }
    /// <summary>
    /// 网格节点系统。
    /// </summary>
    public class GridNodeSystem : AbstractSystem, IGridNodeSystem
    {
        //二维数组，意思就是存储的平面坐标对应的节点，这样可以直接通过坐标取点。以左上角为原点，和屏幕坐标系一样
        //更准确地说，节点位置实际上是以左下角为原点的，因为是按照世界坐标系来设置位置的，但因为边界就是Wall，内部就是Block，所以即使像这样顺序颠倒了，并没有改变边界整体，所以无影响
        private Node[,] mNodes; //二维下标，其实就是作为世界坐标来用的，在GridCreateSystem中可以看到
        Node IGridNodeSystem.GetNode(int x, int y) => mNodes[x, y];
        void IGridNodeSystem.CreateGrid(int w, int h)
        {
            //为空或者数量不够
            if (mNodes == null || mNodes.GetLength(0) * mNodes.GetLength(1) < w * h) mNodes = new Node[w, h];
            var e = new CreateGridEvent();
            //嵌套循环遍历二维数组
            for (int row = 0; row < w; row++)
            {
                for (int col = 0; col < h; col++)
                {//上面只是分配了内存，各个元素还是空引用。
                    if (mNodes[row, col] == null) mNodes[row, col] = new Node();
                    //边界位置就是墙，内部就是可以走的区域
                    mNodes[row, col].type = row == 0 || row == w - 1 || col == 0 || col == h - 1 ? Node.E_Type.Wall : Node.E_Type.Block;
                    e.type = mNodes[row, col].type;
                    e.pos = new Vector2(col, row); //注意这里，列对应横坐标，行对应纵坐标
                    this.SendEvent(e); //就是发送CreateGridEvent事件
                }
            }
        }
        protected override void OnInit()
        {
            this.RegisterEvent<SnakePosUpdateEvent>(OnSnakePosUpdate);
            this.RegisterEvent<CreateFoodEvent>(OnFoodCreated); //（随机）创造食物
            this.RegisterEvent<SnakeBiggerEvent>(OnSnakeBigger); //蛇变长
        }
        //刷新
        private void OnSnakePosUpdate(SnakePosUpdateEvent e)
        {
            Node node = mNodes[(int)e.head.y, (int)e.head.x];
            switch (node.type)
            {
                case Node.E_Type.Snake: //撞到自己身体结束游戏
                case Node.E_Type.Wall: //撞墙结束游戏
                    this.SendEvent<GameOverEvent>();
                    Debug.Log(node.type);
                    break;
                default:
                    if (node.type == Node.E_Type.Food)
                    {
                        this.SendEvent(new EatFoodEvent() { x = (int)e.last.y, y = (int)e.last.x });
                        this.SendEvent(new CreateFoodEvent() { pos = FindBlockPos(mNodes.GetLength(0), mNodes.GetLength(1)) });
                    }
                    node.type = Node.E_Type.Snake;
                    mNodes[(int)e.last.y, (int)e.last.x].type = Node.E_Type.Block;

                    break;
            }

        }
        private void OnFoodCreated(CreateFoodEvent e) => mNodes[e.pos.x, e.pos.y].type = Node.E_Type.Food;
        private void OnSnakeBigger(SnakeBiggerEvent e) => mNodes[e.x, e.y].type = Node.E_Type.Snake;
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            int rowCount = mNodes.GetLength(0);
            int colCount = mNodes.GetLength(1);
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    builder.Append(mNodes[row, col].ToString());
                    if (colCount - 1 != col) builder.Append(",");
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }
        public Vector2Int FindBlockPos(int w, int h)
        {
            Node node;
            int x, y;
            do
            {
                //注意是左开右闭，这里是指定的内部区域
                x = UnityEngine.Random.Range(1, w - 1);
                y = UnityEngine.Random.Range(1, h - 1);
                node = mNodes[x, y];
            }
            while (node.type != Node.E_Type.Block);
            //只要选中是Block节点就直接退出循环并返回节点的坐标，其实就是随机选择一个Block节点作为位置（蛇的初始位置和随机生成的食物的位置）
            return new Vector2Int(x, y);
        }
    }
}