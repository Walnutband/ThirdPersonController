using UnityEngine;

namespace ARPGDemo.Test
{

    public class BoundsVisualizer : MonoBehaviour
    {
        public BoxCollider boxCollider;

        void Update()
        {
            if (boxCollider == null) return;

            // 1. 获取Collider的世界坐标边界框 (Axis-Aligned Bounds)
            Bounds worldBounds = boxCollider.bounds;

            // 2. 在Scene视图中绘制这个边界框 (紫色)
            DrawBounds(worldBounds, Color.magenta);

            // 3. (可选) 如果你想看Collider本身的形状，Unity编辑器会在选中物体时绘制
        }

        void DrawBounds(Bounds b, Color color)
        {
            // 计算边界框的8个角点
            Vector3 p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            Vector3 p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            Vector3 p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            Vector3 p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Vector3 p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            Vector3 p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            Vector3 p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            Vector3 p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            // 绘制底面和顶面的四条边
            Debug.DrawLine(p1, p2, color);
            Debug.DrawLine(p2, p3, color);
            Debug.DrawLine(p3, p4, color);
            Debug.DrawLine(p4, p1, color);

            Debug.DrawLine(p5, p6, color);
            Debug.DrawLine(p6, p7, color);
            Debug.DrawLine(p7, p8, color);
            Debug.DrawLine(p8, p5, color);

            // 绘制连接底面和顶面的四条垂直边
            Debug.DrawLine(p1, p5, color);
            Debug.DrawLine(p2, p6, color);
            Debug.DrawLine(p3, p7, color);
            Debug.DrawLine(p4, p8, color);
        }
    }
}