using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace ARPGDemo.CustomAttributes.EditorSection
{
    [CustomPropertyDrawer(typeof(CustomListInspectorAttribute))]
    public class CustomListInspectorDrawer : PropertyDrawer
    {
        // 样式名称常量
        private const string ListContainerClass = "custom-list-container";
        private const string ListHeaderClass = "custom-list-header";
        private const string ListHeaderLabelClass = "custom-list-header-label";
        private const string ListHeaderCountClass = "custom-list-header-count";
        private const string ListContentClass = "custom-list-content";
        private const string ElementContainerClass = "custom-element-container";
        private const string ElementHeaderClass = "custom-element-header";
        private const string ElementHeaderTextClass = "custom-element-header-text";
        private const string ElementContentClass = "custom-element-content";
        private const string AddButtonClass = "custom-list-add-button";

        private SerializedProperty property;

        // 拖拽相关
        private class DragData
        {
            public int draggedIndex;
            public VisualElement draggedElement;
            public VisualElement dropTarget;
        }

        private bool IsListOrArray()
        {
            // 通过 fieldInfo 获取实际类型
            if (fieldInfo != null)
            {
                var type = fieldInfo.FieldType;

                // 检查数组
                if (type.IsArray)
                    return true;

                // 检查 List<>
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                    return true;
            }

            return false;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        {
            // Debug.Log($"isArray: {_property.isArray}");
            // // 检查是否为数组类型
            // if (!_property.isArray || _property.propertyType == SerializedPropertyType.String)
            // {
            //     Debug.Log("类型不符");
            //     return new PropertyField(_property);
            // }

            if (!IsListOrArray())
            {
                return new PropertyField(_property);
            }

            property = _property;
            var container = new VisualElement();
            container.AddToClassList(ListContainerClass);

            // 添加样式
            AddStyles(container);

            // 获取自定义属性
            var customAttr = attribute as CustomListInspectorAttribute;
            string displayName = string.IsNullOrEmpty(customAttr?.DisplayName) ?
                _property.displayName : customAttr.DisplayName;

            // 创建列表头
            var header = CreateListHeader(_property, displayName);
            container.Add(header);

            // 创建列表内容容器
            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(ListContentClass);
            container.Add(contentContainer);

            // 初始化列表内容
            RefreshListContent(contentContainer, _property);

            // 创建添加按钮
            var addButton = CreateAddButton(_property, contentContainer);
            container.Add(addButton);

            // 监听数组大小变化
            TrackPropertyChanges(_property, contentContainer);

            return container;
        }

        private VisualElement CreateListHeader(SerializedProperty property, string displayName)
        {
            var header = new VisualElement();
            header.AddToClassList(ListHeaderClass);

            // 获取数组大小
            // var arraySize = property.FindPropertyRelative("Array.size");
            // int size = arraySize.intValue;
            int size = property.arraySize;

            // 折叠按钮
            var foldout = new Foldout
            {
                text = displayName,
                value = true
            };
            foldout.AddToClassList(ListHeaderLabelClass);

            // 元素数量标签
            var countLabel = new Label($"Count: {size}");
            countLabel.AddToClassList(ListHeaderCountClass);
            header.Add(countLabel);

            header.Add(foldout);

            // 点击折叠按钮时刷新显示
            foldout.RegisterValueChangedCallback(evt =>
            {
                var content = header.parent?.Q(className: ListContentClass);
                if (content != null)
                {
                    content.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }

                var addButton = header.parent?.Q(className: AddButtonClass);
                if (addButton != null)
                {
                    addButton.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            return header;
        }

        private VisualElement CreateAddButton(SerializedProperty property, VisualElement contentContainer)
        {
            var addButton = new Button(() =>
            {
                // var arraySize = property.FindPropertyRelative("Array.size");
                // arraySize.intValue++;
                property.arraySize++;
                property.serializedObject.ApplyModifiedProperties();

                RefreshListContent(contentContainer, property);
            })
            {
                text = "Add Element"
            };
            addButton.AddToClassList(AddButtonClass);

            return addButton;
        }

        private void RefreshListContent(VisualElement container, SerializedProperty property)
        {
            container.Clear();

            // var arraySize = property.FindPropertyRelative("Array.size");
            // int size = arraySize.intValue;
            int size = property.arraySize;

            for (int i = 0; i < size; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                var elementContainer = CreateElementContainer(property, element, i, container);
                container.Add(elementContainer);
            }

            // 更新列表头的计数
            UpdateHeaderCount(property, size);
        }

        private VisualElement CreateElementContainer(
            SerializedProperty property,
            SerializedProperty element,
            int index,
            VisualElement parentContainer)
        {
            var container = new VisualElement();
            container.AddToClassList(ElementContainerClass);
            container.userData = index;

            // 创建元素头
            var header = CreateElementHeader(property, index, element);
            container.Add(header);

            // 创建元素内容
            var content = new VisualElement();
            content.AddToClassList(ElementContentClass);

            // 使用PropertyField绘制元素
            var propertyField = new PropertyField(element, "");
            propertyField.Bind(property.serializedObject);
            content.Add(propertyField);

            container.Add(content);

            // 设置初始折叠状态
            content.style.display = DisplayStyle.None;

            // 设置拖拽功能
            SetupDragAndDrop(header, container, index, parentContainer);

            return container;
        }

        private VisualElement CreateElementHeader(SerializedProperty property, int index, SerializedProperty element)
        {
            var header = new VisualElement();
            header.AddToClassList(ElementHeaderClass);

            // 创建文本标签
            var label = new Label($"Element {index}");
            label.AddToClassList(ElementHeaderTextClass);

            header.Add(label);

            // 添加点击事件来折叠/展开
            header.RegisterCallback<ClickEvent>(evt =>
            {
                var container = header.parent;
                if (container == null) return;

                var content = container.Q(className: ElementContentClass);
                if (content != null)
                {
                    content.style.display = content.style.display == DisplayStyle.None ?
                        DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            return header;
        }

        private void SetupDragAndDrop(
            VisualElement header,
            VisualElement elementContainer,
            int index,
            VisualElement parentContainer)
        {
            var dragData = new DragData();

            // 开始拖拽
            header.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    dragData.draggedIndex = index;
                    dragData.draggedElement = elementContainer;

                    // 添加拖拽视觉效果
                    elementContainer.style.opacity = 0.6f;
                    evt.StopPropagation();
                }
            });

            // 拖拽进入
            header.RegisterCallback<DragEnterEvent>(evt =>
            {
                if (dragData.draggedElement != null && dragData.draggedIndex != index)
                {
                    dragData.dropTarget = elementContainer;
                    elementContainer.style.backgroundColor = new Color(0.2f, 0.5f, 0.8f, 0.3f);
                }
            });

            // 拖拽离开
            header.RegisterCallback<DragLeaveEvent>(evt =>
            {
                if (dragData.dropTarget == elementContainer)
                {
                    elementContainer.style.backgroundColor = Color.clear;
                    dragData.dropTarget = null;
                }
            });

            // 拖拽释放
            header.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (dragData.draggedElement != null && dragData.dropTarget != null)
                {
                    // var property = this.property;
                    var property = this.property;
                    if (property != null)
                    {
                        // 移动数组元素
                        int fromIndex = dragData.draggedIndex;
                        int toIndex = (int)dragData.dropTarget.userData;

                        if (fromIndex != toIndex)
                        {
                            property.MoveArrayElement(fromIndex, toIndex);
                            property.serializedObject.ApplyModifiedProperties();

                            // 刷新列表
                            RefreshListContent(parentContainer, property);
                        }
                    }
                }

                // 清理
                dragData.draggedElement.style.opacity = 1f;
                dragData.draggedElement = null;
                dragData.dropTarget = null;
                elementContainer.style.backgroundColor = Color.clear;
            });

            // 设置可以接收拖拽
            header.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (dragData.draggedElement != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    evt.StopPropagation();
                }
            });

            // // 使元素可拖拽
            // header.style.unityCursor = CursorType.MoveArrow;
        }

        private void TrackPropertyChanges(SerializedProperty property, VisualElement contentContainer)
        {
            // 使用定时器检查属性变化
            var timer = new VisualElement();
            timer.schedule.Execute(() =>
            {
                if (property.serializedObject != null && property.serializedObject.targetObject != null)
                {
                    property.serializedObject.Update();

                    // 检查数组大小是否变化
                    // var arraySize = property.FindPropertyRelative("Array.size");
                    // int currentSize = arraySize.intValue;
                    int currentSize = property.arraySize;
                    int displayedSize = contentContainer.childCount;

                    if (currentSize != displayedSize)
                    {
                        RefreshListContent(contentContainer, property);
                    }
                }
            }).Every(100);
        }

        private void UpdateHeaderCount(SerializedProperty property, int newSize)
        {
            var header = property.serializedObject.targetObject?.GetType()
                .GetField(property.name)
                ?.GetValue(property.serializedObject.targetObject) as VisualElement;

            if (header != null)
            {
                var countLabel = header.Q(className: ListHeaderCountClass) as Label;
                if (countLabel != null)
                {
                    countLabel.text = $"Count: {newSize}";
                }
            }
        }

        private void AddStyles(VisualElement container)
        {
            var styleSheet = CreateStyleSheet();
            if (styleSheet != null)
            {
                container.styleSheets.Add(styleSheet);
            }
        }

        private StyleSheet CreateStyleSheet()
        {
            // 创建内联样式表
            var styleSheet = ScriptableObject.CreateInstance<StyleSheet>();

            // 这里可以添加USS规则，或者从外部加载USS文件
            // 为了简化，我们直接在代码中设置样式

            return null; // 简化版本，实际应该使用USS或内联样式
        }
    }
    
}