using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SkierFramework
{
    //使用Json进行保存，读取后转换到UIConfig。
    [System.Serializable]
    public class UIConfigJson
    {
        public string uiType; //应当是
        public string path; //资源路径
        public bool isWindow;
        public string uiLayer;
    }

    public class UIConfig
    {
        public string path; //资源路径，就是预制体的路径，因为UI对象往往都要配置为一个个预制体资产，以便加载和复用。
        //ui类型。这个要看具体怎么定义，因为UI本身应该是一个个组件，但实际的UI控件肯定是一个有组织的多个游戏对象构成的整体，而UI控件又会以一定的方式构成UI视图，各个UI视图就构成了UI界面
        public UIType uiType; //具体来说，UIType中定义的各个常量就是UIView的各个派生类的名称
        //UI层级。还是根据具体项目需求，通过划分层级可以更好地处理不同UI视图之间的遮挡关系、打开和关闭，等等功能，以及在UI控件的组织上都更加有条理、有逻辑。
        public UILayer uiLayer; 
        public Type viewType;
        public bool isWindow; //是否为窗口

        private const string UIConfigPath = "Assets/AssetsPackage/UI/UIConfig.json"; //注意这种绝对字符串。json这种文本文件在Unity中都会被转换为TextAsset类。

        /// <summary>
        /// 获取所有配置
        /// </summary>
        /// <remarks></remarks>
        public static AsyncOperationHandle GetAllConfigs(Action<List<UIConfig>> callback)
        {
            //通过Addressable的API异步加载UIConfigPath指定路径上的json配置文件,所以路径和文件内容一定要按规矩设置好.
            return ResourceManager.Instance.LoadAssetAsync<UnityEngine.TextAsset>(UIConfigPath, (textAsset) => //这个回调是加载结束时触发,传入加载得到的TextAsset实例。
            {
                if (textAsset != null)
                {
                    var list = new List<UIConfig>();
                    //使用Newtonsoft.Json的API将读取到的文本内容反序列化为指定的.Net类型对象,当前前提是读取的Json文件内容确实是按照该类型的数据成员来编写的。
                    var uiConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UIConfigJson>>(textAsset.text);
                    foreach (var config in uiConfigs)
                    {
                        //将字符串解析为枚举常量。
                        if (!Enum.TryParse<UILayer>(config.uiLayer, out UILayer layer))
                        {
                            layer = UILayer.NormalLayer; //看来默认就是NormalLayer
                            Debug.LogErrorFormat("UIConfig.json 中的：{0}  uiLayer解析异常 {1}", config.path, config.uiLayer);
                        }
                        if (!Enum.TryParse<UIType>(config.uiType, out UIType type))
                        {
                            Debug.LogErrorFormat("UIConfig.json 中的：{0}  uiType解析异常 {1}", config.path, config.uiType);
                        }
                        Type viewType = GetType(config.uiType.ToString()); //GetType获取的类型名就会包含命名空间，可以通过Type.Name获取类型名，Type.FullName获取完整名。
                        if (viewType == null) //前后的区别在于，这里会加上UIConfig所在的命名空间名称。但是我比较疑惑如果前面都没有找到，那后面加上命名空间就有可能找到吗？
                        {//命名空间.类型名
                            viewType = GetType($"{typeof(UIConfig).Namespace}.{config.uiType}");
                        }
                        list.Add(new UIConfig //设置配置数据，就可以得到一个UIConfig的实例了，也就代表一个特定UI元素（UI控件，UI视图）的各项基本数据。
                        {   
                            path = config.path,
                            uiLayer = layer,
                            uiType = type,
                            viewType = viewType,
                            isWindow = config.isWindow
                        });
                    }
                    callback?.Invoke(list);
                }
                else
                {
                    Debug.LogError("未找到配置：" + UIConfigPath);
                }
            }, true);
        }

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}