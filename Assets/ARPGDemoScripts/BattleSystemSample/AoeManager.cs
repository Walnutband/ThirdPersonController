using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARPGDemo.BuffSystemSample
{
    ///<summary>
    ///负责aoe的移动、生命周期等
    ///还负责aoe和角色、子弹的碰撞，需要加aoe碰撞也在这里。值得注意的是：aoe是主体
    ///aoe捕捉范围与子弹碰撞不同的是，他不判断角色的体型（hitRadius或者bodyRadius），当然如果需要也可以加上，只是这个demo里不需要
    ///</summary>
    public class AoeManager : MonoBehaviour
    {
        private void FixedUpdate()
        {
            GameObject[] aoe = GameObject.FindGameObjectsWithTag("AoE"); //AOE以单独的游戏对象来出现，当然这里现场查找显然不合适，怎么也应该有专门的容器存储，然后遍历处理。
            if (aoe.Length <= 0) return;
            //在这个Demo中，主角和敌人的游戏对象都是Character，并且发射的子弹（其实敌人发射的是小火箭）都是Bullet。
            GameObject[] cha = GameObject.FindGameObjectsWithTag("Character");
            GameObject[] bullet = GameObject.FindGameObjectsWithTag("Bullet");

            float timePassed = Time.fixedDeltaTime;

            for (int i = 0; i < aoe.Length; i++)
            {
                AoeState aoeState = aoe[i].GetComponent<AoeState>();
                if (!aoeState) continue;

                //首先是aoe的移动
                if (aoeState.duration > 0 && aoeState.tween != null)
                {
                    AoeMoveInfo aoeMoveInfo = aoeState.tween(aoe[i], aoeState.tweenRunnedTime);
                    aoeState.tweenRunnedTime += timePassed;
                    aoeState.SetMoveAndRotate(aoeMoveInfo);
                }
                //因为每次都是直接从场景中查找指定Tag的游戏对象，找到的可能是之前创建的，所以通过一个标签变量justCreated来筛选刚创建的AOE对象。
                if (aoeState.justCreated == true)
                {
                    //刚创建的，走onCreate
                    aoeState.justCreated = false;
                    //捕获所有角色
                    for (int m = 0; m < cha.Length; m++)
                    {
                        if (
                            cha[m] &&
                            Utils.InRange( //TODO：这里是以各自的pivot坐标来计算的，没有考虑个体的宽度，因为是手动计算、没有直接使用物理碰撞体，但实际大概需要考虑个体宽度才更加准确，不过也要看游戏设计，因为AOE总会涉及到“擦边”的操作
                                aoe[i].transform.position.x, aoe[i].transform.position.z,
                                cha[m].transform.position.x, cha[m].transform.position.z,
                                aoeState.radius
                            ) == true
                        )
                        {
                            aoeState.characterInRange.Add(cha[m]);
                        }
                    }
                    //捕获所有的子弹
                    for (int m = 0; m < bullet.Length; m++)
                    {
                        if (
                            bullet[m] &&
                            Utils.InRange(
                                aoe[i].transform.position.x, aoe[i].transform.position.z,
                                bullet[m].transform.position.x, bullet[m].transform.position.z,
                                aoeState.radius
                            ) == true
                        )
                        {
                            aoeState.bulletInRange.Add(bullet[m]);
                        }
                    }
                    //执行OnCreate（运行时，在其他类中生成脚本）
                    if (aoeState.model.onCreate != null)
                    {
                        aoeState.model.onCreate(aoe[i]);
                    }

                }
                else
                {
                    //已经创建完成的
                    //先抓角色离开事件
                    List<GameObject> leaveCha = new List<GameObject>();
                    List<GameObject> toRemove = new List<GameObject>();
                    for (int m = 0; m < aoeState.characterInRange.Count; m++)
                    {
                        if (aoeState.characterInRange[m] != null)
                        {
                            if (Utils.InRange(
                                    aoe[i].transform.position.x, aoe[i].transform.position.z,
                                    aoeState.characterInRange[m].gameObject.transform.position.x, aoeState.characterInRange[m].gameObject.transform.position.z,
                                    aoeState.radius
                                ) == false
                            )
                            {
                                leaveCha.Add(aoeState.characterInRange[m]);
                                toRemove.Add(aoeState.characterInRange[m]);
                            }
                        }
                        else
                        {
                            toRemove.Add(aoeState.characterInRange[m]); //移除列表中的空元素（是元素，但是引用为空）
                        }

                    }
                    for (int m = 0; m < toRemove.Count; m++)
                    {
                        aoeState.characterInRange.Remove(toRemove[m]);
                    }
                    if (aoeState.model.onChaLeave != null)
                    {/*直接对这次所有离开AOE的对象统一处理*/
                        aoeState.model.onChaLeave(aoe[i], leaveCha);
                    }

                    //再看进入的角色
                    List<GameObject> enterCha = new List<GameObject>();
                    for (int m = 0; m < cha.Length; m++)
                    {
                        if (
                            cha[m] &&
                            aoeState.characterInRange.IndexOf(cha[m]) < 0 &&
                            Utils.InRange(
                                aoe[i].transform.position.x, aoe[i].transform.position.z,
                                cha[m].transform.position.x, cha[m].transform.position.z,
                                aoeState.radius
                            ) == true
                        )
                        {
                            enterCha.Add(cha[m]);
                        }
                    }
                    if (aoeState.model.onChaEnter != null)
                    {//直接对这次所有进入AOE的对象统一处理
                        aoeState.model.onChaEnter(aoe[i], enterCha);
                    }
                    for (int m = 0; m < enterCha.Count; m++)
                    {
                        if (enterCha[m] != null && enterCha[m].GetComponent<ChaState>() && enterCha[m].GetComponent<ChaState>().dead == false)
                        {
                            aoeState.characterInRange.Add(enterCha[m]);
                        }
                    }

                    //子弹离开
                    List<GameObject> leaveBullet = new List<GameObject>();
                    toRemove = new List<GameObject>();
                    for (int m = 0; m < aoeState.bulletInRange.Count; m++)
                    {
                        if (aoeState.bulletInRange[m])
                        {
                            if (Utils.InRange(
                                    aoe[i].transform.position.x, aoe[i].transform.position.z,
                                    aoeState.bulletInRange[m].gameObject.transform.position.x, aoeState.bulletInRange[m].gameObject.transform.position.z,
                                    aoeState.radius
                                ) == false
                            )
                            {
                                leaveBullet.Add(aoeState.bulletInRange[m]);
                                toRemove.Add(aoeState.bulletInRange[m]);
                            }
                        }
                        else
                        {
                            toRemove.Add(aoeState.bulletInRange[m]);
                        }

                    }
                    for (int m = 0; m < toRemove.Count; m++)
                    {
                        aoeState.bulletInRange.Remove(toRemove[m]);
                    }
                    if (aoeState.model.onBulletLeave != null)
                    {//子弹离开和子弹进入 这类回调就是取决于AOE本身的功能设计了。
                        aoeState.model.onBulletLeave(aoe[i], leaveBullet);
                    }
                    toRemove = null;

                    //子弹进入
                    List<GameObject> enterBullet = new List<GameObject>();
                    for (int m = 0; m < bullet.Length; m++)
                    {
                        if (
                            bullet[m] &&
                            aoeState.bulletInRange.IndexOf(bullet[m]) < 0 &&
                            Utils.InRange(
                                aoe[i].transform.position.x, aoe[i].transform.position.z,
                                bullet[m].transform.position.x, bullet[m].transform.position.z,
                                aoeState.radius
                            ) == true
                        )
                        {
                            enterBullet.Add(bullet[m]);
                        }
                    }
                    if (aoeState.model.onBulletEnter != null)
                    {
                        aoeState.model.onBulletEnter(aoe[i], enterBullet);
                    }
                    for (int m = 0; m < enterBullet.Count; m++)
                    {
                        if (enterBullet[m] != null)
                        {
                            aoeState.bulletInRange.Add(enterBullet[m]);
                        }
                    }
                }
                //然后是aoe的duration
                aoeState.duration -= timePassed; //剩余持续时间减少。
                aoeState.timeElapsed += timePassed; //已经存在时间增加。
                                                    //最后是onTick，remove
                if (aoeState.duration <= 0 || aoeState.HitObstacle() == true)
                {
                    if (aoeState.model.onRemoved != null)
                    {
                        aoeState.model.onRemoved(aoe[i]);
                    }
                    /*销毁AOE对象，不过感觉这种应该会使用对象池而不是临时创建、临时销毁*/
                    Destroy(aoe[i]);
                    continue;
                }
                else
                {
                    if ( //Ques：暂时没看懂下面的算式是怎么回事
                    /*Tip：大概知道了，这是就是将秒s转换为毫秒ms来进行比较，%就是取余数，因为帧的时间单位是毫秒，这样转化为毫秒之后，就像统一了单位，可以进行整数倍数的比较，
                    否则必然会掺杂浮点数导致计算不准确。——但是我仍然怀疑这里取余数真的能每次到时间都能准确取到0吗？？*/
                        aoeState.model.tickTime > 0 && aoeState.model.onTick != null &&
                        Mathf.RoundToInt(aoeState.duration * 1000) % Mathf.RoundToInt(aoeState.model.tickTime * 1000) == 0
                    )
                    {
                        aoeState.model.onTick(aoe[i]);
                    }
                }


            }


        }
    }
}