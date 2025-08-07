using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    static class TrackResourceCache
    {
        private static Dictionary<System.Type, GUIContent> s_TrackIconCache = new Dictionary<Type, GUIContent>(10); //缓存Track图标
        private static Dictionary<System.Type, Color> s_TrackColorCache = new Dictionary<Type, Color>(10); //缓存Track及其CLip标识色
        public static GUIContent s_DefaultIcon = EditorGUIUtility.IconContent("UnityEngine/ScriptableObject Icon");

        public static GUIContent GetTrackIcon(TrackAsset track)
        {
            if (track == null)
                return s_DefaultIcon;

            GUIContent content = null;
            if (!s_TrackIconCache.TryGetValue(track.GetType(), out content))
            {
                content = FindTrackIcon(track);
                s_TrackIconCache[track.GetType()] = content;
            }
            return content;
        }

        public static Texture2D GetTrackIconForType(System.Type trackType)
        {
            if (trackType == null || !typeof(TrackAsset).IsAssignableFrom(trackType))
                return null;

            GUIContent content;
            if (!s_TrackIconCache.TryGetValue(trackType, out content) || content.image == null)
                return s_DefaultIcon.image as Texture2D;
            return content.image as Texture2D;
        }

        public static Color GetTrackColor(TrackAsset track)
        {
            if (track == null)
                return Color.white;

            // Try to ensure DirectorStyles is initialized first
            // Note: GUISkin.current must exist to be able do so
            //Ques：GUISkin.current还可能不存在吗？？？
            if (!DirectorStyles.IsInitialized && GUISkin.current != null)
                DirectorStyles.ReloadStylesIfNeeded();

            Color color;
            //没找到缓存值，就现场获取。
            if (!s_TrackColorCache.TryGetValue(track.GetType(), out color))
            {
                var attr = track.GetType().GetCustomAttributes(typeof(TrackColorAttribute), true);
                if (attr.Length > 0)
                {
                    color = ((TrackColorAttribute)attr[0]).color;
                }
                else
                {
                    // case 1141958
                    // There was an error initializing DirectorStyles
                    /*Tip：就是单例，这里又发现了单例的一个好处，就是可以判断有无，而静态类就不行。该单例就是存储了一系列各部分可以使用的样式值，作为资产文件，可以方便地在检视器中国直接指定，然后全局可用，
                    所以从这里的顺序可以看到，获取Track颜色就是首先从该静态类中的缓存读取，没有就进入分支，
                    先从Track类本身标记的TrackColor特性获取，如果没有就是从DirectorStyles读取，如果还是没有，就设置为默认颜色white。*/
                    if (!DirectorStyles.IsInitialized)
                        return Color.white;

                    color = DirectorStyles.Instance.customSkin.colorDefaultTrackDrawer;
                }
                //缓存起来，使用元数据，这应该算是C#编程的一个思维
                s_TrackColorCache[track.GetType()] = color;
            }
            return color;
        }

        public static void ClearTrackIconCache()
        {
            s_TrackIconCache.Clear();
        }

        public static void SetTrackIcon<T>(GUIContent content) where T : TrackAsset
        {
            s_TrackIconCache[typeof(T)] = content;
        }

        public static void ClearTrackColorCache()
        {
            s_TrackColorCache.Clear();
        }

        public static void SetTrackColor<T>(Color c) where T : TrackAsset
        {
            s_TrackColorCache[typeof(T)] = c;
        }

        private static GUIContent FindTrackIcon(TrackAsset track)
        {
            // backwards compatible -- try to load from Gizmos folder
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/" + track.GetType().Name + ".png");
            if (texture != null)
                return new GUIContent(texture);

            // try to load based on the binding type
            var binding = track.outputs.FirstOrDefault();
            if (binding.outputTargetType != null)
            {
                // Type calls don't properly handle monobehaviours, because an instance is required to
                //  get the monoscript icons
                if (typeof(MonoBehaviour).IsAssignableFrom(binding.outputTargetType))
                {
                    texture = null;
                    var scripts = UnityEngine.Resources.FindObjectsOfTypeAll<MonoScript>();
                    foreach (var script in scripts)
                    {
                        if (script.GetClass() == binding.outputTargetType)
                        {
                            texture = AssetPreview.GetMiniThumbnail(script);
                            break;
                        }
                    }
                }
                else
                {
                    texture = EditorGUIUtility.FindTexture(binding.outputTargetType);
                }

                if (texture != null)
                    return new GUIContent(texture);
            }

            // default to the scriptable object icon
            return s_DefaultIcon;
        }
    }
}
