using System.Collections.Generic;
using UnityEngine;

namespace QFramework.PointGame
{
    public interface IStorage : IUtility
    {
        void SaveInt(string key, int value);
        int LoadInt(string key, int defaultValue = 0); //默认值（也就是初始值），因为PlayerPrefs.GetInt的第二个参数是指定的默认值
    }

    public interface IPrefsStorage : IStorage
    {
        //在接口中只能定义属性而不能定义字段，因为字段其实是实例字段，会破坏接口的抽象性，
        //并且由于通过接口引用时只能访问接口中定义的成员，所以必须在接口中定义，而不能只在实现类中定义
        List<string> Keys { get; set; } //实现类中的定义可以更多但不能更少，比如此处如果只有get，那实现可以有set也可以没有，但必须有get
        void ClearData();
    }

    public class PlayerPrefsStorage : IPrefsStorage
    {
        private List<string> keys = new List<string>();
        public List<string> Keys { get => keys; set => keys = value; } //其实列表本来就是通过方法来操作的，所以不设置set都不影响

        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void ClearData()
        {
            foreach (var key in Keys)
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}