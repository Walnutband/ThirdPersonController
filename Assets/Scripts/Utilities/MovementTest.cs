using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


