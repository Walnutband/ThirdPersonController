using UnityEngine;

namespace SnakeGame
{
    public interface IEvent { }
    public struct EatFoodEvent
    {
        public int x, y;
    }
    public struct SnakeBiggerEvent
    {
        public int x, y;
        public Vector3 dir;
    }
    public struct SnakeMoveEvent
    {
        public int headIndex;
        public int lastIndex;
        public Vector3 nextMove;
    }
    public struct SnakePosUpdateEvent
    {
        public Vector2 head;
        public Vector2 last;
    }
    /// <summary>
    /// 创造网格。但其实更准确的是CreateNode创造节点，因为每次触发这个事件都是创造一个节点，而不是整个网格
    /// </summary>
    public struct CreateGridEvent
    {
        public Node.E_Type type;
        public Vector2 pos;
    }
    public struct CreateFoodEvent
    {
        public Vector2Int pos; //指定二维坐标
    }
    /// <summary>
    /// 方向输入事件
    /// </summary>
    internal struct DirInputEvent
    {
        public int hor, ver; //水平和竖直输入，就是左右和上下
    }
    public struct GameOverEvent { }
    public struct GameInitEndEvent { }
}