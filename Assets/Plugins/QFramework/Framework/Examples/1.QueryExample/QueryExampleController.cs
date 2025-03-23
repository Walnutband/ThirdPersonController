using System.Collections.Generic;
using UnityEngine;

namespace QFramework.Example
{
    public class QueryExampleController : MonoBehaviour, IController
    {
        public class StudentModel : AbstractModel
        {

            public List<string> StudentNames = new List<string>()
            {
                "张三",
                "李四"
            };

            protected override void OnInit()
            {

            }
        }

        public class TeacherModel : AbstractModel
        {
            public List<string> TeacherNames = new List<string>()
            {
                "王五",
                "赵六"
            };

            protected override void OnInit()
            {

            }
        }

        // Architecture查询功能的架构
        public class QueryExampleApp : Architecture<QueryExampleApp>
        {//在框架类中注册各模块，然后就通过框架类来获取模块
            protected override void Init() //注册Model，就是注册数据
            {
                this.RegisterModel(new StudentModel());
                this.RegisterModel(new TeacherModel());
            }
        }


        /// <summary>
        /// 获取学校的全部人数
        /// </summary>
        public class SchoolAllPersonCountQuery : AbstractQuery<int> //返回int类型，规定为对于int类型数据的查询
        {
            //当调用到这个方法时，就已经在AbstractArchitecture类中的SendQuery调用的DoQuery中将传入的Query类设置了架构，然后调用OnDo方法
            //
            protected override int OnDo()
            {//传入对应的Model类作为泛型参数，就会通过Model类本身获取其所属框架类，然后通过框架类获取对应的Model类
                return this.GetModel<StudentModel>().StudentNames.Count +
                       this.GetModel<TeacherModel>().TeacherNames.Count;
            }
        }

        private int mAllPersonCount = 0;

        private void OnGUI()
        {
            GUILayout.Label(mAllPersonCount.ToString()); //标签显示学校总人数

            if (GUILayout.Button("查询学校总人数")) //点击则发送请求
            {
                mAllPersonCount = this.SendQuery(new SchoolAllPersonCountQuery());
            }
        }

        public IArchitecture GetArchitecture()
        {
            return QueryExampleApp.Interface;
        }
    }
}