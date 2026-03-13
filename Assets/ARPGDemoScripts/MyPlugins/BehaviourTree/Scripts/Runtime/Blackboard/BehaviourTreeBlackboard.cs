using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MyPlugins.BehaviourTree
{   
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "MyBehaviourTree/Blackboard")]
    public class BehaviourTreeBlackboard : ScriptableObject
    {
        /*存储了所有blackboard variable。
        在Blackboard视图中使用ListView来显示变量列表， 并且如此一来，可以非常方便地在窗口中添加新变量。但为了做到这一点，由于变量有多种类型，
        而要将其放入到一个列表中，就必须进行封装，提供一个共同的抽象接口，作为列表的元素类型，然后为各个类型的变量编写相应的派生类，就可以将其
        添加到同一个列表中了。
        所以此处的BlackboardVariable就是一切黑板变量的抽象基类，*/
        public List<BlackboardVariable> variables = new List<BlackboardVariable>();

        /*变量key与变量实例构成的字典。因为字典的查找效率高，可以直接映射，不需要线性遍历*/
        private Dictionary<string, BlackboardVariable> dictionary = new Dictionary<string, BlackboardVariable>();

        //所属执行器，每个执行器都有一个自己的黑板
        //这个变量很可能不需要，就像行为树一样，都是作为资产文件，供执行器调用，只是一个单向调用的关系。
        // public BehaviourTreeRunner targetRunnner; 

        //初始化黑板，实际上就是建立字典
        public void InitBlackboard()
        {
            dictionary.Clear();
            for (int i = 0; i < variables.Count; i++)
            {
                BlackboardVariable var = variables[i];
                dictionary.Add(var.key, var); 
            }
        }

        public BehaviourTreeBlackboard Clone()
        {
            //黑板和其所具有的变量均采用资产文件的副本。可能不只是为了防止污染资产文件，因为这是加载到内存中，读写速度比外存快得多
            BehaviourTreeBlackboard blackboard = Instantiate(this);
            blackboard.variables = variables.ToList(); //创建列表副本
            blackboard.InitBlackboard(); //初始化字典
            return blackboard;
        }

        /// <summary>
        /// 获取全部variable
        /// </summary>
        /// <returns></returns>
        public BlackboardVariable[] GetAllVariables()
        {
            return variables.ToArray();
        }

        /// <summary>
        /// 获取key对应的variable
        /// </summary>
        /// <returns></returns>
        public T GetVariable<T>(string key) where T : BlackboardVariable //这个T就是确认的具体类型，取变量的同时顺便进行类型转换
        {
            return dictionary.TryGetValue(key, out BlackboardVariable val) ? (T)val : null;
        }

        /// <summary>
        /// 创建一个新的黑板变量
        /// </summary>
        public void CreateVariable(string newVariableKey, Type variableType, ref int selectedIndex) //传入变量名和类型
        {
            if (string.IsNullOrEmpty(newVariableKey)) //未输入变量名，不进行任何处理
            {
                return;
            }
            //其实这种处理很没必要，因为本来就应该使用者自己遵守一点点规范。这里要遵守的就是变量的命名规范
            // string k = new string( 
            //     //去掉空白字符，从字符串中移除所有空白字符，并返回剩余字符的数组
            //     //空白字符指用于在其他字符之间提供水平或垂直空间的字符，通常是空格、制表符、换行符、回车、换页垂直制表符和换行符
            //     newVariableKey.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()
            // );

            //查重（看人为设置的名称）
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].key == newVariableKey)
                {
                    // Debug.Log("变量 \"" + newVariableKey + "\"已存在");
                    selectedIndex = i;
                    return;
                }
            }
            //创建对应类型的实例。
            //Tip：这里的特殊性在于虽然是编辑时，但是想要通过在编辑窗口中指定的类型自动创建对应类型的实例
            CreateVariableInstance(newVariableKey, variableType);
        }

        //Tip：创建实际变量类型的实例，每次新增变量类型时都要在这里添加分支
        //不过从实际开发中的分工来看，添加变量类型和建立分支本来就都是程序的工作，一般也不会因此出现工作不同步的情况
        //TODO:应该可以使用代码生成相关的技术来自动生成，比如代码模板。
        //Tip：资产必须转换为具体类型，才能按照对应的成员进行存储，否则就只能存储基类的成员。
        private void CreateVariableInstance(string name, Type realType)
        {
            BlackboardVariable newVar = null;
            if (realType == typeof(BoolVariable)) newVar = ScriptableObject.CreateInstance(typeof(BoolVariable)) as BoolVariable;
            else if (realType == typeof(FloatVariable)) newVar = ScriptableObject.CreateInstance(typeof(FloatVariable)) as FloatVariable;
            else if (realType == typeof(StringVariable)) newVar = ScriptableObject.CreateInstance(typeof(StringVariable)) as StringVariable;
            else if (realType == typeof(TransformVariable)) newVar = ScriptableObject.CreateInstance(typeof(TransformVariable)) as TransformVariable;
            else if (realType == typeof(Vector3Variable)) newVar = ScriptableObject.CreateInstance(typeof(Vector3Variable)) as Vector3Variable;
            //注意参考BehaviourTree中的CreateNode方法
            if (newVar != null) 
            {
                //资产可以改名，主要是key不变就行，因为key才是用来访问的。
                //不过注意，这里是将变量都注册到黑板资产下作为子资产，而Unity中子资产是无法编辑的，即不可改名、不可删除等等，但是可以对其检视面板中的属性进行编辑。
                newVar.name = name; 
                newVar.key = name;
                variables.Add(newVar);
                //添加到该黑板作为子资产。不过应该和newVar不是同一个实例。
                AssetDatabase.AddObjectToAsset(newVar, this); 
                AssetDatabase.SaveAssets(); 
                //经测试发现其实在列表中引用的就是对应的子资产。
            }
            else
            {
                /*经测试，==和Equals都可以判断，并且进入分支。
                // if (realType == (typeof(BoolVariable))) Debug.Log("未添加成功变量");
                // if (realType.Equals(typeof(FloatVariable))) Debug.Log("未添加成功变量");
                // if (realType.Equals(typeof(StringVariable))) Debug.Log("未添加成功变量");
                // if (realType.Equals(typeof(TransformVariable))) Debug.Log("未添加成功变量");
                // if (realType.Equals(typeof(Vector3Variable))) Debug.Log("未添加成功变量");
                // if (null == variable) Debug.Log("空？");
                */
                Debug.Log($"未注册变量类型\"{realType.Name}\"\n需要在Blackboard类的CreateVariableInstance方法中建立分支！");
                // return null;
            }
        }

        /// <summary>
        /// 删除指定key的variable
        /// </summary>
        /// <param name="key"></param>
        public void DeleteVariable(string key)
        {
            //删除引用的实例，但元素本身还没有删除
            //注意这里要求类型必须是Object的派生类，所以应该让BlackboardVariable继承自Object或者其派生类
            DestroyImmediate(variables.Find(v => v.key == key)); 
            variables.RemoveAll(v => v == null); //清空所有空引用的对象
        }
    }

    //Tip：这个编辑器的目的，就是为了将黑板中的变量不可直接编辑。
    [CustomEditor(typeof(BehaviourTreeBlackboard))]
    public class BTBlackboardEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            SerializedProperty field = serializedObject.FindProperty("variables");
            PropertyField propertyField = new PropertyField(field);
            propertyField.SetEnabled(false); //限制不可直接在检视面板中编辑，主要是防止误操作，因为本来就是在编辑窗口中编辑
            propertyField.Bind(serializedObject); //绑定数据
            root.Add(propertyField);
            return root;
        }
    }
}