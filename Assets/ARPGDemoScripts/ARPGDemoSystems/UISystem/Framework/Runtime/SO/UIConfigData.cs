using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;

namespace ARPGDemo.UISystem_Old
{

    [System.Serializable]
    public struct UIConfigItem
    {//预制体路径和所属的UILayer都是一个UI视图的基本属性。s
        //这就是注册在配置文件中的每个UI预制体的标识符，并且会使用该标识符来获取到UI视图的逻辑组件即UIView的相应派生类的Type信息，所以必须保证逻辑组件的类名与枚举类型UIViewType中相同
        public UIViewType UIViewType; 
        public string path; //资源路径
        public bool isPopWindow;
        public UILayer UILayer;
    }

    //Ques:到底是直接使用SO资产，还是专门转换为Json文件呢
    [CreateAssetMenu(fileName = "UIConfigData", menuName = "GoodUI/UIConfigData")]
    public class UIConfigData : ScriptableObject //使用UIConfigData可以方便地在检视器中编辑。
    {

        public List<UIConfigItem> UIConfig = new List<UIConfigItem>();

    #if UNITY_EDITOR
        [ContextMenu("导出UIConfigData为Json文件")]
        public void ExportToJson()
        {
            // 设置 Json 序列化设置，启用字符串枚举支持
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,  //加上通常的缩进，否则会发现挤在一团。
                Converters = new List<JsonConverter> { new StringEnumConverter() }, //让枚举类型能够被转换为枚举常量名，而不是枚举int值
                // ContractResolver = new DefaultContractResolver
                // {
                //     NamingStrategy = new CamelCaseNamingStrategy() // 可选：字段改为小写开头
                // }
            };

            // 使用 JsonUtility 将当前对象转换为 Json 字符串，此处会序列化所有可序列化的字段（包括 UIConfig 列表）
            // string json = JsonUtility.ToJson(this, true); // 第二个参数为 true，可以使其格式化输出
            string json = JsonConvert.SerializeObject(this.UIConfig, settings); //注意细节，序列化的对象时UIConfig字段，而不是该资产实例本身，否则会包含一些其他不需要的内容。

            // 打开保存文件对话框，初始目录设置为 Assets 文件夹，默认文件名为 UIConfigData.json
            // 转换为系统路径
            string absolutePath = Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(this);
            string directory = System.IO.Path.GetDirectoryName(absolutePath); //获取目录路径
            // string path = EditorUtility.SaveFilePanel("保存 UIConfig Json 文件", Application.dataPath, "UIConfigData.json", "json");
            string path = EditorUtility.SaveFilePanel("保存 UIConfig Json 文件", directory, "UIConfigData.json", "json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("保存被取消");
                return;
            }

            try
            {
                // 将 json 内容写入指定文件路径
                File.WriteAllText(path, json);
                Debug.Log("文件保存成功，路径：" + path);

                // 若文件保存到了 Assets 目录下，刷新资源数据库
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("保存文件失败：" + ex.Message);
            }
        }
    #endif
    }
}
