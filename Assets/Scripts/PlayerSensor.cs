using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    public enum NextPlayerMovement
    {
        jump,//跳跃
        climbLow,//低位攀爬
        climbHigh,//高位攀爬
        vault//翻越
    }
    public NextPlayerMovement nextMovement = NextPlayerMovement.jump;

    public float lowClimbHeight = 0.5f;//低位攀爬的高度上限
    public float highClimbHeight = 2f;//高位攀爬的高度上限
    public float checkDistance = 1f;
    float climbDistance;
    public float climbAngle = 45f;//攀爬方向夹角范围
    public Vector3 climbHitNormal;//射线碰撞表面的法线
    public Vector3 ledge;
    public float bodyHeight = 1f;//身体从墙上翻越所需的空隙高度，以免下面判定成功，而上面有墙阻挡，就会出错

    // Start is called before the first frame update
    void Start()
    {
        climbDistance = Mathf.Cos(climbAngle) * checkDistance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public NextPlayerMovement ClimbDetect(Transform playerTransform, Vector3 inputDirection, float offset)//还可以加上一个偏移量以在高速移动时能够在更远的距离进行翻越
    {
        /*此处发射方向为人物前方，因为要首先满足最基本的翻越条件：（在一定角度范围内）
         面向墙。然后才是更多层的条件判断，而后的发射方向就是墙壁法线的反向即垂直入墙。
        也可以看到，前者是checkDistance，后者是climbDistance*/
        if (Physics.Raycast(playerTransform.position + Vector3.up * lowClimbHeight, playerTransform.forward, out RaycastHit obsHit, checkDistance + offset))
        {
            climbHitNormal = obsHit.normal;
            float climbOffset = Mathf.Cos(climbAngle) * offset; 
            //在人物朝向与墙面偏离一定距离时不能翻越，这是限制，但同时也能防止误操作
            //加入玩家操作方向的判断（即镜头方向判断）后操作就更精准了
            //其实只要视角是朝向墙壁的话，按一下前进键再加翻越就可以了，所以有些按键教学提示总会加上方向键可能就是为了调整人物的方向
            if (Vector3.Angle(-climbHitNormal, playerTransform.forward) > climbAngle || Vector3.Angle(-climbHitNormal, inputDirection) > climbAngle)
            {
                return NextPlayerMovement.jump;
            }
            //不能翻就跳，可以翻就继续考虑更多情况
            //根据游戏设计自行设计可攀爬高度进行相应编程，此处设计为3.5米
            //思考：一点一点地向上检查，而不是直接从高处向下发射射线检查
            if (Physics.Raycast(playerTransform.position + Vector3.up * lowClimbHeight, -climbHitNormal, out RaycastHit firstWallHit, climbDistance + climbOffset))
            {
                if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight), -climbHitNormal, out RaycastHit secondWallHit, climbDistance + climbOffset))
                {
                    if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 2), -climbHitNormal, out RaycastHit thirdWallHit, climbDistance + climbOffset))
                    {
                        if (Physics.Raycast(playerTransform.position + Vector3.up * (lowClimbHeight + bodyHeight * 3), -climbHitNormal, climbDistance + climbOffset))
                        {
                            return NextPlayerMovement.jump;
                        }
                        /*2.5(0.5+1*2)个单位（一般就是米m）处的墙壁碰撞点，此时已经判断3.5m处没有墙，所以
                         向上到3.5m向下bodyHeight单位长度的射线就可以得到墙壁顶部边缘位置信息
                        并且要知道为什么不直接从高度向下发射一条射线，这就是对于不确定的情况的处理，
                        但是在特殊的具体的游戏设计中可以避免这样的不确定情况，但是如此一来就会在设计
                        上极大受限*/
                        // 2.5~3.5之间
                        else if (Physics.Raycast(thirdWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                        {
                            ledge = ledgeHit.point;
                            Debug.Log("高度" + (ledge.y - playerTransform.position.y));
                            return NextPlayerMovement.climbHigh;
                        }
                    }
                    // 1.5~2.5之间
                    else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                    {
                        ledge = ledgeHit.point;
                        Debug.Log("高度" + (ledge.y - playerTransform.position.y));
                        //计算相对高度即可，所以不用在意绝对高度多少（还是看设计）
                        if (ledge.y - playerTransform.position.y > highClimbHeight)
                        {
                            return NextPlayerMovement.climbHigh;
                        }
                        //（假定平滑）墙体厚度小于0.2m就直接翻越vault
                        else if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.3f, Vector3.down, bodyHeight))
                        {
                            return NextPlayerMovement.climbLow;
                        }
                        else
                        {
                            return NextPlayerMovement.vault;
                        }
                    }

                }
                //0.5~1.5之间
                //显然设定只有在低位攀爬时才可能进行翻越
                else if (Physics.Raycast(firstWallHit.point + Vector3.up * bodyHeight, Vector3.down, out RaycastHit ledgeHit, bodyHeight))
                {
                    ledge = ledgeHit.point;
                    Debug.Log("高度" + (ledge.y - playerTransform.position.y));
                    /*（假定平滑）墙体厚度小于0.2m就直接翻越vault，问题在于，墙的这一边可能更矮，墙的另一边可能更高，
                     * 所以即使高度小于2m，厚度小于指定长度，也可能因为检测到对面更高的地面而低位攀爬*/
                    if (Physics.Raycast(secondWallHit.point + Vector3.up * bodyHeight - climbHitNormal * 0.2f, Vector3.down, bodyHeight))
                    {
                        return NextPlayerMovement.climbLow;
                    }
                    else
                    {
                        return NextPlayerMovement.vault;
                    }
                }
            }
        }
        return NextPlayerMovement.jump;
    }


}
