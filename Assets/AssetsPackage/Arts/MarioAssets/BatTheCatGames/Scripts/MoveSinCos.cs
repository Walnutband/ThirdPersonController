using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSinCos : MonoBehaviour
{
    [SerializeField] float amplitudex, frequencyx, amplitudey, frequencyy, amplitudez, frequencyz;
    private Vector3 initialPos;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //这里就是没有左右偏移（具有中心对称性）的正弦函数Asin(wx)。这里Time.time代表的是x即横坐标，frequency就是频率，不同轴就代表了不同平面上的正弦运动
        float x = initialPos.x + amplitudex * Mathf.Sin(Time.time * frequencyx);
        float y = initialPos.y + amplitudey * Mathf.Sin(Time.time * frequencyy);
        float z = initialPos.z + amplitudez * Mathf.Sin(Time.time * frequencyz);

        transform.localPosition = new Vector3(x, y, z);
    }
}
