using UnityEngine;

namespace Ilumisoft.VisualStateMachine
{
    //该特性只对类声明有效，路径取决于打开菜单时鼠标所选择的文件位置
    //[CreateAssetMenu(fileName = "ConfigurationNew", menuName = "VisualSM/Configuration")]
    public class Configuration : ScriptableObject
    {
        //放在Resources文件夹下。注意这里的命名和实际的文件命名一定要保持相同，总之要保证能够找到
        //注意这里const和路径都是必要的，const需要在声明时就确定其值，并且属于类本身，即为隐含的static，所以可以被下面的静态方法所访问
        public const string ConfigurationPath = "Visual State Machine/Configuration";

        public TransitionMode TransitionMode;

        /// <summary>
        /// 加载预配置文件
        /// </summary>
        /// <returns></returns>
        public static Configuration Find() //写在该类是因为可以直接使用同类中定义的变量，也就是这里的路径
        {
            var result = Resources.Load<Configuration>(ConfigurationPath);

            return result;
        }
    }
}