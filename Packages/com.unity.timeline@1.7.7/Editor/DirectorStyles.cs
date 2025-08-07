using UnityEditor.Experimental;
using UnityEditor.StyleSheets;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    class DirectorStyles
    {
        const string k_Elipsis = "…";
        const string k_ImagePath = "Packages/com.unity.timeline/Editor/StyleSheets/Images/Icons/{0}.png";
        public const string resourcesPath = "Packages/com.unity.timeline/Editor/StyleSheets/res/";

        //Timeline resources
        public const string newTimelineDefaultNameSuffix = "Timeline";

        public static readonly GUIContent referenceTrackLabel = TrTextContent("R", "This track references an external asset");
        public static readonly GUIContent recordingLabel = TrTextContent("Recording...");
        public static readonly GUIContent noTimelineAssetSelected = TrTextContent("To start creating a timeline, select a GameObject");
        public static readonly GUIContent createTimelineOnSelection = TrTextContent("To begin a new timeline with {0}, create {1}");
        public static readonly GUIContent noTimelinesInScene = TrTextContent("No timeline found in the scene");
        public static readonly GUIContent createNewTimelineText = TrTextContent("Create a new Timeline and Director Component for Game Object");
        public static readonly GUIContent previewContent = TrTextContent("Preview", "Enable/disable scene preview mode");
        public static readonly GUIContent previewDisabledContent = L10n.TextContentWithIcon("Preview", "Scene preview is disabled for this TimelineAsset", MessageType.Info);
        public static readonly GUIContent mixOff = TrIconContent("TimelineEditModeMixOFF", "Mix Mode (1)");
        public static readonly GUIContent mixOn = TrIconContent("TimelineEditModeMixON", "Mix Mode (1)");
        public static readonly GUIContent rippleOff = TrIconContent("TimelineEditModeRippleOFF", "Ripple Mode (2)");
        public static readonly GUIContent rippleOn = TrIconContent("TimelineEditModeRippleON", "Ripple Mode (2)");
        public static readonly GUIContent replaceOff = TrIconContent("TimelineEditModeReplaceOFF", "Replace Mode (3)");
        public static readonly GUIContent replaceOn = TrIconContent("TimelineEditModeReplaceON", "Replace Mode (3)");
        public static readonly GUIContent showMarkersOn = TrIconContent("TimelineCollapseMarkerButtonEnabled", "Show / Hide Timeline Markers");
        public static readonly GUIContent showMarkersOff = TrIconContent("TimelineCollapseMarkerButtonDisabled", "Show / Hide Timeline Markers");
        public static readonly GUIContent showMarkersOnTimeline = TrTextContent("Show markers");
        public static readonly GUIContent timelineMarkerTrackHeader = TrTextContentWithIcon("Markers", string.Empty, "TimelineHeaderMarkerIcon");
        public static readonly GUIContent signalTrackIcon = IconContent("TimelineSignal");

        //Unity Default Resources
        public static readonly GUIContent playContent = L10n.IconContent("Animation.Play", "Play the timeline (Space)");
        public static readonly GUIContent gotoBeginingContent = L10n.IconContent("Animation.FirstKey", "Go to the beginning of the timeline (Shift+<)");
        public static readonly GUIContent gotoEndContent = L10n.IconContent("Animation.LastKey", "Go to the end of the timeline (Shift+>)");
        public static readonly GUIContent nextFrameContent = L10n.IconContent("Animation.NextKey", "Go to the next frame");
        public static readonly GUIContent previousFrameContent = L10n.IconContent("Animation.PrevKey", "Go to the previous frame");
        public static readonly GUIContent newContent = L10n.IconContent("CreateAddNew", "Add new tracks.");
        public static readonly GUIContent optionsCogIcon = L10n.IconContent("_Popup", "Options");
        public static readonly GUIContent animationTrackIcon = EditorGUIUtility.IconContent("AnimationClip Icon");
        public static readonly GUIContent audioTrackIcon = EditorGUIUtility.IconContent("AudioSource Icon");
        public static readonly GUIContent playableTrackIcon = EditorGUIUtility.IconContent("cs Script Icon");
        public static readonly GUIContent timelineSelectorArrow = L10n.IconContent("icon dropdown", "Timeline Selector");

        public GUIContent playrangeContent;

        public static readonly float kBaseIndent = 15.0f;
        public static readonly float kDurationGuiThickness = 5.0f;

        // matches dark skin warning color.
        public static readonly Color kClipErrorColor = new Color(0.957f, 0.737f, 0.008f, 1f);

        // TODO: Make skinnable? If we do, we should probably also make the associated cursors skinnable...
        public static readonly Color kMixToolColor = Color.white;
        public static readonly Color kRippleToolColor = new Color(255f / 255f, 210f / 255f, 51f / 255f);
        public static readonly Color kReplaceToolColor = new Color(165f / 255f, 30f / 255f, 30f / 255f);

        public const string markerDefaultStyle = "MarkerItem";

        public GUIStyle groupBackground;
        public GUIStyle displayBackground;
        public GUIStyle fontClip;
        public GUIStyle fontClipLoop;
        public GUIStyle trackHeaderFont;
        public GUIStyle trackGroupAddButton;
        public GUIStyle groupFont;
        public GUIStyle timeCursor;
        public GUIStyle endmarker;
        public GUIStyle tinyFont;
        public GUIStyle foldout;
        public GUIStyle trackMuteButton;
        public GUIStyle trackLockButton;
        public GUIStyle trackRecordButton;
        public GUIStyle playTimeRangeStart;
        public GUIStyle playTimeRangeEnd;
        public GUIStyle selectedStyle;
        public GUIStyle trackSwatchStyle;
        public GUIStyle connector;
        public GUIStyle keyframe;
        public GUIStyle warning;
        public GUIStyle extrapolationHold;
        public GUIStyle extrapolationLoop;
        public GUIStyle extrapolationPingPong;
        public GUIStyle extrapolationContinue;
        public GUIStyle trackMarkerButton;
        public GUIStyle markerMultiOverlay;
        public GUIStyle bottomShadow;
        public GUIStyle trackOptions;
        public GUIStyle infiniteTrack;
        public GUIStyle clipOut;
        public GUIStyle clipIn;
        public GUIStyle trackCurvesButton;
        public GUIStyle trackLockOverlay;
        public GUIStyle activation;
        public GUIStyle playrange;
        public GUIStyle timelineLockButton;
        public GUIStyle trackAvatarMaskButton;
        public GUIStyle markerWarning;
        public GUIStyle editModeBtn;
        public GUIStyle showMarkersBtn;
        public GUIStyle sequenceSwitcher;
        public GUIStyle inlineCurveHandle;
        public GUIStyle timeReferenceButton;
        public GUIStyle trackButtonSuite;
        public GUIStyle previewButtonDisabled;

        static internal DirectorStyles s_Instance;

        DirectorNamedColor m_DarkSkinColors;
        DirectorNamedColor m_LightSkinColors;
        DirectorNamedColor m_DefaultSkinColors;

        const string k_DarkSkinPath = resourcesPath + "Timeline_DarkSkin.txt";
        const string k_LightSkinPath = resourcesPath + "Timeline_LightSkin.txt";

        static readonly GUIContent s_TempContent = new GUIContent();

        public static bool IsInitialized
        {
            get { return s_Instance != null; }
        }

        public static DirectorStyles Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    //别忘了在构造函数中首先会调用LoadStyles，然后才是Initialize
                    s_Instance = new DirectorStyles(); //注意SO实例这样还只会存在于内存中，不会自动保存在外存上
                    s_Instance.Initialize();
                }

                return s_Instance;
            }
        }

        public static void ReloadStylesIfNeeded()
        {
            if (Instance.ShouldLoadStyles())
            {
                Instance.LoadStyles();
                if (!Instance.ShouldLoadStyles())
                    Instance.Initialize();
            }
        }

        public DirectorNamedColor customSkin
        {
            get { return EditorGUIUtility.isProSkin ? m_DarkSkinColors : m_LightSkinColors; }
            internal set
            {
                if (EditorGUIUtility.isProSkin)
                    m_DarkSkinColors = value;
                else
                    m_LightSkinColors = value;
            }
        }

        DirectorNamedColor LoadColorSkin(string path)
        {
            var asset = EditorGUIUtility.LoadRequired(path) as TextAsset;

            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                return DirectorNamedColor.CreateAndLoadFromText(asset.text);
            }

            return m_DefaultSkinColors;
        }

        static DirectorNamedColor CreateDefaultSkin()
        {
            //这样也只是存在于内存中，但是会参与到Unity底层管理，而new就只是在C#层创建一个托管对象
            var nc = ScriptableObject.CreateInstance<DirectorNamedColor>(); 
            nc.SetDefault();
            return nc;
        }

        public void ExportSkinToFile()
        {
            if (customSkin == m_DarkSkinColors)
                customSkin.ToText(k_DarkSkinPath);

            if (customSkin == m_LightSkinColors)
                customSkin.ToText(k_LightSkinPath);
        }

        public void ReloadSkin()
        {
            if (customSkin == m_DarkSkinColors)
            {
                m_DarkSkinColors = LoadColorSkin(k_DarkSkinPath);
            }
            else if (customSkin == m_LightSkinColors)
            {
                m_LightSkinColors = LoadColorSkin(k_LightSkinPath);
            }
        }

        public void Initialize()
        {
            m_DefaultSkinColors = CreateDefaultSkin();
            m_DarkSkinColors = LoadColorSkin(k_DarkSkinPath);
            m_LightSkinColors = LoadColorSkin(k_LightSkinPath);

            /*Tip：这里就是为各个已经确定存在（内置）的Track添加默认的颜色和图标，作为渲染时的首选，没有默认值的Track就是将TrackColor特性作为首选。*/

            // add the built in colors (control track uses attribute)
            TrackResourceCache.ClearTrackColorCache(); //清空所有Track颜色的缓存
            TrackResourceCache.SetTrackColor<AnimationTrack>(customSkin.colorAnimation);
            TrackResourceCache.SetTrackColor<PlayableTrack>(Color.white);
            TrackResourceCache.SetTrackColor<AudioTrack>(customSkin.colorAudio);
            TrackResourceCache.SetTrackColor<ActivationTrack>(customSkin.colorActivation);
            TrackResourceCache.SetTrackColor<GroupTrack>(customSkin.colorGroup);
            TrackResourceCache.SetTrackColor<ControlTrack>(customSkin.colorControl);

            // add default icons
            TrackResourceCache.ClearTrackIconCache();
            TrackResourceCache.SetTrackIcon<AnimationTrack>(animationTrackIcon);
            TrackResourceCache.SetTrackIcon<AudioTrack>(audioTrackIcon);
            TrackResourceCache.SetTrackIcon<PlayableTrack>(playableTrackIcon);
            TrackResourceCache.SetTrackIcon<ActivationTrack>(new GUIContent(GetBackgroundImage(activation)));
            TrackResourceCache.SetTrackIcon<SignalTrack>(signalTrackIcon);
        }

        DirectorStyles() //私有构造，防止外部直接构造，而在访问Instance时该类内部就会自动调用该构造方法
        {
            LoadStyles();
        }

        bool ShouldLoadStyles()
        {
            return endmarker == null ||
                endmarker.name == GUISkin.error.name;
        }

        //Tip：这里都是内置样式，无法在Unity编辑器中直接查看，而且IMGUI中所谓的uss似乎与UI Toolkit中指的uss还不太一样，总之现在只要知道这里就是加载了一系列Unity内置样式，作为Timeline编辑器中各个部分的默认样式即可
        void LoadStyles()
        {
            endmarker = GetGUIStyle("Icon-Endmarker");
            groupBackground = GetGUIStyle("groupBackground");
            displayBackground = GetGUIStyle("sequenceClip");
            fontClip = GetGUIStyle("Font-Clip");
            trackHeaderFont = GetGUIStyle("sequenceTrackHeaderFont");
            trackGroupAddButton = GetGUIStyle("sequenceTrackGroupAddButton");
            groupFont = GetGUIStyle("sequenceGroupFont");
            timeCursor = GetGUIStyle("Icon-TimeCursor");
            tinyFont = GetGUIStyle("tinyFont");
            foldout = GetGUIStyle("Icon-Foldout");
            trackMuteButton = GetGUIStyle("trackMuteButton");
            trackLockButton = GetGUIStyle("trackLockButton");
            trackRecordButton = GetGUIStyle("trackRecordButton");
            playTimeRangeStart = GetGUIStyle("Icon-PlayAreaStart");
            playTimeRangeEnd = GetGUIStyle("Icon-PlayAreaEnd");
            selectedStyle = GetGUIStyle("Color-Selected");
            trackSwatchStyle = GetGUIStyle("Icon-TrackHeaderSwatch");
            connector = GetGUIStyle("Icon-Connector");
            keyframe = GetGUIStyle("Icon-Keyframe");
            warning = GetGUIStyle("Icon-Warning");
            extrapolationHold = GetGUIStyle("Icon-ExtrapolationHold");
            extrapolationLoop = GetGUIStyle("Icon-ExtrapolationLoop");
            extrapolationPingPong = GetGUIStyle("Icon-ExtrapolationPingPong");
            extrapolationContinue = GetGUIStyle("Icon-ExtrapolationContinue");
            bottomShadow = GetGUIStyle("Icon-Shadow");
            trackOptions = GetGUIStyle("Icon-TrackOptions");
            infiniteTrack = GetGUIStyle("Icon-InfiniteTrack");
            clipOut = GetGUIStyle("Icon-ClipOut");
            clipIn = GetGUIStyle("Icon-ClipIn");
            trackCurvesButton = GetGUIStyle("trackCurvesButton");
            trackLockOverlay = GetGUIStyle("trackLockOverlay");
            activation = GetGUIStyle("Icon-Activation");
            playrange = GetGUIStyle("Icon-Playrange");
            timelineLockButton = GetGUIStyle("IN LockButton");
            trackAvatarMaskButton = GetGUIStyle("trackAvatarMaskButton");
            trackMarkerButton = GetGUIStyle("trackCollapseMarkerButton");
            markerMultiOverlay = GetGUIStyle("MarkerMultiOverlay");
            editModeBtn = GetGUIStyle("editModeBtn");
            showMarkersBtn = GetGUIStyle("showMarkerBtn");
            markerWarning = GetGUIStyle("markerWarningOverlay");
            sequenceSwitcher = GetGUIStyle("sequenceSwitcher");
            inlineCurveHandle = GetGUIStyle("RL DragHandle");
            timeReferenceButton = GetGUIStyle("timeReferenceButton");
            trackButtonSuite = GetGUIStyle("trackButtonSuite");
            previewButtonDisabled = GetGUIStyle("previewButtonDisabled");

            playrangeContent = new GUIContent(GetBackgroundImage(playrange)) { tooltip = L10n.Tr("Toggle play range markers.") };

            fontClipLoop = new GUIStyle(fontClip) { fontStyle = FontStyle.Bold };
        }

        public static GUIStyle GetGUIStyle(string s)
        {
            return EditorStyles.FromUSS(s);
        }

        public static GUIContent TrIconContent(string iconName, string tooltip = null)
        {
            return L10n.IconContent(iconName == null ? null : ResolveIcon(iconName), tooltip);
        }

        public static GUIContent IconContent(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName == null ? null : ResolveIcon(iconName));
        }

        public static GUIContent TrTextContentWithIcon(string text, string tooltip, string iconName)
        {
            return L10n.TextContentWithIcon(text, tooltip, iconName == null ? null : ResolveIcon(iconName));
        }

        public static GUIContent TrTextContent(string text, string tooltip = null)
        {
            return L10n.TextContent(text, tooltip);
        }

        public static Texture2D LoadIcon(string iconName)
        {
            return EditorGUIUtility.LoadIconRequired(iconName == null ? null : ResolveIcon(iconName));
        }

        static string ResolveIcon(string icon)
        {
            return string.Format(k_ImagePath, icon);
        }

        public static string Elipsify(string label, Rect rect, GUIStyle style)
        {
            var ret = label;

            if (label.Length == 0)
                return ret;

            s_TempContent.text = label;
            float neededWidth = style.CalcSize(s_TempContent).x;

            return Elipsify(label, rect.width, neededWidth);
        }

        public static string Elipsify(string label, float destinationWidth, float neededWidth)
        {
            var ret = label;

            if (label.Length == 0)
                return ret;

            if (destinationWidth < neededWidth)
            {
                float averageWidthOfOneChar = neededWidth / label.Length;
                int floor = Mathf.Max((int)Mathf.Floor(destinationWidth / averageWidthOfOneChar), 0);

                if (floor < k_Elipsis.Length)
                    ret = string.Empty;
                else if (floor == k_Elipsis.Length)
                    ret = k_Elipsis;
                else if (floor < label.Length)
                    ret = label.Substring(0, floor - k_Elipsis.Length) + k_Elipsis;
            }

            return ret;
        }

        public static Texture2D GetBackgroundImage(GUIStyle style, StyleState state = StyleState.normal)
        {
            var blockName = GUIStyleExtensions.StyleNameToBlockName(style.name, false);
            var styleBlock = EditorResources.GetStyle(blockName, state);
            return styleBlock.GetTexture(StyleCatalogKeyword.backgroundImage);
        }
    }
}
