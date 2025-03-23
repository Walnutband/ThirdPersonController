using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;
    [SerializeField] private Vector3 posDir;
    [SerializeField] private float distance;
    [SerializeField] private Vector3 posOffset;


    // Update is called once per frame
    void Update()
    {
        posDir = playerTrans.rotation * Vector3.back;
        transform.position = playerTrans.position + posDir * distance;
        transform.LookAt(playerTrans);
        transform.Translate(posOffset, Space.Self);
    }
}

//将World Space的幕布的边界对齐到相机视椎体
public class MatchCanvasToCamera : MonoBehaviour
{
    public Canvas canvas; // 要调整的Canvas
    public Camera targetCamera; // 目标相机

    void Start()
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // 获取相机的Near Plane距离和视锥体信息
            float nearPlane = targetCamera.nearClipPlane;
            float fov = targetCamera.fieldOfView;
            float aspect = targetCamera.aspect;

            // 计算视椎体宽高（Near Plane的宽高）
            float height = 2.0f * nearPlane * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float width = height * aspect;

            // 设置Canvas的RectTransform尺寸
            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);

            // 将Canvas的位置对齐到相机的Near Plane
            canvas.transform.position = targetCamera.transform.position + targetCamera.transform.forward * nearPlane;

            // 确保Canvas正对相机
            canvas.transform.rotation = targetCamera.transform.rotation;
        }
        else
        {
            Debug.LogWarning("Canvas必须设置为World Space模式！");
        }
    }
}
