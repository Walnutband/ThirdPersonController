using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
//点击按钮切换一个有色元素的样式类，使得背景颜色值发生变化，并且在标签元素中打印发生的事件以及相邻事件经过的时间

public class TransitionEventsExample : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    private Button clickMeButton;
    private VisualElement colorChanger;
    private Label eventLabel;
    private Label timeLabel;

    private DateTime lastEvent;
    //表示10ms的时间间隔
    private static readonly TimeSpan NearlyInstantaneousThreshold = TimeSpan.FromMilliseconds(10);

    private static readonly string ClickMeButtonClass = "click-me";
    private static readonly string ColorChangerClass = "color-changer";
    private static readonly string ColorChangerTransitionClass = "color-transition";
    private static readonly string EventLabelName = "eventLabel";
    private static readonly string TimeLabelName = "timeLabel";
    private static readonly string TimeBelowThresholdText = "Almost instantaneous.";

    [MenuItem("Window/UI Toolkit/示例/TransitionEventsExample")]
    public static void ShowExample()
    {
        TransitionEventsExample wnd = GetWindow<TransitionEventsExample>();
        wnd.titleContent = new GUIContent("TransitionEventsExample");
        wnd.minSize = new Vector2(500f, 400f);
    }

    public void CreateGUI()
    {
        lastEvent = DateTime.Now;

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement uxmlAsset = m_VisualTreeAsset.Instantiate();
        root.Add(uxmlAsset);

        // Get the relevant elements by querying the root element
        clickMeButton = root.Q<Button>(className: ClickMeButtonClass);

        colorChanger = root.Q<VisualElement>(className: ColorChangerClass);

        eventLabel = root.Q<Label>(name: EventLabelName);

        timeLabel = root.Q<Label>(name: TimeLabelName);

        // Add callbacks for clicking on the button and monitoring the color changing element.
        clickMeButton.RegisterCallback<ClickEvent>(OnClickEvent);

        colorChanger.RegisterCallback<TransitionRunEvent>(OnTransitionRun);
        colorChanger.RegisterCallback<TransitionStartEvent>(OnTransitionStart);
        colorChanger.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        colorChanger.RegisterCallback<TransitionCancelEvent>(OnTransitionCancel);
    }

    private void OnDisable()
    {
        clickMeButton.UnregisterCallback<ClickEvent>(OnClickEvent);

        colorChanger.UnregisterCallback<TransitionRunEvent>(OnTransitionRun);
        colorChanger.UnregisterCallback<TransitionStartEvent>(OnTransitionStart);
        colorChanger.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        colorChanger.UnregisterCallback<TransitionCancelEvent>(OnTransitionCancel);
    }

    private void OnClickEvent(ClickEvent evt)
    {
        //Toggle就是在两个状态之间切换，在此即有就移除，无就添加
        colorChanger.ToggleInClassList(ColorChangerTransitionClass);
    }

    private void OnTransitionRun(TransitionRunEvent evt)
    {
        DisplayLatestEvent("TransitionRunEvent", DateTime.Now);
    }

    private void OnTransitionStart(TransitionStartEvent evt)
    {
        DisplayLatestEvent("TransitionStartEvent", DateTime.Now);
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        DisplayLatestEvent("TransitionEndEvent", DateTime.Now);
    }

    private void OnTransitionCancel(TransitionCancelEvent evt)
    {
        DisplayLatestEvent("TransitionCancelEvent", DateTime.Now);
    }

    private void DisplayLatestEvent(string eventType, DateTime timestamp)
    {
        // If two events are sent too close together, add both to the Latest event line.
        // This happens if the delay is set to 0 and the TransitionRun and TransitionStart
        // are sent at the same time, or if the button was pressed before the transition
        // was over, thus sending TransitionCancel and TransitionRun (and potentially
        // TransitionStart) events close together.
        var elapsed = timestamp - lastEvent;
        //两个事件发生的间隔时间过短，则将后一个事件与前一个事件的文本并列，而不是直接去掉前一个文本
        if (elapsed <= NearlyInstantaneousThreshold)
        {
            timeLabel.text = TimeBelowThresholdText;
            eventLabel.text = $"{eventLabel.text}, {eventType}";
        }
        else
        {
            timeLabel.text = $"{elapsed:s\\.ff} s";
            eventLabel.text = eventType;
        }

        lastEvent = timestamp;
    }
}