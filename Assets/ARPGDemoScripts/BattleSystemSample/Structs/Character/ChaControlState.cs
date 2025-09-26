using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    /*Tip：专门用一个结构体来记录角色的操作状态，在改变（ChaState）状态时直接修改成员值或者直接替换该结构体实例即可。
当然这个结构体的成员设置也是与游戏设计紧密相关的，而且应当是与专门记录基础属性的ChaProperty结合来编写的。
比如魂游的话，其实就不存在什么沉默、禁锢之类的花里胡哨的状态，也就是减慢移动速度、减慢体力恢复速度、减少血量上限，等等这样的作用于基础属性的效果；
而要是在比如英雄联盟这种游戏里面，角色操作就是移动、平A和QWER四个技能以及DF两个召唤师技能，效果主要分为伤害、位移、控制，*/
    ///<summary>
    ///角色的可操作状态，这个是根据游戏玩法来细节设计的，目前就用这个demo需要的
    ///</summary>
    public struct ChaControlState
    {
        ///<summary>
        ///是否可以移动坐标
        ///</summary>
        public bool canMove;

        ///<summary>
        ///是否可以转身
        ///</summary>
        public bool canRotate;

        ///<summary>
        ///是否可以使用技能，这里的是“使用技能”特指整个技能流程是否可以开启
        ///如果是类似中了沉默，则应该走buff的onCast，尤其是类似wow里面沉默了不能施法但是还能放致死打击（部分技能被分类为法术，会被沉默，而不是法术的不会）
        ///</summary>
        public bool canUseSkill;

        public ChaControlState(bool canMove = true, bool canRotate = true, bool canUseSkill = true)
        {
            this.canMove = canMove;
            this.canRotate = canRotate;
            this.canUseSkill = canUseSkill;
        }

        //恢复原状，就是默认状态，全部允许。
        public void Origin()
        {
            this.canMove = true; //禁锢
            this.canRotate = true; //我只知道可以旋转、不能移动的，没见过可以移动却不能旋转的。。。
            this.canUseSkill = true; //沉默
        }

        public static ChaControlState origin = new ChaControlState(true, true, true);

        ///<summary>
        ///昏迷效果（击晕之类的）
        ///</summary>
        public static ChaControlState stun = new ChaControlState(false, false, false);

        public static ChaControlState operator +(ChaControlState cs1, ChaControlState cs2)
        {
            return new ChaControlState(
                cs1.canMove & cs2.canMove,
                cs1.canRotate & cs2.canRotate,
                cs1.canUseSkill & cs2.canUseSkill
            );
        }
    }
}