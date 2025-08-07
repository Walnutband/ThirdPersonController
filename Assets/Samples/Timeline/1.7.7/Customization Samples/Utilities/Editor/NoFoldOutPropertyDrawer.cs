using UnityEditor;
using UnityEngine;

namespace Timeline.Samples
{
    // Custom property drawer that draws all child properties inline
    [CustomPropertyDrawer(typeof(NoFoldOutAttribute))]
    public class NoFoldOutPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.hasChildren)
                return base.GetPropertyHeight(property, label);
            property.isExpanded = true;
            return EditorGUI.GetPropertyHeight(property, label, true) -
                EditorGUI.GetPropertyHeight(property, label, false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //hasChildren指的是被序列化的成员变量。只要有，那么Next(true)就保证不会为空，就一定至少可以显示出一个字段出来。
            if (!property.hasChildren) //简单理解为类，因为类会有成员变量，在此处就称为children，所以下面的Next方法就是是否要进入其类内部即成员变量。
                EditorGUI.PropertyField(position, property, label);
            else
            {
                SerializedProperty iter = property.Copy();
                var nextSibling = property.Copy();
                nextSibling.Next(false); 
                property.Next(true);
                do
                {
                    // We need to check against nextSibling to properly stop
                    // otherwise we will draw properties that are not child of this
                    // foldout.
                    if (SerializedProperty.EqualContents(property, nextSibling))
                        break;
                    //visibleChildren就是被序列化且没有标记HideInInspector的字段。
                    float height = EditorGUI.GetPropertyHeight(property, property.hasVisibleChildren);
                    position.height = height;
                    EditorGUI.PropertyField(position, property, property.hasVisibleChildren);
                    position.y = position.y + height; //看这样子，似乎向下才是Y轴正方相关
                }
                while (property.NextVisible(false)); //不进入
            }
        }
    }
}
