using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct Health
{
    public int armor;
    public int life;
}

[ExecuteInEditMode]
public class DestructibleTankScript : MonoBehaviour
{
    public string tankName = "Tank";
    public float tankSize = 1;
    //使用UXMl进行自动绑定，这里就属于绑定嵌套属性，就需要将父元素的binding path设置为health即该结构体属性，然后其两个子元素PropertyField的binding path分别设置为armor和life
    public Health health;

    private void Update()
    {
        gameObject.name = tankName;
        gameObject.transform.localScale = new Vector3(tankSize, tankSize, tankSize);
    }

    public void Reset()
    {
        health.armor = 100;
        health.life = 10;
    }
}
