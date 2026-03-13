using UnityEngine.UIElements;
using UnityEditor;

namespace MyPlugins.AnimationPlayer.EditorSection
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using System.Collections.Generic;

    [CustomEditor(typeof(AnimatorAgentSettings))]
    public class AnimatorAgentSettingsEditor : Editor
    {
        private VisualElement root;
        private VisualElement layersContainer;
        private List<VisualElement> layerElements = new List<VisualElement>();

        public override VisualElement CreateInspectorGUI()
        {
            // 创建根容器
            root = new VisualElement();

            // 加载样式表
            LoadStyles();

            // 创建标题
            CreateHeader();

            // 创建层级控制按钮区域
            CreateLayerControlButtons();

            // 创建分隔线
            root.Add(new VisualElement { style = { height = 10 } });

            // 创建层级容器
            layersContainer = new VisualElement();
            layersContainer.name = "layers-container";
            root.Add(layersContainer);

            // 初始绘制所有层级
            RefreshLayers();

            // 监听资产更改事件
            root.TrackSerializedObjectValue(serializedObject, (obj) =>
            {
                RefreshLayers();
            });

            return root;
        }

        private void LoadStyles()
        {
            // 添加内联样式
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;
        }

        private void CreateHeader()
        {
            var headerLabel = new Label("动画层级设置");
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.fontSize = 14;
            headerLabel.style.marginBottom = 10;
            root.Add(headerLabel);
        }

        private void CreateLayerControlButtons()
        {
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginBottom = 5;

            // 增加层级按钮
            var addButton = new Button(() =>
            {
                Undo.RecordObject(target, "Add Layer");
                var settings = (AnimatorAgentSettings)target;
                settings.AddLayer();
                EditorUtility.SetDirty(settings);
                RefreshLayers();
            });
            addButton.text = "增加层级";
            addButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            addButton.style.height = 30;
            addButton.style.flexGrow = 1;
            addButton.style.marginRight = 2;
            buttonContainer.Add(addButton);

            // 移除层级按钮
            var removeButton = new Button(() =>
            {
                Undo.RecordObject(target, "Remove Layer");
                var settings = (AnimatorAgentSettings)target;

                if (settings.layerCount > 0)
                {
                    settings.RemoveLayer();
                    EditorUtility.SetDirty(settings);
                    RefreshLayers();
                }
            });
            removeButton.text = "移除层级";
            removeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            removeButton.style.height = 30;
            removeButton.style.flexGrow = 1;
            removeButton.style.marginLeft = 2;

            // 根据layerCount启用/禁用按钮
            removeButton.SetEnabled(((AnimatorAgentSettings)target).layerCount > 0);

            buttonContainer.Add(removeButton);
            root.Add(buttonContainer);
        }

        private void RefreshLayers()
        {
            var settings = (AnimatorAgentSettings)target;

            // 清理旧的层级元素
            layersContainer.Clear();
            layerElements.Clear();

            // 确保数据一致性
            EnsureDataConsistency(settings);

            // 重新绘制所有层级
            for (int i = 0; i < settings.layerCount; i++)
            {
                var layerElement = CreateLayerElement(settings, i);
                layersContainer.Add(layerElement);
                layerElements.Add(layerElement);

                // 添加分隔线（除了最后一个）
                if (i < settings.layerCount - 1)
                {
                    var separator = new VisualElement();
                    separator.style.height = 1;
                    separator.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    separator.style.marginTop = 5;
                    separator.style.marginBottom = 5;
                    layersContainer.Add(separator);
                }
            }

            // 如果没有层级，显示提示信息
            if (settings.layerCount == 0)
            {
                var emptyLabel = new Label("暂无层级，点击\"增加层级\"添加");
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                emptyLabel.style.marginTop = 10;
                emptyLabel.style.marginBottom = 10;
                layersContainer.Add(emptyLabel);
            }

            // 更新移除按钮状态
            UpdateRemoveButtonState();
        }

        private VisualElement CreateLayerElement(AnimatorAgentSettings settings, int layerIndex)
        {
            // 创建层级的根容器
            var layerRoot = new VisualElement();
            layerRoot.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            layerRoot.style.paddingLeft = 10;
            layerRoot.style.paddingRight = 10;
            layerRoot.style.paddingTop = 10;
            layerRoot.style.paddingBottom = 10;
            layerRoot.style.marginBottom = 2;
            layerRoot.style.borderTopLeftRadius = 3;
            layerRoot.style.borderTopRightRadius = 3;
            layerRoot.style.borderBottomLeftRadius = 3;
            layerRoot.style.borderBottomRightRadius = 3;

            // 层级标题
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.marginBottom = 5;

            var titleLabel = new Label(GetLayerName(layerIndex));
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 12;
            headerContainer.Add(titleLabel);

            // 层级序号
            var indexLabel = new Label($"Layer {layerIndex}");
            indexLabel.style.color = Color.gray;
            indexLabel.style.fontSize = 10;
            headerContainer.Add(indexLabel);

            layerRoot.Add(headerContainer);

            // 层级遮罩
            var maskField = new ObjectField("层级遮罩");
            maskField.objectType = typeof(AvatarMask);
            maskField.tooltip = "选择层级使用的Avatar遮罩";

            // 绑定mask属性
            var maskProperty = serializedObject.FindProperty($"layerInfos.Array.data[{layerIndex}].mask");
            if (maskField != null) maskField.BindProperty(maskProperty);

            layerRoot.Add(maskField);

            // 层级混合模式
            var modeContainer = new VisualElement();
            modeContainer.style.flexDirection = FlexDirection.Row;
            modeContainer.style.alignItems = Align.Center;
            modeContainer.style.justifyContent = Justify.SpaceBetween; //分别在于左侧和右侧。
            modeContainer.style.marginTop = 5;

            var modeLabel = new Label("层级混合模式");
            // modeLabel.style.width = 150;
            modeContainer.Add(modeLabel);

            // 获取additive属性
            var additiveProperty = serializedObject.FindProperty($"layerInfos.Array.data[{layerIndex}].additive");

            var modeDropdown = new PopupField<string> (
                new List<string> { "Override", "Additive" },
                additiveProperty.boolValue ? 1 : 0
            );

            modeDropdown.style.flexGrow = 1;
            modeDropdown.style.flexDirection = FlexDirection.Column;
            modeDropdown.RegisterValueChangedCallback(evt =>
            {
                additiveProperty.boolValue = evt.newValue == "Additive";
                serializedObject.ApplyModifiedProperties();
            });

            modeContainer.Add(modeDropdown);
            layerRoot.Add(modeContainer);

            return layerRoot;
        }

        private void EnsureDataConsistency(AnimatorAgentSettings settings)
        {
            // 确保layerInfos的数量与layerCount一致
            if (settings.layerInfos == null)
            {
                settings.layerInfos = new List<AnimatorAgentSettings.LayerInfo>();
            }

            while (settings.layerInfos.Count < settings.layerCount)
            {
                settings.layerInfos.Add(default);
            }

            while (settings.layerInfos.Count > settings.layerCount && settings.layerInfos.Count > 0)
            {
                settings.layerInfos.RemoveAt(settings.layerInfos.Count - 1);
            }
        }

        private void UpdateRemoveButtonState()
        {
            var settings = (AnimatorAgentSettings)target;
            var removeButton = root.Q<Button>(name:"移除层级");
            if (removeButton != null)
            {
                removeButton.SetEnabled(settings.layerCount > 0);
            }
        }

        private string GetLayerName(int index)
        {
            switch (index)
            {
                case 0: return "层级一";
                case 1: return "层级二";
                case 2: return "层级三";
                case 3: return "层级四";
                case 4: return "层级五";
                default: return $"层级 {index + 1}";
            }
        }
    }
}