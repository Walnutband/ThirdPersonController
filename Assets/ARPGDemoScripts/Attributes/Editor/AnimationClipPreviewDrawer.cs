using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ARPGDemo.CustomAttributes.EditorSection
{
    
    [CustomPropertyDrawer(typeof(AnimationClipPreviewAttribute))]
    public class AnimationClipPreviewDrawer : PropertyDrawer
    {
        private class PreviewState
        {
            public bool isPreviewing;
            public float previewTime;
            public PlayableGraph playableGraph;
            public AnimationClipPlayable clipPlayable;
            public GameObject previewObject;
            public AnimationClip currentClip;
            public bool isGraphPlaying;

            public void Cleanup()
            {
                if (playableGraph.IsValid())
                {
                    playableGraph.Stop();
                    playableGraph.Destroy();
                }

                if (previewObject != null)
                {
                    Object.DestroyImmediate(previewObject);
                    previewObject = null;
                }

                currentClip = null;
                isPreviewing = false;
                isGraphPlaying = false;
            }
        }

        // 使用字典存储每个属性的预览状态
        private Dictionary<string, PreviewState> previewStates = new Dictionary<string, PreviewState>();
        private const float PREVIEW_HEIGHT = 70f;
        private const float TOGGLE_WIDTH = 50f;
        private const float SLIDER_WIDTH = 200f;
        private const float TIME_FIELD_WIDTH = 80f;

        private Animator animator;
        private Label timeLabel;
        private Slider slider;

        private PreviewState state;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Debug.Log("AnimationClipPreviewDrawer, propertyPath:" + property.propertyPath);
            // 创建根容器
            var root = new VisualElement();

            // 确保是AnimationClip类型
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != null && !(property.objectReferenceValue is AnimationClip))
            {
                root.Add(new Label("AnimationClipPreviewAttribute只能用于AnimationClip字段"));
                return root;
            }

            //从特性获取要控制的Animator，因为特性所在位置就是原字段所在位置，可以访问到所在对象的相关信息。
            // animator = ((AnimationClipPreviewAttribute)attribute).target;

            var animatorField = new ObjectField();
            animatorField.objectType = typeof(Animator);
            animatorField.style.flexGrow = 1;
            animatorField.RegisterValueChangedCallback(evt => { animator = evt.newValue as Animator; });
            // animator = animatorField.value as Animator;
            root.Add(animatorField);

            // 获取属性路径作为唯一标识
            string propertyPath = property.propertyPath;

            // 创建默认的PropertyField
            var propertyField = new PropertyField(property, property.displayName);
            propertyField.BindProperty(property); //UI元素与序列化属性绑定。
            root.Add(propertyField);

            // 创建预览区域容器
            var previewContainer = new VisualElement();
            previewContainer.style.marginTop = 2;
            previewContainer.style.marginBottom = 2;

            // 获取或创建预览状态
            if (!previewStates.ContainsKey(propertyPath))
            {
                previewStates[propertyPath] = new PreviewState();
            }

            // var state = previewStates[propertyPath];
            state = previewStates[propertyPath];

            // 创建预览控件行
            var controlsRow = new VisualElement();
            controlsRow.style.flexDirection = FlexDirection.Row;
            controlsRow.style.alignItems = Align.Center;
            controlsRow.style.justifyContent = Justify.SpaceBetween;
            controlsRow.style.flexShrink = 1;
            // controlsRow.style.paddingLeft = 16; // 缩进以对齐字段

            // Toggle控件
            var toggle = new Toggle("预览");
            toggle.style.flexShrink = 0f;
            toggle.style.justifyContent = Justify.FlexStart;
            toggle.Q<Label>().style.minWidth = TOGGLE_WIDTH;
            // toggle.style.width = TOGGLE_WIDTH + 50;
            toggle.value = state.isPreviewing;
            toggle.RegisterValueChangedCallback(evt =>
            {//值同步。
             // state.isPreviewing = evt.newValue;
             // UpdatePreview(property, state);
             // slider.SetEnabled(state.isPreviewing);
             // Debug.Log("isPreviewing:" + state.isPreviewing + ", Toggle值为" + evt.newValue);
             // 每次回调时重新获取最新的state
                if (previewStates.TryGetValue(propertyPath, out var currentState))
                {
                    state = currentState;
                    currentState.isPreviewing = evt.newValue;
                    UpdatePreview(property, currentState);
                    slider.SetEnabled(currentState.isPreviewing);
                    Debug.Log($"isPreviewing: {currentState.isPreviewing}, Toggle值为: {evt.newValue}, Path: {propertyPath}");
                }
                else
                {
                    Debug.LogWarning($"State not found for path: {propertyPath}");
                }
            });
            controlsRow.Add(toggle); //先在左边加入一个Toggle表示“预览按钮”

            // 滑动条
            // var slider = new Slider(0f, 1f);
            slider = new Slider(0f, 1f);
            slider.style.flexShrink = 1f;
            // slider.style.width = SLIDER_WIDTH;
            slider.style.marginLeft = 5;
            slider.style.marginRight = 5;
            slider.value = state.previewTime;
            // slider.SetEnabled(state.isPreviewing); //是否预览，决定能否交互。
            slider.RegisterValueChangedCallback(evt =>
            {
                // 每次回调时重新获取最新的state
                if (previewStates.TryGetValue(propertyPath, out var currentState))
                {
                    state = currentState;
                }
                else
                {
                    Debug.LogWarning($"State not found for path: {propertyPath}");
                }
                // Debug.Log("Slider值变化");
                Debug.Log($"{(state.isPreviewing ? "预览" : "停止")}, Slider值为{evt.newValue}, Clip值为{state.currentClip}");
                if (state.isPreviewing && state.currentClip != null)
                {
                    Debug.Log("");
                    // 更新预览时间（归一化的进度值）
                    state.previewTime = evt.newValue;
                    float timeInSeconds = state.previewTime * state.currentClip.length;

                    // 更新PlayableGraph的位置
                    if (state.clipPlayable.IsValid())
                    {
                        state.clipPlayable.SetTime(timeInSeconds);

                        // 强制更新图形
                        if (!state.isGraphPlaying)
                        {
                            state.playableGraph.Evaluate();
                        }
                    }

                    // 更新滑动条显示的值（通过UpdateUI）
                    UpdateSliderLabel(slider, timeInSeconds);
                }
            });
            controlsRow.Add(slider);

            // 时间显示标签
            // var timeLabel = new Label();
            timeLabel = new Label();
            // timeLabel.style.width = TIME_FIELD_WIDTH;
            timeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            if (state.currentClip != null)
            {
                timeLabel.text = (state.previewTime * state.currentClip.length).ToString("F3") + "s";
            }
            else
            {
                timeLabel.text = "0.000s";
            }
            controlsRow.Add(timeLabel);

            // 帮助更新滑动条显示的值的辅助方法
            void UpdateSliderLabel(Slider s, float timeInSeconds)
            {
                timeLabel.text = timeInSeconds.ToString("F3") + "s";
            }

            // 存储对slider和timeLabel的引用以便更新
            previewContainer.userData = new PreviewControls { Slider = slider, TimeLabel = timeLabel, State = state };

            previewContainer.Add(controlsRow);
            root.Add(previewContainer);

            // 监听属性变化
            //Tip：这里是监听所引用的AnimatinnClip的变化。
            root.TrackPropertyValue(property, changedProperty =>
            {
                // 清理旧状态
                if (state.currentClip != changedProperty.objectReferenceValue)
                {
                    state.Cleanup();
                    state.currentClip = changedProperty.objectReferenceValue as AnimationClip;

                    // 重置UI状态
                    toggle.SetEnabled(true);
                    toggle.value = false;
                    slider.SetEnabled(false);
                    slider.value = 0;
                    timeLabel.text = "0.000s";
                }
            });

            // 注册销毁清理
            root.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (previewStates.TryGetValue(propertyPath, out var s))
                {
                    s.Cleanup();
                    previewStates.Remove(propertyPath);
                }
            });

            return root;
        }

        //更新预览状态。
        private void UpdatePreview(SerializedProperty property, PreviewState state)
        {
            if (state.isPreviewing)
            {

                AnimationClip clip = property.objectReferenceValue as AnimationClip;
                if (clip != null && state.currentClip == clip)
                {
                    StartPreview(state, clip);
                }
                else
                {//就是更换了PreviewState的Clip。
                    state.Cleanup();
                    state.currentClip = clip;
                    if (clip != null)
                    {
                        StartPreview(state, clip);
                    }
                }
            }
            else
            {
                StopPreview(state); //停止预览。
            }
        }

        private void StartPreview(PreviewState state, AnimationClip clip)
        {
            if (clip == null) return;

            try
            {
                //Tip：之前深入探究Playables系统发现的，动画要实现编辑时预览功能其实非常简单，因为PlayableGraph都把工作做好了。
                // 创建PlayableGraph
                state.playableGraph = PlayableGraph.Create("AnimationClipPreview");
                state.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual); //手动。

                // 创建AnimationClipPlayable
                state.clipPlayable = AnimationClipPlayable.Create(state.playableGraph, clip);

                // 创建输出
                // var output = AnimationPlayableOutput.Create(state.playableGraph, "PreviewOutput", GetOrCreatePreviewObject(state));
                var output = AnimationPlayableOutput.Create(state.playableGraph, "PreviewOutput", animator);
                output.SetSourcePlayable(state.clipPlayable); //直接连接，因为这就是预览单个动画片段，不搞混合或分层之类的。

                // 设置初始时间（进度乘以长度）
                state.clipPlayable.SetTime(state.previewTime * clip.length); 

                // 播放但不自动更新
                state.playableGraph.Play();
                state.isGraphPlaying = true; //记录状态

                // 立即评估一次以显示当前帧。注意一定要有SetTime设置进度之后才评估，Evaluate就是基于当前进度进行一次计算。
                state.playableGraph.Evaluate();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start animation preview: {e.Message}");
                state.Cleanup();
            }
        }

        private void StopPreview(PreviewState state)
        {
            state.Cleanup();
        }

        //Tip：我估计，Unity自带的那个动画预览功能（在检视面板下方）就有这样一段逻辑。
        private GameObject GetOrCreatePreviewObject(PreviewState state)
        {
            if (state.previewObject == null)
            {
                // 创建一个临时的GameObject用于预览
                state.previewObject = new GameObject("AnimationPreview_Temp");
                state.previewObject.hideFlags = HideFlags.HideAndDontSave;

                // 添加必要的组件
                var animator = state.previewObject.AddComponent<Animator>();

                // 可以在这里添加一个简单的渲染组件以便看到预览效果
                // 例如添加一个Cube作为预览对象
                // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // cube.transform.SetParent(state.previewObject.transform);
                // cube.hideFlags = HideFlags.HideAndDontSave;
            }
            return state.previewObject;
        }

        // 用于在Update中更新预览
        [InitializeOnLoadMethod]
        private static void RegisterUpdate()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            // 可以在这里添加持续更新的逻辑，如果需要实时预览的话
        }

        // 辅助类
        private class PreviewControls
        {
            public Slider Slider;
            public Label TimeLabel;
            public PreviewState State;
        }
    }
}