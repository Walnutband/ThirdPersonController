using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//其实这种围绕旋转的同时自身也在旋转，就相当于行星的公转和自转速度相同，比如月球，面向地球的始终是同一个面，这样就不会看到月球的背面，也就是潮汐锁定现象

public class MovementTest : MonoBehaviour
{
    public Transform[] orbitalObjects;
    public Renderer[] renderers;
    public Color[] matColors;
    public float rotateSpeed = 30f;

    void Awake()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            //动态创建实例，消耗内存
            Material newMaterial = new Material(renderers[i].material); // 创建一个材质实例
            newMaterial.color = matColors[i]; // 设置颜色
            renderers[i].material = newMaterial;
        }
    }

    void Update()
    {
        RotateAround();
    }

    /// <summary>
    /// 让所有物体绕着自身旋转
    /// </summary>
    public void RotateAround()
    {
        foreach (Transform orbitalObject in orbitalObjects)
        {
            orbitalObject.RotateAround(transform.position, Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}


