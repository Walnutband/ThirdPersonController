
using System;
using UnityEngine;

namespace MyPlugins.UILayout
{
    public class AnchoredLayoutObject : MonoBehaviour
    {
        [Serializable]
        public class CornerObject
        {//从左下角开始，逆时针方向的顺序。
            public Transform leftDown; //左下
            public Transform rightDown; //右下
            public Transform rightUp; //右上
            public Transform leftUp; //左上
        }
        
        //左手坐标系，就按照坐标轴正方向来决定左右、上下、前后。
        public class Vertices
        {//从后面的左下角开始，逆时针方向的顺序，然后是前面
            public Vector3 leftDownBack; 
            public Vector3 rightDownBack; 
            public Vector3 rightUpBack; 
            public Vector3 leftUpBack; 
            public Vector3 leftDownFront;
            public Vector3 rightDownFront;
            public Vector3 rightUpFront;
            public Vector3 leftUpFront;
        }

        [Serializable]
        public class AnchorPoint //锚点设置
        {
            public Vector3 anchorMin;
            public Vector3 anchorMax;
        }

        public Color m_GizmosColor = Color.green;

        /*TODO：因为暂时是直接使用GO来作为Corner，所以无法利用GO的层级关系来表达矩形之间的父子关系，所以就这样显式设置引用，所以从此处应该抓住本质，即Transform带来的父子关系只是
        因为transform.parent相关属性，而且这些属性会被相关逻辑所使用，所以表现出来父子关系，而实际上要表达父子关系本质上就是字段引用加上逻辑使用，不管要不要用Transform之类的。*/
        [Header("父对象")]
        [SerializeField] private AnchoredLayoutObject m_Parent;
        [Header("布局属性")]
        //TODO：先假设Y为0，即在水平面上进行布局，后续考虑扩展到整个3D空间？？
        private Vertices m_Vertices = new Vertices(); //8个顶点
        [SerializeField] private AnchorPoint m_Anchors;
        [SerializeField] private Vector3 m_Pivot; //轴心点
        [SerializeField] private Vector3 m_AnchoredPosition; //相对于锚点矩形的坐标
        [SerializeField] private Vector3 m_SizeDelta; //与锚点矩形的尺寸差值
        [SerializeField] private Vector3 m_Size; //Tip：在系统假设中，Size就应该为正，所以为负时出现意外情况并不代表逻辑错误，不用过多考虑。
        private Action onLayoutChanged;

        private Vector3 anchorPivotPos;
        private Vector3 anchorRectSize;
        private Vertices anchorVertices = new Vertices();

        private void OnValidate()
        {
            UpdateVerticesPosition(); //TODO：现在是每次有字段值改变都要更新顶点坐标，应该可以更加细致化。
            onLayoutChanged?.Invoke();
        }

        //Tip：貌似这个时候确实体会到了这两个消息方法的作用了。退出前注销，进入后注册。通过设置这个事件，在父对象布局发生变化时通知子对象。
        //Tip：这样的话就是完全按照Transform的层级来决定父子关系，就不要在检视器中直接赋值了，除非专门编写编辑器逻辑。
        private void OnTransformParentChanged()
        {
            Debug.Log("OnTransformParentChanged");
            if (transform.parent == null) return;
            m_Parent = transform.parent.GetComponent<AnchoredLayoutObject>();
            if (m_Parent != null)
            {
                m_Parent.onLayoutChanged += this.UpdateVerticesPosition;
            }
        }
        private void OnBeforeTransformParentChanged()
        {
            Debug.Log("OnBeforeTransformParentChanged");
            if (m_Parent != null)
            {
                m_Parent.onLayoutChanged -= this.UpdateVerticesPosition;
                m_Parent = null;
            }
        }

        //Tip：更新轴心点所在坐标，这是基于锚点布局系统而计算出来的坐标，也就是受到父对象影响的坐标。
        private Vector3 UpdatePivotPosition()
        {
            if (m_Parent == null)
            {
                //没有父对象，也就是不会受到锚点布局的影响，那么就保持当前原本的位置即可。
                return transform.position;
            }

            /*Tip：还真是，不知道RectTransform中计算布局是否如此，因为是相同类型，所以可以访问彼此的私有成员，而且这些成员也确实只会被这些类自己所使用，完全不需要对外公开*/
            // Vector3 pSizeDelta = m_Parent.m_SizeDelta; 
            // Vector2 pSize = m_Parent.m_Size;
            //Tip：首先求出锚点矩形的相关信息
            // Vector3 pPivotPos = m_Parent.UpdatePivotPosition();
            Vector3 pLCBPos = m_Parent.GetLeftDownBackPos();
            //根据父对象leftDownBack顶点坐标结合锚点设置，计算出锚点矩形的Pivot坐标
            Vector3 anchorPivotPos = pLCBPos + m_Anchors.anchorMin.Multiply(m_Parent.m_Size) + (m_Anchors.anchorMax - m_Anchors.anchorMin).Multiply(m_Parent.m_Size).Multiply(m_Pivot);
            this.anchorPivotPos = anchorPivotPos;
            this.anchorRectSize = (m_Anchors.anchorMax - m_Anchors.anchorMin).Multiply(m_Size);
            //锚点矩形坐标加上偏移坐标（anchoredPosition）就是该子对象的Pivot坐标
            Vector3 mPivotPos = anchorPivotPos + m_AnchoredPosition;
            return mPivotPos;
        }

        private void UpdateSize()
        {
            if (m_Parent == null) return; //无父对象影响
            m_Size = m_SizeDelta + m_Parent.m_Size.Multiply(m_Anchors.anchorMax - m_Anchors.anchorMin);
        }

        private Vector3 GetLeftDownBackPos()
        {
            Vector3 pivotPos = UpdatePivotPosition();
            // return new Vector3(pivotPos.x - m_Pivot.x * m_Size.x, pivotPos.y - m_Pivot.y * m_Size.y, pivotPos.z - m_Pivot.z * m_Size.z);
            return pivotPos - m_Pivot.Multiply(m_Size);
        }

        //根据Pivot和Size计算八个顶点的位置
        private void UpdateVerticesPosition()
        {
            UpdateSize(); //首先更新尺寸
            // Vector3 pos = transform.position;
            Vector3 pos = UpdatePivotPosition(); //获得轴心点坐标，然后分配尺寸，得到各个顶点的坐标。
            //Tip：其实只有leftDownBack和rightUpFront这两个顶点才能直接Multiply，其他顶点都是2和1。
            m_Vertices.leftDownBack = pos - m_Pivot.Multiply(m_Size);
            m_Vertices.rightDownBack = new Vector3(pos.x + (1 - m_Pivot.x) * m_Size.x, pos.y - m_Pivot.y * m_Size.y, pos.z - m_Pivot.z * m_Size.z);
            m_Vertices.rightUpBack = new Vector3(pos.x + (1 - m_Pivot.x) * m_Size.x, pos.y + (1 - m_Pivot.y) * m_Size.y, pos.z - m_Pivot.z * m_Size.z);
            m_Vertices.leftUpBack = new Vector3(pos.x - m_Pivot.x * m_Size.x, pos.y + (1 - m_Pivot.y) * m_Size.y, pos.z - m_Pivot.z * m_Size.z);
            m_Vertices.leftDownFront = new Vector3(pos.x - m_Pivot.x * m_Size.x, pos.y - m_Pivot.y * m_Size.y, pos.z + (1 - m_Pivot.z) * m_Size.z);
            m_Vertices.rightDownFront = new Vector3(pos.x + (1 - m_Pivot.x) * m_Size.x, pos.y - m_Pivot.y * m_Size.y, pos.z + (1 - m_Pivot.z) * m_Size.z);
            m_Vertices.rightUpFront = new Vector3(pos.x + (1 - m_Pivot.x) * m_Size.x, pos.y + (1 - m_Pivot.y) * m_Size.y, pos.z + (1 - m_Pivot.z) * m_Size.z);
            m_Vertices.leftUpFront = new Vector3(pos.x - m_Pivot.x * m_Size.x, pos.y + (1 - m_Pivot.y) * m_Size.y, pos.z + (1 - m_Pivot.z) * m_Size.z);
            // Vector3 ldPos = pos + new Vector3(pos.x - m_Pivot.x * size.x, pos.y - m_Pivot.y * size.y, pos.z);
            // Vector3 rdPos = pos + new Vector3(pos.x + (1 - m_Pivot.x) * size.x, pos.y - m_Pivot.y * size.y, pos.z);
            // Vector3 ruPos = pos + new Vector3(pos.x + (1 - m_Pivot.x) * size.x, pos.y + (1 - m_Pivot.y) * size.y, pos.z);
            // Vector3 luPos = pos + new Vector3(pos.x - m_Pivot.x * size.x, pos.y + (1 - m_Pivot.y) * size.y, pos.z);
            // m_CornerObjects.leftDown.position = ldPos;
            // m_CornerObjects.rightDown.position = rdPos;
            // m_CornerObjects.rightUp.position = ruPos;
            // m_CornerObjects.leftUp.position = luPos;

        }

        private void OnDrawGizmos()
        {
            // if (m_CornerObjects == null) return;

            // Transform leftDown = m_CornerObjects.leftDown;
            // Transform rightDown = m_CornerObjects.rightDown;
            // Transform rightUp = m_CornerObjects.rightUp;
            // Transform leftUp = m_CornerObjects.leftUp;

            // if (leftDown == null || rightDown == null || rightUp == null || leftUp == null)
            //     return;

            // 绘制矩形边框（绿色）
            // Gizmos.color = Color.green;
            Gizmos.color = m_GizmosColor;
            // Gizmos.DrawLine(leftDown.position, rightDown.position);
            // Gizmos.DrawLine(rightDown.position, rightUp.position);
            // Gizmos.DrawLine(rightUp.position, leftUp.position);
            // Gizmos.DrawLine(leftUp.position, leftDown.position);

            //Tip：绘制该对象的八个顶点

            //绘制后面
            Gizmos.DrawLine(m_Vertices.leftDownBack, m_Vertices.rightDownBack);
            Gizmos.DrawLine(m_Vertices.rightDownBack, m_Vertices.rightUpBack);
            Gizmos.DrawLine(m_Vertices.rightUpBack, m_Vertices.leftUpBack);
            Gizmos.DrawLine(m_Vertices.leftUpBack, m_Vertices.leftDownBack);
            //绘制前面
            Gizmos.DrawLine(m_Vertices.leftDownFront, m_Vertices.rightDownFront);
            Gizmos.DrawLine(m_Vertices.rightDownFront, m_Vertices.rightUpFront);
            Gizmos.DrawLine(m_Vertices.rightUpFront, m_Vertices.leftUpFront);
            Gizmos.DrawLine(m_Vertices.leftUpFront, m_Vertices.leftDownFront);
            //补上左面和右面
            Gizmos.DrawLine(m_Vertices.leftUpBack, m_Vertices.leftUpFront);
            Gizmos.DrawLine(m_Vertices.leftDownBack, m_Vertices.leftDownFront);
            Gizmos.DrawLine(m_Vertices.rightUpBack, m_Vertices.rightUpFront);
            Gizmos.DrawLine(m_Vertices.rightDownBack, m_Vertices.rightDownFront);

            //Tip：绘制锚点矩形的八个顶点
            // Gizmos.color = Color.red;
            // anchorVertices.leftDownBack = anchorPivotPos - m_Pivot.Multiply(anchorRectSize);
            // anchorVertices.rightDownBack = new Vector3(anchorPivotPos.x + (1 - m_Pivot.x) * anchorRectSize.x, anchorPivotPos.y - m_Pivot.y * anchorRectSize.y, anchorPivotPos.z - m_Pivot.z * anchorRectSize.z);
            // anchorVertices.rightUpBack = new Vector3(anchorPivotPos.x + (1 - m_Pivot.x) * anchorRectSize.x, anchorPivotPos.y + (1 - m_Pivot.y) * anchorRectSize.y, anchorPivotPos.z - m_Pivot.z * anchorRectSize.z);
            // anchorVertices.leftUpBack = new Vector3(anchorPivotPos.x - m_Pivot.x * anchorRectSize.x, anchorPivotPos.y + (1 - m_Pivot.y) * anchorRectSize.y, anchorPivotPos.z - m_Pivot.z * anchorRectSize.z);
            // anchorVertices.leftDownFront = new Vector3(anchorPivotPos.x - m_Pivot.x * anchorRectSize.x, anchorPivotPos.y - m_Pivot.y * anchorRectSize.y, anchorPivotPos.z + (1 - m_Pivot.z) * anchorRectSize.z);
            // anchorVertices.rightDownFront = new Vector3(anchorPivotPos.x + (1 - m_Pivot.x) * anchorRectSize.x, anchorPivotPos.y - m_Pivot.y * anchorRectSize.y, anchorPivotPos.z + (1 - m_Pivot.z) * anchorRectSize.z);
            // anchorVertices.rightUpFront = new Vector3(anchorPivotPos.x + (1 - m_Pivot.x) * anchorRectSize.x, anchorPivotPos.y + (1 - m_Pivot.y) * anchorRectSize.y, anchorPivotPos.z + (1 - m_Pivot.z) * anchorRectSize.z);
            // anchorVertices.leftUpFront = new Vector3(anchorPivotPos.x - m_Pivot.x * anchorRectSize.x, anchorPivotPos.y + (1 - m_Pivot.y) * anchorRectSize.y, anchorPivotPos.z + (1 - m_Pivot.z) * anchorRectSize.z);

        }


    }



}
#if UNITY_EDITOR

namespace MyPlugins.UILayout.EditorSection
{
    
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(AnchoredLayoutObject.CornerObject))]
    public class CornerObjectDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.paddingTop = 2;
            container.style.paddingBottom = 2;

            // 获取四个角的属性
            var leftDownProp = property.FindPropertyRelative("leftDown");
            var rightDownProp = property.FindPropertyRelative("rightDown");
            var rightUpProp = property.FindPropertyRelative("rightUp");
            var leftUpProp = property.FindPropertyRelative("leftUp");

            // 创建第一行（上边）
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.marginBottom = 2;

            var leftUpField = new PropertyField(leftUpProp, "");
            // var leftUpField = new ObjectField();
            leftUpField.style.flexGrow = 1;
            leftUpField.style.marginRight = 2;

            var rightUpField = new PropertyField(rightUpProp, "");
            rightUpField.style.flexGrow = 1;
            rightUpField.style.marginLeft = 2;

            topRow.Add(leftUpField);
            topRow.Add(rightUpField);

            // 创建第二行（下边）
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;

            var leftDownField = new PropertyField(leftDownProp, "");
            leftDownField.style.flexGrow = 1;
            leftDownField.style.marginRight = 2;

            var rightDownField = new PropertyField(rightDownProp, "");
            rightDownField.style.flexGrow = 1;
            rightDownField.style.marginLeft = 2;

            bottomRow.Add(leftDownField);
            bottomRow.Add(rightDownField);

            // 添加到容器
            container.Add(topRow);
            container.Add(bottomRow);

            return container;
        }
    }
}
#endif