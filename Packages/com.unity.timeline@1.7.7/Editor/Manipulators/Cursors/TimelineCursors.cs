using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// 负责管理鼠标指针的样式切换，以便在不同交互模式下给用户提供直观的视觉反馈。
    /// </summary>
    class TimelineCursors
    {
        //该常量纯粹作为键来映射到CursorInfo实例，访问非常快捷。
        public enum CursorType
        {
            MixBoth,
            MixLeft,
            MixRight,
            Replace,
            Ripple,

            Pan
        }

        class CursorInfo
        {
            public readonly string assetPath; //指向光标图片（PNG）在项目编辑器资源中的路径
            public readonly Vector2 hotSpot; //有效点，通常是左上角
            public readonly MouseCursor mouseCursorType; //Unity内置的鼠标样式类型

            public CursorInfo(string assetPath, Vector2 hotSpot, MouseCursor mouseCursorType)
            {
                this.assetPath = assetPath;
                this.hotSpot = hotSpot;
                this.mouseCursorType = mouseCursorType;
            }
        }

        const string k_CursorAssetRoot = "Cursors/";
        const string k_CursorAssetsNamespace = "Timeline.";
        const string k_CursorAssetExtension = ".png";

        const string k_MixBothCursorAssetName = k_CursorAssetsNamespace + "MixBoth" + k_CursorAssetExtension;
        const string k_MixLeftCursorAssetName = k_CursorAssetsNamespace + "MixLeft" + k_CursorAssetExtension;
        const string k_MixRightCursorAssetName = k_CursorAssetsNamespace + "MixRight" + k_CursorAssetExtension;
        const string k_ReplaceCursorAssetName = k_CursorAssetsNamespace + "Replace" + k_CursorAssetExtension;
        const string k_RippleCursorAssetName = k_CursorAssetsNamespace + "Ripple" + k_CursorAssetExtension;

        static readonly string s_PlatformPath = (Application.platform == RuntimePlatform.WindowsEditor) ? "Windows/" : "macOS/";
        static readonly string s_CursorAssetDirectory = k_CursorAssetRoot + s_PlatformPath;

        static readonly Dictionary<CursorType, CursorInfo> s_CursorInfoLookup = new Dictionary<CursorType, CursorInfo>
        {
            {CursorType.MixBoth,  new CursorInfo(s_CursorAssetDirectory + k_MixBothCursorAssetName,  new Vector2(16, 18), MouseCursor.CustomCursor)},
            {CursorType.MixLeft,  new CursorInfo(s_CursorAssetDirectory + k_MixLeftCursorAssetName,  new Vector2(7, 18), MouseCursor.CustomCursor)},
            {CursorType.MixRight, new CursorInfo(s_CursorAssetDirectory + k_MixRightCursorAssetName, new Vector2(25, 18), MouseCursor.CustomCursor)},
            {CursorType.Replace,  new CursorInfo(s_CursorAssetDirectory + k_ReplaceCursorAssetName,  new Vector2(16, 28), MouseCursor.CustomCursor)},
            {CursorType.Ripple,   new CursorInfo(s_CursorAssetDirectory + k_RippleCursorAssetName,   new Vector2(26, 19), MouseCursor.CustomCursor)},
            {CursorType.Pan,      new CursorInfo(null, Vector2.zero, MouseCursor.Pan)}
        };

        static readonly Dictionary<string, Texture2D> s_CursorAssetCache = new Dictionary<string, Texture2D>();

        static CursorType? s_CurrentCursor; //加？表示该变量可为空，这样可以用非空状态来进行一些判断。

        public static void SetCursor(CursorType cursorType)
        {
            //比对 s_CurrentCursor 避免重复设置
            if (s_CurrentCursor.HasValue && s_CurrentCursor.Value == cursorType) return;

            s_CurrentCursor = cursorType;
            var cursorInfo = s_CursorInfoLookup[cursorType]; //获取信息实例

            Texture2D cursorAsset = null;

            if (cursorInfo.mouseCursorType == MouseCursor.CustomCursor)
            {
                cursorAsset = LoadCursorAsset(cursorInfo.assetPath);
            }
            //Tip：只要知道，调用该方法，传入指针图片以及相关参数，就可以设置系统鼠标的图标。
            EditorGUIUtility.SetCurrentViewCursor(cursorAsset, cursorInfo.hotSpot, cursorInfo.mouseCursorType);
        }

        public static void ClearCursor()
        {
            if (!s_CurrentCursor.HasValue) return;

            EditorGUIUtility.ClearCurrentViewCursor();
            s_CurrentCursor = null;
        }

        static Texture2D LoadCursorAsset(string assetPath)
        {
            if (!s_CursorAssetCache.ContainsKey(assetPath))
            { //缓存技巧
                s_CursorAssetCache.Add(assetPath, (Texture2D)EditorGUIUtility.Load(assetPath));
            }

            return s_CursorAssetCache[assetPath];
        }
    }
}
