using QFramework;
using QFramework.Example;
using QFramework.PointGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetDataCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        IPrefsStorage storage = this.GetUtility<IPrefsStorage>();
        storage.ClearData();
        //这里的空引用错误也和GameModel的一样，因为获取时传入的泛型和在架构初始化中注册时传入的泛型类型不同
        this.GetModel<IGameModel>().LoadData(storage); //清空后记得重新加载数据
    }
}
