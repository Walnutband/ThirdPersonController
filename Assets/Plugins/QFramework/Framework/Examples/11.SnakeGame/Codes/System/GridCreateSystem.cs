using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace SnakeGame
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGridCreateSystem : ISystem { }
    public class GridCreateSystem : AbstractSystem, IGridCreateSystem
    {
        private List<SpriteRenderer> renders;
        private Transform MapRoot; //网格节点的共同父对象
        private Sprite mWallSprite;
        private int mBlockIndex = 0;

        private Transform mFoodTrans; //食物对象的transform

        protected override void OnInit()
        {
            mWallSprite = Resources.Load<Sprite>("Sprites/Block"); //加载节点图片
            //初始位置在原点
            MapRoot = new GameObject("GameMap").transform; //新建一个名为“GameMap”的空游戏对象，并且获取其transform组件
            renders = new List<SpriteRenderer>(16); //初始容量为16个

            var foodRender = new GameObject("Food").AddComponent<SpriteRenderer>(); //新建“Food”对象并添加SpriteRenderer组件以渲染二维图片
            foodRender.sprite = Resources.Load<Sprite>("Sprites/Circle");
            foodRender.color = Color.yellow;
            foodRender.sortingOrder = 1; //设置渲染顺序，以便显示在节点表面
            mFoodTrans = foodRender.transform; //任何组件都可以直接访问其所挂载对象的transform组件（Component.transform属性）

            this.RegisterEvent<CreateGridEvent>(OnGridCreated); //创造网格（其实是创造单个节点）            
            this.RegisterEvent<CreateFoodEvent>(OnFoodCreated); //创造食物 
            this.RegisterEvent<GameInitEndEvent>(OnGameInitEnd); //游戏初始化结束
        }
        private void OnFoodCreated(CreateFoodEvent e) => mFoodTrans.localPosition = new Vector2(e.pos.y, e.pos.x);
        private void OnGameInitEnd(GameInitEndEvent e) => mBlockIndex = 0;
        private void OnGridCreated(CreateGridEvent e)
        {
            //实际节点数量超过预设的renders容量之后逐个增加（其实没感到这个预设容量有啥用）
            if (mBlockIndex == renders.Count) renders.Add(new GameObject(e.type.ToString()).AddComponent<SpriteRenderer>());
            renders[mBlockIndex].color = e.type == Node.E_Type.Wall ? Color.black : Color.gray; //墙是黑色
            renders[mBlockIndex].transform.localPosition = e.pos;
            renders[mBlockIndex].transform.SetParent(MapRoot);
            renders[mBlockIndex].sprite = mWallSprite;
            mBlockIndex++;
        }
    }
}