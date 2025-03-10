namespace Ilumisoft.VisualStateMachine
{
    using UnityEngine;
    using UnityEngine.Serialization;

    [System.Serializable]
    public class Node
    {
        //在窗口中的矩形的宽度和高度
        private static readonly int Width = 120;
        private static readonly int Height = 60;

        [SerializeField] private Vector2 position;

        //FormerlySerializedAs("name")用于重命名字段。它告诉 Unity 在反序列化时，如果找到一个名为 "name" 的旧字段，将其映射到当前字段 id 上。
        //这个特性主要用于在项目开发过程中重构代码。当你将字段重命名时，可以使用这个特性来保持与旧版本的兼容性，确保数据不会丢失或出错。
        [SerializeField, FormerlySerializedAs("name")]
        private string id = string.Empty;

        /// <summary>
        /// Gets or sets the name of the node
        /// </summary>
        public string ID
        {
            get => this.id;
            set => this.id = value;
        }

        /// <summary>
        /// Gets or sets the rect of the node.
        /// Remark: The setter will only apply the center of the position
        /// </summary>
        public Rect Rect
        {
            get => new Rect()
            {
                x = Position.x - Width / 2.0f,
                y = Position.y - Height / 2.0f,
                width = Width,
                height = Height
            };

            set => Position = value.center; //矩形中心，所以上面各自减一半就得到了左上角的屏幕坐标
        }

        /// <summary>
        /// Gets or sets the position (center) of the node in the graph
        /// </summary>
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
    }
}