using UnityEditor;
using UnityEngine;

namespace Ilumisoft.VisualStateMachine
{
    [System.Serializable]
    public enum TransitionMode
    {
        Default = 0,
        Locked = 1,
    }

    [AddComponentMenu("")] //不会出现在Add Component菜单中
    public class StateMachineManager : MonoBehaviour
    {
        public static StateMachineManager Instance; //单例

        public Configuration Configuration;

        //当游戏运行刚开始并且正在加载第一个场景时回调。BeforeSceneLoad即表示在第一个场景的对象已经加载进入内存，但在Awake调用之前，回调。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] //必须是静态方法
        public static void InitializeOnLoad()
        {
            var container = new GameObject("State Machine Manager"); //创建空游戏对象
            var manager = container.AddComponent<StateMachineManager>(); //添加该组件

            manager.LoadSettings();

            //加载新场景时不要摧毁该对象，实际上就是自动把该对象放到一个叫做“DontDestroyOnLoad”的额外场景中。不过貌似是一开始就放在该场景中
            DontDestroyOnLoad(container);

            Instance = manager; //引用实例

            container.hideFlags = HideFlags.HideInHierarchy; //不会在层级视图中显示，因为不需要在编辑中对其进行读写？
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        void LoadSettings()
        {
            var config = Configuration.Find(); //获取配置文件

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<Configuration>(); //只存在于内存中
            }

            Configuration = config;
        }

        //[CreateAssetMenu(fileName = "Configuration", menuName = "VisualSM/Configuration")] //该特性只对类声明有效
        // 添加菜单项，用于创建并保存 ScriptableObject 实例
        [MenuItem("Assets/Create/VisualSM/Configuration")]
        public static void CreateMyAsset()
        {
            // 创建 ScriptableObject 实例。该方法创建的实例存在于内存中，需要使用AssetDatabase类才能将其保存到硬盘上
            Configuration asset = ScriptableObject.CreateInstance<Configuration>();

            // 定义保存路径
            string path = "Assets/ConfigurationNew.asset";

            // 创建资产文件并保存
            AssetDatabase.CreateAsset(asset, path); //指定保存路径
            AssetDatabase.SaveAssets(); //将所有未保存的素材修改写入到硬盘中

            // 选择并聚焦新创建的资产（聚焦到project窗口并且选中新创建的配置文件）
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}