using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace QFramework.PointGame
{
    public interface IAchievementSystem : ISystem
    {
    }

    public class AchievementItem
    {
        public string Name { get; set; } //成就名字

        //Func 有返回值，用于处理需要结果的逻辑。Action 没有返回值，只处理执行逻辑。都是委托类型
        public Func<bool> CheckComplete { get; set; }

        public bool Unlocked { get; set; } //是否解锁
    }


    public class AchievementSystem : AbstractSystem, IAchievementSystem
    {
        private List<AchievementItem> mItems = new List<AchievementItem>(); //成就列表

        private bool mMissed = false;
        protected override void OnInit()
        {
            this.RegisterEvent<OnMissEvent>(e =>
            {
                mMissed = true;
            });

            this.RegisterEvent<GameStartEvent>(e =>
            {
                mMissed = false;
            });

            mItems.Add(new AchievementItem()
            {
                Name = "百分成就",
                CheckComplete = () => this.GetModel<IGameModel>().BestScore.Value > 100
            });

            mItems.Add(new AchievementItem()
            {
                Name = "手残",
                CheckComplete = () => this.GetModel<IGameModel>().Score.Value < 0
            });

            mItems.Add(new AchievementItem()
            {
                Name = "零失误成就",
                CheckComplete = () => !mMissed
            });

            mItems.Add(new AchievementItem()
            {
                Name = "零失误成就",
                CheckComplete = () => mItems.Count(item => item.Unlocked) >= 3
            });

            // 成就系统一般是持久化的，所以如果需要持久化也是在这个时机进行，可以让 Unlocked 变成 BindableProperty
            /*
            在游戏通关事件中检查完成的成就。更常见的的应该是在游戏过程中一旦完成就跳成就，
            但这样的问题是会增加检测成就的负担，尤其是对于成就很多的游戏，所以在通关时统一检查就更节省性能，《吸血鬼幸存者》就是这样的
            */
            //async e使用了 异步 Lambda 表达式，表示事件处理逻辑是异步的。e 是传入的事件对象（这里没有对 e 的属性进行操作，但它是事件触发的上下文数据）。
            this.RegisterEvent<GamePassEvent>(async e =>
            {
                //事件触发后，处理逻辑延迟了 0.1 秒。可能是为了确保某些异步逻辑或状态更新的完成。
                await Task.Delay(TimeSpan.FromSeconds(0.1f));

                foreach (var achievementItem in mItems)
                {
                    //没有解锁并且检查完成
                    if (!achievementItem.Unlocked && achievementItem.CheckComplete())
                    {
                        achievementItem.Unlocked = true;

                        Debug.Log("解锁 成就:" + achievementItem.Name);
                    }
                }
            });
        }
    }
}