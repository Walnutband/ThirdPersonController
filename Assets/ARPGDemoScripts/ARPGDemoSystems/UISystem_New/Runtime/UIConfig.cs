using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.UISystem_New
{
    [CreateAssetMenu(fileName = "UIConfig",menuName = "ARPGDemo/UISystem_New/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        public List<PanelInfo> panels = new List<PanelInfo>();

        [System.Serializable]
        public class PanelInfo
        {
            public UIPanelType panelType; //使用枚举作为标识，方便编辑，也避免字符串硬编码。
            // public GameObject prefab; //一个UI面板对应一个预制体。
            public UILayerType layerType; //所属UI层级，只需要指明标识信息，UI层级是唯一的，并且由UIManager来管理。
            public string prefabPath; //预制体路径，
            public bool isFullView; //是否为全屏UI。
            // public bool cachePanel = true;       // 是否缓存
        }
    }

}