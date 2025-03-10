using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using UnityEngine.UIElements;

public class TextInputExample : MonoBehaviour
{
    //在字段初始化中不能访问非静态实体，不过可以使用get（和set），在进行读写时执行相应的操作
    //注意set访问器如果写了，就必须实现，如果没写则为只读
    public Vector3 position { get { return transform.position; } set { transform.position = value; } }
    public Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }

    public Transform target;
    public Transform startPoint;
    public Transform endPoint;
    public bool cursorLock;
    public float transSpeed = 0.5f;
    public bool transDir;

    public bool setFirst;

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    //void Update()
    //{
    //    if (cursorLock) Cursor.lockState = CursorLockMode.Locked;
    //    else Cursor.lockState = CursorLockMode.None;

    //    if (target != null && startPoint != null && endPoint != null)
    //        Transport(transform);

    //}

    private void Update()
    {
        if (setFirst)
        {
            transform.SetAsFirstSibling();
        }
    }

    private Vector3 GetDirection(Vector3 startPoint, Vector3 endPoint)
    {
        return endPoint - startPoint;
    }

    private void Transport(Transform transportTarget)
    {
        Vector3 dir = GetDirection(startPoint.position, endPoint.position).normalized;
        transportTarget.RotateAround(target.transform.position, dir, 20 * Time.deltaTime);
        //RotateAround(target.transform.position, dir, 20 * Time.deltaTime);
        if (transDir) transportTarget.Translate(dir * transSpeed * Time.deltaTime, Space.World);
        else transportTarget.Translate((-1) * dir * transSpeed * Time.deltaTime, Space.World);
    }

    #region 似乎实现了RotateAround方法
    //这是RotateAround方法的源码写法
    public void RotateAround(Vector3 point, Vector3 axis, float angle)
    {
        Vector3 worldPos = position;
        Quaternion q = Quaternion.AngleAxis(angle, axis); //获取绕axis旋转angle角度的四元数
        //就是向量减法，以及向量乘以四元数相当于进行相应旋转
        Vector3 dif = worldPos - point;
        dif = q * dif;
        worldPos = point + dif; //起点加上旋转后的向量得到终点。
        position = worldPos;
        //对象本身旋转，上面是对象绕轴旋转，但其实本质上就是位移，只是看起来像旋转，不过确实可以用一个父对象旋转带动该对象位移，也可以看作是旋转，不过是父对象。
        //RotateAroundInternal(axis, angle * Mathf.Deg2Rad);
        RotateAroundInternal(axis, angle);
    }

    private void RotateAroundInternal(Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rotation *= q;
    }
    #endregion
}
