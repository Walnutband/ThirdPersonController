using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///负责子弹的一切，包括移动、生命周期等
    ///还负责子弹和角色等的碰撞，需要加入子弹与子弹碰撞也在这里。值得注意的是：子弹是主体
    ///</summary>
    public class BulletManager : MonoBehaviour
    {
        private void FixedUpdate()
        {
            /*Tip：没有使用物理引擎，而是在自己实现的FixedUpdate方法中对场景中存在的Bullet和Character进行遍历，逐个判断子弹是否命令目标、以及执行相关逻辑，
            显然这是一个O(m*n)的操作，但是这相当于替代了物理引擎的子弹碰撞检测，其实物理检测的逻辑也是这样类似的遍历，只是可能会进行一些额外的优化比如数据密集计算，
            但都是基于遍历。
            暂时不知道这种做法是否具有真正的价值，只是可以肯定对于检测过程具有更高的控制权。但是这毕竟只是一个2D俯视角Demo（虽然是3D素材，但是游戏本质是2D的），在很多
            检测方式上都极大简化，如果是3D的话，要用到各种形状的碰撞体，估计还是只能使用物理引擎，但也会存在与2D游戏那样比较简单的检测，这样可能也可以自主实现检测逻辑。*/

            /*TODO：正式项目中当然不能这样每帧查找，而是应该在这些对象比如Bullet产生的同时就注册到这里，*/
            GameObject[] bullet = GameObject.FindGameObjectsWithTag("Bullet");
            if (bullet.Length <= 0) return;
            GameObject[] character = GameObject.FindGameObjectsWithTag("Character");
            if (bullet.Length <= 0 || character.Length <= 0) return;

            float timePassed = Time.fixedDeltaTime;

            //逐个子弹地处理逻辑。
            for (int i = 0; i < bullet.Length; i++)
            {
                BulletState bs = bullet[i].GetComponent<BulletState>();
                if (!bs || bs.hp <= 0) continue; //TODO：如果存在但是hp不为正的话，说明没有及时移除掉，那么应该在此处补上移除逻辑的。

                //如果是刚创建的，那么就要处理刚创建的事情
                if (bs.timeElapsed <= 0 && bs.model.onCreate != null)
                {
                    bs.model.onCreate(bullet[i]);
                }

                //处理子弹命中纪录信息
                //Tip：这里非常关键，属于攻击判定所要处理的核心问题之一。
                int hIndex = 0;
                while (hIndex < bs.hitRecords.Count)
                {
                    bs.hitRecords[hIndex].timeToCanHit -= timePassed;
                    if (bs.hitRecords[hIndex].timeToCanHit <= 0 || bs.hitRecords[hIndex].target == null)
                    {
                        //理论上应该支持可以鞭尸，所以即使target dead了也得留着……
                        bs.hitRecords.RemoveAt(hIndex); //Tip：移除后会自动将后续元素向前移动，所以不需要++，但可能更好的处理方式是直接置空而非移除，等待遍历完成之后再统一移除。
                    }
                    else
                    {
                        hIndex += 1;
                    }
                }

                //处理子弹的移动信息
                bs.SetMoveForce(
                    bs.tween == null ? Vector3.forward : bs.tween(bs.timeElapsed, bullet[i], bs.followingTarget)
                );

                //处理子弹的碰撞信息，如果子弹可以碰撞，才会执行碰撞逻辑
                if (bs.canHitAfterCreated > 0)
                {
                    bs.canHitAfterCreated -= timePassed;
                }
                else
                {
                    float bRadius = bs.model.radius;
                    int bSide = -1;
                    if (bs.caster)
                    {
                        ChaState bcs = bs.caster.GetComponent<ChaState>();
                        if (bcs)
                        {
                            bSide = bcs.side; //明确角色的阵营，说白了就是敌伤和友伤。
                        }
                    }

                    for (int j = 0; j < character.Length; j++)
                    {
                        //这是是否可以命中的前提条件。
                        if (bs.CanHit(character[j]) == false) continue;

                        ChaState cs = character[j].GetComponent<ChaState>();
                        //不可能发生命中的情况。
                        if (!cs || cs.dead == true || cs.immuneTime > 0) continue;
                        //处理角色的阵营
                        if (
                            (bs.model.hitAlly == false && bSide == cs.side) ||
                            (bs.model.hitFoe == false && bSide != cs.side)
                        ) continue;

                        float cRadius = cs.property.hitRadius;
                        Vector3 dis = bullet[i].transform.position - character[j].transform.position;
                        /*就是判断Bullet与目标的距离是否小于子弹半径加上目标半径，或者说两个圆是否有交集。*/
                        //Tip：不使用物理系统，那么就是按照这样的几何算法、向量算法来判断的，当然物理系统内部逻辑其实也是这样的算法。
                        if (Mathf.Pow(dis.x, 2) + Mathf.Pow(dis.z, 2) <= Mathf.Pow(bRadius + cRadius, 2))
                        {
                            //命中了，减一次命中次数
                            bs.hp -= 1; 
                            //触发命中回调
                            if (bs.model.onHit != null)
                            {
                                bs.model.onHit(bullet[i], character[j]); //子弹命中时，该事件本身能够提供的信息就是子弹对象本身以及击中者对象 本身
                            }
                            //还能命中才记录，否则记录就没意义了。
                            if (bs.hp > 0)
                            {
                                bs.AddHitRecord(character[j]); //记录到击中列表中
                            }
                            else
                            {
                                Destroy(bullet[i]);
                                continue;
                            }
                        }
                    }
                }

                //两种情况销毁：
                //1，命中次数用完了
                //2，持续时间用完了

                ///生命周期的结算
                bs.duration -= timePassed;
                bs.timeElapsed += timePassed;
                if (bs.duration <= 0 || bs.HitObstacle() == true)
                {
                    if (bs.model.onRemoved != null)
                    {
                        bs.model.onRemoved(bullet[i]);
                    }
                    Destroy(bullet[i]); //销毁子弹游戏对象（其实更好的做法是放入对象池）
                    continue;
                }
            }
        }
    }
}