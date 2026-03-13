using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.UISystem_New
{
    public class UIResourceManager : Singleton<UIResourceManager>
    // public class UIResourceManager
    {

        /*Tip：记录的是由预制体加载得到的原始的GO，没有出现在场景中，而当要打开某个面板、也就是要使用对应预制体时，就从这里实例化获取GO。*/
        // 缓存已加载的预制体
        // private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
        private Dictionary<UIPanelType, GameObject> prefabCache = new Dictionary<UIPanelType, GameObject>();
        //Tip：由资源管理器自己记录路径信息，而不是记录在运行时参与逻辑的控制器中，因为这个信息本来就不属于控制器的职责。
        private Dictionary<UIPanelType, string> prefabPaths = new Dictionary<UIPanelType, string>();

        //在管理器初始化时就必须在这里准备好。
        public void AddPrefabPath(UIPanelType _panel, string _path)
        {
            prefabPaths.Add(_panel, _path);
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="_path">Resources目录下的路径（不包含扩展名）</param>
        public GameObject LoadPrefab(UIPanelType _panelType)
        {
            // 先从缓存中查找
            if (prefabCache.ContainsKey(_panelType))
            {
                return prefabCache[_panelType];
            }

            // 加载新的预制体
            GameObject prefab = Resources.Load<GameObject>(prefabPaths.GetValueOrDefault(_panelType));

            if (prefab != null)
            {
                prefabCache[_panelType] = prefab;
                Debug.Log($"成功加载预制体: {_panelType}   {prefabPaths.GetValueOrDefault(_panelType)}");
            }
            else
            {
                Debug.LogError($"加载预制体失败: {_panelType}   {prefabPaths.GetValueOrDefault(_panelType)}");
            }

            return prefab;
        }

        /*Tip：实例化后就会直接出现在当前Active的场景中了。*/

        /// <summary>
        /// 实例化预制体
        /// </summary>
        public GameObject InstantiatePrefab(UIPanelType _panelType, Transform parent = null)
        {
            GameObject prefab = LoadPrefab(_panelType);
            if (prefab != null)
            {
                GameObject instance = GameObject.Instantiate(prefab, parent);
                instance.name = prefab.name; // 去掉"(Clone)"后缀
                return instance;
            }

            return null;
        }

        /// <summary>
        /// 实例化预制体（带位置和旋转）
        /// </summary>
        public GameObject InstantiatePrefab(UIPanelType _panelType, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject prefab = LoadPrefab(_panelType);
            if (prefab != null)
            {
                GameObject instance = GameObject.Instantiate(prefab, position, rotation, parent);
                instance.name = prefab.name;
                return instance;
            }

            return null;
        }

        /// <summary>
        /// 从缓存中获取预制体
        /// </summary>
        public GameObject GetPrefab(UIPanelType _panel)
        {
            return prefabCache.ContainsKey(_panel) ? prefabCache[_panel] : null;
        }

        /// <summary>
        /// 检查是否已加载
        /// </summary>
        public bool IsLoaded(UIPanelType _panel)
        {
            return prefabCache.ContainsKey(_panel);
        }

        /// <summary>
        /// 从缓存中移除预制体
        /// </summary>
        public void UnloadPrefab(UIPanelType _panel)
        {
            if (prefabCache.ContainsKey(_panel))
            {
                // Resources.UnloadUnusedAssets(); // 复杂项目中才需要，简单测试可以不加
                prefabCache.Remove(_panel);
                Debug.Log($"已卸载: {_panel}");
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearCache()
        {
            prefabCache.Clear();
            // Resources.UnloadUnusedAssets();
            Debug.Log("已清空所有资源缓存");
        }
    }
}