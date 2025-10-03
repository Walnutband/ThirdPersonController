using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ARPGDemo.SkillSystemtest;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARPGDemo.SkillSystemtest
{
    public class BasicTimelineEditor : EditorWindow
    {
        TimelineEditorSettings settings;

        //记录元数据，以便处理引用类型与实际类型
        private Dictionary<Type, Action> typeActions = new Dictionary<Type, Action>();

        private TimelineModel_SO currentModel;

        private TrackSearchWindowProvider trackSearchWindowProvider;

        [MenuItem("MyPlugins/BasicTimelineEditor")]
        public static void ShowExample()
        {
            BasicTimelineEditor wnd = GetWindow<BasicTimelineEditor>();
            wnd.titleContent = new GUIContent("TimelineEditor");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += SelectModel;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= SelectModel;
        }

        private void SelectModel()
        {
            currentModel = Selection.activeObject as TimelineModel_SO;
            if (currentModel != null)
            {
                var clipView = rootVisualElement.Q("ClipView");
                // Editor editor = Editor.CreateEditor(currentModel);
                clipView.Clear();
                InspectorElement modelInspector = new InspectorElement(currentModel);
                clipView.Add(modelInspector);
                // InspectorElement.FillDefaultInspector(clipView, new SerializedObject(currentModel), editor);
            }
        }

        //获取所有轨道类型
        private void GetAllTrackTypes()
        {
            List<Type> types = Assembly.GetAssembly(typeof(TimelineTrack)).GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(TimelineTrack)))
                .ToList();


        }

        private void CreateGUI()
        {
            settings = TimelineEditorSettings.GetOrCreateSettings();
            VisualElement root = rootVisualElement;

            var tree = settings.timelineEditorUXML;
            tree.CloneTree(root);

            // var clipView = root.Q("ClipView");
            // if (currentModel != null)
            // {
            //     Editor editor = Editor.CreateEditor(currentModel);
            //     // InspectorElement modelInspector = new InspectorElement(currentModel);
            //     // clipView.Add(modelInspector);
            //     InspectorElement.FillDefaultInspector(clipView, new SerializedObject(currentModel), editor);
            // }

            var menu = root.Q<ToolbarMenu>();
            menu.menu.AppendAction("新建Timeline", (ctx) =>
            {
                CreateAndSaveAsset<TimelineModel_SO>();
            });
            trackSearchWindowProvider = new TrackSearchWindowProvider(AddTrack);
            menu.menu.AppendAction("新建Track", (ctx) =>
            {
                if (currentModel == null) return;
                // var mousePos = UnityEngine.GUIUtility.GUIToScreenPoint(menu.LocalToWorld(ctx.eventInfo.localMousePosition));
                // if (ctx == null) Debug.Log("ctx is null");
                // var mousePos = menu.LocalToWorld(ctx.eventInfo.localMousePosition);
                // SearchWindowContext context = new SearchWindowContext(mousePos);
                SearchWindowContext context = new SearchWindowContext();
                SearchWindow.Open(context, trackSearchWindowProvider);
            });

        }

        private void AddTrack(TimelineTrack _track, Type _type)
        {
            if (_track is TimelineTrack_Animation)
            {
                /*BugFix：WCNM！！！泛型有坑！！！*/
                // TimelineTrack_SO<TimelineTrack_Animation> trackSO = ScriptableObject.CreateInstance<TimelineTrack_SO<TimelineTrack_Animation>>();
                TimelineTrack_Animation_SO trackSO = ScriptableObject.CreateInstance<TimelineTrack_Animation_SO>();
                trackSO.name = "AnimationTrack";
                // TimelineTrack_Animation_SO trackSO = ScriptableObject.CreateInstance<TimelineTrack_Animation_SO>();
                // Debug.Log($"{(trackSO == null ? "track为空" : "track不为空")}");
                // Debug.Log($"{(trackSO.Track == null ? "track为空" : "track不为空")}");
                // currentModel.Tracks.Add(trackSO);
                // AssetDatabase.CreateAsset(trackSO, AssetDatabase.GetAssetPath(currentModel));

                /*Tip：草了，我才反应过来，trackSO.track本来就是参与序列化的，直接自己创建实例了，而且本来轨道的类型信息已经包含在了trackSO中，这里很明显信息冗余了。*/
                // trackSO.track = _track as TimelineTrack_Animation;
                currentModel.Tracks.Add(trackSO);
                AssetDatabase.AddObjectToAsset(trackSO, currentModel);
                AssetDatabase.SaveAssets();

            }
            else if (_track is TimelineTrack_Audio)
            {
                TimelineTrack_SO<TimelineTrack_Audio> trackSO = ScriptableObject.CreateInstance<TimelineTrack_SO<TimelineTrack_Audio>>();
                // TimelineTrack_Audio_SO trackSO = ScriptableObject.CreateInstance<TimelineTrack_Audio_SO>();
                // trackSO.track = _track as TimelineTrack_Audio;
                // currentModel.Tracks.Add(trackSO);
                AssetDatabase.AddObjectToAsset(trackSO, currentModel);
                AssetDatabase.SaveAssets();
                currentModel.Tracks.Add(trackSO);
            }
            else if (_track is TimelineTrack_Hitbox)
            {
                TimelineTrack_SO<TimelineTrack_Hitbox> trackSO = ScriptableObject.CreateInstance<TimelineTrack_SO<TimelineTrack_Hitbox>>();
                // TimelineTrack_Hitbox_SO trackSO = ScriptableObject.CreateInstance<TimelineTrack_Hitbox_SO>();
                // trackSO.track = _track as TimelineTrack_Hitbox;
                // currentModel.Tracks.Add(trackSO);
                AssetDatabase.AddObjectToAsset(trackSO, currentModel);
                AssetDatabase.SaveAssets();
                currentModel.Tracks.Add(trackSO);
            }
            else if (_track is TimelineTrack_Particle)
            {
                TimelineTrack_SO<TimelineTrack_Particle> trackSO = ScriptableObject.CreateInstance<TimelineTrack_SO<TimelineTrack_Particle>>();
                // TimelineTrack_Particle_SO trackSO = ScriptableObject.CreateInstance<TimelineTrack_Particle_SO>();
                // trackSO.track = _track as TimelineTrack_Particle;
                // currentModel.Tracks.Add(trackSO);
                AssetDatabase.AddObjectToAsset(trackSO, currentModel);
                AssetDatabase.SaveAssets();
                currentModel.Tracks.Add(trackSO);
            }


            // AssetDatabase.CreateAsset(trackSO,)
            
            // switch (_type)
            // {
            //     case typeof(TimelineTrack_Animation):
            //         _track = new TimelineTrack_Animation();
            //         break;
            // }
        }

        public static void CreateAndSaveAsset<T>() where T : ScriptableObject
        {
            string path = EditorUtility.SaveFilePanel("选择保存路径", "Assets", typeof(T).Name,"asset");
            if (string.IsNullOrEmpty(path)) return;
            path = path.Substring(Application.dataPath.Length - "Assets".Length);
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            // EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
